using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Services;

public class CollectionServiceReorderTests
{
    private readonly Mock<IRepository<Collection>> _mockRepository;
    private readonly Mock<ISecretVariablesService> _mockSecretVariablesService;
    private readonly CollectionService _service;

    public CollectionServiceReorderTests()
    {
        _mockRepository = new Mock<IRepository<Collection>>();
        _mockSecretVariablesService = new Mock<ISecretVariablesService>();
        _mockSecretVariablesService.Setup(s => s.GetCollectionSecretsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Dictionary<string, string>());
        _service = new CollectionService(_mockRepository.Object, _mockSecretVariablesService.Object);
    }

    [Fact]
    public async Task UpdateCollectionOrderAsync_WithValidOrders_ShouldUpdateOrderIndex()
    {
        // Arrange
        var collection1 = new Collection { Id = Guid.NewGuid(), Name = "Collection 1", OrderIndex = 0 };
        var collection2 = new Collection { Id = Guid.NewGuid(), Name = "Collection 2", OrderIndex = 1 };

        _mockRepository.Setup(r => r.GetByIdAsync(collection1.Id)).ReturnsAsync(collection1);
        _mockRepository.Setup(r => r.GetByIdAsync(collection2.Id)).ReturnsAsync(collection2);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Collection>())).ReturnsAsync((Collection c) => c);

        var orders = new[] { (collection1.Id, 10), (collection2.Id, 20) };

        // Act
        await _service.UpdateCollectionOrderAsync(orders);

        // Assert
        Assert.Equal(10, collection1.OrderIndex);
        Assert.Equal(20, collection2.OrderIndex);
        _mockRepository.Verify(r => r.UpdateAsync(collection1), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(collection2), Times.Once);
    }

    [Fact]
    public async Task MoveCollectionAsync_MoveUp_ShouldSwapOrderIndex()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var collection1 = new Collection { Id = Guid.NewGuid(), Name = "First", ParentCollectionId = parentId, OrderIndex = 0, CreatedAt = DateTime.UtcNow.AddMinutes(-2) };
        var collection2 = new Collection { Id = Guid.NewGuid(), Name = "Second", ParentCollectionId = parentId, OrderIndex = 1, CreatedAt = DateTime.UtcNow.AddMinutes(-1) };
        var collection3 = new Collection { Id = Guid.NewGuid(), Name = "Third", ParentCollectionId = parentId, OrderIndex = 2, CreatedAt = DateTime.UtcNow };

        var allCollections = new List<Collection> { collection1, collection2, collection3 };

        _mockRepository.Setup(r => r.GetByIdAsync(collection2.Id)).ReturnsAsync(collection2);
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allCollections);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Collection>())).ReturnsAsync((Collection c) => c);

        // Act - Move collection2 up (should swap with collection1)
        await _service.MoveCollectionAsync(collection2.Id, moveUp: true);

        // Assert
        Assert.Equal(1, collection1.OrderIndex); // Moved down
        Assert.Equal(0, collection2.OrderIndex); // Moved up
        _mockRepository.Verify(r => r.UpdateAsync(collection1), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(collection2), Times.Once);
    }

    [Fact]
    public async Task MoveCollectionAsync_MoveDown_ShouldSwapOrderIndex()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var collection1 = new Collection { Id = Guid.NewGuid(), Name = "First", ParentCollectionId = parentId, OrderIndex = 0, CreatedAt = DateTime.UtcNow.AddMinutes(-2) };
        var collection2 = new Collection { Id = Guid.NewGuid(), Name = "Second", ParentCollectionId = parentId, OrderIndex = 1, CreatedAt = DateTime.UtcNow.AddMinutes(-1) };
        var collection3 = new Collection { Id = Guid.NewGuid(), Name = "Third", ParentCollectionId = parentId, OrderIndex = 2, CreatedAt = DateTime.UtcNow };

        var allCollections = new List<Collection> { collection1, collection2, collection3 };

        _mockRepository.Setup(r => r.GetByIdAsync(collection2.Id)).ReturnsAsync(collection2);
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allCollections);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Collection>())).ReturnsAsync((Collection c) => c);

        // Act - Move collection2 down (should swap with collection3)
        await _service.MoveCollectionAsync(collection2.Id, moveUp: false);

        // Assert
        Assert.Equal(2, collection2.OrderIndex); // Moved down
        Assert.Equal(1, collection3.OrderIndex); // Moved up
        _mockRepository.Verify(r => r.UpdateAsync(collection2), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(collection3), Times.Once);
    }

    [Fact]
    public async Task MoveCollectionAsync_MoveUpFirstItem_ShouldNotChange()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var collection1 = new Collection { Id = Guid.NewGuid(), Name = "First", ParentCollectionId = parentId, OrderIndex = 0 };
        var collection2 = new Collection { Id = Guid.NewGuid(), Name = "Second", ParentCollectionId = parentId, OrderIndex = 1 };

        var allCollections = new List<Collection> { collection1, collection2 };

        _mockRepository.Setup(r => r.GetByIdAsync(collection1.Id)).ReturnsAsync(collection1);
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allCollections);

        // Act - Try to move first item up (should do nothing)
        await _service.MoveCollectionAsync(collection1.Id, moveUp: true);

        // Assert - No updates should occur
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Collection>()), Times.Never);
    }

    [Fact]
    public async Task MoveCollectionAsync_MoveDownLastItem_ShouldNotChange()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var collection1 = new Collection { Id = Guid.NewGuid(), Name = "First", ParentCollectionId = parentId, OrderIndex = 0 };
        var collection2 = new Collection { Id = Guid.NewGuid(), Name = "Second", ParentCollectionId = parentId, OrderIndex = 1 };

        var allCollections = new List<Collection> { collection1, collection2 };

        _mockRepository.Setup(r => r.GetByIdAsync(collection2.Id)).ReturnsAsync(collection2);
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allCollections);

        // Act - Try to move last item down (should do nothing)
        await _service.MoveCollectionAsync(collection2.Id, moveUp: false);

        // Assert - No updates should occur
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Collection>()), Times.Never);
    }

    [Fact]
    public async Task MoveCollectionAsync_OnlyMovesWithinSameParent_ShouldRespectHierarchy()
    {
        // Arrange
        var parent1Id = Guid.NewGuid();
        var parent2Id = Guid.NewGuid();
        var collection1 = new Collection { Id = Guid.NewGuid(), Name = "Parent1-First", ParentCollectionId = parent1Id, OrderIndex = 0 };
        var collection2 = new Collection { Id = Guid.NewGuid(), Name = "Parent1-Second", ParentCollectionId = parent1Id, OrderIndex = 1 };
        var collection3 = new Collection { Id = Guid.NewGuid(), Name = "Parent2-First", ParentCollectionId = parent2Id, OrderIndex = 0 };

        var allCollections = new List<Collection> { collection1, collection2, collection3 };

        _mockRepository.Setup(r => r.GetByIdAsync(collection2.Id)).ReturnsAsync(collection2);
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allCollections);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Collection>())).ReturnsAsync((Collection c) => c);

        // Act - Move collection2 up (should only consider siblings with same parent)
        await _service.MoveCollectionAsync(collection2.Id, moveUp: true);

        // Assert - Should swap with collection1, not collection3
        Assert.Equal(1, collection1.OrderIndex);
        Assert.Equal(0, collection2.OrderIndex);
        Assert.Equal(0, collection3.OrderIndex); // Unchanged
        _mockRepository.Verify(r => r.UpdateAsync(collection3), Times.Never);
    }

    [Fact]
    public async Task MoveCollectionAsync_NonExistentCollection_ShouldThrowException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(nonExistentId)).ReturnsAsync((Collection?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.MoveCollectionAsync(nonExistentId, moveUp: true)
        );
    }
}
