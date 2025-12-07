using System.Text.Json;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Persistence;

namespace HolyConnect.Infrastructure.Tests.Persistence;

public class RequestJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public RequestJsonConverterTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new RequestJsonConverter());
    }

    [Fact]
    public void Write_RestRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test Request",
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get
        };

        // Act
        var json = JsonSerializer.Serialize<Request>(request, _options);

        // Assert
        Assert.Contains("Test Request", json);
        Assert.Contains("https://api.example.com/test", json);
    }

    [Fact]
    public void Write_GraphQLRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = new GraphQLRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test GraphQL",
            Url = "https://api.example.com/graphql",
            Query = "query { user { name } }"
        };

        // Act
        var json = JsonSerializer.Serialize<Request>(request, _options);

        // Assert
        Assert.Contains("Test GraphQL", json);
        Assert.Contains("https://api.example.com/graphql", json);
        Assert.Contains("query { user { name } }", json);
    }

    [Fact]
    public void Read_RestRequest_WithStringType_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = @"{
            ""Id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""Name"": ""Test Request"",
            ""Url"": ""https://api.example.com/test"",
            ""Type"": ""Rest"",
            ""Method"": 0,
            ""Headers"": {},
            ""CollectionId"": ""00000000-0000-0000-0000-000000000000"",
            ""CreatedAt"": ""2024-01-01T00:00:00Z""
        }";

        // Act
        var request = JsonSerializer.Deserialize<Request>(json, _options);

        // Assert
        Assert.NotNull(request);
        Assert.IsType<RestRequest>(request);
        Assert.Equal("Test Request", request.Name);
        Assert.Equal("https://api.example.com/test", request.Url);
    }

    [Fact]
    public void Read_GraphQLRequest_WithStringType_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = @"{
            ""Id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""Name"": ""Test GraphQL"",
            ""Url"": ""https://api.example.com/graphql"",
            ""Type"": ""GraphQL"",
            ""Query"": ""query { user { name } }"",
            ""Headers"": {},
            ""CollectionId"": ""00000000-0000-0000-0000-000000000000"",
            ""CreatedAt"": ""2024-01-01T00:00:00Z"",
        }";

        // Act
        var request = JsonSerializer.Deserialize<Request>(json, _options);

        // Assert
        Assert.NotNull(request);
        Assert.IsType<GraphQLRequest>(request);
        Assert.Equal("Test GraphQL", request.Name);
        var gqlRequest = (GraphQLRequest)request;
        Assert.Equal("query { user { name } }", gqlRequest.Query);
    }

    [Fact]
    public void Read_RestRequest_WithNumericType_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = @"{
            ""Id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""Name"": ""Test Request"",
            ""Url"": ""https://api.example.com/test"",
            ""Type"": 0,
            ""Method"": 0,
            ""Headers"": {},
            ""CollectionId"": ""00000000-0000-0000-0000-000000000000"",
            ""CreatedAt"": ""2024-01-01T00:00:00Z"",
        }";

        // Act
        var request = JsonSerializer.Deserialize<Request>(json, _options);

        // Assert
        Assert.NotNull(request);
        Assert.IsType<RestRequest>(request);
        Assert.Equal(RequestType.Rest, request.Type);
    }

    [Fact]
    public void Read_GraphQLRequest_WithNumericType_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = @"{
            ""Id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""Name"": ""Test GraphQL"",
            ""Url"": ""https://api.example.com/graphql"",
            ""Type"": 1,
            ""Query"": ""query { user { name } }"",
            ""Headers"": {},
            ""CollectionId"": ""00000000-0000-0000-0000-000000000000"",
            ""CreatedAt"": ""2024-01-01T00:00:00Z"",
        }";

        // Act
        var request = JsonSerializer.Deserialize<Request>(json, _options);

        // Assert
        Assert.NotNull(request);
        Assert.IsType<GraphQLRequest>(request);
        Assert.Equal(RequestType.GraphQL, request.Type);
    }

    [Fact]
    public void Read_WithMissingType_ShouldThrowJsonException()
    {
        // Arrange
        var json = @"{
            ""Id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""Name"": ""Test Request"",
            ""Url"": ""https://api.example.com/test""
        }";

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Request>(json, _options));
        Assert.Contains("Missing Type discriminator", exception.Message);
    }

    [Fact]
    public void Read_WithInvalidStringType_ShouldThrowJsonException()
    {
        // Arrange
        var json = @"{
            ""Id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""Name"": ""Test Request"",
            ""Url"": ""https://api.example.com/test"",
            ""Type"": ""InvalidType""
        }";

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Request>(json, _options));
        Assert.Contains("Unknown Request type", exception.Message);
    }

    [Fact]
    public void Read_WithInvalidNumericType_ShouldThrowJsonException()
    {
        // Arrange
        var json = @"{
            ""Id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""Name"": ""Test Request"",
            ""Url"": ""https://api.example.com/test"",
            ""Type"": 999
        }";

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Request>(json, _options));
        Assert.Contains("Invalid numeric Request type", exception.Message);
    }

    [Fact]
    public void Read_WithUnsupportedRequestType_ShouldThrowJsonException()
    {
        // Arrange - Using Soap type which is not currently supported
        var json = @"{
            ""Id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""Name"": ""Test Request"",
            ""Url"": ""https://api.example.com/test"",
            ""Type"": ""Soap"",
            ""Headers"": {},
            ""CollectionId"": ""00000000-0000-0000-0000-000000000000"",
            ""CreatedAt"": ""2024-01-01T00:00:00Z"",
        }";

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Request>(json, _options));
        Assert.Contains("Unsupported Request type", exception.Message);
    }

    [Fact]
    public void RoundTrip_RestRequest_ShouldPreserveData()
    {
        // Arrange
        var originalRequest = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test Request",
            Description = "Test Description",
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Post,
            Body = "{\"test\": true}",
            CollectionId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        originalRequest.Headers["Authorization"] = "Bearer token";

        // Act
        var json = JsonSerializer.Serialize<Request>(originalRequest, _options);
        var deserializedRequest = JsonSerializer.Deserialize<Request>(json, _options);

        // Assert
        Assert.NotNull(deserializedRequest);
        var restRequest = Assert.IsType<RestRequest>(deserializedRequest);
        Assert.Equal(originalRequest.Id, restRequest.Id);
        Assert.Equal(originalRequest.Name, restRequest.Name);
        Assert.Equal(originalRequest.Description, restRequest.Description);
        Assert.Equal(originalRequest.Url, restRequest.Url);
        Assert.Equal(originalRequest.Method, restRequest.Method);
        Assert.Equal(originalRequest.Body, restRequest.Body);
        Assert.Equal(originalRequest.CollectionId, restRequest.CollectionId);
    }

    [Fact]
    public void RoundTrip_GraphQLRequest_ShouldPreserveData()
    {
        // Arrange
        var originalRequest = new GraphQLRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test GraphQL",
            Description = "GraphQL Description",
            Url = "https://api.example.com/graphql",
            Query = "query { user { name } }",
            Variables = "{\"id\": \"123\"}",
            OperationName = "GetUser",
            CollectionId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        originalRequest.Headers["X-API-Key"] = "secret";

        // Act
        var json = JsonSerializer.Serialize<Request>(originalRequest, _options);
        var deserializedRequest = JsonSerializer.Deserialize<Request>(json, _options);

        // Assert
        Assert.NotNull(deserializedRequest);
        var gqlRequest = Assert.IsType<GraphQLRequest>(deserializedRequest);
        Assert.Equal(originalRequest.Id, gqlRequest.Id);
        Assert.Equal(originalRequest.Name, gqlRequest.Name);
        Assert.Equal(originalRequest.Description, gqlRequest.Description);
        Assert.Equal(originalRequest.Url, gqlRequest.Url);
        Assert.Equal(originalRequest.Query, gqlRequest.Query);
        Assert.Equal(originalRequest.Variables, gqlRequest.Variables);
        Assert.Equal(originalRequest.OperationName, gqlRequest.OperationName);
        Assert.Equal(originalRequest.CollectionId, gqlRequest.CollectionId);
    }

    [Fact]
    public void Read_WithBooleanType_ShouldThrowJsonException()
    {
        // Arrange
        var json = @"{
            ""Id"": ""3fa85f64-5717-4562-b3fc-2c963f66afa6"",
            ""Name"": ""Test Request"",
            ""Type"": true
        }";

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Request>(json, _options));
        Assert.Contains("Unsupported Type value kind", exception.Message);
    }

    [Fact]
    public void Write_RestRequestWithSecretHeaders_ShouldReplaceSecretValues()
    {
        // Arrange
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test Request",
            Url = "https://api.example.com/test"
        };
        request.Headers["Authorization"] = "Bearer secret-token";
        request.Headers["X-API-Key"] = "public-key";
        request.SecretHeaders.Add("Authorization");

        // Act
        var json = JsonSerializer.Serialize<Request>(request, _options);

        // Assert
        Assert.Contains("***SECRET***", json);
        Assert.DoesNotContain("secret-token", json);
        Assert.Contains("public-key", json); // Non-secret header should remain
        
        // Verify original request object is not modified
        Assert.Equal("Bearer secret-token", request.Headers["Authorization"]);
        Assert.Equal("public-key", request.Headers["X-API-Key"]);
    }

    [Fact]
    public void Write_GraphQLRequestWithSecretHeaders_ShouldReplaceSecretValues()
    {
        // Arrange
        var request = new GraphQLRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test GraphQL",
            Url = "https://api.example.com/graphql",
            Query = "query { user { name } }"
        };
        request.Headers["X-Secret-Token"] = "my-secret-value";
        request.Headers["X-Public-Header"] = "public-value";
        request.SecretHeaders.Add("X-Secret-Token");

        // Act
        var json = JsonSerializer.Serialize<Request>(request, _options);

        // Assert
        Assert.Contains("***SECRET***", json);
        Assert.DoesNotContain("my-secret-value", json);
        Assert.Contains("public-value", json);
        
        // Verify original request object is not modified
        Assert.Equal("my-secret-value", request.Headers["X-Secret-Token"]);
    }

    [Fact]
    public void Write_WebSocketRequestWithSecretHeaders_ShouldReplaceSecretValues()
    {
        // Arrange
        var request = new WebSocketRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test WebSocket",
            Url = "wss://api.example.com/ws"
        };
        request.Headers["Authorization"] = "Bearer ws-secret";
        request.SecretHeaders.Add("Authorization");

        // Act
        var json = JsonSerializer.Serialize<Request>(request, _options);

        // Assert
        Assert.Contains("***SECRET***", json);
        Assert.DoesNotContain("ws-secret", json);
        Assert.Equal("Bearer ws-secret", request.Headers["Authorization"]);
    }

    [Fact]
    public void RoundTrip_RequestWithSecretHeaders_ShouldPreserveSecretHeadersList()
    {
        // Arrange
        var originalRequest = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test Request",
            Url = "https://api.example.com/test"
        };
        originalRequest.Headers["Authorization"] = "Bearer token";
        originalRequest.SecretHeaders.Add("Authorization");

        // Act - Serialize with secret placeholder
        var json = JsonSerializer.Serialize<Request>(originalRequest, _options);
        var deserializedRequest = JsonSerializer.Deserialize<Request>(json, _options);

        // Assert
        Assert.NotNull(deserializedRequest);
        var restRequest = Assert.IsType<RestRequest>(deserializedRequest);
        Assert.Contains("Authorization", restRequest.SecretHeaders);
        // The value should be the placeholder since we serialized it
        Assert.Equal("***SECRET***", restRequest.Headers["Authorization"]);
    }
}
