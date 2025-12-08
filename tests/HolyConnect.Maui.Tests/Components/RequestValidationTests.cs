using HolyConnect.Domain.Entities;

namespace HolyConnect.Maui.Tests.Components;

/// <summary>
/// Tests for request validation logic used in UI components
/// </summary>
public class RequestValidationTests
{
    [Fact]
    public void RestRequest_IsValid_WhenUrlAndMethodProvided()
    {
        // Arrange
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test Request",
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get
        };

        // Act & Assert
        Assert.NotNull(request.Url);
        Assert.NotEqual(string.Empty, request.Url);
        Assert.Equal(Domain.Entities.HttpMethod.Get, request.Method);
    }

    [Fact]
    public void GraphQLRequest_IsValid_WhenUrlAndQueryProvided()
    {
        // Arrange
        var request = new GraphQLRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test GraphQL",
            Url = "https://api.example.com/graphql",
            Query = "query { test }"
        };

        // Act & Assert
        Assert.NotNull(request.Url);
        Assert.NotNull(request.Query);
        Assert.NotEqual(string.Empty, request.Query);
    }

    [Fact]
    public void WebSocketRequest_IsValid_WhenUrlProvided()
    {
        // Arrange
        var request = new WebSocketRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test WebSocket",
            Url = "wss://api.example.com/ws"
        };

        // Act & Assert
        Assert.NotNull(request.Url);
        Assert.StartsWith("wss://", request.Url);
    }

    [Theory]
    [InlineData("https://api.example.com")]
    [InlineData("http://localhost:8080")]
    [InlineData("https://api.example.com/path/to/endpoint")]
    public void RestRequest_AcceptsValidHttpUrls(string url)
    {
        // Arrange & Act
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Url = url,
            Method = Domain.Entities.HttpMethod.Get
        };

        // Assert
        Assert.Equal(url, request.Url);
    }

    [Theory]
    [InlineData("wss://api.example.com/ws")]
    [InlineData("ws://localhost:8080/socket")]
    public void WebSocketRequest_AcceptsValidWebSocketUrls(string url)
    {
        // Arrange & Act
        var request = new WebSocketRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Url = url
        };

        // Assert
        Assert.Equal(url, request.Url);
    }

    [Fact]
    public void Request_SupportsAuthenticationTypes()
    {
        // Arrange
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Url = "https://api.example.com",
            Method = Domain.Entities.HttpMethod.Get
        };

        // Act
        request.AuthType = AuthenticationType.BearerToken;
        request.BearerToken = "test-token-123";

        // Assert
        Assert.Equal(AuthenticationType.BearerToken, request.AuthType);
        Assert.Equal("test-token-123", request.BearerToken);
    }

    [Fact]
    public void Request_SupportsBasicAuthentication()
    {
        // Arrange
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Url = "https://api.example.com",
            Method = Domain.Entities.HttpMethod.Get
        };

        // Act
        request.AuthType = AuthenticationType.Basic;
        request.BasicAuthUsername = "user";
        request.BasicAuthPassword = "pass";

        // Assert
        Assert.Equal(AuthenticationType.Basic, request.AuthType);
        Assert.Equal("user", request.BasicAuthUsername);
        Assert.Equal("pass", request.BasicAuthPassword);
    }

    [Fact]
    public void Request_SupportsQueryParameters()
    {
        // Arrange
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Url = "https://api.example.com",
            Method = Domain.Entities.HttpMethod.Get
        };

        // Act
        request.QueryParameters["page"] = "1";
        request.QueryParameters["limit"] = "10";

        // Assert
        Assert.Equal(2, request.QueryParameters.Count);
        Assert.Equal("1", request.QueryParameters["page"]);
        Assert.Equal("10", request.QueryParameters["limit"]);
    }

    [Fact]
    public void GraphQLRequest_SupportsVariables()
    {
        // Arrange
        var request = new GraphQLRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Url = "https://api.example.com/graphql",
            Query = "query($id: ID!) { user(id: $id) { name } }"
        };

        // Act
        request.Variables = "{\"id\": \"123\"}";

        // Assert
        Assert.NotNull(request.Variables);
        Assert.Contains("123", request.Variables);
    }

    [Theory]
    [InlineData(Domain.Entities.HttpMethod.Get)]
    [InlineData(Domain.Entities.HttpMethod.Post)]
    [InlineData(Domain.Entities.HttpMethod.Put)]
    [InlineData(Domain.Entities.HttpMethod.Delete)]
    [InlineData(Domain.Entities.HttpMethod.Patch)]
    [InlineData(Domain.Entities.HttpMethod.Head)]
    [InlineData(Domain.Entities.HttpMethod.Options)]
    public void RestRequest_SupportsAllHttpMethods(Domain.Entities.HttpMethod method)
    {
        // Arrange & Act
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Url = "https://api.example.com",
            Method = method
        };

        // Assert
        Assert.Equal(method, request.Method);
    }

    [Theory]
    [InlineData(BodyType.None)]
    [InlineData(BodyType.Json)]
    [InlineData(BodyType.Xml)]
    [InlineData(BodyType.Text)]
    [InlineData(BodyType.Html)]
    [InlineData(BodyType.JavaScript)]
    public void RestRequest_SupportsAllBodyTypes(BodyType bodyType)
    {
        // Arrange & Act
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Url = "https://api.example.com",
            Method = Domain.Entities.HttpMethod.Post,
            BodyType = bodyType
        };

        // Assert
        Assert.Equal(bodyType, request.BodyType);
    }

    [Fact]
    public void Request_TracksCreationDate()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        
        // Act
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Url = "https://api.example.com",
            Method = Domain.Entities.HttpMethod.Get,
            CreatedAt = DateTime.UtcNow
        };

        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.True(request.CreatedAt >= beforeCreate);
        Assert.True(request.CreatedAt <= afterCreate);
    }

    [Fact]
    public void Request_CanBeAssignedToCollection()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Url = "https://api.example.com",
            Method = Domain.Entities.HttpMethod.Get
        };

        // Act
        request.CollectionId = collectionId;

        // Assert
        Assert.Equal(collectionId, request.CollectionId);
    }

    [Fact]
    public void Request_SupportsResponseExtraction()
    {
        // Arrange
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Url = "https://api.example.com",
            Method = Domain.Entities.HttpMethod.Get
        };

        // Act
        request.ResponseExtractions.Add(new ResponseExtraction
        {
            Id = Guid.NewGuid(),
            Name = "userId",
            Pattern = "$.data.user.id",
            VariableName = "user_id",
            SaveToCollection = true
        });

        // Assert
        Assert.Single(request.ResponseExtractions);
        Assert.Equal("userId", request.ResponseExtractions[0].Name);
        Assert.Equal("user_id", request.ResponseExtractions[0].VariableName);
        Assert.True(request.ResponseExtractions[0].SaveToCollection);
    }
}
