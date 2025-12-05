using HolyConnect.Domain.Entities;

namespace HolyConnect.Domain.Tests.Entities;

public class CollectionTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange & Act
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test Collection",
            Description = "Test Description"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, collection.Id);
        Assert.Equal("Test Collection", collection.Name);
        Assert.Equal("Test Description", collection.Description);
        Assert.NotNull(collection.Requests);
        Assert.Empty(collection.Requests);
        Assert.NotNull(collection.SubCollections);
        Assert.Empty(collection.SubCollections);
    }

    [Fact]
    public void Requests_ShouldBeModifiable()
    {
        // Arrange
        var collection = new Collection { Name = "Test" };
        var request = new RestRequest { Name = "Test Request" };

        // Act
        collection.Requests.Add(request);

        // Assert
        Assert.Single(collection.Requests);
        Assert.Equal("Test Request", collection.Requests[0].Name);
    }

    [Fact]
    public void SubCollections_ShouldSupportHierarchy()
    {
        // Arrange
        var parentCollection = new Collection { Name = "Parent" };
        var childCollection = new Collection { Name = "Child" };

        // Act
        parentCollection.SubCollections.Add(childCollection);

        // Assert
        Assert.Single(parentCollection.SubCollections);
        Assert.Equal("Child", parentCollection.SubCollections[0].Name);
    }

    [Fact]
    public void ParentCollectionId_ShouldBeNullableForRootCollections()
    {
        // Arrange
        var rootCollection = new Collection { Name = "Root" };
        var childCollection = new Collection
        {
            Name = "Child",
            ParentCollectionId = Guid.NewGuid()
        };

        // Assert
        Assert.Null(rootCollection.ParentCollectionId);
        Assert.NotNull(childCollection.ParentCollectionId);
    }
}
