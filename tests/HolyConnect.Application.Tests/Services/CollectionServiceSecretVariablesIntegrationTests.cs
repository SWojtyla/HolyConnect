using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Services;

/// <summary>
/// Integration tests for secret variables functionality in CollectionService
/// </summary>
public class CollectionServiceSecretVariablesIntegrationTests
{
    private readonly Mock<IRepository<Collection>> _mockRepository;
    private readonly Mock<ISecretVariablesService> _mockSecretVariablesService;
    private readonly CollectionService _service;

    public CollectionServiceSecretVariablesIntegrationTests()
    {
        _mockRepository = new Mock<IRepository<Collection>>();
        _mockSecretVariablesService = new Mock<ISecretVariablesService>();
        _service = new CollectionService(_mockRepository.Object, _mockSecretVariablesService.Object);
    }

    [Fact]
    public async Task UpdateCollectionAsync_WithSecretVariables_ShouldSeparateSecretsFromNormalVariables()
    {
        // Arrange
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test Collection",
            Variables = new Dictionary<string, string>
            {
                { "BASE_URL", "https://example.com" },
                { "TOKEN", "secret-token" },
                { "API_SECRET", "secret456" }
            },
            SecretVariableNames = new HashSet<string> { "TOKEN", "API_SECRET" }
        };

        Collection? capturedCollection = null;
        Dictionary<string, string>? capturedSecrets = null;

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Collection>()))
            .Callback<Collection>(c => 
            {
                // Capture a snapshot of the variables at the time of the call
                capturedCollection = new Collection
                {
                    Id = c.Id,
                    Name = c.Name,
                    Variables = new Dictionary<string, string>(c.Variables),
                    SecretVariableNames = new HashSet<string>(c.SecretVariableNames)
                };
            })
            .ReturnsAsync((Collection c) => c);

        _mockSecretVariablesService.Setup(s => s.SaveCollectionSecretsAsync(It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>()))
            .Callback<Guid, Dictionary<string, string>>((id, secrets) => capturedSecrets = secrets)
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateCollectionAsync(collection);

        // Assert - Verify secrets were extracted
        Assert.NotNull(capturedSecrets);
        Assert.Equal(2, capturedSecrets.Count);
        Assert.True(capturedSecrets.ContainsKey("TOKEN"));
        Assert.Equal("secret-token", capturedSecrets["TOKEN"]);
        Assert.True(capturedSecrets.ContainsKey("API_SECRET"));
        Assert.Equal("secret456", capturedSecrets["API_SECRET"]);

        // Assert - Verify only non-secret variables are saved to repository
        Assert.NotNull(capturedCollection);
        Assert.Single(capturedCollection.Variables);
        Assert.True(capturedCollection.Variables.ContainsKey("BASE_URL"));
        Assert.Equal("https://example.com", capturedCollection.Variables["BASE_URL"]);
        Assert.False(capturedCollection.Variables.ContainsKey("TOKEN"));
        Assert.False(capturedCollection.Variables.ContainsKey("API_SECRET"));

        // Assert - Verify SecretVariableNames is preserved
        Assert.Equal(2, capturedCollection.SecretVariableNames.Count);
        Assert.Contains("TOKEN", capturedCollection.SecretVariableNames);
        Assert.Contains("API_SECRET", capturedCollection.SecretVariableNames);
    }

    [Fact]
    public async Task GetCollectionByIdAsync_ShouldMergeSecretsWithNormalVariables()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var collection = new Collection
        {
            Id = collectionId,
            Name = "Test Collection",
            Variables = new Dictionary<string, string>
            {
                { "BASE_URL", "https://example.com" }
            },
            SecretVariableNames = new HashSet<string> { "TOKEN" }
        };

        var secrets = new Dictionary<string, string>
        {
            { "TOKEN", "secret-token" }
        };

        _mockRepository.Setup(r => r.GetByIdAsync(collectionId))
            .ReturnsAsync(collection);

        _mockSecretVariablesService.Setup(s => s.GetCollectionSecretsAsync(collectionId))
            .ReturnsAsync(secrets);

        // Act
        var result = await _service.GetCollectionByIdAsync(collectionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Variables.Count);
        Assert.Equal("https://example.com", result.Variables["BASE_URL"]);
        Assert.Equal("secret-token", result.Variables["TOKEN"]);
    }

    [Fact]
    public async Task DeleteCollectionAsync_ShouldDeleteBothCollectionAndSecrets()
    {
        // Arrange
        var collectionId = Guid.NewGuid();

        _mockRepository.Setup(r => r.DeleteAsync(collectionId))
            .Returns(Task.CompletedTask);

        _mockSecretVariablesService.Setup(s => s.DeleteCollectionSecretsAsync(collectionId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteCollectionAsync(collectionId);

        // Assert
        _mockSecretVariablesService.Verify(s => s.DeleteCollectionSecretsAsync(collectionId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(collectionId), Times.Once);
    }

    [Fact]
    public async Task UpdateCollectionAsync_ReturnedCollection_ShouldIncludeAllVariables()
    {
        // Arrange
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test Collection",
            Variables = new Dictionary<string, string>
            {
                { "BASE_URL", "https://example.com" },
                { "TOKEN", "secret-token" }
            },
            SecretVariableNames = new HashSet<string> { "TOKEN" }
        };

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Collection>()))
            .ReturnsAsync((Collection c) => c);

        _mockSecretVariablesService.Setup(s => s.SaveCollectionSecretsAsync(It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateCollectionAsync(collection);

        // Assert - The returned object should have all variables (including secrets) restored
        Assert.Equal(2, result.Variables.Count);
        Assert.Equal("https://example.com", result.Variables["BASE_URL"]);
        Assert.Equal("secret-token", result.Variables["TOKEN"]);
    }
}
