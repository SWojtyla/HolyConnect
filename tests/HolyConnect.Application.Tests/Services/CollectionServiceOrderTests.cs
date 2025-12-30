using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Services;

public class CollectionServiceOrderTests
{
    private readonly Mock<IRepository<Collection>> _mockRepository;
    private readonly Mock<ISecretVariablesService> _mockSecretVariablesService;
    private readonly CollectionService _service;

    public CollectionServiceOrderTests()
    {
        _mockRepository = new Mock<IRepository<Collection>>();
        _mockSecretVariablesService = new Mock<ISecretVariablesService>();
        
        // Setup secret variables service to return empty dictionary
        _mockSecretVariablesService.Setup(s => s.GetCollectionSecretsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Dictionary<string, string>());
        
        _service = new CollectionService(_mockRepository.Object, _mockSecretVariablesService.Object);
    }

    [Fact]
    public async Task MoveCollectionAsync_WithValidData_ShouldUpdateCollectionParentAndOrder()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var oldParentId = Guid.NewGuid();
        var newParentId = Guid.NewGuid();
        var collection = new Collection
        {
            Id = collectionId,
            Name = "Test Collection",
            ParentCollectionId = oldParentId,
            Order = 0
        };

        _mockRepository.Setup(r => r.GetByIdAsync(collectionId))
            .ReturnsAsync(collection);
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Collection> { collection });
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Collection>()))
            .ReturnsAsync((Collection c) => c);

        // Act
        await _service.MoveCollectionAsync(collectionId, newParentId, 5);

        // Assert
        Assert.Equal(newParentId, collection.ParentCollectionId);
        // Order is normalized during reordering, so it will be 0 after normalization
        Assert.Equal(0, collection.Order);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Collection>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task MoveCollectionAsync_WithNullNewParent_ShouldMoveToRoot()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var oldParentId = Guid.NewGuid();
        var collection = new Collection
        {
            Id = collectionId,
            Name = "Test Collection",
            ParentCollectionId = oldParentId,
            Order = 0
        };

        _mockRepository.Setup(r => r.GetByIdAsync(collectionId))
            .ReturnsAsync(collection);
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Collection> { collection });
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Collection>()))
            .ReturnsAsync((Collection c) => c);

        // Act
        await _service.MoveCollectionAsync(collectionId, null, 0);

        // Assert
        Assert.Null(collection.ParentCollectionId);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Collection>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task MoveCollectionAsync_WithNonExistentCollection_ShouldThrowException()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(collectionId))
            .ReturnsAsync((Collection?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.MoveCollectionAsync(collectionId, Guid.NewGuid(), 0));
    }

    [Fact]
    public async Task MoveCollectionAsync_ShouldReorderSiblings()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var collection1 = new Collection { Id = collectionId, Name = "Collection 1", ParentCollectionId = parentId, Order = 0 };
        var collection2 = new Collection { Id = Guid.NewGuid(), Name = "Collection 2", ParentCollectionId = parentId, Order = 1 };
        var collection3 = new Collection { Id = Guid.NewGuid(), Name = "Collection 3", ParentCollectionId = parentId, Order = 2 };

        _mockRepository.Setup(r => r.GetByIdAsync(collectionId))
            .ReturnsAsync(collection1);
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Collection> { collection1, collection2, collection3 });
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Collection>()))
            .ReturnsAsync((Collection c) => c);

        // Act
        await _service.MoveCollectionAsync(collectionId, parentId, 10);

        // Assert - Siblings should be reordered with sequential order values
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Collection>(c => 
            c.ParentCollectionId == parentId)), Times.AtLeast(3));
    }
}
