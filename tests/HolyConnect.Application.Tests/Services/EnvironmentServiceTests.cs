using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;
using DomainEnvironment = HolyConnect.Domain.Entities.Environment;

namespace HolyConnect.Application.Tests.Services;

public class EnvironmentServiceTests
{
    private readonly Mock<IRepository<DomainEnvironment>> _mockRepository;
    private readonly EnvironmentService _service;

    public EnvironmentServiceTests()
    {
        _mockRepository = new Mock<IRepository<DomainEnvironment>>();
        _service = new EnvironmentService(_mockRepository.Object);
    }

    [Fact]
    public async Task CreateEnvironmentAsync_ShouldCreateEnvironmentWithCorrectProperties()
    {
        // Arrange
        var name = "Test Environment";
        var description = "Test Description";
        DomainEnvironment? capturedEnvironment = null;

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<DomainEnvironment>()))
            .Callback<DomainEnvironment>(e => capturedEnvironment = e)
            .ReturnsAsync((DomainEnvironment e) => e);

        // Act
        var result = await _service.CreateEnvironmentAsync(name, description);

        // Assert
        Assert.NotNull(capturedEnvironment);
        Assert.NotEqual(Guid.Empty, capturedEnvironment.Id);
        Assert.Equal(name, capturedEnvironment.Name);
        Assert.Equal(description, capturedEnvironment.Description);
        Assert.True(capturedEnvironment.CreatedAt > DateTime.MinValue);
        Assert.True(capturedEnvironment.UpdatedAt > DateTime.MinValue);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<DomainEnvironment>()), Times.Once);
    }

    [Fact]
    public async Task CreateEnvironmentAsync_WithoutDescription_ShouldCreateEnvironment()
    {
        // Arrange
        var name = "Test Environment";

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<DomainEnvironment>()))
            .ReturnsAsync((DomainEnvironment e) => e);

        // Act
        var result = await _service.CreateEnvironmentAsync(name);

        // Assert
        Assert.Equal(name, result.Name);
        Assert.Null(result.Description);
    }

    [Fact]
    public async Task GetAllEnvironmentsAsync_ShouldReturnAllEnvironments()
    {
        // Arrange
        var environments = new List<DomainEnvironment>
        {
            new() { Id = Guid.NewGuid(), Name = "Dev" },
            new() { Id = Guid.NewGuid(), Name = "Prod" }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(environments);

        // Act
        var result = await _service.GetAllEnvironmentsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetEnvironmentByIdAsync_ShouldReturnEnvironment()
    {
        // Arrange
        var id = Guid.NewGuid();
        var environment = new DomainEnvironment { Id = id, Name = "Test" };

        _mockRepository.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(environment);

        // Act
        var result = await _service.GetEnvironmentByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task UpdateEnvironmentAsync_ShouldUpdateTimestamp()
    {
        // Arrange
        var oldTimestamp = DateTime.UtcNow.AddDays(-1);
        var environment = new DomainEnvironment
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            UpdatedAt = oldTimestamp
        };

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<DomainEnvironment>()))
            .ReturnsAsync((DomainEnvironment e) => e);

        // Act
        var result = await _service.UpdateEnvironmentAsync(environment);

        // Assert
        Assert.True(result.UpdatedAt >= oldTimestamp);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<DomainEnvironment>()), Times.Once);
    }

    [Fact]
    public async Task DeleteEnvironmentAsync_ShouldCallRepository()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockRepository.Setup(r => r.DeleteAsync(id))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteEnvironmentAsync(id);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
