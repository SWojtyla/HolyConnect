using HolyConnect.Application.Common;
using HolyConnect.Domain.Entities;
using Xunit;

namespace HolyConnect.Application.Tests.Common;

public class RequestConverterTests
{
    [Fact]
    public void ConvertTo_SameType_ShouldReturnClone()
    {
        // Arrange
        var restRequest = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test Request",
            Url = "https://api.example.com",
            Method = Domain.Entities.HttpMethod.Post,
            Body = "{\"test\": true}"
        };

        // Act
        var result = RequestConverter.ConvertTo(restRequest, RequestType.Rest);

        // Assert
        Assert.IsType<RestRequest>(result);
        // When converting to same type, it should return a clone with same ID (RequestCloner behavior)
        Assert.Equal(restRequest.Id, result.Id);
        Assert.Equal(restRequest.Name, result.Name);
        Assert.Equal(restRequest.Url, result.Url);
    }

    [Fact]
    public void ConvertTo_RestToGraphQL_ShouldConvertCorrectly()
    {
        // Arrange
        var restRequest = new RestRequest
        {
            Name = "REST Request",
            Url = "https://api.example.com/graphql",
            Method = Domain.Entities.HttpMethod.Post,
            Body = "{\"query\":\"{ users { id name } }\",\"variables\":{\"limit\":10}}",
            Headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } }
        };

        // Act
        var result = RequestConverter.ConvertTo(restRequest, RequestType.GraphQL);

        // Assert
        Assert.IsType<GraphQLRequest>(result);
        var graphQLRequest = (GraphQLRequest)result;
        Assert.Equal("REST Request (GraphQL)", graphQLRequest.Name);
        Assert.Equal("https://api.example.com/graphql", graphQLRequest.Url);
        Assert.Equal("{ users { id name } }", graphQLRequest.Query);
        Assert.Contains("limit", graphQLRequest.Variables ?? "");
        Assert.Equal("Bearer token", graphQLRequest.Headers["Authorization"]);
    }

    [Fact]
    public void ConvertTo_RestToWebSocket_ShouldConvertCorrectly()
    {
        // Arrange
        var restRequest = new RestRequest
        {
            Name = "REST Request",
            Url = "https://api.example.com",
            Method = Domain.Entities.HttpMethod.Post,
            Body = "{\"action\":\"subscribe\"}",
            Headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } }
        };

        // Act
        var result = RequestConverter.ConvertTo(restRequest, RequestType.WebSocket);

        // Assert
        Assert.IsType<WebSocketRequest>(result);
        var wsRequest = (WebSocketRequest)result;
        Assert.Equal("REST Request (WebSocket)", wsRequest.Name);
        Assert.Equal("wss://api.example.com", wsRequest.Url); // Should convert to wss
        Assert.Equal("{\"action\":\"subscribe\"}", wsRequest.Message);
        Assert.Equal("Bearer token", wsRequest.Headers["Authorization"]);
    }

    [Fact]
    public void ConvertTo_GraphQLToRest_ShouldConvertCorrectly()
    {
        // Arrange
        var graphQLRequest = new GraphQLRequest
        {
            Name = "GraphQL Request",
            Url = "https://api.example.com/graphql",
            Query = "query GetUsers($limit: Int) { users(limit: $limit) { id name } }",
            Variables = "{\"limit\":10}",
            OperationName = "GetUsers",
            Headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } }
        };

        // Act
        var result = RequestConverter.ConvertTo(graphQLRequest, RequestType.Rest);

        // Assert
        Assert.IsType<RestRequest>(result);
        var restRequest = (RestRequest)result;
        Assert.Equal("GraphQL Request (Rest)", restRequest.Name);
        Assert.Equal("https://api.example.com/graphql", restRequest.Url);
        Assert.Equal(Domain.Entities.HttpMethod.Post, restRequest.Method);
        Assert.Contains("query", restRequest.Body);
        Assert.Contains("variables", restRequest.Body);
        Assert.Contains("GetUsers", restRequest.Body);
        Assert.Equal("Bearer token", restRequest.Headers["Authorization"]);
    }

    [Fact]
    public void ConvertTo_GraphQLToWebSocket_ShouldConvertCorrectly()
    {
        // Arrange
        var graphQLRequest = new GraphQLRequest
        {
            Name = "GraphQL Subscription",
            Url = "https://api.example.com/graphql",
            Query = "subscription OnNewMessage { messageAdded { id text } }",
            Variables = "{\"roomId\":\"123\"}",
            Headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } }
        };

        // Act
        var result = RequestConverter.ConvertTo(graphQLRequest, RequestType.WebSocket);

        // Assert
        Assert.IsType<WebSocketRequest>(result);
        var wsRequest = (WebSocketRequest)result;
        Assert.Equal("GraphQL Subscription (WebSocket)", wsRequest.Name);
        Assert.Equal("wss://api.example.com/graphql", wsRequest.Url); // Should convert to wss
        Assert.Contains("subscribe", wsRequest.Message ?? "");
        Assert.Contains("messageAdded", wsRequest.Message ?? "");
        Assert.Equal(WebSocketConnectionType.GraphQLSubscription, wsRequest.ConnectionType);
        Assert.Contains("graphql-transport-ws", wsRequest.Protocols);
        Assert.Equal("Bearer token", wsRequest.Headers["Authorization"]);
    }

    [Fact]
    public void ConvertTo_WebSocketToRest_ShouldConvertCorrectly()
    {
        // Arrange
        var wsRequest = new WebSocketRequest
        {
            Name = "WebSocket Request",
            Url = "wss://api.example.com/ws",
            Message = "{\"action\":\"getData\",\"id\":123}",
            Headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } }
        };

        // Act
        var result = RequestConverter.ConvertTo(wsRequest, RequestType.Rest);

        // Assert
        Assert.IsType<RestRequest>(result);
        var restRequest = (RestRequest)result;
        Assert.Equal("WebSocket Request (Rest)", restRequest.Name);
        Assert.Equal("https://api.example.com/ws", restRequest.Url); // Should convert to https
        Assert.Equal(Domain.Entities.HttpMethod.Post, restRequest.Method);
        Assert.Equal("{\"action\":\"getData\",\"id\":123}", restRequest.Body);
        Assert.Equal(BodyType.Json, restRequest.BodyType);
        Assert.Equal("Bearer token", restRequest.Headers["Authorization"]);
    }

    [Fact]
    public void ConvertTo_WebSocketToGraphQL_WithGraphQLMessage_ShouldConvertCorrectly()
    {
        // Arrange
        var wsRequest = new WebSocketRequest
        {
            Name = "WebSocket Request",
            Url = "wss://api.example.com/graphql",
            Message = "{\"query\":\"subscription { onUpdate { id } }\",\"variables\":{\"id\":1}}",
            Headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } }
        };

        // Act
        var result = RequestConverter.ConvertTo(wsRequest, RequestType.GraphQL);

        // Assert
        Assert.IsType<GraphQLRequest>(result);
        var graphQLRequest = (GraphQLRequest)result;
        Assert.Equal("WebSocket Request (GraphQL)", graphQLRequest.Name);
        Assert.Equal("https://api.example.com/graphql", graphQLRequest.Url); // Should convert to https
        Assert.Equal("subscription { onUpdate { id } }", graphQLRequest.Query);
        Assert.Contains("id", graphQLRequest.Variables ?? "");
        Assert.Equal("Bearer token", graphQLRequest.Headers["Authorization"]);
    }

    [Fact]
    public void ConvertTo_ShouldPreserveAuthenticationSettings()
    {
        // Arrange
        var restRequest = new RestRequest
        {
            Name = "Authenticated Request",
            Url = "https://api.example.com",
            AuthType = AuthenticationType.Basic,
            BasicAuthUsername = "user",
            BasicAuthPassword = "pass"
        };

        // Act
        var result = RequestConverter.ConvertTo(restRequest, RequestType.GraphQL);

        // Assert
        Assert.Equal(AuthenticationType.Basic, result.AuthType);
        Assert.Equal("user", result.BasicAuthUsername);
        Assert.Equal("pass", result.BasicAuthPassword);
    }

    [Fact]
    public void ConvertTo_ShouldPreserveBearerToken()
    {
        // Arrange
        var restRequest = new RestRequest
        {
            Name = "Bearer Token Request",
            Url = "https://api.example.com",
            AuthType = AuthenticationType.BearerToken,
            BearerToken = "my-secret-token"
        };

        // Act
        var result = RequestConverter.ConvertTo(restRequest, RequestType.WebSocket);

        // Assert
        Assert.Equal(AuthenticationType.BearerToken, result.AuthType);
        Assert.Equal("my-secret-token", result.BearerToken);
    }

    [Fact]
    public void ConvertTo_ShouldPreserveResponseExtractions()
    {
        // Arrange
        var restRequest = new RestRequest
        {
            Name = "Request with Extractions",
            Url = "https://api.example.com",
            ResponseExtractions = new List<ResponseExtraction>
            {
                new ResponseExtraction
                {
                    VariableName = "userId",
                    Pattern = "$.data.user.id",
                    SaveToCollection = false
                }
            }
        };

        // Act
        var result = RequestConverter.ConvertTo(restRequest, RequestType.GraphQL);

        // Assert
        Assert.Single(result.ResponseExtractions);
        Assert.Equal("userId", result.ResponseExtractions[0].VariableName);
        Assert.Equal("$.data.user.id", result.ResponseExtractions[0].Pattern);
        Assert.False(result.ResponseExtractions[0].SaveToCollection);
    }

    [Fact]
    public void ConvertTo_ShouldHandleHttpToWsUrlConversion()
    {
        // Arrange
        var restRequest = new RestRequest
        {
            Name = "HTTP Request",
            Url = "http://api.example.com/ws"
        };

        // Act
        var result = RequestConverter.ConvertTo(restRequest, RequestType.WebSocket);

        // Assert
        Assert.IsType<WebSocketRequest>(result);
        Assert.Equal("ws://api.example.com/ws", result.Url);
    }

    [Fact]
    public void ConvertTo_ShouldHandleWsToHttpUrlConversion()
    {
        // Arrange
        var wsRequest = new WebSocketRequest
        {
            Name = "WS Request",
            Url = "ws://api.example.com/endpoint"
        };

        // Act
        var result = RequestConverter.ConvertTo(wsRequest, RequestType.Rest);

        // Assert
        Assert.IsType<RestRequest>(result);
        Assert.Equal("http://api.example.com/endpoint", result.Url);
    }

    [Fact]
    public void ConvertTo_ShouldGenerateNewId()
    {
        // Arrange
        var originalId = Guid.NewGuid();
        var restRequest = new RestRequest
        {
            Id = originalId,
            Name = "Test Request",
            Url = "https://api.example.com"
        };

        // Act
        var result = RequestConverter.ConvertTo(restRequest, RequestType.GraphQL);

        // Assert
        Assert.NotEqual(originalId, result.Id);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public void ConvertTo_ShouldAppendTargetTypeToName()
    {
        // Arrange
        var restRequest = new RestRequest
        {
            Name = "My Request",
            Url = "https://api.example.com"
        };

        // Act
        var result = RequestConverter.ConvertTo(restRequest, RequestType.GraphQL);

        // Assert
        Assert.Equal("My Request (GraphQL)", result.Name);
    }

    [Fact]
    public void ConvertTo_RestWithoutBody_ShouldCreateGetRequest()
    {
        // Arrange
        var restRequest = new RestRequest
        {
            Name = "Empty Request",
            Url = "https://api.example.com",
            Method = Domain.Entities.HttpMethod.Get,
            Body = string.Empty
        };

        // Act
        var resultWs = RequestConverter.ConvertTo(restRequest, RequestType.WebSocket);
        var resultGql = RequestConverter.ConvertTo(restRequest, RequestType.GraphQL);

        // Assert
        Assert.IsType<WebSocketRequest>(resultWs);
        Assert.Null(((WebSocketRequest)resultWs).Message);
        
        Assert.IsType<GraphQLRequest>(resultGql);
        Assert.Empty(((GraphQLRequest)resultGql).Query);
    }
}
