using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Services;

public class RequestServiceReorderTests
{
    private readonly Mock<IRepository<Request>> _mockRequestRepository;
    private readonly Mock<IRepository<Domain.Entities.Environment>> _mockEnvironmentRepository;
    private readonly Mock<IRepository<Collection>> _mockCollectionRepository;
    private readonly RepositoryAccessor _repositories;
    private readonly RequestExecutionContext _executionContext;
    private readonly Mock<IEnvironmentService> _mockEnvironmentService;
    private readonly Mock<ICollectionService> _mockCollectionService;
    private readonly RequestService _service;

    public RequestServiceReorderTests()
    {
        _mockRequestRepository = new Mock<IRepository<Request>>();
        _mockEnvironmentRepository = new Mock<IRepository<Domain.Entities.Environment>>();
        _mockCollectionRepository = new Mock<IRepository<Collection>>();
        
        _repositories = new RepositoryAccessor(
            _mockRequestRepository.Object,
            _mockCollectionRepository.Object,
            _mockEnvironmentRepository.Object,
            Mock.Of<IRepository<Flow>>(),
            Mock.Of<IRepository<RequestHistoryEntry>>());

        _executionContext = new RequestExecutionContext(
            Mock.Of<IActiveEnvironmentService>(),
            Mock.Of<IVariableResolver>(),
            Mock.Of<IRequestExecutorFactory>(),
            Mock.Of<IResponseValueExtractor>());

        _mockEnvironmentService = new Mock<IEnvironmentService>();
        _mockCollectionService = new Mock<ICollectionService>();

        _service = new RequestService(
            _repositories,
            _executionContext,
            _mockEnvironmentService.Object,
            _mockCollectionService.Object);
    }

