using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using System.Text.RegularExpressions;

namespace HolyConnect.Infrastructure.Services;

/// <summary>
/// Service for importing requests from various formats
/// </summary>
public class ImportService : IImportService
{
    private readonly IRequestService _requestService;
    
    // Constants for request name generation
    private const int MaxSegmentLengthForNaming = 20;

    public ImportService(IRequestService requestService)
    {
        _requestService = requestService;
    }

    public bool CanImport(ImportSource source)
    {
        return source == ImportSource.Curl || source == ImportSource.Bruno;
    }

    public async Task<ImportResult> ImportFromCurlAsync(string curlCommand, Guid environmentId, Guid? collectionId = null, string? customName = null)
    {
        var result = new ImportResult();

        try
        {
            // Clean up the curl command
            curlCommand = curlCommand.Trim();
            
            // Remove line breaks and extra spaces for easier parsing
            curlCommand = Regex.Replace(curlCommand, @"\s*\\\s*\r?\n\s*", " ");
            curlCommand = Regex.Replace(curlCommand, @"\s+", " ");

            // Check if it's a curl command
            if (!curlCommand.StartsWith("curl", StringComparison.OrdinalIgnoreCase))
            {
                result.ErrorMessage = "Invalid curl command. Command must start with 'curl'.";
                return result;
            }

            // Parse the curl command
            var request = ParseCurlCommand(curlCommand, environmentId, collectionId, customName);
            
            if (request == null)
            {
                result.ErrorMessage = "Failed to parse curl command. Please check the format.";
                return result;
            }

            // Save the request
            var savedRequest = await _requestService.CreateRequestAsync(request);
            
            result.Success = true;
            result.ImportedRequest = savedRequest;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error importing curl command: {ex.Message}";
        }

        return result;
    }

    public async Task<ImportResult> ImportFromBrunoAsync(string brunoFileContent, Guid environmentId, Guid? collectionId = null, string? customName = null)
    {
        var result = new ImportResult();

        try
        {
            // Clean up the content
            brunoFileContent = brunoFileContent.Trim();
            
            if (string.IsNullOrWhiteSpace(brunoFileContent))
            {
                result.ErrorMessage = "Bruno file content is empty.";
                return result;
            }

            // Parse the Bruno file
            var request = ParseBrunoFile(brunoFileContent, environmentId, collectionId, customName);
            
            if (request == null)
            {
                result.ErrorMessage = "Failed to parse Bruno file. Please check the format.";
                return result;
            }

            // Save the request
            var savedRequest = await _requestService.CreateRequestAsync(request);
            
            result.Success = true;
            result.ImportedRequest = savedRequest;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error importing Bruno file: {ex.Message}";
        }

        return result;
    }

    private Request? ParseBrunoFile(string brunoFileContent, Guid environmentId, Guid? collectionId, string? customName)
    {
        try
        {
            // Parse sections from Bruno file
            var sections = ParseBrunoSections(brunoFileContent);
            
            // Extract meta information
            var meta = ParseBrunoMeta(sections);
            var requestName = !string.IsNullOrWhiteSpace(customName) ? customName : meta.Name;
            var requestType = meta.Type;
            
            // Determine if it's REST or GraphQL based on type or body content
            var isGraphQL = requestType.Equals("graphql", StringComparison.OrdinalIgnoreCase) ||
                           sections.ContainsKey("body:graphql");
            
            if (isGraphQL)
            {
                return ParseBrunoGraphQLRequest(sections, environmentId, collectionId, requestName);
            }
            else
            {
                return ParseBrunoRestRequest(sections, environmentId, collectionId, requestName);
            }
        }
        catch
        {
            return null;
        }
    }

