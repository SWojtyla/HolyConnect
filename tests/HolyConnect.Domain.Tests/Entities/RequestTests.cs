using HolyConnect.Domain.Entities;

namespace HolyConnect.Domain.Tests.Entities;

public class RequestTests
{
    [Fact]
    public void RestRequest_Type_ShouldBeRest()
    {
        // Arrange & Act
        var request = new RestRequest();

        // Assert
        Assert.Equal(RequestType.Rest, request.Type);
    }

    [Fact]
    public void GraphQLRequest_Type_ShouldBeGraphQL()
    {
        // Arrange & Act
        var request = new GraphQLRequest();

        // Assert
        Assert.Equal(RequestType.GraphQL, request.Type);
    }

    [Fact]
    public void Request_Id_ShouldBeSettable()
    {
        // Arrange
        var request = new RestRequest();
        var id = Guid.NewGuid();

        // Act
        request.Id = id;

        // Assert
        Assert.Equal(id, request.Id);
    }

    [Fact]
    public void Request_Name_ShouldBeSettable()
    {
        // Arrange
        var request = new RestRequest();
        var name = "Test Request";

        // Act
        request.Name = name;

        // Assert
        Assert.Equal(name, request.Name);
    }

    [Fact]
    public void Request_Description_ShouldBeNullable()
    {
        // Arrange
        var request = new RestRequest();

        // Act
        request.Description = null;

        // Assert
        Assert.Null(request.Description);
    }

    [Fact]
    public void Request_Description_ShouldBeSettable()
    {
        // Arrange
        var request = new RestRequest();
        var description = "Test Description";

        // Act
        request.Description = description;

        // Assert
        Assert.Equal(description, request.Description);
    }

    [Fact]
    public void Request_Url_ShouldBeSettable()
    {
        // Arrange
        var request = new RestRequest();
        var url = "https://api.example.com/test";

        // Act
        request.Url = url;

        // Assert
        Assert.Equal(url, request.Url);
    }

    [Fact]
    public void Request_Headers_ShouldBeInitializedAsEmptyDictionary()
    {
        // Arrange & Act
        var request = new RestRequest();

        // Assert
        Assert.NotNull(request.Headers);
        Assert.Empty(request.Headers);
    }

    [Fact]
    public void Request_Headers_ShouldBeModifiable()
    {
        // Arrange
        var request = new RestRequest();

        // Act
        request.Headers["Authorization"] = "Bearer token123";
        request.Headers["Content-Type"] = "application/json";

        // Assert
        Assert.Equal(2, request.Headers.Count);
        Assert.Equal("Bearer token123", request.Headers["Authorization"]);
        Assert.Equal("application/json", request.Headers["Content-Type"]);
    }

    [Fact]
    public void Request_CollectionId_ShouldBeSettable()
    {
        // Arrange
        var request = new RestRequest();
        var collectionId = Guid.NewGuid();

        // Act
        request.CollectionId = collectionId;

        // Assert
        Assert.Equal(collectionId, request.CollectionId);
    }

    [Fact]
    public void Request_Collection_ShouldBeSettable()
    {
        // Arrange
        var request = new RestRequest();
        var collection = new Collection { Id = Guid.NewGuid(), Name = "Test Collection" };

        // Act
        request.Collection = collection;

        // Assert
        Assert.NotNull(request.Collection);
        Assert.Equal(collection.Id, request.Collection.Id);
        Assert.Equal("Test Collection", request.Collection.Name);
    }

    [Fact]
    public void Request_CreatedAt_ShouldBeSettable()
    {
        // Arrange
        var request = new RestRequest();
        var createdAt = DateTime.UtcNow;

        // Act
        request.CreatedAt = createdAt;

        // Assert
        Assert.Equal(createdAt, request.CreatedAt);
    }