    [Fact]
    public async Task UpdateRequestOrderAsync_WithValidOrders_ShouldUpdateOrderIndex()
    {
        // Arrange
        var request1 = new RestRequest { Id = Guid.NewGuid(), Name = "Request 1", OrderIndex = 0 };
        var request2 = new RestRequest { Id = Guid.NewGuid(), Name = "Request 2", OrderIndex = 1 };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(request1.Id)).ReturnsAsync(request1);
        _mockRequestRepository.Setup(r => r.GetByIdAsync(request2.Id)).ReturnsAsync(request2);
        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<Request>())).ReturnsAsync((Request r) => r);

        var orders = new[] { (request1.Id, 10), (request2.Id, 20) };

        // Act
        await _service.UpdateRequestOrderAsync(orders);

        // Assert
        Assert.Equal(10, request1.OrderIndex);
        Assert.Equal(20, request2.OrderIndex);
        _mockRequestRepository.Verify(r => r.UpdateAsync(request1), Times.Once);
        _mockRequestRepository.Verify(r => r.UpdateAsync(request2), Times.Once);
    }

    [Fact]
    public async Task MoveRequestAsync_MoveUp_ShouldSwapOrderIndex()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var request1 = new RestRequest { Id = Guid.NewGuid(), Name = "First", CollectionId = collectionId, OrderIndex = 0, CreatedAt = DateTime.UtcNow.AddMinutes(-2) };
        var request2 = new RestRequest { Id = Guid.NewGuid(), Name = "Second", CollectionId = collectionId, OrderIndex = 1, CreatedAt = DateTime.UtcNow.AddMinutes(-1) };
        var request3 = new RestRequest { Id = Guid.NewGuid(), Name = "Third", CollectionId = collectionId, OrderIndex = 2, CreatedAt = DateTime.UtcNow };

        var allRequests = new List<Request> { request1, request2, request3 };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(request2.Id)).ReturnsAsync(request2);
        _mockRequestRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allRequests);
        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<Request>())).ReturnsAsync((Request r) => r);

        // Act - Move request2 up (should swap with request1)
        await _service.MoveRequestAsync(request2.Id, moveUp: true);

        // Assert
        Assert.Equal(1, request1.OrderIndex); // Moved down
        Assert.Equal(0, request2.OrderIndex); // Moved up
        _mockRequestRepository.Verify(r => r.UpdateAsync(request1), Times.Once);
        _mockRequestRepository.Verify(r => r.UpdateAsync(request2), Times.Once);
    }

    [Fact]
    public async Task MoveRequestAsync_MoveDown_ShouldSwapOrderIndex()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var request1 = new RestRequest { Id = Guid.NewGuid(), Name = "First", CollectionId = collectionId, OrderIndex = 0, CreatedAt = DateTime.UtcNow.AddMinutes(-2) };
        var request2 = new RestRequest { Id = Guid.NewGuid(), Name = "Second", CollectionId = collectionId, OrderIndex = 1, CreatedAt = DateTime.UtcNow.AddMinutes(-1) };
        var request3 = new RestRequest { Id = Guid.NewGuid(), Name = "Third", CollectionId = collectionId, OrderIndex = 2, CreatedAt = DateTime.UtcNow };

        var allRequests = new List<Request> { request1, request2, request3 };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(request2.Id)).ReturnsAsync(request2);
        _mockRequestRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allRequests);
        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<Request>())).ReturnsAsync((Request r) => r);

        // Act - Move request2 down (should swap with request3)
        await _service.MoveRequestAsync(request2.Id, moveUp: false);

        // Assert
        Assert.Equal(2, request2.OrderIndex); // Moved down
        Assert.Equal(1, request3.OrderIndex); // Moved up
        _mockRequestRepository.Verify(r => r.UpdateAsync(request2), Times.Once);
        _mockRequestRepository.Verify(r => r.UpdateAsync(request3), Times.Once);
    }

    [Fact]
    public async Task MoveRequestAsync_MoveUpFirstItem_ShouldNotChange()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var request1 = new RestRequest { Id = Guid.NewGuid(), Name = "First", CollectionId = collectionId, OrderIndex = 0 };
        var request2 = new RestRequest { Id = Guid.NewGuid(), Name = "Second", CollectionId = collectionId, OrderIndex = 1 };

        var allRequests = new List<Request> { request1, request2 };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(request1.Id)).ReturnsAsync(request1);
        _mockRequestRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allRequests);

        // Act - Try to move first item up (should do nothing)
        await _service.MoveRequestAsync(request1.Id, moveUp: true);

        // Assert - No updates should occur
        _mockRequestRepository.Verify(r => r.UpdateAsync(It.IsAny<Request>()), Times.Never);
    }

    [Fact]
    public async Task MoveRequestAsync_MoveDownLastItem_ShouldNotChange()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var request1 = new RestRequest { Id = Guid.NewGuid(), Name = "First", CollectionId = collectionId, OrderIndex = 0 };
        var request2 = new RestRequest { Id = Guid.NewGuid(), Name = "Second", CollectionId = collectionId, OrderIndex = 1 };

        var allRequests = new List<Request> { request1, request2 };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(request2.Id)).ReturnsAsync(request2);
        _mockRequestRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allRequests);

        // Act - Try to move last item down (should do nothing)
        await _service.MoveRequestAsync(request2.Id, moveUp: false);

        // Assert - No updates should occur
        _mockRequestRepository.Verify(r => r.UpdateAsync(It.IsAny<Request>()), Times.Never);
    }

    [Fact]
    public async Task MoveRequestAsync_OnlyMovesWithinSameCollection_ShouldRespectHierarchy()
    {
        // Arrange
        var collection1Id = Guid.NewGuid();
        var collection2Id = Guid.NewGuid();
        var request1 = new RestRequest { Id = Guid.NewGuid(), Name = "Collection1-First", CollectionId = collection1Id, OrderIndex = 0 };
        var request2 = new RestRequest { Id = Guid.NewGuid(), Name = "Collection1-Second", CollectionId = collection1Id, OrderIndex = 1 };
        var request3 = new RestRequest { Id = Guid.NewGuid(), Name = "Collection2-First", CollectionId = collection2Id, OrderIndex = 0 };

        var allRequests = new List<Request> { request1, request2, request3 };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(request2.Id)).ReturnsAsync(request2);
        _mockRequestRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allRequests);
        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<Request>())).ReturnsAsync((Request r) => r);

        // Act - Move request2 up (should only consider siblings in same collection)
        await _service.MoveRequestAsync(request2.Id, moveUp: true);

        // Assert - Should swap with request1, not request3
        Assert.Equal(1, request1.OrderIndex);
        Assert.Equal(0, request2.OrderIndex);
        Assert.Equal(0, request3.OrderIndex); // Unchanged
        _mockRequestRepository.Verify(r => r.UpdateAsync(request3), Times.Never);
    }

    [Fact]
    public async Task MoveRequestAsync_RequestsWithNoCollection_ShouldGroupCorrectly()
    {
        // Arrange - Requests without collection should be grouped together
        var request1 = new RestRequest { Id = Guid.NewGuid(), Name = "NoCollection-First", CollectionId = null, OrderIndex = 0 };
        var request2 = new RestRequest { Id = Guid.NewGuid(), Name = "NoCollection-Second", CollectionId = null, OrderIndex = 1 };
        var request3 = new RestRequest { Id = Guid.NewGuid(), Name = "InCollection", CollectionId = Guid.NewGuid(), OrderIndex = 0 };

        var allRequests = new List<Request> { request1, request2, request3 };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(request2.Id)).ReturnsAsync(request2);
        _mockRequestRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(allRequests);
        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<Request>())).ReturnsAsync((Request r) => r);

        // Act - Move request2 up (should only consider siblings without collection)
        await _service.MoveRequestAsync(request2.Id, moveUp: true);

        // Assert
        Assert.Equal(1, request1.OrderIndex);
        Assert.Equal(0, request2.OrderIndex);
        Assert.Equal(0, request3.OrderIndex); // Unchanged
        _mockRequestRepository.Verify(r => r.UpdateAsync(request3), Times.Never);
    }

    [Fact]
    public async Task MoveRequestAsync_NonExistentRequest_ShouldThrowException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _mockRequestRepository.Setup(r => r.GetByIdAsync(nonExistentId)).ReturnsAsync((Request?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.MoveRequestAsync(nonExistentId, moveUp: true)
        );
    }
}