    private Dictionary<string, string> ParseBrunoSections(string content)
    {
        var sections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var lines = content.Split('\n');
        string? currentSection = null;
        var currentContent = new System.Text.StringBuilder();
        int braceDepth = 0;
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip comments
            if (trimmedLine.StartsWith("//"))
                continue;
            
            // Check if this is a section header (e.g., "meta {", "headers {", "body:json {")
            // Only when we're not inside a section (braceDepth == 0)
            if (braceDepth == 0 && trimmedLine.EndsWith('{') && !string.IsNullOrWhiteSpace(trimmedLine))
            {
                // Save previous section if exists
                if (currentSection != null)
                {
                    sections[currentSection] = currentContent.ToString().Trim();
                }
                
                // Start new section
                currentSection = trimmedLine.TrimEnd('{').Trim();
                currentContent.Clear();
                braceDepth = 1;
            }
            else if (currentSection != null)
            {
                // Count braces to track nesting depth
                foreach (char c in line)
                {
                    if (c == '{') braceDepth++;
                    else if (c == '}') braceDepth--;
                }
                
                // If we're back to depth 0, the section is complete
                if (braceDepth == 0)
                {
                    sections[currentSection] = currentContent.ToString().Trim();
                    currentSection = null;
                    currentContent.Clear();
                }
                else
                {
                    // Add content to current section (but not the closing brace line if it closes the section)
                    if (braceDepth > 0)
                    {
                        currentContent.AppendLine(line);
                    }
                }
            }
        }
        
