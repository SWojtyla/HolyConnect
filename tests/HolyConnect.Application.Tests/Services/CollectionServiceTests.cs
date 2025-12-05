using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Services;

public class CollectionServiceTests
{
    private readonly Mock<IRepository<Collection>> _mockRepository;
    private readonly CollectionService _service;

    public CollectionServiceTests()
    {
        _mockRepository = new Mock<IRepository<Collection>>();
        _service = new CollectionService(_mockRepository.Object);
    }

    [Fact]
    public async Task CreateCollectionAsync_ShouldCreateCollectionWithCorrectProperties()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var name = "Test Collection";
        var description = "Test Description";
        Collection? capturedCollection = null;

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Collection>()))
            .Callback<Collection>(c => capturedCollection = c)
            .ReturnsAsync((Collection c) => c);

        // Act
        var result = await _service.CreateCollectionAsync(name, environmentId, null, description);

        // Assert
        Assert.NotNull(capturedCollection);
        Assert.NotEqual(Guid.Empty, capturedCollection.Id);
        Assert.Equal(environmentId, capturedCollection.EnvironmentId);
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
}
