using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using System.Text.Json;

namespace HolyConnect.Infrastructure.Services.ImportStrategies;

/// <summary>
/// Strategy for importing requests from Postman collections
/// </summary>
public class PostmanImportStrategy : IImportStrategy
{
    public ImportSource Source => ImportSource.Postman;

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

            // Parse Postman JSON format
            using var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;

            // Check if this is a Postman collection or a single request
            if (root.TryGetProperty("info", out var infoElement) && 
                root.TryGetProperty("item", out var itemsElement))
            {
                // This is a full collection, we need to handle this differently
                // For single request parse, we'll just take the first request item
                if (itemsElement.ValueKind == JsonValueKind.Array && itemsElement.GetArrayLength() > 0)
                {
                    var firstItem = itemsElement[0];
                    return ParsePostmanRequest(firstItem, collectionId, customName);
                }
                return null;
            }
            else
            {
                // This might be a single request item
                return ParsePostmanRequest(root, collectionId, customName);
            }
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parse a full Postman collection and return all requests, collections, and environments
    /// </summary>
    public (List<Request> Requests, List<Collection> Collections, List<Domain.Entities.Environment> Environments) ParseCollection(
        string content, 
        Guid? parentCollectionId = null)
    {
        var requests = new List<Request>();
        var collections = new List<Collection>();
        var environments = new List<Domain.Entities.Environment>();

        try
        {
            using var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;

            // Validate this is a Postman collection
            if (!root.TryGetProperty("info", out var infoElement))
            {
                return (requests, collections, environments);
            }

            // Get collection name
            var collectionName = "Imported Collection";
            if (infoElement.TryGetProperty("name", out var nameElement))
            {
                collectionName = nameElement.GetString() ?? collectionName;
            }

            // Create the main collection
            var mainCollection = new Collection
            {
                Id = Guid.NewGuid(),
                Name = collectionName,
                ParentCollectionId = parentCollectionId,
                CreatedAt = DateTime.UtcNow,
                Variables = new Dictionary<string, string>()
            };

            // Extract collection-level variables
            if (root.TryGetProperty("variable", out var variablesElement))
            {
                mainCollection.Variables = ParsePostmanVariables(variablesElement);
            }

            collections.Add(mainCollection);

            // Process items (requests and folders)
            if (root.TryGetProperty("item", out var itemsElement) && 
                itemsElement.ValueKind == JsonValueKind.Array)
            {
                ProcessPostmanItems(itemsElement, mainCollection.Id, requests, collections);
            }
        }
        catch
        {
            // Return empty results on error
        }

        return (requests, collections, environments);
    }

    /// <summary>
    /// Parse Postman environment JSON
    /// </summary>
    public Domain.Entities.Environment? ParseEnvironment(string content, string? customName = null)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;

            // Validate this is a Postman environment
            if (!root.TryGetProperty("values", out var valuesElement))
            {
                return null;
            }

            // Get environment name
            var environmentName = "Imported Environment";
            if (!string.IsNullOrWhiteSpace(customName))
            {
                environmentName = customName;
            }
            else if (root.TryGetProperty("name", out var nameElement))
            {
                environmentName = nameElement.GetString() ?? environmentName;
            }

            var environment = new Domain.Entities.Environment
            {
                Id = Guid.NewGuid(),
                Name = environmentName,
                CreatedAt = DateTime.UtcNow,
                Variables = new Dictionary<string, string>(),
                SecretVariableNames = new HashSet<string>()
            };

