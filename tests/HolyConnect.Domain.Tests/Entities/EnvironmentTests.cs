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
        environment.UpdatedAt = now;

        // Assert
        Assert.Equal(now, environment.CreatedAt);
        Assert.Equal(now, environment.UpdatedAt);
    }
}