        return sections;
    }

    private (string Name, string Type) ParseBrunoMeta(Dictionary<string, string> sections)
    {
        var name = "Imported Request";
        var type = "http";
        
        if (sections.TryGetValue("meta", out var metaContent))
        {
            var nameMatch = Regex.Match(metaContent, @"name:\s*(.+)", RegexOptions.Multiline);
            if (nameMatch.Success)
            {
                name = nameMatch.Groups[1].Value.Trim();
            }
            
            var typeMatch = Regex.Match(metaContent, @"type:\s*(\w+)", RegexOptions.Multiline);
            if (typeMatch.Success)
            {
                type = typeMatch.Groups[1].Value.Trim();
            }
        }
        
        return (name, type);
    }

    private RestRequest ParseBrunoRestRequest(Dictionary<string, string> sections, Guid environmentId, Guid? collectionId, string requestName)
    {
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            EnvironmentId = environmentId,
            CollectionId = collectionId,
            CreatedAt = DateTime.UtcNow,
            Name = requestName,
            Method = Domain.Entities.HttpMethod.Get
        };
        
        // Extract HTTP method and URL from method sections (get, post, put, delete, patch, etc.)
        var methodInfo = ExtractBrunoHttpMethod(sections);
        request.Method = methodInfo.Method;
        request.Url = methodInfo.Url;
        
        // Extract headers
        if (sections.TryGetValue("headers", out var headersContent))
        {
            request.Headers = ParseBrunoHeaders(headersContent);
        }
        
        // Extract authentication
        ParseBrunoAuthentication(sections, request);
        
        // Extract body
        var bodyInfo = ExtractBrunoBody(sections);
        if (!string.IsNullOrEmpty(bodyInfo.Body))
        {
            request.Body = bodyInfo.Body;
            request.BodyType = bodyInfo.BodyType;
            request.ContentType = bodyInfo.ContentType;
        }
        
        return request;
    }

    private GraphQLRequest ParseBrunoGraphQLRequest(Dictionary<string, string> sections, Guid environmentId, Guid? collectionId, string requestName)
    {
        var request = new GraphQLRequest
        {
            Id = Guid.NewGuid(),
            EnvironmentId = environmentId,
            CollectionId = collectionId,
            CreatedAt = DateTime.UtcNow,
            Name = requestName,
            Query = string.Empty
        };
        
        // Extract URL from post section
        if (sections.TryGetValue("post", out var postContent))
        {
            var urlMatch = Regex.Match(postContent, @"url:\s*(.+)", RegexOptions.Multiline);
            if (urlMatch.Success)
            {
                request.Url = urlMatch.Groups[1].Value.Trim();
            }
        }
        
        // Extract headers
        if (sections.TryGetValue("headers", out var headersContent))
        {
            request.Headers = ParseBrunoHeaders(headersContent);
        }
        
        // Extract authentication
        ParseBrunoAuthentication(sections, request);
        
        // Extract GraphQL query
        if (sections.TryGetValue("body:graphql", out var queryContent))
        {
            request.Query = queryContent.Trim();
            
            // Determine operation type from query
            if (queryContent.TrimStart().StartsWith("mutation", StringComparison.OrdinalIgnoreCase))
            {
                request.OperationType = GraphQLOperationType.Mutation;
            }
            else if (queryContent.TrimStart().StartsWith("subscription", StringComparison.OrdinalIgnoreCase))
            {
                request.OperationType = GraphQLOperationType.Subscription;
            }
            else
            {
                request.OperationType = GraphQLOperationType.Query;
            }
        }
        
        // Extract GraphQL variables
        if (sections.TryGetValue("body:graphql:vars", out var varsContent))
        {
            request.Variables = varsContent.Trim();
        }
        
        return request;
    }

    private (Domain.Entities.HttpMethod Method, string Url) ExtractBrunoHttpMethod(Dictionary<string, string> sections)
    {
        // Check for each HTTP method section
        var methods = new[]
        {
            ("get", Domain.Entities.HttpMethod.Get),
            ("post", Domain.Entities.HttpMethod.Post),
            ("put", Domain.Entities.HttpMethod.Put),
            ("delete", Domain.Entities.HttpMethod.Delete),
            ("patch", Domain.Entities.HttpMethod.Patch),
            ("head", Domain.Entities.HttpMethod.Head),
            ("options", Domain.Entities.HttpMethod.Options)
        };
        
        foreach (var (sectionName, method) in methods)
        {
            if (sections.TryGetValue(sectionName, out var methodContent))
            {
                var urlMatch = Regex.Match(methodContent, @"url:\s*(.+)", RegexOptions.Multiline);
                if (urlMatch.Success)
                {
                    var url = urlMatch.Groups[1].Value.Trim();
                    return (method, url);
                }
            }
        }
        
        return (Domain.Entities.HttpMethod.Get, string.Empty);
    }

    private Dictionary<string, string> ParseBrunoHeaders(string headersContent)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var lines = headersContent.Split('\n');
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;
                
            var parts = trimmedLine.Split(':', 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                headers[key] = value;
            }
        }
        
        return headers;
    }

    private void ParseBrunoAuthentication(Dictionary<string, string> sections, Request request)
    {
        // Check for auth:bearer
        if (sections.TryGetValue("auth:bearer", out var bearerContent))
        {
            var tokenMatch = Regex.Match(bearerContent, @"token:\s*(.+)", RegexOptions.Multiline);
            if (tokenMatch.Success)
            {
                request.AuthType = AuthenticationType.BearerToken;
                request.BearerToken = tokenMatch.Groups[1].Value.Trim();
            }
        }
        // Check for auth:basic
        else if (sections.TryGetValue("auth:basic", out var basicContent))
        {
            var usernameMatch = Regex.Match(basicContent, @"username:\s*(.+)", RegexOptions.Multiline);
            var passwordMatch = Regex.Match(basicContent, @"password:\s*(.+)", RegexOptions.Multiline);
            
            if (usernameMatch.Success && passwordMatch.Success)
            {
                request.AuthType = AuthenticationType.Basic;
                request.BasicAuthUsername = usernameMatch.Groups[1].Value.Trim();
                request.BasicAuthPassword = passwordMatch.Groups[1].Value.Trim();
            }
        }
        else
        {
            request.AuthType = AuthenticationType.None;
        }
    }

    private (string? Body, BodyType BodyType, string? ContentType) ExtractBrunoBody(Dictionary<string, string> sections)
    {
        // Check for different body types
        if (sections.TryGetValue("body:json", out var jsonBody))
        {
            return (jsonBody.Trim(), BodyType.Json, "application/json");
        }
        if (sections.TryGetValue("body:xml", out var xmlBody))
        {
            return (xmlBody.Trim(), BodyType.Xml, "application/xml");
        }
        if (sections.TryGetValue("body:text", out var textBody))
        {
            return (textBody.Trim(), BodyType.Text, "text/plain");
        }
        if (sections.TryGetValue("body:html", out var htmlBody))
        {
            return (htmlBody.Trim(), BodyType.Html, "text/html");
        }
        if (sections.TryGetValue("body:javascript", out var jsBody))
        {
            return (jsBody.Trim(), BodyType.JavaScript, "application/javascript");
        }
        
        return (null, BodyType.None, null);
    }

    private RestRequest? ParseCurlCommand(string curlCommand, Guid environmentId, Guid? collectionId, string? customName)
    {
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            EnvironmentId = environmentId,
            CollectionId = collectionId,
            CreatedAt = DateTime.UtcNow,
            Name = "Imported Request",
            Method = Domain.Entities.HttpMethod.Get
        };

        try
        {
            // Extract URL (required)
            var url = ExtractUrl(curlCommand);
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            request.Url = url;
            
            // Use custom name if provided, otherwise generate from URL
            if (!string.IsNullOrWhiteSpace(customName))
            {
                request.Name = customName.Trim();
            }
            else
            {
                request.Name = GenerateNameFromUrl(url);
            }

            // Extract HTTP method (-X or --request)
            var method = ExtractMethod(curlCommand);
            if (method.HasValue)
            {
                request.Method = method.Value;
            }

            // Extract headers (-H or --header)
            request.Headers = ExtractHeaders(curlCommand);

            // Extract body data (-d, --data, --data-raw, --data-binary)
            var (body, contentType) = ExtractBodyAndContentType(curlCommand, request.Headers);
            if (!string.IsNullOrEmpty(body))
            {
                request.Body = body;
                request.BodyType = DetermineBodyType(contentType);
                
                // If content-type wasn't explicitly set in headers, add it
                if (!string.IsNullOrEmpty(contentType) && !request.Headers.ContainsKey("Content-Type"))
                {
                    request.Headers["Content-Type"] = contentType;
                }
            }

            // Extract authentication
            ExtractAuthentication(curlCommand, request);

            return request;
        }
        catch
        {
            return null;
        }
    }

    private string? ExtractUrl(string curlCommand)
    {
        // Try multiple patterns to extract the URL from curl command
        // URLs can be quoted or unquoted and may appear before or after flags
        
        // Pattern 1: URL in single or double quotes after curl
        // Example: curl 'https://example.com' or curl "https://example.com"
        var quotedUrlMatch = Regex.Match(curlCommand, @"curl\s+[^\s]*\s*['""]([^'""]+)['""]");
        if (quotedUrlMatch.Success)
        {
            return quotedUrlMatch.Groups[1].Value;
        }

        // Pattern 2: URL without quotes (after curl and optional flags)
        // Example: curl -X POST https://example.com
        // Matches first non-flag argument (doesn't start with -)
        var urlMatch = Regex.Match(curlCommand, @"curl\s+(?:--[^\s]+\s+|--[^\s]+\s+['""][^'""]*['""]|)*\s*([^\s-][^\s]*?)(?:\s+|$)");
        if (urlMatch.Success)
        {
            var url = urlMatch.Groups[1].Value.Trim();
            // Clean up any trailing whitespace or options
            url = Regex.Replace(url, @"[\s]+.*$", "");
            return url;
        }

        // Pattern 3: Simple case - just curl followed by URL
        // Example: curl https://example.com
        var simpleMatch = Regex.Match(curlCommand, @"curl\s+([^\s]+)");
        if (simpleMatch.Success)
        {
            return simpleMatch.Groups[1].Value;
        }

        return null;
    }

    private string GenerateNameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath.TrimEnd('/');
            
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                return $"{uri.Host} Request";
            }
            
            var segments = path.Split('/');
            var lastSegment = segments[^1];
            
            // If last segment looks like an ID or is too long, use the second-to-last
            if (lastSegment.Length > MaxSegmentLengthForNaming || Guid.TryParse(lastSegment, out _) || lastSegment.All(char.IsDigit))
            {
                if (segments.Length > 1)
                {
                    lastSegment = segments[^2];
                }
            }
            
            // Convert to title case
            return $"{char.ToUpper(lastSegment[0])}{lastSegment[1..]} Request";
        }
        catch
        {
            return "Imported Request";
        }
    }

    private Domain.Entities.HttpMethod? ExtractMethod(string curlCommand)
    {
        // Match -X or --request followed by the method
        var methodMatch = Regex.Match(curlCommand, @"(?:-X|--request)\s+([A-Z]+)", RegexOptions.IgnoreCase);
        if (methodMatch.Success)
        {
            var methodStr = methodMatch.Groups[1].Value.ToUpperInvariant();
            return methodStr switch
            {
                "GET" => Domain.Entities.HttpMethod.Get,
                "POST" => Domain.Entities.HttpMethod.Post,
                "PUT" => Domain.Entities.HttpMethod.Put,
                "DELETE" => Domain.Entities.HttpMethod.Delete,
                "PATCH" => Domain.Entities.HttpMethod.Patch,
                "HEAD" => Domain.Entities.HttpMethod.Head,
                "OPTIONS" => Domain.Entities.HttpMethod.Options,
                _ => null
            };
        }

        return null;
    }

    private Dictionary<string, string> ExtractHeaders(string curlCommand)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Match -H or --header followed by the header value in quotes
        var headerMatches = Regex.Matches(curlCommand, @"(?:-H|--header)\s+['""]([^'""]+)['""]");
        foreach (Match match in headerMatches)
        {
            var headerLine = match.Groups[1].Value;
            var parts = headerLine.Split(':', 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                headers[key] = value;
            }
        }

        return headers;
    }

    private (string? body, string? contentType) ExtractBodyAndContentType(string curlCommand, Dictionary<string, string> headers)
    {
        string? body = null;
        string? contentType = null;

        // Check for Content-Type in headers
        if (headers.TryGetValue("Content-Type", out var headerContentType))
        {
            contentType = headerContentType;
        }

        // Extract data from -d, --data, --data-raw, --data-binary, --data-urlencode flags
        // Pattern matches: -d "data" or --data 'data'
        // Uses non-greedy matching with backreference to match opening/closing quotes
        // Group 1: quote character (' or ")
        // Group 2: the actual data content
        var dataPattern = @"(?:-d|--data|--data-raw|--data-binary|--data-urlencode)\s+(['""])(.+?)\1";
        var dataMatch = Regex.Match(curlCommand, dataPattern);
        if (dataMatch.Success)
        {
            body = dataMatch.Groups[2].Value;
            
            // Unescape backslash-escaped quotes that were in the original curl command
            body = body.Replace("\\\"", "\"");
            
            // Infer content type from body structure if not explicitly set
            if (string.IsNullOrEmpty(contentType))
            {
                if (body.TrimStart().StartsWith("{") || body.TrimStart().StartsWith("["))
                {
                    contentType = "application/json";
                }
                else if (body.Contains("=") && body.Contains("&"))
                {
                    contentType = "application/x-www-form-urlencoded";
                }
                else
                {
                    contentType = "text/plain";
                }
            }
        }

        // Fallback: try to match unquoted data (simple key=value format)
        if (body == null)
        {
            var simpleDataMatch = Regex.Match(curlCommand, @"(?:-d|--data)\s+([^\s-]+)");
            if (simpleDataMatch.Success)
            {
                body = simpleDataMatch.Groups[1].Value;
                if (string.IsNullOrEmpty(contentType))
                {
                    contentType = "application/x-www-form-urlencoded";
                }
            }
        }

        return (body, contentType);
    }

    private BodyType DetermineBodyType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            return BodyType.Text;
        }

        return contentType.ToLowerInvariant() switch
        {
            var ct when ct.Contains("json") => BodyType.Json,
            var ct when ct.Contains("xml") => BodyType.Xml,
            var ct when ct.Contains("html") => BodyType.Html,
            var ct when ct.Contains("javascript") => BodyType.JavaScript,
            _ => BodyType.Text
        };
    }

    private void ExtractAuthentication(string curlCommand, RestRequest request)
    {
        // Check for -u or --user (basic auth)
        var userMatch = Regex.Match(curlCommand, @"(?:-u|--user)\s+['""]?([^'"":\s]+):([^'"":\s]+)['""]?");
        if (userMatch.Success)
        {
            request.AuthType = AuthenticationType.Basic;
            request.BasicAuthUsername = userMatch.Groups[1].Value;
            request.BasicAuthPassword = userMatch.Groups[2].Value;
            return;
        }

        // Check for Bearer token in Authorization header
        if (request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                request.AuthType = AuthenticationType.BearerToken;
                request.BearerToken = authHeader.Substring(7).Trim();
                // Remove from headers since it will be set via auth
                request.Headers.Remove("Authorization");
            }
        }
    }
}
