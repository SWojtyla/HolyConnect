using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Common;

/// <summary>
/// Helper class for cloning Request objects to avoid modifying originals during variable resolution
/// </summary>
public static class RequestCloner
{
    /// <summary>
    /// Creates a deep clone of a request, supporting all request types
    /// </summary>
    public static Request Clone(Request source)
    {
        return source switch
        {
            RestRequest restRequest => CloneRestRequest(restRequest),
            GraphQLRequest graphQLRequest => CloneGraphQLRequest(graphQLRequest),
            WebSocketRequest webSocketRequest => CloneWebSocketRequest(webSocketRequest),
            _ => throw new NotSupportedException($"Request type {source.Type} is not supported for cloning")
        };
    }

    /// <summary>
    /// Clones common request properties into the target request
    /// </summary>
    private static void CloneBaseProperties(Request source, Request target)
    {
        target.Id = source.Id;
        target.Name = source.Name;
        target.Description = source.Description;
        target.Url = source.Url;
        target.Headers = new Dictionary<string, string>(source.Headers);
        target.DisabledHeaders = new HashSet<string>(source.DisabledHeaders);
        target.CollectionId = source.CollectionId;
        target.Collection = source.Collection;
        target.EnvironmentId = source.EnvironmentId;
        target.Environment = source.Environment;
        target.CreatedAt = source.CreatedAt;
        target.UpdatedAt = source.UpdatedAt;
        target.AuthType = source.AuthType;
        target.BasicAuthUsername = source.BasicAuthUsername;
        target.BasicAuthPassword = source.BasicAuthPassword;
        target.BearerToken = source.BearerToken;
        target.ResponseExtractions = new List<ResponseExtraction>(source.ResponseExtractions);
    }

    private static RestRequest CloneRestRequest(RestRequest source)
    {
        var clone = new RestRequest
        {
            Method = source.Method,
            Body = source.Body,
            ContentType = source.ContentType,
            BodyType = source.BodyType,
            QueryParameters = new Dictionary<string, string>(source.QueryParameters),
            DisabledQueryParameters = new HashSet<string>(source.DisabledQueryParameters)
        };

        CloneBaseProperties(source, clone);
        return clone;
    }

    private static GraphQLRequest CloneGraphQLRequest(GraphQLRequest source)
    {
        var clone = new GraphQLRequest
        {
            Query = source.Query,
            Variables = source.Variables,
            OperationName = source.OperationName,
            OperationType = source.OperationType,
            SubscriptionProtocol = source.SubscriptionProtocol
        };

        CloneBaseProperties(source, clone);
        return clone;
    }

    private static WebSocketRequest CloneWebSocketRequest(WebSocketRequest source)
    {
        var clone = new WebSocketRequest
        {
            Message = source.Message,
            Protocols = new List<string>(source.Protocols),
            ConnectionType = source.ConnectionType
        };

        CloneBaseProperties(source, clone);
        return clone;
    }
}
