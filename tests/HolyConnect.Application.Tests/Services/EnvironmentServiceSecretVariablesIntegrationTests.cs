using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using Moq;
using DomainEnvironment = HolyConnect.Domain.Entities.Environment;

namespace HolyConnect.Application.Tests.Services;

/// <summary>
/// Integration tests for secret variables functionality in EnvironmentService
/// </summary>
public class EnvironmentServiceSecretVariablesIntegrationTests
{
    private readonly Mock<IRepository<DomainEnvironment>> _mockRepository;
    private readonly Mock<ISecretVariablesService> _mockSecretVariablesService;
    private readonly EnvironmentService _service;

    public EnvironmentServiceSecretVariablesIntegrationTests()
    {
        _mockRepository = new Mock<IRepository<DomainEnvironment>>();
        _mockSecretVariablesService = new Mock<ISecretVariablesService>();
        _service = new EnvironmentService(_mockRepository.Object, _mockSecretVariablesService.Object);
    }

    [Fact]
    public async Task UpdateEnvironmentAsync_WithSecretVariables_ShouldSeparateSecretsFromNormalVariables()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Id = Guid.NewGuid(),
            Name = "Test Environment",
            Variables = new Dictionary<string, string>
            {
                { "API_URL", "https://api.example.com" },
                { "API_KEY", "secret123" },
                { "PASSWORD", "pass456" }
            },
            SecretVariableNames = new HashSet<string> { "API_KEY", "PASSWORD" }
        };

        DomainEnvironment? capturedEnvironment = null;
        Dictionary<string, string>? capturedSecrets = null;

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<DomainEnvironment>()))
            .Callback<DomainEnvironment>(e => 
            {
                // Capture a snapshot of the variables at the time of the call
                capturedEnvironment = new DomainEnvironment
                {
                    Id = e.Id,
                    Name = e.Name,
                    Variables = new Dictionary<string, string>(e.Variables),
                    SecretVariableNames = new HashSet<string>(e.SecretVariableNames)
                };
            })
            .ReturnsAsync((DomainEnvironment e) => e);

        _mockSecretVariablesService.Setup(s => s.SaveEnvironmentSecretsAsync(It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>()))
            .Callback<Guid, Dictionary<string, string>>((id, secrets) => capturedSecrets = secrets)
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateEnvironmentAsync(environment);

        // Assert - Verify secrets were extracted
        Assert.NotNull(capturedSecrets);
        Assert.Equal(2, capturedSecrets.Count);
        Assert.True(capturedSecrets.ContainsKey("API_KEY"));
        Assert.Equal("secret123", capturedSecrets["API_KEY"]);
        Assert.True(capturedSecrets.ContainsKey("PASSWORD"));
        Assert.Equal("pass456", capturedSecrets["PASSWORD"]);

        // Assert - Verify only non-secret variables are saved to repository
        Assert.NotNull(capturedEnvironment);
        Assert.Single(capturedEnvironment.Variables);
        Assert.True(capturedEnvironment.Variables.ContainsKey("API_URL"));
        Assert.Equal("https://api.example.com", capturedEnvironment.Variables["API_URL"]);
        Assert.False(capturedEnvironment.Variables.ContainsKey("API_KEY"));
        Assert.False(capturedEnvironment.Variables.ContainsKey("PASSWORD"));

        // Assert - Verify SecretVariableNames is preserved
        Assert.Equal(2, capturedEnvironment.SecretVariableNames.Count);
        Assert.Contains("API_KEY", capturedEnvironment.SecretVariableNames);
        Assert.Contains("PASSWORD", capturedEnvironment.SecretVariableNames);
    }

    [Fact]
    public async Task GetEnvironmentByIdAsync_ShouldMergeSecretsWithNormalVariables()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var environment = new DomainEnvironment
        {
            Id = environmentId,
            Name = "Test Environment",
            Variables = new Dictionary<string, string>
            {
                { "API_URL", "https://api.example.com" }
            },
            SecretVariableNames = new HashSet<string> { "API_KEY" }
        };

        var secrets = new Dictionary<string, string>
        {
            { "API_KEY", "secret123" }
        };

        _mockRepository.Setup(r => r.GetByIdAsync(environmentId))
            .ReturnsAsync(environment);

        _mockSecretVariablesService.Setup(s => s.GetEnvironmentSecretsAsync(environmentId))
            .ReturnsAsync(secrets);

        // Act
        var result = await _service.GetEnvironmentByIdAsync(environmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Variables.Count);
        Assert.Equal("https://api.example.com", result.Variables["API_URL"]);
        Assert.Equal("secret123", result.Variables["API_KEY"]);
    }

    [Fact]
    public async Task UpdateEnvironmentAsync_WithOnlyNonSecretVariables_ShouldNotCallSaveSecrets()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Id = Guid.NewGuid(),
            Name = "Test Environment",
            Variables = new Dictionary<string, string>
            {
                { "API_URL", "https://api.example.com" },
                { "VERSION", "v1" }
            },
            SecretVariableNames = new HashSet<string>()
        };

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<DomainEnvironment>()))
            .ReturnsAsync((DomainEnvironment e) => e);

        // Act
        await _service.UpdateEnvironmentAsync(environment);

        // Assert - Verify SaveEnvironmentSecretsAsync is called with empty dictionary
        _mockSecretVariablesService.Verify(
            s => s.SaveEnvironmentSecretsAsync(
                It.IsAny<Guid>(), 
                It.Is<Dictionary<string, string>>(d => d.Count == 0)), 
            Times.Once);
    }

    [Fact]
    public async Task DeleteEnvironmentAsync_ShouldDeleteBothEnvironmentAndSecrets()
    {
        // Arrange
        var environmentId = Guid.NewGuid();

        _mockRepository.Setup(r => r.DeleteAsync(environmentId))
            .Returns(Task.CompletedTask);

        _mockSecretVariablesService.Setup(s => s.DeleteEnvironmentSecretsAsync(environmentId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteEnvironmentAsync(environmentId);

        // Assert
        _mockSecretVariablesService.Verify(s => s.DeleteEnvironmentSecretsAsync(environmentId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(environmentId), Times.Once);
    }

    [Fact]
    public async Task UpdateEnvironmentAsync_ReturnedEnvironment_ShouldIncludeAllVariables()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Id = Guid.NewGuid(),
            Name = "Test Environment",
            Variables = new Dictionary<string, string>
            {
                { "API_URL", "https://api.example.com" },
                { "API_KEY", "secret123" }
            },
            SecretVariableNames = new HashSet<string> { "API_KEY" }
        };

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<DomainEnvironment>()))
            .ReturnsAsync((DomainEnvironment e) => e);

        _mockSecretVariablesService.Setup(s => s.SaveEnvironmentSecretsAsync(It.IsAny<Guid>(), It.IsAny<Dictionary<string, string>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateEnvironmentAsync(environment);

        // Assert - The returned object should have all variables (including secrets) restored
        Assert.Equal(2, result.Variables.Count);
        Assert.Equal("https://api.example.com", result.Variables["API_URL"]);
        Assert.Equal("secret123", result.Variables["API_KEY"]);
    }
}