    [Fact]
    public void RequestType_Enum_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)RequestType.Rest);
        Assert.Equal(1, (int)RequestType.GraphQL);
        Assert.Equal(2, (int)RequestType.Soap);
        Assert.Equal(3, (int)RequestType.WebSocket);
    }

    [Fact]
    public void Request_CollectionId_ShouldBeNullableForEnvironmentLevelRequests()
    {
        // Arrange
        var request = new RestRequest();

        // Act
        request.CollectionId = null;

        // Assert
        Assert.Null(request.CollectionId);
    }



    [Fact]
    public void Request_DisabledHeaders_ShouldBeInitializedAsEmptyHashSet()
    {
        // Arrange & Act
        var request = new RestRequest();

        // Assert
        Assert.NotNull(request.DisabledHeaders);
        Assert.Empty(request.DisabledHeaders);
    }

    [Fact]
    public void Request_DisabledHeaders_ShouldBeModifiable()
    {
        // Arrange
        var request = new RestRequest();
        request.Headers["Content-Type"] = "application/json";
        request.Headers["Authorization"] = "Bearer token";

        // Act
        request.DisabledHeaders.Add("Authorization");

        // Assert
        Assert.Single(request.DisabledHeaders);
        Assert.Contains("Authorization", request.DisabledHeaders);
    }

    [Fact]
    public void Request_AuthType_ShouldDefaultToNone()
    {
        // Arrange & Act
        var request = new RestRequest();

        // Assert
        Assert.Equal(AuthenticationType.None, request.AuthType);
    }

    [Fact]
    public void Request_AuthType_ShouldBeSettable()
    {
        // Arrange
        var request = new RestRequest();

        // Act
        request.AuthType = AuthenticationType.Basic;

        // Assert
        Assert.Equal(AuthenticationType.Basic, request.AuthType);
    }

    [Theory]
    [InlineData(AuthenticationType.None)]
    [InlineData(AuthenticationType.Basic)]
    [InlineData(AuthenticationType.BearerToken)]
    public void Request_AuthType_ShouldSupportAllAuthenticationTypes(AuthenticationType authType)
    {
        // Arrange
        var request = new RestRequest { AuthType = authType };

        // Assert
        Assert.Equal(authType, request.AuthType);
    }

    [Fact]
    public void Request_BasicAuthUsername_ShouldBeNullable()
    {
        // Arrange
        var request = new RestRequest();

        // Act
        request.BasicAuthUsername = null;

        // Assert
        Assert.Null(request.BasicAuthUsername);
    }

    [Fact]
    public void Request_BasicAuthUsername_ShouldBeSettable()
    {
        // Arrange
        var request = new RestRequest();
        var username = "testuser";

        // Act
        request.BasicAuthUsername = username;

        // Assert
        Assert.Equal(username, request.BasicAuthUsername);
    }

    [Fact]
    public void Request_BasicAuthPassword_ShouldBeNullable()
    {
        // Arrange
        var request = new RestRequest();

        // Act
        request.BasicAuthPassword = null;

        // Assert
        Assert.Null(request.BasicAuthPassword);
    }

    [Fact]
    public void Request_BasicAuthPassword_ShouldBeSettable()
    {
        // Arrange
        var request = new RestRequest();
        var password = "testpassword";

        // Act
        request.BasicAuthPassword = password;

        // Assert
        Assert.Equal(password, request.BasicAuthPassword);
    }

    [Fact]
    public void Request_BearerToken_ShouldBeNullable()
    {
        // Arrange
        var request = new RestRequest();

        // Act
        request.BearerToken = null;

        // Assert
        Assert.Null(request.BearerToken);
    }

    [Fact]
    public void Request_BearerToken_ShouldBeSettable()
    {
        // Arrange
        var request = new RestRequest();
        var token = "abc123token";

        // Act
        request.BearerToken = token;

        // Assert
        Assert.Equal(token, request.BearerToken);
    }

    [Fact]
    public void Request_WithBasicAuth_ShouldHaveUsernameAndPassword()
    {
        // Arrange & Act
        var request = new RestRequest
        {
            AuthType = AuthenticationType.Basic,
            BasicAuthUsername = "admin",
            BasicAuthPassword = "secret"
        };

        // Assert
        Assert.Equal(AuthenticationType.Basic, request.AuthType);
        Assert.Equal("admin", request.BasicAuthUsername);
        Assert.Equal("secret", request.BasicAuthPassword);
    }

    [Fact]
    public void Request_WithBearerToken_ShouldHaveToken()
    {
        // Arrange & Act
        var request = new RestRequest
        {
            AuthType = AuthenticationType.BearerToken,
            BearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9"
        };

        // Assert
        Assert.Equal(AuthenticationType.BearerToken, request.AuthType);
        Assert.Equal("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9", request.BearerToken);
    }

    [Fact]
    public void AuthenticationType_Enum_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)AuthenticationType.None);
        Assert.Equal(1, (int)AuthenticationType.Basic);
        Assert.Equal(2, (int)AuthenticationType.BearerToken);
    }

    [Fact]
    public void Request_Order_ShouldBeSettable()
    {
        // Arrange & Act
        var request = new RestRequest
        {
            Name = "Test",
            Order = 5
        };

        // Assert
        Assert.Equal(5, request.Order);
    }

    [Fact]
    public void Request_Order_ShouldDefaultToZero()
    {
        // Arrange & Act
        var request = new RestRequest { Name = "Test" };

        // Assert
        Assert.Equal(0, request.Order);
    }
}