            // Parse environment variables
            if (valuesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var variable in valuesElement.EnumerateArray())
                {
                    if (variable.TryGetProperty("key", out var keyElement) &&
                        variable.TryGetProperty("value", out var valueElement))
                    {
                        var key = keyElement.GetString();
                        var value = valueElement.GetString();

                        if (!string.IsNullOrEmpty(key))
                        {
                            environment.Variables[key] = value ?? string.Empty;

                            // Check if this is a secret variable
                            if (variable.TryGetProperty("type", out var typeElement))
                            {
                                var type = typeElement.GetString();
                                if (type == "secret")
                                {
                                    environment.SecretVariableNames.Add(key);
                                }
                            }
                        }
                    }
                }
            }

            return environment;
        }
        catch
        {
            return null;
        }
    }

    private Request? ParsePostmanRequest(JsonElement item, Guid? collectionId, string? customName)
    {
        try
        {
            // Skip if this is a folder (has 'item' property)
            if (item.TryGetProperty("item", out _))
            {
                return null;
            }

            // Get request name
            var requestName = "Imported Request";
            if (!string.IsNullOrWhiteSpace(customName))
            {
                requestName = customName;
            }
            else if (item.TryGetProperty("name", out var nameElement))
            {
                requestName = nameElement.GetString() ?? requestName;
            }

            // Get request details
            if (!item.TryGetProperty("request", out var requestElement))
            {
                return null;
            }

            // Determine if it's a GraphQL request based on the request body
            var isGraphQL = IsGraphQLRequest(requestElement);

            if (isGraphQL)
            {
                return ParsePostmanGraphQLRequest(requestElement, collectionId, requestName);
            }
            else
            {
                return ParsePostmanRestRequest(requestElement, collectionId, requestName);
            }
        }
        catch
        {
            return null;
        }
    }

    private bool IsGraphQLRequest(JsonElement requestElement)
    {
        // Check if body mode is "graphql" or if body contains GraphQL-specific properties
        if (requestElement.TryGetProperty("body", out var bodyElement))
        {
            if (bodyElement.TryGetProperty("mode", out var modeElement))
            {
                var mode = modeElement.GetString();
                if (mode?.Equals("graphql", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return true;
                }
            }

            // Also check if there's a graphql property in the body
            if (bodyElement.TryGetProperty("graphql", out _))
            {
                return true;
            }
        }

        return false;
    }

    private RestRequest ParsePostmanRestRequest(JsonElement requestElement, Guid? collectionId, string requestName)
    {
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            CollectionId = collectionId,
            CreatedAt = DateTime.UtcNow,
            Name = requestName,
            Method = Domain.Entities.HttpMethod.Get
        };

        // Extract HTTP method
        if (requestElement.TryGetProperty("method", out var methodElement))
        {
            var methodStr = methodElement.GetString()?.ToUpperInvariant();
            request.Method = methodStr switch
            {
                "GET" => Domain.Entities.HttpMethod.Get,
                "POST" => Domain.Entities.HttpMethod.Post,
                "PUT" => Domain.Entities.HttpMethod.Put,
                "DELETE" => Domain.Entities.HttpMethod.Delete,
                "PATCH" => Domain.Entities.HttpMethod.Patch,
                "HEAD" => Domain.Entities.HttpMethod.Head,
                "OPTIONS" => Domain.Entities.HttpMethod.Options,
                _ => Domain.Entities.HttpMethod.Get
            };
        }

        // Extract URL
        request.Url = ExtractPostmanUrl(requestElement);

        // Extract headers
        request.Headers = ExtractPostmanHeaders(requestElement);

        // Extract authentication
        ExtractPostmanAuthentication(requestElement, request);

        // Extract body
        var bodyInfo = ExtractPostmanBody(requestElement);
        if (!string.IsNullOrEmpty(bodyInfo.Body))
        {
            request.Body = bodyInfo.Body;
            request.BodyType = bodyInfo.BodyType;
            request.ContentType = bodyInfo.ContentType;
        }

        return request;
    }

    private GraphQLRequest ParsePostmanGraphQLRequest(JsonElement requestElement, Guid? collectionId, string requestName)
    {
        var request = new GraphQLRequest
        {
            Id = Guid.NewGuid(),
            CollectionId = collectionId,
            CreatedAt = DateTime.UtcNow,
            Name = requestName,
            Query = string.Empty
        };

        // Extract URL
        request.Url = ExtractPostmanUrl(requestElement);

        // Extract headers
        request.Headers = ExtractPostmanHeaders(requestElement);

        // Extract authentication
        ExtractPostmanAuthentication(requestElement, request);

        // Extract GraphQL query and variables
        if (requestElement.TryGetProperty("body", out var bodyElement) &&
            bodyElement.TryGetProperty("graphql", out var graphqlElement))
        {
            if (graphqlElement.TryGetProperty("query", out var queryElement))
            {
                request.Query = queryElement.GetString() ?? string.Empty;

                // Determine operation type
                var queryTrimmed = request.Query.TrimStart();
                if (queryTrimmed.StartsWith("mutation", StringComparison.OrdinalIgnoreCase))
                {
                    request.OperationType = GraphQLOperationType.Mutation;
                }
                else if (queryTrimmed.StartsWith("subscription", StringComparison.OrdinalIgnoreCase))
                {
                    request.OperationType = GraphQLOperationType.Subscription;
                }
                else
                {
                    request.OperationType = GraphQLOperationType.Query;
                }
            }

            if (graphqlElement.TryGetProperty("variables", out var variablesElement))
            {
                request.Variables = variablesElement.GetString() ?? string.Empty;
            }
        }

        return request;
    }

    private string ExtractPostmanUrl(JsonElement requestElement)
    {
        if (!requestElement.TryGetProperty("url", out var urlElement))
        {
            return string.Empty;
        }

        // URL can be a string or an object
        if (urlElement.ValueKind == JsonValueKind.String)
        {
            return urlElement.GetString() ?? string.Empty;
        }
        else if (urlElement.ValueKind == JsonValueKind.Object)
        {
            // URL object format: { "raw": "https://...", "protocol": "https", "host": [...], "path": [...] }
            if (urlElement.TryGetProperty("raw", out var rawElement))
            {
                return rawElement.GetString() ?? string.Empty;
            }

            // Build URL from components
            var protocol = "https";
            var host = string.Empty;
            var path = string.Empty;

            if (urlElement.TryGetProperty("protocol", out var protocolElement))
            {
                protocol = protocolElement.GetString() ?? "https";
            }

            if (urlElement.TryGetProperty("host", out var hostElement))
            {
                if (hostElement.ValueKind == JsonValueKind.Array)
                {
                    var hostParts = new List<string>();
                    foreach (var part in hostElement.EnumerateArray())
                    {
                        hostParts.Add(part.GetString() ?? string.Empty);
                    }
                    host = string.Join(".", hostParts);
                }
                else if (hostElement.ValueKind == JsonValueKind.String)
                {
                    host = hostElement.GetString() ?? string.Empty;
                }
            }

            if (urlElement.TryGetProperty("path", out var pathElement))
            {
                if (pathElement.ValueKind == JsonValueKind.Array)
                {
                    var pathParts = new List<string>();
                    foreach (var part in pathElement.EnumerateArray())
                    {
                        pathParts.Add(part.GetString() ?? string.Empty);
                    }
                    path = string.Join("/", pathParts);
                }
                else if (pathElement.ValueKind == JsonValueKind.String)
                {
                    path = pathElement.GetString() ?? string.Empty;
                }
            }

            var url = $"{protocol}://{host}";
            if (!string.IsNullOrEmpty(path))
            {
                url += $"/{path}";
            }

            // Add query parameters if present
            if (urlElement.TryGetProperty("query", out var queryElement) && 
                queryElement.ValueKind == JsonValueKind.Array)
            {
                var queryParams = new List<string>();
                foreach (var param in queryElement.EnumerateArray())
                {
                    if (param.TryGetProperty("key", out var keyElement) &&
                        param.TryGetProperty("value", out var valueElement))
                    {
                        var key = keyElement.GetString();
                        var value = valueElement.GetString();
                        if (!string.IsNullOrEmpty(key))
                        {
                            queryParams.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value ?? string.Empty)}");
                        }
                    }
                }
                if (queryParams.Any())
                {
                    url += $"?{string.Join("&", queryParams)}";
                }
            }

            return url;
        }

        return string.Empty;
    }

    private Dictionary<string, string> ExtractPostmanHeaders(JsonElement requestElement)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (requestElement.TryGetProperty("header", out var headerElement) &&
            headerElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var header in headerElement.EnumerateArray())
            {
                if (header.TryGetProperty("key", out var keyElement) &&
                    header.TryGetProperty("value", out var valueElement))
                {
                    var key = keyElement.GetString();
                    var value = valueElement.GetString();

                    if (!string.IsNullOrEmpty(key))
                    {
                        // Check if header is disabled
                        var disabled = false;
                        if (header.TryGetProperty("disabled", out var disabledElement))
                        {
                            disabled = disabledElement.GetBoolean();
                        }

                        if (!disabled)
                        {
                            headers[key] = value ?? string.Empty;
                        }
                    }
                }
            }
        }

        return headers;
    }

    private void ExtractPostmanAuthentication(JsonElement requestElement, Request request)
    {
        if (!requestElement.TryGetProperty("auth", out var authElement))
        {
            request.AuthType = AuthenticationType.None;
            return;
        }

        if (!authElement.TryGetProperty("type", out var typeElement))
        {
            request.AuthType = AuthenticationType.None;
            return;
        }

        var authType = typeElement.GetString()?.ToLowerInvariant();

        switch (authType)
        {
            case "bearer":
                request.AuthType = AuthenticationType.BearerToken;
                if (authElement.TryGetProperty("bearer", out var bearerElement))
                {
                    foreach (var item in bearerElement.EnumerateArray())
                    {
                        if (item.TryGetProperty("key", out var keyElement) &&
                            keyElement.GetString() == "token" &&
                            item.TryGetProperty("value", out var valueElement))
                        {
                            request.BearerToken = valueElement.GetString() ?? string.Empty;
                            break;
                        }
                    }
                }
                break;

            case "basic":
                request.AuthType = AuthenticationType.Basic;
                if (authElement.TryGetProperty("basic", out var basicElement))
                {
                    foreach (var item in basicElement.EnumerateArray())
                    {
                        if (item.TryGetProperty("key", out var keyElement) &&
                            item.TryGetProperty("value", out var valueElement))
                        {
                            var key = keyElement.GetString();
                            var value = valueElement.GetString();

                            if (key == "username")
                            {
                                request.BasicAuthUsername = value ?? string.Empty;
                            }
                            else if (key == "password")
                            {
                                request.BasicAuthPassword = value ?? string.Empty;
                            }
                        }
                    }
                }
                break;

            default:
                request.AuthType = AuthenticationType.None;
                break;
        }
    }

    private (string? Body, BodyType BodyType, string? ContentType) ExtractPostmanBody(JsonElement requestElement)
    {
        if (!requestElement.TryGetProperty("body", out var bodyElement))
        {
            return (null, BodyType.None, null);
        }

        if (!bodyElement.TryGetProperty("mode", out var modeElement))
        {
            return (null, BodyType.None, null);
        }

        var mode = modeElement.GetString()?.ToLowerInvariant();

        switch (mode)
        {
            case "raw":
                if (bodyElement.TryGetProperty("raw", out var rawElement))
                {
                    var body = rawElement.GetString();
                    var contentType = "text/plain";
                    var bodyType = BodyType.Text;

                    // Try to determine content type from options
                    if (bodyElement.TryGetProperty("options", out var optionsElement) &&
                        optionsElement.TryGetProperty("raw", out var rawOptionsElement) &&
                        rawOptionsElement.TryGetProperty("language", out var languageElement))
                    {
                        var language = languageElement.GetString()?.ToLowerInvariant();
                        (contentType, bodyType) = language switch
                        {
                            "json" => ("application/json", BodyType.Json),
                            "xml" => ("application/xml", BodyType.Xml),
                            "html" => ("text/html", BodyType.Html),
                            "javascript" => ("application/javascript", BodyType.JavaScript),
                            _ => ("text/plain", BodyType.Text)
                        };
                    }
                    else if (!string.IsNullOrEmpty(body))
                    {
                        // Try to infer from body content
                        var trimmedBody = body.TrimStart();
                        if (trimmedBody.StartsWith("{") || trimmedBody.StartsWith("["))
                        {
                            contentType = "application/json";
                            bodyType = BodyType.Json;
                        }
                        else if (trimmedBody.StartsWith("<"))
                        {
                            contentType = "application/xml";
                            bodyType = BodyType.Xml;
                        }
                    }

                    return (body, bodyType, contentType);
                }
                break;

            case "formdata":
            case "urlencoded":
                // Convert form data to appropriate format
                var formData = new List<string>();
                var formDataProperty = mode == "formdata" ? "formdata" : "urlencoded";
                
                if (bodyElement.TryGetProperty(formDataProperty, out var formElement) &&
                    formElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in formElement.EnumerateArray())
                    {
                        if (item.TryGetProperty("key", out var keyElement) &&
                            item.TryGetProperty("value", out var valueElement))
                        {
                            var key = keyElement.GetString();
                            var value = valueElement.GetString();
                            
                            // Check if item is disabled
                            var disabled = false;
                            if (item.TryGetProperty("disabled", out var disabledElement))
                            {
                                disabled = disabledElement.GetBoolean();
                            }

                            if (!disabled && !string.IsNullOrEmpty(key))
                            {
                                formData.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value ?? string.Empty)}");
                            }
                        }
                    }
                }

                if (formData.Any())
                {
                    var body = string.Join("&", formData);
                    var contentType = mode == "formdata" 
                        ? "multipart/form-data" 
                        : "application/x-www-form-urlencoded";
                    return (body, BodyType.Text, contentType);
                }
                break;
        }

        return (null, BodyType.None, null);
    }

    private Dictionary<string, string> ParsePostmanVariables(JsonElement variablesElement)
    {
        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (variablesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var variable in variablesElement.EnumerateArray())
            {
                if (variable.TryGetProperty("key", out var keyElement) &&
                    variable.TryGetProperty("value", out var valueElement))
                {
                    var key = keyElement.GetString();
                    var value = valueElement.GetString();

                    if (!string.IsNullOrEmpty(key))
                    {
                        variables[key] = value ?? string.Empty;
                    }
                }
            }
        }

        return variables;
    }

    private void ProcessPostmanItems(
        JsonElement itemsElement,
        Guid parentCollectionId,
        List<Request> requests,
        List<Collection> collections)
    {
        foreach (var item in itemsElement.EnumerateArray())
        {
            // Check if this is a folder or a request
            if (item.TryGetProperty("item", out var subItemsElement))
            {
                // This is a folder - create a subcollection
                var folderName = "Subfolder";
                if (item.TryGetProperty("name", out var nameElement))
                {
                    folderName = nameElement.GetString() ?? folderName;
                }

                var subCollection = new Collection
                {
                    Id = Guid.NewGuid(),
                    Name = folderName,
                    ParentCollectionId = parentCollectionId,
                    CreatedAt = DateTime.UtcNow,
                    Variables = new Dictionary<string, string>()
                };

                collections.Add(subCollection);

                // Process items in this folder recursively
                ProcessPostmanItems(subItemsElement, subCollection.Id, requests, collections);
            }
            else
            {
                // This is a request
                var request = ParsePostmanRequest(item, parentCollectionId, null);
                if (request != null)
                {
                    requests.Add(request);
                }
            }
        }
    }
}
