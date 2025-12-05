using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Services;

public class RequestServiceTests
{
    private readonly Mock<IRepository<Request>> _mockRepository;
    private readonly Mock<IRequestExecutor> _mockExecutor;
    private readonly RequestService _service;

    public RequestServiceTests()
    {
        _mockRepository = new Mock<IRepository<Request>>();
        _mockExecutor = new Mock<IRequestExecutor>();
        var executors = new List<IRequestExecutor> { _mockExecutor.Object };
        _service = new RequestService(_mockRepository.Object, executors);
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
        Assert.True(capturedRequest.UpdatedAt > DateTime.MinValue);
        // CreatedAt and UpdatedAt should be close but may differ by microseconds
        Assert.True((capturedRequest.UpdatedAt - capturedRequest.CreatedAt).TotalMilliseconds < 100);
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
    public async Task UpdateRequestAsync_ShouldUpdateRequestAndTimestamp()
    {
        // Arrange
        var originalTime = DateTime.UtcNow.AddDays(-1);
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Updated Request",
            CreatedAt = originalTime,
            UpdatedAt = originalTime
        };

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.UpdateRequestAsync(request);

        // Assert
        Assert.Equal(request.Id, result.Id);
        Assert.Equal(request.Name, result.Name);
        Assert.True(result.UpdatedAt > originalTime);
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
        var request = new RestRequest { Id = Guid.NewGuid(), Name = "Test Request" };
        var expectedResponse = new RequestResponse
        {
            StatusCode = 200,
            Body = "Success"
        };

        _mockExecutor.Setup(e => e.CanExecute(request))
            .Returns(true);
        _mockExecutor.Setup(e => e.ExecuteAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _service.ExecuteRequestAsync(request);

        // Assert
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Success", result.Body);
        _mockExecutor.Verify(e => e.CanExecute(request), Times.Once);
        _mockExecutor.Verify(e => e.ExecuteAsync(request), Times.Once);
    }

    [Fact]
    public async Task ExecuteRequestAsync_ShouldThrowException_WhenNoExecutorFound()
    {
        // Arrange
        var request = new RestRequest { Id = Guid.NewGuid(), Name = "Test Request" };

        _mockExecutor.Setup(e => e.CanExecute(request))
            .Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotSupportedException>(() => _service.ExecuteRequestAsync(request));
        Assert.Contains("No executor found", exception.Message);
        Assert.Contains(request.Type.ToString(), exception.Message);
    }
}
