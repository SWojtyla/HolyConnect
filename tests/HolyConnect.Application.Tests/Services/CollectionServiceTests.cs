using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Services;

public class CollectionServiceTests
{
    private readonly Mock<IRepository<Collection>> _mockRepository;
    private readonly Mock<ISecretVariablesService> _mockSecretVariablesService;
    private readonly CollectionService _service;

    public CollectionServiceTests()
    {
        _mockRepository = new Mock<IRepository<Collection>>();
        _mockSecretVariablesService = new Mock<ISecretVariablesService>();
        _mockSecretVariablesService.Setup(s => s.GetCollectionSecretsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Dictionary<string, string>());
        _service = new CollectionService(_mockRepository.Object, _mockSecretVariablesService.Object);
    }

    [Fact]
    public async Task CreateCollectionAsync_ShouldCreateCollectionWithCorrectProperties()
    {
        // Arrange
        var name = "Test Collection";
        var description = "Test Description";
        Collection? capturedCollection = null;

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Collection>()))
            .Callback<Collection>(c => capturedCollection = c)
            .ReturnsAsync((Collection c) => c);

        // Act
        var result = await _service.CreateCollectionAsync(name, null, description);

        // Assert
        Assert.NotNull(capturedCollection);
        Assert.NotEqual(Guid.Empty, capturedCollection.Id);
        Assert.Equal(name, capturedCollection.Name);
        Assert.Equal(description, capturedCollection.Description);
        Assert.True(capturedCollection.CreatedAt > DateTime.MinValue);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Collection>()), Times.Once);
    }

    [Fact]
    public async Task GetCollectionByIdAsync_ShouldReturnCollection()
    {
        // Arrange
        var id = Guid.NewGuid();
        var collection = new Collection { Id = id, Name = "Test" };

        _mockRepository.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(collection);

        // Act
        var result = await _service.GetCollectionByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task GetAllCollectionsAsync_ShouldReturnAllCollections()
    {
        // Arrange
        var collections = new List<Collection>
        {
            new() { Id = Guid.NewGuid(), Name = "Collection 1" },
            new() { Id = Guid.NewGuid(), Name = "Collection 2" },
            new() { Id = Guid.NewGuid(), Name = "Collection 3" }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(collections);

        // Act
        var result = await _service.GetAllCollectionsAsync();

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task UpdateCollectionAsync_ShouldUpdateCollection()
    {
        // Arrange
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Updated Collection"
        };

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Collection>()))
            .ReturnsAsync((Collection c) => c);

        // Act
        var result = await _service.UpdateCollectionAsync(collection);

        // Assert
        Assert.Equal(collection.Id, result.Id);
        Assert.Equal(collection.Name, result.Name);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Collection>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCollectionAsync_ShouldCallRepository()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockRepository.Setup(r => r.DeleteAsync(id))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteCollectionAsync(id);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task CreateCollectionAsync_ShouldCreateSubCollectionWithParentId()
    {
        // Arrange
        var parentCollectionId = Guid.NewGuid();
        var name = "Sub-Collection";
        var description = "Test Sub-Collection";
        Collection? capturedCollection = null;

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Collection>()))
            .Callback<Collection>(c => capturedCollection = c)
            .ReturnsAsync((Collection c) => c);

        // Act
        var result = await _service.CreateCollectionAsync(name, parentCollectionId, description);

        // Assert
        Assert.NotNull(capturedCollection);
        Assert.NotEqual(Guid.Empty, capturedCollection.Id);
        Assert.Equal(parentCollectionId, capturedCollection.ParentCollectionId);
        Assert.Equal(name, capturedCollection.Name);
        Assert.Equal(description, capturedCollection.Description);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Collection>()), Times.Once);
    }

    [Fact]
    public async Task CreateCollectionAsync_ShouldCreateRootCollectionWithNullParentId()
    {
        // Arrange
        var name = "Root Collection";
        Collection? capturedCollection = null;

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Collection>()))
            .Callback<Collection>(c => capturedCollection = c)
            .ReturnsAsync((Collection c) => c);

        // Act
        var result = await _service.CreateCollectionAsync(name, null, null);

        // Assert
        Assert.NotNull(capturedCollection);
        Assert.Null(capturedCollection.ParentCollectionId);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Collection>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCollectionAsync_WithVariables_ShouldPreserveVariables()
    {
        // Arrange
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test Collection",
            Variables = new Dictionary<string, string>
            {
                { "API_URL", "https://api.staging.com" },
                { "TEST_VAR", "test_value" }
            }
        };

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Collection>()))
            .ReturnsAsync((Collection c) => c);

        // Act
        var result = await _service.UpdateCollectionAsync(collection);

        // Assert
        Assert.Equal(2, result.Variables.Count);
        Assert.Equal("https://api.staging.com", result.Variables["API_URL"]);
        Assert.Equal("test_value", result.Variables["TEST_VAR"]);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Collection>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCollectionAsync_ModifyingVariables_ShouldUpdateCorrectly()
    {
        // Arrange
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test Collection",
            Variables = new Dictionary<string, string>
            {
                { "OLD_VAR", "old_value" }
            }
        };

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Collection>()))
            .ReturnsAsync((Collection c) => c);

        // Act
        collection.Variables.Remove("OLD_VAR");
        collection.Variables["NEW_VAR"] = "new_value";
        collection.Variables["ANOTHER_VAR"] = "another_value";
        var result = await _service.UpdateCollectionAsync(collection);

        // Assert
        Assert.Equal(2, result.Variables.Count);
        Assert.False(result.Variables.ContainsKey("OLD_VAR"));
        Assert.Equal("new_value", result.Variables["NEW_VAR"]);
        Assert.Equal("another_value", result.Variables["ANOTHER_VAR"]);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Collection>()), Times.Once);
    }

    [Fact]
    public async Task CreateCollectionAsync_WithDuplicateName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var name = "Duplicate Collection";
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Collection>()))
            .ThrowsAsync(new InvalidOperationException($"An entity with the name '{name}' already exists."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateCollectionAsync(name));
        Assert.Contains("already exists", exception.Message);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Collection>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCollectionAsync_WithDuplicateName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Duplicate Collection"
        };
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Collection>()))
            .ThrowsAsync(new InvalidOperationException($"An entity with the name '{collection.Name}' already exists."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateCollectionAsync(collection));
        Assert.Contains("already exists", exception.Message);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Collection>()), Times.Once);
    }

    [Fact]
    public async Task MoveCollectionAsync_WithValidDestination_ShouldMoveCollection()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var newParentId = Guid.NewGuid();
        var collection = new Collection
        {
            Id = collectionId,
            Name = "Test Collection",
            ParentCollectionId = null
        };

        _mockRepository.Setup(r => r.GetByIdAsync(collectionId))
            .ReturnsAsync(collection);
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Collection> { collection });
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Collection>()))
            .ReturnsAsync((Collection c) => c);

        // Act
        var result = await _service.MoveCollectionAsync(collectionId, newParentId);

        // Assert
        Assert.Equal(newParentId, result.ParentCollectionId);
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Collection>(c => c.ParentCollectionId == newParentId)), Times.Once);
    }

    [Fact]
    public async Task MoveCollectionAsync_ToRoot_ShouldSetParentToNull()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var collection = new Collection
        {
            Id = collectionId,
            Name = "Test Collection",
            ParentCollectionId = parentId
        };

        _mockRepository.Setup(r => r.GetByIdAsync(collectionId))
            .ReturnsAsync(collection);
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Collection> { collection });
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Collection>()))
            .ReturnsAsync((Collection c) => c);

        // Act
        var result = await _service.MoveCollectionAsync(collectionId, null);

        // Assert
        Assert.Null(result.ParentCollectionId);
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Collection>(c => c.ParentCollectionId == null)), Times.Once);
    }

    [Fact]
    public async Task MoveCollectionAsync_WithCircularReference_ShouldThrowException()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var parent = new Collection
        {
            Id = parentId,
            Name = "Parent",
            ParentCollectionId = null
        };
        var child = new Collection
        {
            Id = childId,
            Name = "Child",
            ParentCollectionId = parentId
        };

        _mockRepository.Setup(r => r.GetByIdAsync(parentId))
            .ReturnsAsync(parent);
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Collection> { parent, child });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.MoveCollectionAsync(parentId, childId));
        Assert.Contains("descendant", exception.Message);
    }

    [Fact]
    public async Task MoveCollectionAsync_WithNonExistentCollection_ShouldThrowException()
    {
        // Arrange
        var collectionId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAsync(collectionId))
            .ReturnsAsync((Collection?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.MoveCollectionAsync(collectionId, Guid.NewGuid()));
        Assert.Contains("not found", exception.Message);
    }
}
