using HolyConnect.Domain.Entities;
using DomainEnvironment = HolyConnect.Domain.Entities.Environment;

namespace HolyConnect.Domain.Tests.Entities;

public class EnvironmentTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange & Act
        var environment = new DomainEnvironment
        {
            Id = Guid.NewGuid(),
            Name = "Test Environment",
            Description = "Test Description"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, environment.Id);
        Assert.Equal("Test Environment", environment.Name);
        Assert.Equal("Test Description", environment.Description);
        Assert.NotNull(environment.Variables);
        Assert.Empty(environment.Variables);
        Assert.NotNull(environment.Collections);
        Assert.Empty(environment.Collections);
    }

    [Fact]
    public void Variables_ShouldBeModifiable()
    {
        // Arrange
        var environment = new DomainEnvironment { Name = "Test" };

        // Act
        environment.Variables["API_URL"] = "https://api.example.com";
        environment.Variables["API_KEY"] = "secret";

        // Assert
        Assert.Equal(2, environment.Variables.Count);
        Assert.Equal("https://api.example.com", environment.Variables["API_URL"]);
        Assert.Equal("secret", environment.Variables["API_KEY"]);
    }

    [Fact]
    public void Collections_ShouldBeModifiable()
    {
        // Arrange
        var environment = new DomainEnvironment { Name = "Test" };
        var collection = new Collection { Name = "Test Collection" };

        // Act
        environment.Collections.Add(collection);

        // Assert
        Assert.Single(environment.Collections);
        Assert.Equal("Test Collection", environment.Collections[0].Name);
    }

    [Fact]
    public void Timestamps_ShouldBeSettable()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var environment = new DomainEnvironment { Name = "Test" };

        // Act
        environment.CreatedAt = now;

        // Assert
        Assert.Equal(now, environment.CreatedAt);
    }

    [Fact]
    public void Requests_ShouldBeInitializedAsEmptyList()
    {
        // Arrange & Act
        var environment = new DomainEnvironment { Name = "Test" };

        // Assert
        Assert.NotNull(environment.Requests);
        Assert.Empty(environment.Requests);
    }

    [Fact]
    public void Requests_ShouldBeModifiable()
    {
        // Arrange
        var environment = new DomainEnvironment { Name = "Test" };
        var request = new RestRequest { Name = "Test Request" };

        // Act
        environment.Requests.Add(request);

        // Assert
        Assert.Single(environment.Requests);
        Assert.Equal("Test Request", environment.Requests[0].Name);
    }

    [Fact]
    public void SecretVariableNames_ShouldBeInitializedAsEmptySet()
    {
        // Arrange & Act
        var environment = new DomainEnvironment { Name = "Test" };

        // Assert
        Assert.NotNull(environment.SecretVariableNames);
        Assert.Empty(environment.SecretVariableNames);
    }

    [Fact]
    public void SecretVariableNames_ShouldBeModifiable()
    {
        // Arrange
        var environment = new DomainEnvironment { Name = "Test" };

        // Act
        environment.SecretVariableNames.Add("API_KEY");
        environment.SecretVariableNames.Add("PASSWORD");

        // Assert
        Assert.Equal(2, environment.SecretVariableNames.Count);
        Assert.Contains("API_KEY", environment.SecretVariableNames);
        Assert.Contains("PASSWORD", environment.SecretVariableNames);
    }

    [Fact]
    public void SecretVariableNames_ShouldNotAllowDuplicates()
    {
        // Arrange
        var environment = new DomainEnvironment { Name = "Test" };

        // Act
        environment.SecretVariableNames.Add("API_KEY");
        environment.SecretVariableNames.Add("API_KEY"); // Duplicate

        // Assert
        Assert.Single(environment.SecretVariableNames);
    }
}
