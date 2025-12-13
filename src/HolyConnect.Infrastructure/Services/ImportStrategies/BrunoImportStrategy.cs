using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using System.Text.RegularExpressions;

namespace HolyConnect.Infrastructure.Services.ImportStrategies;

/// <summary>
/// Strategy for importing requests from Bruno files
/// </summary>
public class BrunoImportStrategy : IImportStrategy
{
    public ImportSource Source => ImportSource.Bruno;

    public Request? Parse(string content, Guid? collectionId, string? customName)
    {
        try
        {
            // Clean up the content
            content = content.Trim();
            
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            return ParseBrunoFile(content, collectionId, customName);
        }
        catch
        {
            return null;
        }
    }

    private Request? ParseBrunoFile(string brunoFileContent, Guid? collectionId, string? customName)
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
                return ParseBrunoGraphQLRequest(sections, collectionId, requestName);
            }
            else
            {
                return ParseBrunoRestRequest(sections, collectionId, requestName);
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
        int bracketDepth = 0;
        bool isArraySection = false;
        
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip comments
            if (trimmedLine.StartsWith("//"))
                continue;
            
            // Check if this is a section header with braces (e.g., "meta {", "headers {", "body:json {")
            if (braceDepth == 0 && bracketDepth == 0 && trimmedLine.EndsWith('{') && !string.IsNullOrWhiteSpace(trimmedLine))
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
                isArraySection = false;
            }
            // Check if this is a section header with brackets (e.g., "vars:secret [")
            else if (braceDepth == 0 && bracketDepth == 0 && trimmedLine.EndsWith('[') && !string.IsNullOrWhiteSpace(trimmedLine))
            {
                // Save previous section if exists
                if (currentSection != null)
                {
                    sections[currentSection] = currentContent.ToString().Trim();
                }
                
                // Start new section
                currentSection = trimmedLine.TrimEnd('[').Trim();
                currentContent.Clear();
                bracketDepth = 1;
                isArraySection = true;
            }
            else if (currentSection != null)
            {
                // Count braces/brackets to track nesting depth
                foreach (char c in line)
                {
                    if (c == '{') braceDepth++;
                    else if (c == '}') braceDepth--;
                    else if (c == '[') bracketDepth++;
                    else if (c == ']') bracketDepth--;
                }
                
                // If we're back to depth 0, the section is complete
                if ((isArraySection && bracketDepth == 0) || (!isArraySection && braceDepth == 0))
                {
                    sections[currentSection] = currentContent.ToString().Trim();
                    currentSection = null;
                    currentContent.Clear();
                    isArraySection = false;
                }
                else
                {
                    // Add content to current section (but not the closing bracket/brace line if it closes the section)
                    if (braceDepth > 0 || bracketDepth > 0)
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

    private RestRequest ParseBrunoRestRequest(Dictionary<string, string> sections, Guid? collectionId, string requestName)
    {
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
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

    private GraphQLRequest ParseBrunoGraphQLRequest(Dictionary<string, string> sections, Guid? collectionId, string requestName)
    {
        var request = new GraphQLRequest
        {
            Id = Guid.NewGuid(),
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

    /// <summary>
    /// Parse a Bruno environment file (.bru file in environments/ folder)
    /// </summary>
    /// <param name="content">Content of the .bru environment file</param>
    /// <param name="environmentName">Name for the environment (typically from filename)</param>
    /// <returns>Parsed Environment object or null if parsing fails</returns>
    public Domain.Entities.Environment? ParseEnvironment(string content, string environmentName)
    {
        try
        {
            content = content.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            var sections = ParseBrunoSections(content);
            
            // Create environment
            var environment = new Domain.Entities.Environment
            {
                Id = Guid.NewGuid(),
                Name = environmentName,
                CreatedAt = DateTime.UtcNow,
                Variables = new Dictionary<string, string>(),
                SecretVariableNames = new HashSet<string>()
            };

            // Extract variables from vars section
            if (sections.TryGetValue("vars", out var varsContent))
            {
                environment.Variables = ParseBrunoVariables(varsContent);
            }

            // Extract secret variable names from vars:secret section
            if (sections.TryGetValue("vars:secret", out var secretsContent))
            {
                var secretVars = ParseBrunoSecretVariables(secretsContent);
                foreach (var secretVar in secretVars)
                {
                    environment.SecretVariableNames.Add(secretVar);
                }
            }

            return environment;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parse collection variables from collection.bru or bruno.json
    /// </summary>
    /// <param name="content">Content of the collection configuration file</param>
    /// <returns>Dictionary of variables</returns>
    public Dictionary<string, string> ParseCollectionVariables(string content)
    {
        try
        {
            content = content.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                return new Dictionary<string, string>();
            }

            var sections = ParseBrunoSections(content);
            
            // Extract variables from vars section
            if (sections.TryGetValue("vars", out var varsContent))
            {
                return ParseBrunoVariables(varsContent);
            }

            return new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Parse secret variable names from collection.bru
    /// </summary>
    public HashSet<string> ParseCollectionSecretVariables(string content)
    {
        try
        {
            content = content.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                return new HashSet<string>();
            }

            var sections = ParseBrunoSections(content);
            
            // Extract secret variable names from vars:secret section
            if (sections.TryGetValue("vars:secret", out var secretsContent))
            {
                return ParseBrunoSecretVariables(secretsContent);
            }

            return new HashSet<string>();
        }
        catch
        {
            return new HashSet<string>();
        }
    }

    private Dictionary<string, string> ParseBrunoVariables(string varsContent)
    {
        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var lines = varsContent.Split('\n');

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;

            // Variables are in format: key: value
            var parts = trimmedLine.Split(':', 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                variables[key] = value;
            }
        }

        return variables;
    }

    private HashSet<string> ParseBrunoSecretVariables(string secretsContent)
    {
        var secrets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lines = secretsContent.Split('\n');

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;

            // Skip array markers
            if (trimmedLine == "[" || trimmedLine == "]")
                continue;

            secrets.Add(trimmedLine);
        }

        return secrets;
    }
}
