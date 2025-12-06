using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Services;

public class RequestHistoryServiceTests
{
    private readonly Mock<IRepository<RequestHistoryEntry>> _mockRepository;
    private readonly RequestHistoryService _service;

    public RequestHistoryServiceTests()
    {
        _mockRepository = new Mock<IRepository<RequestHistoryEntry>>();
        _service = new RequestHistoryService(_mockRepository.Object);
    }

    [Fact]
    public async Task AddHistoryEntryAsync_ShouldSetIdAndTimestamp()
    {
        // Arrange
        var entry = new RequestHistoryEntry
        {
            RequestName = "Test Request",
            RequestType = RequestType.Rest,
            SentRequest = new SentRequest { Url = "https://api.example.com" },
            Response = new RequestResponse { StatusCode = 200 }
        };
        RequestHistoryEntry? capturedEntry = null;

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<RequestHistoryEntry>()))
            .Callback<RequestHistoryEntry>(e => capturedEntry = e)
            .ReturnsAsync((RequestHistoryEntry e) => e);

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<RequestHistoryEntry>());

        // Act
        await _service.AddHistoryEntryAsync(entry);

        // Assert
        Assert.NotNull(capturedEntry);
        Assert.NotEqual(Guid.Empty, capturedEntry.Id);
        Assert.NotEqual(DateTime.MinValue, capturedEntry.Timestamp);
        Assert.Equal("Test Request", capturedEntry.RequestName);
    }

    [Fact]
    public async Task AddHistoryEntryAsync_ShouldTrimHistoryWhenExceedsLimit()
    {
        // Arrange
        var existingEntries = Enumerable.Range(0, 10).Select(i => new RequestHistoryEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow.AddMinutes(-i),
            RequestName = $"Request {i}"
        }).ToList();

        var newEntry = new RequestHistoryEntry
        {
            RequestName = "New Request",
            RequestType = RequestType.Rest,
            SentRequest = new SentRequest(),
            Response = new RequestResponse()
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<RequestHistoryEntry>()))
            .ReturnsAsync((RequestHistoryEntry e) => e);

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(existingEntries.Concat(new[] { newEntry }));

        var deletedEntries = new List<Guid>();
        _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<Guid>()))
            .Callback<Guid>(id => deletedEntries.Add(id))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AddHistoryEntryAsync(newEntry);

        // Assert
        // Should delete the oldest entry when adding an 11th entry
        Assert.Single(deletedEntries);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldReturnEntriesInDescendingOrder()
    {
        // Arrange
        var entries = new List<RequestHistoryEntry>
        {
            new RequestHistoryEntry { Id = Guid.NewGuid(), Timestamp = DateTime.UtcNow.AddMinutes(-5), RequestName = "Third" },
            new RequestHistoryEntry { Id = Guid.NewGuid(), Timestamp = DateTime.UtcNow.AddMinutes(-1), RequestName = "First" },
            new RequestHistoryEntry { Id = Guid.NewGuid(), Timestamp = DateTime.UtcNow.AddMinutes(-3), RequestName = "Second" }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(entries);

        // Act
        var result = await _service.GetHistoryAsync();

        // Assert
        var resultList = result.ToList();
        Assert.Equal(3, resultList.Count);
        Assert.Equal("First", resultList[0].RequestName);
        Assert.Equal("Second", resultList[1].RequestName);
        Assert.Equal("Third", resultList[2].RequestName);
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldRespectMaxCount()
    {
        // Arrange
        var entries = Enumerable.Range(0, 15).Select(i => new RequestHistoryEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow.AddMinutes(-i),
            RequestName = $"Request {i}"
        }).ToList();

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(entries);

        // Act
        var result = await _service.GetHistoryAsync(5);

        // Assert
        Assert.Equal(5, result.Count());
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldNotExceedMaxLimit()
    {
        // Arrange
        var entries = Enumerable.Range(0, 15).Select(i => new RequestHistoryEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow.AddMinutes(-i),
            RequestName = $"Request {i}"
        }).ToList();

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(entries);

        // Act - Request 20 entries but should only return max 10
        var result = await _service.GetHistoryAsync(20);

        // Assert
        Assert.Equal(10, result.Count());
    }

    [Fact]
    public async Task ClearHistoryAsync_ShouldDeleteAllEntries()
    {
        // Arrange
        var entries = new List<RequestHistoryEntry>
        {
            new RequestHistoryEntry { Id = Guid.NewGuid(), RequestName = "First" },
            new RequestHistoryEntry { Id = Guid.NewGuid(), RequestName = "Second" },
            new RequestHistoryEntry { Id = Guid.NewGuid(), RequestName = "Third" }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(entries);

        _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ClearHistoryAsync();

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Exactly(3));
    }

    [Fact]
    public async Task AddHistoryEntryAsync_ShouldPreserveNavigationProperties()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var environmentId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        
        var entry = new RequestHistoryEntry
        {
            RequestName = "Test Request",
            RequestType = RequestType.Rest,
            SentRequest = new SentRequest { Url = "https://api.example.com" },
            Response = new RequestResponse { StatusCode = 200 },
            RequestId = requestId,
            EnvironmentId = environmentId,
            CollectionId = collectionId
        };
        RequestHistoryEntry? capturedEntry = null;

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<RequestHistoryEntry>()))
            .Callback<RequestHistoryEntry>(e => capturedEntry = e)
            .ReturnsAsync((RequestHistoryEntry e) => e);

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<RequestHistoryEntry>());

        // Act
        await _service.AddHistoryEntryAsync(entry);

        // Assert
        Assert.NotNull(capturedEntry);
        Assert.Equal(requestId, capturedEntry.RequestId);
        Assert.Equal(environmentId, capturedEntry.EnvironmentId);
        Assert.Equal(collectionId, capturedEntry.CollectionId);
    }
}
