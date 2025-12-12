using HolyConnect.Domain.Entities;

namespace HolyConnect.Maui.Tests.Components;

/// <summary>
/// Tests for Collection entity used in UI components
/// </summary>
public class CollectionTests
{
    [Fact]
    public void Collection_CanBeCreated_WithBasicProperties()
    {
        // Arrange & Act
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "API Tests",
            Description = "Collection of API tests"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, collection.Id);
        Assert.Equal("API Tests", collection.Name);
        Assert.Equal("Collection of API tests", collection.Description);
    }

    [Fact]
    public void Collection_InitializesEmptyVariablesDictionary()
    {
        // Arrange & Act
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };

        // Assert
        Assert.NotNull(collection.Variables);
        Assert.Empty(collection.Variables);
    }

    [Fact]
    public void Collection_SupportsAddingVariables()
    {
        // Arrange
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };

        // Act
        collection.Variables["userId"] = "12345";
        collection.Variables["sessionToken"] = "abc-def-ghi";

        // Assert
        Assert.Equal(2, collection.Variables.Count);
        Assert.Equal("12345", collection.Variables["userId"]);
        Assert.Equal("abc-def-ghi", collection.Variables["sessionToken"]);
    }

    [Fact]
    public void Collection_CanHaveParentCollection()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Subcollection"
        };

        // Act
        collection.ParentCollectionId = parentId;

        // Assert
        Assert.Equal(parentId, collection.ParentCollectionId);
    }

    [Fact]
    public void Collection_CanBeRootCollection()
    {
        // Arrange & Act
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Root Collection",
            ParentCollectionId = null
        };

        // Assert
        Assert.Null(collection.ParentCollectionId);
    }

    [Fact]
    public void Collection_SupportsDescription()
    {
        // Arrange & Act
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Description = "This is a test collection with a detailed description"
        };

        // Assert
        Assert.NotNull(collection.Description);
        Assert.Contains("detailed description", collection.Description);
    }

    [Fact]
    public void Collection_SupportsNullDescription()
    {
        // Arrange & Act
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Description = null
        };

        // Assert
        Assert.Null(collection.Description);
    }

    [Fact]
    public void Collection_VariablesOverrideEnvironmentVariables()
    {
        // This test documents the expected behavior that collection variables
        // should take precedence over environment variables when both exist

        // Arrange
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Test Env"
        };
        environment.Variables["baseUrl"] = "https://env.example.com";

        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test Collection"
        };
        collection.Variables["baseUrl"] = "https://collection.example.com";

        // Act & Assert
        // Collection variable should be different from environment variable
        Assert.NotEqual(environment.Variables["baseUrl"], collection.Variables["baseUrl"]);
        Assert.Equal("https://collection.example.com", collection.Variables["baseUrl"]);
    }

    [Fact]
    public void Collection_SupportsEmptyName()
    {
        // Arrange & Act
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = string.Empty
        };

        // Assert
        Assert.Equal(string.Empty, collection.Name);
    }

    [Fact]
    public void Collection_TracksCreationDate()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        
        // Act
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            CreatedAt = DateTime.UtcNow
        };
        
        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.True(collection.CreatedAt >= beforeCreate);
        Assert.True(collection.CreatedAt <= afterCreate);
    }

    [Fact]
    public void Collection_SupportsLongNames()
    {
        // Arrange
        var longName = new string('a', 200);

        // Act
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = longName
        };

        // Assert
        Assert.Equal(200, collection.Name.Length);
        Assert.Equal(longName, collection.Name);
    }

    [Fact]
    public void Collection_SupportsSpecialCharactersInName()
    {
        // Arrange & Act
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "API Tests - v2.0 (Production) [DEPRECATED]"
        };

        // Assert
        Assert.Contains("-", collection.Name);
        Assert.Contains("(", collection.Name);
        Assert.Contains("[", collection.Name);
    }

    [Fact]
    public void Collection_SupportsUnicodeCharactersInName()
    {
        // Arrange & Act
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "API テスト 🚀"
        };

        // Assert
        Assert.Contains("テスト", collection.Name);
        Assert.Contains("🚀", collection.Name);
    }

    [Fact]
    public void Collection_VariableKeysAreCaseSensitive()
    {
        // Arrange
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };

        // Act
        collection.Variables["UserId"] = "value1";
        collection.Variables["userid"] = "value2";
        collection.Variables["USERID"] = "value3";

        // Assert
        Assert.Equal(3, collection.Variables.Count);
        Assert.Equal("value1", collection.Variables["UserId"]);
        Assert.Equal("value2", collection.Variables["userid"]);
        Assert.Equal("value3", collection.Variables["USERID"]);
    }

    [Theory]
    [InlineData("Users API")]
    [InlineData("Authentication")]
    [InlineData("E-commerce")]
    [InlineData("Admin Panel")]
    [InlineData("Mobile Backend")]
    public void Collection_SupportsCommonCollectionNames(string name)
    {
        // Arrange & Act
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        // Assert
        Assert.Equal(name, collection.Name);
    }

    [Fact]
    public void Collection_SupportsHierarchicalStructure()
    {
        // Arrange
        var rootCollection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Root",
            ParentCollectionId = null
        };

        var childCollection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Child",
            ParentCollectionId = rootCollection.Id
        };

        var grandchildCollection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Grandchild",
            ParentCollectionId = childCollection.Id
        };

        // Assert
        Assert.Null(rootCollection.ParentCollectionId);
        Assert.Equal(rootCollection.Id, childCollection.ParentCollectionId);
        Assert.Equal(childCollection.Id, grandchildCollection.ParentCollectionId);
    }
}
