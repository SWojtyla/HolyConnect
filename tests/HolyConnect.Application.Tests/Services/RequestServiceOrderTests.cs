using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Services;

public class RequestServiceOrderTests
{
    private readonly Mock<IRepository<Request>> _mockRequestRepository;
    private readonly Mock<IRepository<Collection>> _mockCollectionRepository;
    private readonly Mock<IRepository<Domain.Entities.Environment>> _mockEnvironmentRepository;
    private readonly Mock<IRepository<Flow>> _mockFlowRepository;
    private readonly Mock<IRepository<RequestHistoryEntry>> _mockHistoryRepository;
    private readonly Mock<IActiveEnvironmentService> _mockActiveEnvironmentService;
    private readonly Mock<IEnvironmentService> _mockEnvironmentService;
    private readonly Mock<ICollectionService> _mockCollectionService;
    private readonly Mock<IRequestExecutorFactory> _mockExecutorFactory;
    private readonly Mock<IVariableResolver> _mockVariableResolver;
    private readonly RequestService _service;

    public RequestServiceOrderTests()
    {
        _mockRequestRepository = new Mock<IRepository<Request>>();
        _mockCollectionRepository = new Mock<IRepository<Collection>>();
        _mockEnvironmentRepository = new Mock<IRepository<Domain.Entities.Environment>>();
        _mockFlowRepository = new Mock<IRepository<Flow>>();
        _mockHistoryRepository = new Mock<IRepository<RequestHistoryEntry>>();
        _mockActiveEnvironmentService = new Mock<IActiveEnvironmentService>();
        _mockEnvironmentService = new Mock<IEnvironmentService>();
        _mockCollectionService = new Mock<ICollectionService>();
        _mockExecutorFactory = new Mock<IRequestExecutorFactory>();
        _mockVariableResolver = new Mock<IVariableResolver>();
        
        var repositories = new RepositoryAccessor(
            _mockRequestRepository.Object,
            _mockCollectionRepository.Object,
            _mockEnvironmentRepository.Object,
            _mockFlowRepository.Object,
            _mockHistoryRepository.Object);

        var executionContext = new RequestExecutionContext(
            _mockActiveEnvironmentService.Object,
            _mockVariableResolver.Object,
            _mockExecutorFactory.Object);

        _service = new RequestService(
            repositories,
            executionContext,
            _mockEnvironmentService.Object,
            _mockCollectionService.Object);
    }

    [Fact]
    public async Task MoveRequestAsync_WithValidData_ShouldUpdateRequestCollectionAndOrder()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var oldCollectionId = Guid.NewGuid();
        var newCollectionId = Guid.NewGuid();
        var request = new RestRequest
        {
            Id = requestId,
            Name = "Test Request",
            CollectionId = oldCollectionId,
            Order = 0
        };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);
        _mockRequestRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Request> { request });
        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        await _service.MoveRequestAsync(requestId, newCollectionId, 3);

        // Assert
        Assert.Equal(newCollectionId, request.CollectionId);
        // Order is normalized during reordering, so it will be 0 after normalization
        Assert.Equal(0, request.Order);
        _mockRequestRepository.Verify(r => r.UpdateAsync(It.IsAny<Request>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task MoveRequestAsync_WithNullNewCollection_ShouldMoveToNoCollection()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var oldCollectionId = Guid.NewGuid();
        var request = new RestRequest
        {
            Id = requestId,
            Name = "Test Request",
            CollectionId = oldCollectionId,
            Order = 0
        };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);
        _mockRequestRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Request> { request });
        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        await _service.MoveRequestAsync(requestId, null, 0);

        // Assert
        Assert.Null(request.CollectionId);
        _mockRequestRepository.Verify(r => r.UpdateAsync(It.IsAny<Request>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task MoveRequestAsync_WithNonExistentRequest_ShouldThrowException()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        _mockRequestRepository.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync((Request?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.MoveRequestAsync(requestId, Guid.NewGuid(), 0));
    }

    [Fact]
    public async Task MoveRequestAsync_ShouldReorderSiblings()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        var request1 = new RestRequest { Id = requestId, Name = "Request 1", CollectionId = collectionId, Order = 0 };
        var request2 = new RestRequest { Id = Guid.NewGuid(), Name = "Request 2", CollectionId = collectionId, Order = 1 };
        var request3 = new RestRequest { Id = Guid.NewGuid(), Name = "Request 3", CollectionId = collectionId, Order = 2 };

        _mockRequestRepository.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request1);
        _mockRequestRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Request> { request1, request2, request3 });
        _mockRequestRepository.Setup(r => r.UpdateAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        await _service.MoveRequestAsync(requestId, collectionId, 10);

        // Assert - Siblings should be reordered with sequential order values
        _mockRequestRepository.Verify(r => r.UpdateAsync(It.Is<Request>(req => 
            req.CollectionId == collectionId)), Times.AtLeast(3));
    }
}
