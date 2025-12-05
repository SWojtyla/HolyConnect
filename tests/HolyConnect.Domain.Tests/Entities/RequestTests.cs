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
    public void Request_UpdatedAt_ShouldBeSettable()
    {
        // Arrange
        var request = new RestRequest();
        var updatedAt = DateTime.UtcNow;

        // Act
        request.UpdatedAt = updatedAt;

        // Assert
        Assert.Equal(updatedAt, request.UpdatedAt);
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
}
