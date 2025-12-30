using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Services;

public class RequestServiceTests
{
    private readonly Mock<IRepository<Request>> _mockRepository;
    private readonly Mock<IActiveEnvironmentService> _mockActiveEnvironmentService;
    private readonly Mock<IEnvironmentService> _mockEnvironmentService;
    private readonly Mock<ICollectionService> _mockCollectionService;
    private readonly Mock<IRepository<Collection>> _mockCollectionRepository;
    private readonly Mock<IRepository<Domain.Entities.Environment>> _mockEnvironmentRepository;
    private readonly Mock<IRepository<Flow>> _mockFlowRepository;
    private readonly Mock<IRepository<RequestHistoryEntry>> _mockHistoryRepository;
    private readonly Mock<IRequestExecutor> _mockExecutor;
    private readonly Mock<IRequestExecutorFactory> _mockExecutorFactory;
    private readonly Mock<IVariableResolver> _mockVariableResolver;
    private readonly RequestService _service;

    public RequestServiceTests()
    {
        _mockRepository = new Mock<IRepository<Request>>();
        _mockActiveEnvironmentService = new Mock<IActiveEnvironmentService>();
        _mockEnvironmentService = new Mock<IEnvironmentService>();
        _mockCollectionService = new Mock<ICollectionService>();
        _mockCollectionRepository = new Mock<IRepository<Collection>>();
        _mockEnvironmentRepository = new Mock<IRepository<Domain.Entities.Environment>>();
        _mockFlowRepository = new Mock<IRepository<Flow>>();
        _mockHistoryRepository = new Mock<IRepository<RequestHistoryEntry>>();
        _mockExecutor = new Mock<IRequestExecutor>();
        _mockExecutorFactory = new Mock<IRequestExecutorFactory>();
        _mockVariableResolver = new Mock<IVariableResolver>();
        
        // Setup factory to return mock executor
        _mockExecutorFactory.Setup(f => f.GetExecutor(It.IsAny<Request>()))
            .Returns(_mockExecutor.Object);

        var repositories = new RepositoryAccessor(
            _mockRepository.Object,
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
    public async Task CreateRequestAsync_ShouldCreateRequestWithCorrectProperties()
    {
        // Arrange
        var request = new RestRequest
        {
            Name = "Test Request",
            Url = "https://api.example.com/test",
            CollectionId = Guid.NewGuid()
        };
        Request? capturedRequest = null;

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.CreateRequestAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotEqual(Guid.Empty, capturedRequest.Id);
        Assert.True(capturedRequest.CreatedAt > DateTime.MinValue);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public async Task GetAllRequestsAsync_ShouldReturnAllRequests()
    {
        // Arrange
        var requests = new List<Request>
        {
            new RestRequest { Id = Guid.NewGuid(), Name = "Request 1" },
            new RestRequest { Id = Guid.NewGuid(), Name = "Request 2" },
            new GraphQLRequest { Id = Guid.NewGuid(), Name = "Request 3" }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(requests);

        // Act
        var result = await _service.GetAllRequestsAsync();

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetRequestByIdAsync_ShouldReturnRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new RestRequest { Id = id, Name = "Test Request" };

        _mockRepository.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(request);

        // Act
        var result = await _service.GetRequestByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Test Request", result.Name);
    }

    [Fact]
    public async Task GetRequestsByCollectionIdAsync_ShouldReturnRequestsForCollection()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var requests = new List<Request>
        {
            new RestRequest { Id = Guid.NewGuid(), Name = "Request 1", CollectionId = collectionId },
            new RestRequest { Id = Guid.NewGuid(), Name = "Request 2", CollectionId = collectionId },
            new RestRequest { Id = Guid.NewGuid(), Name = "Request 3", CollectionId = Guid.NewGuid() }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(requests);

        // Act
        var result = await _service.GetRequestsByCollectionIdAsync(collectionId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, r => Assert.Equal(collectionId, r.CollectionId));
    }

    [Fact]
    public async Task UpdateRequestAsync_ShouldUpdateRequest()
    {
        // Arrange
        var originalTime = DateTime.UtcNow.AddDays(-1);
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Updated Request",
            CreatedAt = originalTime
        };

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.UpdateRequestAsync(request);

        // Assert
        Assert.Equal(request.Id, result.Id);
        Assert.Equal(request.Name, result.Name);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRequestAsync_ShouldCallRepository()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockRepository.Setup(r => r.DeleteAsync(id))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteRequestAsync(id);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task ExecuteRequestAsync_ShouldUseCorrectExecutor()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var environment = new Domain.Entities.Environment
        {
            Id = environmentId,
            Name = "Test",
            Variables = new Dictionary<string, string>()
        };
        var request = new RestRequest 
        { 
            Id = Guid.NewGuid(), 
            Name = "Test Request",            Url = "https://api.example.com"
        };
        var expectedResponse = new RequestResponse
        {
            StatusCode = 200,
            Body = "Success"
        };

        _mockActiveEnvironmentService.Setup(s => s.GetActiveEnvironmentIdAsync())
            .ReturnsAsync(environmentId);
        
        _mockEnvironmentService.Setup(s => s.GetEnvironmentByIdAsync(environmentId))
            .ReturnsAsync(environment);
        _mockVariableResolver.Setup(v => v.ResolveVariables(It.IsAny<string>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>(), It.IsAny<Request>()))
            .Returns<string, Domain.Entities.Environment, Collection, Request>((input, env, coll, req) => input);
        _mockExecutor.Setup(e => e.ExecuteAsync(It.IsAny<Request>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.ExecuteRequestAsync(request);

        // Assert
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Success", result.Body);
        _mockExecutorFactory.Verify(f => f.GetExecutor(It.IsAny<Request>()), Times.Once);
        _mockExecutor.Verify(e => e.ExecuteAsync(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteRequestAsync_ShouldThrowException_WhenNoExecutorFound()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var environment = new Domain.Entities.Environment
        {
            Id = environmentId,
            Name = "Test",
            Variables = new Dictionary<string, string>()
        };
        var request = new RestRequest 
        { 
            Id = Guid.NewGuid(), 
            Name = "Test Request",        };

        _mockActiveEnvironmentService.Setup(s => s.GetActiveEnvironmentIdAsync())
            .ReturnsAsync(environmentId);
        
        _mockEnvironmentService.Setup(s => s.GetEnvironmentByIdAsync(environmentId))
            .ReturnsAsync(environment);
        _mockVariableResolver.Setup(v => v.ResolveVariables(It.IsAny<string>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>(), It.IsAny<Request>()))
            .Returns<string, Domain.Entities.Environment, Collection, Request>((input, env, coll, req) => input);
        
        // Setup factory to throw exception when no executor is found
        _mockExecutorFactory.Setup(f => f.GetExecutor(It.IsAny<Request>()))
            .Throws(new NotSupportedException($"No executor found for request type: {request.Type}"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotSupportedException>(() => _service.ExecuteRequestAsync(request));
        Assert.Contains("No executor found", exception.Message);
        Assert.Contains(request.Type.ToString(), exception.Message);
    }

    [Fact]
    public async Task CreateRequestAsync_ShouldCreateRequestWithoutCollection()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var request = new RestRequest
        {
            Name = "Test Request",
            Url = "https://api.example.com/test",            CollectionId = null
        };
        Request? capturedRequest = null;

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.CreateRequestAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);        Assert.Null(capturedRequest.CollectionId);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteRequestAsync_ShouldPreserveAuthenticationProperties()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var environment = new Domain.Entities.Environment
        {
            Id = environmentId,
            Name = "Test Env",
            Variables = new Dictionary<string, string>
            {
                { "token", "resolved-token-123" }
            }
        };

        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Auth Test",
            Url = "https://api.example.com/test",            AuthType = AuthenticationType.BearerToken,
            BearerToken = "{{ token }}"
        };

        Request? executedRequest = null;

        _mockActiveEnvironmentService.Setup(s => s.GetActiveEnvironmentIdAsync())
            .ReturnsAsync(environmentId);
        
        _mockEnvironmentService.Setup(s => s.GetEnvironmentByIdAsync(environmentId))
            .ReturnsAsync(environment);

        _mockExecutor.Setup(e => e.CanExecute(It.IsAny<Request>()))
            .Returns(true);

        _mockExecutor.Setup(e => e.ExecuteAsync(It.IsAny<Request>()))
            .Callback<Request>(r => executedRequest = r)
            .ReturnsAsync(new RequestResponse
            {
                StatusCode = 200,
                StatusMessage = "OK"
            });

        _mockVariableResolver.Setup(v => v.ResolveVariables(It.IsAny<string>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>(), It.IsAny<Request>()))
            .Returns((string input, Domain.Entities.Environment env, Collection col, Request req) => 
                input.Replace("{{ token }}", "resolved-token-123"));

        // Act
        await _service.ExecuteRequestAsync(request);

        // Assert
        Assert.NotNull(executedRequest);
        Assert.Equal(AuthenticationType.BearerToken, executedRequest.AuthType);
        Assert.Equal("resolved-token-123", executedRequest.BearerToken);
    }

    [Fact]
    public async Task ExecuteRequestAsync_ShouldPreserveBasicAuthProperties()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var environment = new Domain.Entities.Environment
        {
            Id = environmentId,
            Name = "Test Env",
            Variables = new Dictionary<string, string>()
        };

        var request = new GraphQLRequest
        {
            Id = Guid.NewGuid(),
            Name = "Basic Auth Test",
            Url = "https://api.example.com/graphql",            Query = "query { test }",
            AuthType = AuthenticationType.Basic,
            BasicAuthUsername = "testuser",
            BasicAuthPassword = "testpass"
        };

        Request? executedRequest = null;

        _mockActiveEnvironmentService.Setup(s => s.GetActiveEnvironmentIdAsync())
            .ReturnsAsync(environmentId);
        
        _mockEnvironmentService.Setup(s => s.GetEnvironmentByIdAsync(environmentId))
            .ReturnsAsync(environment);

        _mockExecutor.Setup(e => e.CanExecute(It.IsAny<Request>()))
            .Returns(true);

        _mockExecutor.Setup(e => e.ExecuteAsync(It.IsAny<Request>()))
            .Callback<Request>(r => executedRequest = r)
            .ReturnsAsync(new RequestResponse
            {
                StatusCode = 200,
                StatusMessage = "OK"
            });

        _mockVariableResolver.Setup(v => v.ResolveVariables(It.IsAny<string>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>(), It.IsAny<Request>()))
            .Returns((string input, Domain.Entities.Environment env, Collection col, Request req) => input);

        // Act
        await _service.ExecuteRequestAsync(request);

        // Assert
        Assert.NotNull(executedRequest);
        Assert.Equal(AuthenticationType.Basic, executedRequest.AuthType);
        Assert.Equal("testuser", executedRequest.BasicAuthUsername);
        Assert.Equal("testpass", executedRequest.BasicAuthPassword);
    }

    [Fact]
    public async Task ExecuteRequestAsync_ShouldSaveHistoryWithNavigationProperties()
    {
        // Arrange
        var mockHistoryService = new Mock<IRequestHistoryService>();
        var environmentId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        
        var environment = new Domain.Entities.Environment
        {
            Id = environmentId,
            Name = "Test Environment",
            Variables = new Dictionary<string, string>()
        };
        
        var request = new RestRequest
        {
            Id = requestId,
            Name = "Test Request",            CollectionId = collectionId,
            Url = "https://api.example.com",
            Method = Domain.Entities.HttpMethod.Get
        };
        
        var sentRequest = new SentRequest
        {
            Url = "https://api.example.com",
            Method = "GET",
            Headers = new Dictionary<string, string>(),
            QueryParameters = new Dictionary<string, string>(),
            Body = string.Empty
        };
        
        var response = new RequestResponse
        {
            StatusCode = 200,
            Body = "Success",
            SentRequest = sentRequest
        };
        
        RequestHistoryEntry? capturedHistoryEntry = null;
        
        mockHistoryService.Setup(h => h.AddHistoryEntryAsync(It.IsAny<RequestHistoryEntry>()))
            .Callback<RequestHistoryEntry>(entry => capturedHistoryEntry = entry)
            .Returns(Task.CompletedTask);
        
        var mockExecutorFactoryForHistory = new Mock<IRequestExecutorFactory>();
        mockExecutorFactoryForHistory.Setup(f => f.GetExecutor(It.IsAny<Request>()))
            .Returns(_mockExecutor.Object);

        var repositoriesForHistory = new RepositoryAccessor(
            _mockRepository.Object,
            _mockCollectionRepository.Object,
            _mockEnvironmentRepository.Object,
            _mockFlowRepository.Object,
            _mockHistoryRepository.Object);

        var executionContextForHistory = new RequestExecutionContext(
            _mockActiveEnvironmentService.Object,
            _mockVariableResolver.Object,
            mockExecutorFactoryForHistory.Object);
        
        var serviceWithHistory = new RequestService(
            repositoriesForHistory,
            executionContextForHistory,
            _mockEnvironmentService.Object,
            _mockCollectionService.Object,
            mockHistoryService.Object);
        
        _mockActiveEnvironmentService.Setup(s => s.GetActiveEnvironmentIdAsync())
            .ReturnsAsync(environmentId);
        
        _mockEnvironmentService.Setup(s => s.GetEnvironmentByIdAsync(environmentId))
            .ReturnsAsync(environment);
        _mockVariableResolver.Setup(v => v.ResolveVariables(It.IsAny<string>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>(), It.IsAny<Request>()))
            .Returns<string, Domain.Entities.Environment, Collection, Request>((input, env, coll, req) => input);
        _mockExecutor.Setup(e => e.CanExecute(It.IsAny<Request>()))
            .Returns(true);
        _mockExecutor.Setup(e => e.ExecuteAsync(It.IsAny<Request>()))
            .ReturnsAsync(response);
        
        // Act
        await serviceWithHistory.ExecuteRequestAsync(request);
        
        // Assert
        Assert.NotNull(capturedHistoryEntry);
        Assert.Equal("Test Request", capturedHistoryEntry.RequestName);
        Assert.Equal(RequestType.Rest, capturedHistoryEntry.RequestType);
        Assert.Equal(requestId, capturedHistoryEntry.RequestId);        Assert.Equal(collectionId, capturedHistoryEntry.CollectionId);
        Assert.Equal(200, capturedHistoryEntry.Response.StatusCode);
        mockHistoryService.Verify(h => h.AddHistoryEntryAsync(It.IsAny<RequestHistoryEntry>()), Times.Once);
    }

    [Fact]
    public async Task CreateRequestAsync_WithDuplicateName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new RestRequest
        {
            Name = "Duplicate Request",
            Url = "https://api.example.com",        };
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Request>()))
            .ThrowsAsync(new InvalidOperationException($"An entity with the name '{request.Name}' already exists."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateRequestAsync(request));
        Assert.Contains("already exists", exception.Message);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRequestAsync_WithDuplicateName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Duplicate Request",
            Url = "https://api.example.com",        };
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Request>()))
            .ThrowsAsync(new InvalidOperationException($"An entity with the name '{request.Name}' already exists."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateRequestAsync(request));
        Assert.Contains("already exists", exception.Message);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Request>()), Times.Once);
    }
}
