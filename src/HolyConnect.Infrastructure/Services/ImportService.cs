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

    public ImportService(IRequestService requestService)
    {
        _requestService = requestService;
    }

    public bool CanImport(ImportSource source)
    {
        return source == ImportSource.Curl;
    }

    public async Task<ImportResult> ImportFromCurlAsync(string curlCommand, Guid environmentId, Guid? collectionId = null)
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
            var request = ParseCurlCommand(curlCommand, environmentId, collectionId);
            
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

    private RestRequest? ParseCurlCommand(string curlCommand, Guid environmentId, Guid? collectionId)
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
            
            // Generate a name from the URL
            request.Name = GenerateNameFromUrl(url);

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
        // Match URL patterns - it's usually the first unquoted argument or in quotes
        // Pattern 1: URL in single or double quotes
        var quotedUrlMatch = Regex.Match(curlCommand, @"curl\s+[^\s]*\s*['""]([^'""]+)['""]");
        if (quotedUrlMatch.Success)
        {
            return quotedUrlMatch.Groups[1].Value;
        }

        // Pattern 2: URL without quotes (first argument after curl and any flags)
        var urlMatch = Regex.Match(curlCommand, @"curl\s+(?:--[^\s]+\s+|--[^\s]+\s+['""][^'""]*['""]|)*\s*([^\s-][^\s]*?)(?:\s+|$)");
        if (urlMatch.Success)
        {
            var url = urlMatch.Groups[1].Value.Trim();
            // Remove trailing flags or options
            url = Regex.Replace(url, @"[\s]+.*$", "");
            return url;
        }

        // Pattern 3: Simple case - curl URL
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
            if (lastSegment.Length > 20 || Guid.TryParse(lastSegment, out _) || lastSegment.All(char.IsDigit))
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

        // Match -d, --data, --data-raw, --data-binary, --data-urlencode
        // Use a more sophisticated pattern to handle escaped quotes inside the data
        var dataPattern = @"(?:-d|--data|--data-raw|--data-binary|--data-urlencode)\s+(['""])(.+?)\1";
        var dataMatch = Regex.Match(curlCommand, dataPattern);
        if (dataMatch.Success)
        {
            body = dataMatch.Groups[2].Value;
            
            // Unescape backslash-escaped quotes
            body = body.Replace("\\\"", "\"");
            
            // If no content-type specified, try to infer it
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

        // Also check for --data without quotes (for simple data)
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
