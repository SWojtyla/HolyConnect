using HolyConnect.Infrastructure.Persistence;

namespace HolyConnect.Infrastructure.Tests.Persistence;

public class SecretVariablesRepositoryTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly SecretVariablesRepository _repository;

    public SecretVariablesRepositoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"HolyConnectTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _repository = new SecretVariablesRepository(() => _testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public async Task GetSecretsAsync_WithNoFile_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        // Act
        var result = await _repository.GetSecretsAsync("environment", entityId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SaveSecretsAsync_ShouldCreateSecretsFile()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var secrets = new Dictionary<string, string>
        {
            ["API_KEY"] = "secret123",
            ["PASSWORD"] = "pass456"
        };

        // Act
        await _repository.SaveSecretsAsync("environment", entityId, secrets);

        // Assert
        var secretsDir = Path.Combine(_testDirectory, "secrets");
        Assert.True(Directory.Exists(secretsDir));
        var files = Directory.GetFiles(secretsDir, "*.json");
        Assert.Single(files);
        Assert.Contains($"environment-{entityId}-secrets.json", files[0]);
    }

    [Fact]
    public async Task SaveAndGetSecretsAsync_ShouldRoundTrip()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var originalSecrets = new Dictionary<string, string>
        {
            ["API_KEY"] = "secret123",
            ["PASSWORD"] = "pass456"
        };

        // Act
        await _repository.SaveSecretsAsync("environment", entityId, originalSecrets);
        var retrievedSecrets = await _repository.GetSecretsAsync("environment", entityId);

        // Assert
        Assert.Equal(2, retrievedSecrets.Count);
        Assert.Equal("secret123", retrievedSecrets["API_KEY"]);
        Assert.Equal("pass456", retrievedSecrets["PASSWORD"]);
    }

    [Fact]
    public async Task SaveSecretsAsync_WithEmptyDictionary_ShouldDeleteFile()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var secrets = new Dictionary<string, string>
        {
            ["API_KEY"] = "secret123"
        };
        await _repository.SaveSecretsAsync("environment", entityId, secrets);

        // Act
        await _repository.SaveSecretsAsync("environment", entityId, new Dictionary<string, string>());

        // Assert
        var secretsDir = Path.Combine(_testDirectory, "secrets");
        var files = Directory.GetFiles(secretsDir, $"environment-{entityId}-secrets.json");
        Assert.Empty(files);
    }

    [Fact]
    public async Task DeleteSecretsAsync_ShouldRemoveFile()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var secrets = new Dictionary<string, string>
        {
            ["API_KEY"] = "secret123"
        };
        await _repository.SaveSecretsAsync("environment", entityId, secrets);

        // Act
        await _repository.DeleteSecretsAsync("environment", entityId);

        // Assert
        var secretsDir = Path.Combine(_testDirectory, "secrets");
        var files = Directory.GetFiles(secretsDir, $"environment-{entityId}-secrets.json");
        Assert.Empty(files);
    }

    [Fact]
    public async Task SaveSecretsAsync_ForCollection_ShouldUseDifferentPrefix()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var secrets = new Dictionary<string, string>
        {
            ["API_KEY"] = "secret123"
        };

        // Act
        await _repository.SaveSecretsAsync("collection", collectionId, secrets);

        // Assert
        var secretsDir = Path.Combine(_testDirectory, "secrets");
        var files = Directory.GetFiles(secretsDir, "*.json");
        Assert.Single(files);
        Assert.Contains($"collection-{collectionId}-secrets.json", files[0]);
    }

    [Fact]
    public async Task SaveSecretsAsync_ShouldOverwriteExistingSecrets()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var originalSecrets = new Dictionary<string, string>
        {
            ["API_KEY"] = "secret123"
        };
        var updatedSecrets = new Dictionary<string, string>
        {
            ["API_KEY"] = "newsecret456",
            ["PASSWORD"] = "pass789"
        };

        // Act
        await _repository.SaveSecretsAsync("environment", entityId, originalSecrets);
        await _repository.SaveSecretsAsync("environment", entityId, updatedSecrets);
        var retrievedSecrets = await _repository.GetSecretsAsync("environment", entityId);

        // Assert
        Assert.Equal(2, retrievedSecrets.Count);
        Assert.Equal("newsecret456", retrievedSecrets["API_KEY"]);
        Assert.Equal("pass789", retrievedSecrets["PASSWORD"]);
    }

    [Fact]
    public async Task GetSecretsAsync_WithCorruptedFile_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var secretsDir = Path.Combine(_testDirectory, "secrets");
        Directory.CreateDirectory(secretsDir);
        var filePath = Path.Combine(secretsDir, $"environment-{entityId}-secrets.json");
        await File.WriteAllTextAsync(filePath, "{ invalid json }");

        // Act
        var result = await _repository.GetSecretsAsync("environment", entityId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
