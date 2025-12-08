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
        Assert.NotNull(collection.Variables);
        Assert.Empty(collection.Variables);
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

    [Fact]
    public void Variables_ShouldBeModifiable()
    {
        // Arrange
        var collection = new Collection { Name = "Test" };

        // Act
        collection.Variables["API_URL"] = "https://api.example.com";
        collection.Variables["API_KEY"] = "secret";

        // Assert
        Assert.Equal(2, collection.Variables.Count);
        Assert.Equal("https://api.example.com", collection.Variables["API_URL"]);
        Assert.Equal("secret", collection.Variables["API_KEY"]);
    }

    [Fact]
    public void SecretVariableNames_ShouldBeInitializedAsEmptySet()
    {
        // Arrange & Act
        var collection = new Collection { Name = "Test" };

        // Assert
        Assert.NotNull(collection.SecretVariableNames);
        Assert.Empty(collection.SecretVariableNames);
    }

    [Fact]
    public void SecretVariableNames_ShouldBeModifiable()
    {
        // Arrange
        var collection = new Collection { Name = "Test" };

        // Act
        collection.SecretVariableNames.Add("API_KEY");
        collection.SecretVariableNames.Add("PASSWORD");

        // Assert
        Assert.Equal(2, collection.SecretVariableNames.Count);
        Assert.Contains("API_KEY", collection.SecretVariableNames);
        Assert.Contains("PASSWORD", collection.SecretVariableNames);
    }

    [Fact]
    public void SecretVariableNames_ShouldNotAllowDuplicates()
    {
        // Arrange
        var collection = new Collection { Name = "Test" };

        // Act
        collection.SecretVariableNames.Add("API_KEY");
        collection.SecretVariableNames.Add("API_KEY"); // Duplicate

        // Assert
        Assert.Single(collection.SecretVariableNames);
    }
}
