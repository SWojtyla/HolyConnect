using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using Moq;

namespace HolyConnect.Application.Tests.Services;

public class SecretVariablesServiceTests
{
    private readonly Mock<ISecretVariablesRepository> _mockRepository;
    private readonly SecretVariablesService _service;

    public SecretVariablesServiceTests()
    {
        _mockRepository = new Mock<ISecretVariablesRepository>();
        _service = new SecretVariablesService(_mockRepository.Object);
    }

    [Fact]
    public async Task GetEnvironmentSecretsAsync_ShouldCallRepository()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var expectedSecrets = new Dictionary<string, string>
        {
            ["API_KEY"] = "secret123",
            ["PASSWORD"] = "pass456"
        };
        _mockRepository.Setup(r => r.GetSecretsAsync("environment", environmentId))
            .ReturnsAsync(expectedSecrets);

        // Act
        var result = await _service.GetEnvironmentSecretsAsync(environmentId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("secret123", result["API_KEY"]);
        Assert.Equal("pass456", result["PASSWORD"]);
        _mockRepository.Verify(r => r.GetSecretsAsync("environment", environmentId), Times.Once);
    }

    [Fact]
    public async Task SaveEnvironmentSecretsAsync_ShouldCallRepository()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var secrets = new Dictionary<string, string>
        {
            ["API_KEY"] = "secret123"
        };

        // Act
        await _service.SaveEnvironmentSecretsAsync(environmentId, secrets);

        // Assert
        _mockRepository.Verify(r => r.SaveSecretsAsync("environment", environmentId, secrets), Times.Once);
    }

    [Fact]
    public async Task DeleteEnvironmentSecretsAsync_ShouldCallRepository()
    {
        // Arrange
        var environmentId = Guid.NewGuid();

        // Act
        await _service.DeleteEnvironmentSecretsAsync(environmentId);

        // Assert
        _mockRepository.Verify(r => r.DeleteSecretsAsync("environment", environmentId), Times.Once);
    }

    [Fact]
    public async Task GetCollectionSecretsAsync_ShouldCallRepository()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var expectedSecrets = new Dictionary<string, string>
        {
            ["API_KEY"] = "secret123"
        };
        _mockRepository.Setup(r => r.GetSecretsAsync("collection", collectionId))
            .ReturnsAsync(expectedSecrets);

        // Act
        var result = await _service.GetCollectionSecretsAsync(collectionId);

        // Assert
        Assert.Single(result);
        Assert.Equal("secret123", result["API_KEY"]);
        _mockRepository.Verify(r => r.GetSecretsAsync("collection", collectionId), Times.Once);
    }

    [Fact]
    public async Task SaveCollectionSecretsAsync_ShouldCallRepository()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var secrets = new Dictionary<string, string>
        {
            ["API_KEY"] = "secret123"
        };

        // Act
        await _service.SaveCollectionSecretsAsync(collectionId, secrets);

        // Assert
        _mockRepository.Verify(r => r.SaveSecretsAsync("collection", collectionId, secrets), Times.Once);
    }

    [Fact]
    public async Task DeleteCollectionSecretsAsync_ShouldCallRepository()
    {
        // Arrange
        var collectionId = Guid.NewGuid();

        // Act
        await _service.DeleteCollectionSecretsAsync(collectionId);

        // Assert
        _mockRepository.Verify(r => r.DeleteSecretsAsync("collection", collectionId), Times.Once);
    }
}
