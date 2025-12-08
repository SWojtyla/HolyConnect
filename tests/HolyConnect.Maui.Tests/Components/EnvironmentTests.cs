using HolyConnect.Domain.Entities;

namespace HolyConnect.Maui.Tests.Components;

/// <summary>
/// Tests for Environment entity used in UI components
/// </summary>
public class EnvironmentTests
{
    [Fact]
    public void Environment_CanBeCreated_WithBasicProperties()
    {
        // Arrange & Act
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Development",
            Description = "Development environment"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, environment.Id);
        Assert.Equal("Development", environment.Name);
        Assert.Equal("Development environment", environment.Description);
    }

    [Fact]
    public void Environment_InitializesEmptyVariablesDictionary()
    {
        // Arrange & Act
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };

        // Assert
        Assert.NotNull(environment.Variables);
        Assert.Empty(environment.Variables);
    }

    [Fact]
    public void Environment_SupportsAddingVariables()
    {
        // Arrange
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };

        // Act
        environment.Variables["baseUrl"] = "https://api.example.com";
        environment.Variables["apiKey"] = "secret-key-123";

        // Assert
        Assert.Equal(2, environment.Variables.Count);
        Assert.Equal("https://api.example.com", environment.Variables["baseUrl"]);
        Assert.Equal("secret-key-123", environment.Variables["apiKey"]);
    }

    [Fact]
    public void Environment_SupportsRemovingVariables()
    {
        // Arrange
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };
        environment.Variables["baseUrl"] = "https://api.example.com";
        environment.Variables["apiKey"] = "secret-key-123";

        // Act
        environment.Variables.Remove("apiKey");

        // Assert
        Assert.Single(environment.Variables);
        Assert.True(environment.Variables.ContainsKey("baseUrl"));
        Assert.False(environment.Variables.ContainsKey("apiKey"));
    }

    [Fact]
    public void Environment_SupportsUpdatingVariables()
    {
        // Arrange
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };
        environment.Variables["baseUrl"] = "https://api.example.com";

        // Act
        environment.Variables["baseUrl"] = "https://api.newdomain.com";

        // Assert
        Assert.Single(environment.Variables);
        Assert.Equal("https://api.newdomain.com", environment.Variables["baseUrl"]);
    }

    [Fact]
    public void Environment_CanCheckIfVariableExists()
    {
        // Arrange
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };
        environment.Variables["baseUrl"] = "https://api.example.com";

        // Act & Assert
        Assert.True(environment.Variables.ContainsKey("baseUrl"));
        Assert.False(environment.Variables.ContainsKey("apiKey"));
    }

    [Fact]
    public void Environment_SupportsEmptyStringValues()
    {
        // Arrange
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };

        // Act
        environment.Variables["emptyValue"] = string.Empty;

        // Assert
        Assert.Single(environment.Variables);
        Assert.Equal(string.Empty, environment.Variables["emptyValue"]);
    }

    [Fact]
    public void Environment_SupportsSpecialCharactersInVariableNames()
    {
        // Arrange
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };

        // Act
        environment.Variables["api_key"] = "value1";
        environment.Variables["api-token"] = "value2";
        environment.Variables["api.url"] = "value3";

        // Assert
        Assert.Equal(3, environment.Variables.Count);
        Assert.Equal("value1", environment.Variables["api_key"]);
        Assert.Equal("value2", environment.Variables["api-token"]);
        Assert.Equal("value3", environment.Variables["api.url"]);
    }

    [Fact]
    public void Environment_SupportsLongVariableValues()
    {
        // Arrange
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };
        var longValue = new string('a', 1000);

        // Act
        environment.Variables["longValue"] = longValue;

        // Assert
        Assert.Single(environment.Variables);
        Assert.Equal(1000, environment.Variables["longValue"].Length);
    }

    [Fact]
    public void Environment_SupportsMultilineVariableValues()
    {
        // Arrange
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };
        var multilineValue = "line1\nline2\nline3";

        // Act
        environment.Variables["multiline"] = multilineValue;

        // Assert
        Assert.Single(environment.Variables);
        Assert.Contains("\n", environment.Variables["multiline"]);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Staging")]
    [InlineData("Production")]
    [InlineData("Local")]
    [InlineData("Test")]
    public void Environment_SupportsCommonEnvironmentNames(string name)
    {
        // Arrange & Act
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        // Assert
        Assert.Equal(name, environment.Name);
    }

    [Fact]
    public void Environment_CanHaveNullDescription()
    {
        // Arrange & Act
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Description = null
        };

        // Assert
        Assert.Null(environment.Description);
    }

    [Fact]
    public void Environment_CanHaveEmptyDescription()
    {
        // Arrange & Act
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Description = string.Empty
        };

        // Assert
        Assert.Equal(string.Empty, environment.Description);
    }

    [Fact]
    public void Environment_TracksCreationDate()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        
        // Act
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            CreatedAt = DateTime.UtcNow
        };
        
        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.True(environment.CreatedAt >= beforeCreate);
        Assert.True(environment.CreatedAt <= afterCreate);
    }

    [Fact]
    public void Environment_VariableKeysAreCaseSensitive()
    {
        // Arrange
        var environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Test"
        };

        // Act
        environment.Variables["ApiKey"] = "value1";
        environment.Variables["apikey"] = "value2";
        environment.Variables["APIKEY"] = "value3";

        // Assert
        Assert.Equal(3, environment.Variables.Count);
        Assert.Equal("value1", environment.Variables["ApiKey"]);
        Assert.Equal("value2", environment.Variables["apikey"]);
        Assert.Equal("value3", environment.Variables["APIKEY"]);
    }
}
