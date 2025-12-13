using HolyConnect.Application.Common;

namespace HolyConnect.Application.Tests.Common;

public class SecretVariableHelperTests
{
    [Fact]
    public void SeparateVariables_ShouldSeparateSecretAndNonSecretVariables()
    {
        // Arrange
        var variables = new Dictionary<string, string>
        {
            { "apiKey", "secret123" },
            { "baseUrl", "https://api.example.com" },
            { "token", "token456" },
            { "timeout", "30" }
        };
        var secretNames = new HashSet<string> { "apiKey", "token" };

        // Act
        var result = SecretVariableHelper.SeparateVariables(variables, secretNames);

        // Assert
        Assert.Equal(2, result.SecretVariables.Count);
        Assert.Equal(2, result.NonSecretVariables.Count);
        Assert.Equal("secret123", result.SecretVariables["apiKey"]);
        Assert.Equal("token456", result.SecretVariables["token"]);
        Assert.Equal("https://api.example.com", result.NonSecretVariables["baseUrl"]);
        Assert.Equal("30", result.NonSecretVariables["timeout"]);
    }

    [Fact]
    public void SeparateVariables_WithNoSecrets_ShouldReturnAllAsNonSecret()
    {
        // Arrange
        var variables = new Dictionary<string, string>
        {
            { "baseUrl", "https://api.example.com" },
            { "timeout", "30" }
        };
        var secretNames = new HashSet<string>();

        // Act
        var result = SecretVariableHelper.SeparateVariables(variables, secretNames);

        // Assert
        Assert.Empty(result.SecretVariables);
        Assert.Equal(2, result.NonSecretVariables.Count);
    }

    [Fact]
    public void SeparateVariables_WithAllSecrets_ShouldReturnAllAsSecret()
    {
        // Arrange
        var variables = new Dictionary<string, string>
        {
            { "apiKey", "secret123" },
            { "token", "token456" }
        };
        var secretNames = new HashSet<string> { "apiKey", "token" };

        // Act
        var result = SecretVariableHelper.SeparateVariables(variables, secretNames);

        // Assert
        Assert.Equal(2, result.SecretVariables.Count);
        Assert.Empty(result.NonSecretVariables);
    }

    [Fact]
    public void MergeSecretVariables_ShouldMergeSecretsIntoTarget()
    {
        // Arrange
        var target = new Dictionary<string, string>
        {
            { "baseUrl", "https://api.example.com" }
        };
        var secrets = new Dictionary<string, string>
        {
            { "apiKey", "secret123" },
            { "token", "token456" }
        };

        // Act
        SecretVariableHelper.MergeSecretVariables(target, secrets);

        // Assert
        Assert.Equal(3, target.Count);
        Assert.Equal("https://api.example.com", target["baseUrl"]);
        Assert.Equal("secret123", target["apiKey"]);
        Assert.Equal("token456", target["token"]);
    }

    [Fact]
    public void MergeSecretVariables_WithEmptySecrets_ShouldNotChangeTarget()
    {
        // Arrange
        var target = new Dictionary<string, string>
        {
            { "baseUrl", "https://api.example.com" }
        };
        var secrets = new Dictionary<string, string>();

        // Act
        SecretVariableHelper.MergeSecretVariables(target, secrets);

        // Assert
        Assert.Single(target);
        Assert.Equal("https://api.example.com", target["baseUrl"]);
    }

    [Fact]
    public async Task LoadAndMergeSecretsAsync_ShouldLoadAndMergeSecrets()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var target = new Dictionary<string, string>
        {
            { "baseUrl", "https://api.example.com" }
        };
        var secrets = new Dictionary<string, string>
        {
            { "apiKey", "secret123" }
        };

        Task<Dictionary<string, string>> LoadSecrets(Guid id)
        {
            Assert.Equal(entityId, id);
            return Task.FromResult(secrets);
        }

        // Act
        await SecretVariableHelper.LoadAndMergeSecretsAsync(entityId, target, LoadSecrets);

        // Assert
        Assert.Equal(2, target.Count);
        Assert.Equal("https://api.example.com", target["baseUrl"]);
        Assert.Equal("secret123", target["apiKey"]);
    }
}
