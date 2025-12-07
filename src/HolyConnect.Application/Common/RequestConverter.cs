using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Common;

/// <summary>
/// Helper class for converting between different request types
/// </summary>
public static class RequestConverter
{
    /// <summary>
    /// Converts a request to a different type, preserving common properties
    /// </summary>
    public static Request ConvertTo(Request source, RequestType targetType)
    {
        if (source.Type == targetType)
        {
            return RequestCloner.Clone(source);
        }

        return targetType switch
        {
            RequestType.Rest => ConvertToRest(source),
            RequestType.GraphQL => ConvertToGraphQL(source),
            RequestType.WebSocket => ConvertToWebSocket(source),
            _ => throw new NotSupportedException($"Request type {targetType} is not supported for conversion")
        };
    }

    private static RestRequest ConvertToRest(Request source)
    {
        var target = new RestRequest
        {
            Method = Domain.Entities.HttpMethod.Get,
            Body = string.Empty,
            BodyType = BodyType.Json,
            QueryParameters = new Dictionary<string, string>(),
            DisabledQueryParameters = new HashSet<string>()
        };

        // Try to preserve some data from source type
        if (source is GraphQLRequest graphQLRequest)
        {
            // Convert GraphQL query to JSON body
            if (!string.IsNullOrWhiteSpace(graphQLRequest.Query))
            {
                target.Method = Domain.Entities.HttpMethod.Post;
                target.Body = System.Text.Json.JsonSerializer.Serialize(new
                {
                    query = graphQLRequest.Query,
                    variables = graphQLRequest.Variables,
                    operationName = graphQLRequest.OperationName
                }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                target.BodyType = BodyType.Json;
            }
        }
        else if (source is WebSocketRequest wsRequest)
        {
            // Convert WebSocket message to body if present
            if (!string.IsNullOrWhiteSpace(wsRequest.Message))
            {
                target.Method = Domain.Entities.HttpMethod.Post;
                target.Body = wsRequest.Message;
                
                // Try to detect if message is JSON
                if (wsRequest.Message.TrimStart().StartsWith("{") || wsRequest.Message.TrimStart().StartsWith("["))
                {
                    target.BodyType = BodyType.Json;
                }
            }
        }

        CopyBaseProperties(source, target);
        
        // Update URL scheme if needed
        target.Url = ConvertUrlScheme(source.Url, "http", "https");
        
        return target;
    }

    private static GraphQLRequest ConvertToGraphQL(Request source)
    {
        var target = new GraphQLRequest
        {
            Query = string.Empty,
            Variables = null,
            OperationName = null,
            OperationType = GraphQLOperationType.Query,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.WebSocket
        };

        // Try to preserve some data from source type
        if (source is RestRequest restRequest)
        {
            // Try to extract GraphQL from REST body
            if (!string.IsNullOrWhiteSpace(restRequest.Body))
            {
                try
                {
                    var jsonDoc = System.Text.Json.JsonDocument.Parse(restRequest.Body);
                    if (jsonDoc.RootElement.TryGetProperty("query", out var queryElement))
                    {
                        target.Query = queryElement.GetString() ?? string.Empty;
                    }
                    if (jsonDoc.RootElement.TryGetProperty("variables", out var variablesElement))
                    {
                        target.Variables = variablesElement.GetRawText();
                    }
                    if (jsonDoc.RootElement.TryGetProperty("operationName", out var operationNameElement))
                    {
                        target.OperationName = operationNameElement.GetString();
                    }
                }
                catch
                {
                    // If not valid JSON or doesn't have GraphQL structure, leave query empty
                }
            }
        }
        else if (source is WebSocketRequest wsRequest)
        {
            // WebSocket might contain GraphQL subscription
            if (!string.IsNullOrWhiteSpace(wsRequest.Message))
            {
                try
                {
                    var jsonDoc = System.Text.Json.JsonDocument.Parse(wsRequest.Message);
                    if (jsonDoc.RootElement.TryGetProperty("query", out var queryElement))
                    {
                        target.Query = queryElement.GetString() ?? string.Empty;
                        target.OperationType = GraphQLOperationType.Subscription;
                    }
                    if (jsonDoc.RootElement.TryGetProperty("variables", out var variablesElement))
                    {
                        target.Variables = variablesElement.GetRawText();
                    }
                }
                catch
                {
                    // If not valid JSON, leave query empty
                }
            }
        }

        CopyBaseProperties(source, target);
        
        // Update URL scheme if needed
        target.Url = ConvertUrlScheme(source.Url, "http", "https");
        
        return target;
    }

    private static WebSocketRequest ConvertToWebSocket(Request source)
    {
        var target = new WebSocketRequest
        {
            Message = null,
            Protocols = new List<string>(),
            ConnectionType = WebSocketConnectionType.Standard
        };

        // Try to preserve some data from source type
        if (source is RestRequest restRequest)
        {
            // Copy body as message if present
            if (!string.IsNullOrWhiteSpace(restRequest.Body))
            {
                target.Message = restRequest.Body;
            }
        }
        else if (source is GraphQLRequest graphQLRequest)
        {
            // Convert GraphQL to WebSocket message (subscription format)
            if (!string.IsNullOrWhiteSpace(graphQLRequest.Query))
            {
                target.Message = System.Text.Json.JsonSerializer.Serialize(new
                {
                    type = "subscribe",
                    payload = new
                    {
                        query = graphQLRequest.Query,
                        variables = graphQLRequest.Variables,
                        operationName = graphQLRequest.OperationName
                    }
                }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                
                target.ConnectionType = WebSocketConnectionType.GraphQLSubscription;
                target.Protocols.Add("graphql-transport-ws");
            }
        }

        CopyBaseProperties(source, target);
        
        // Update URL scheme for WebSocket
        target.Url = ConvertUrlScheme(source.Url, "ws", "wss");
        
        return target;
    }

    private static void CopyBaseProperties(Request source, Request target)
    {
        target.Id = Guid.NewGuid(); // New ID for the converted request
        target.Name = $"{source.Name} ({target.Type})";
        target.Description = source.Description;
        target.Headers = new Dictionary<string, string>(source.Headers);
        target.DisabledHeaders = new HashSet<string>(source.DisabledHeaders);
        target.SecretHeaders = new HashSet<string>(source.SecretHeaders);
        target.CollectionId = source.CollectionId;
        target.Collection = source.Collection;
        target.EnvironmentId = source.EnvironmentId;
        target.Environment = source.Environment;
        target.CreatedAt = DateTime.UtcNow;
        target.AuthType = source.AuthType;
        target.BasicAuthUsername = source.BasicAuthUsername;
        target.BasicAuthPassword = source.BasicAuthPassword;
        target.BearerToken = source.BearerToken;
        target.ResponseExtractions = new List<ResponseExtraction>(source.ResponseExtractions);
    }

    private static string ConvertUrlScheme(string url, string httpScheme, string httpsScheme)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            return httpScheme + "://" + url.Substring(7);
        }
        else if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return httpsScheme + "://" + url.Substring(8);
        }
        else if (url.StartsWith("ws://", StringComparison.OrdinalIgnoreCase))
        {
            return httpScheme + "://" + url.Substring(5);
        }
        else if (url.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))
        {
            return httpsScheme + "://" + url.Substring(6);
        }

        return url;
    }
}
