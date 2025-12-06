using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using DomainEnvironment = HolyConnect.Domain.Entities.Environment;

namespace HolyConnect.Application.Tests.Services;

public class VariableResolverTests
{
    private readonly VariableResolver _variableResolver;

    public VariableResolverTests()
    {
        _variableResolver = new VariableResolver();
    }

    [Fact]
    public void ResolveVariables_WithEnvironmentVariable_ShouldResolve()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>
            {
                ["API_URL"] = "https://api.example.com"
            }
        };
        var input = "{{ API_URL }}/users";

        // Act
        var result = _variableResolver.ResolveVariables(input, environment);

        // Assert
        Assert.Equal("https://api.example.com/users", result);
    }

    [Fact]
    public void ResolveVariables_WithCollectionVariable_ShouldResolve()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>
            {
                ["API_URL"] = "https://api.example.com"
            }
        };
        var collection = new Collection
        {
            Name = "Test Collection",
            Variables = new Dictionary<string, string>
            {
                ["API_KEY"] = "secret123"
            }
        };
        var input = "{{ API_URL }}/users?key={{ API_KEY }}";

        // Act
        var result = _variableResolver.ResolveVariables(input, environment, collection);

        // Assert
        Assert.Equal("https://api.example.com/users?key=secret123", result);
    }

    [Fact]
    public void ResolveVariables_CollectionVariableOverridesEnvironment_ShouldUseCollectionValue()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>
            {
                ["API_URL"] = "https://api.example.com"
            }
        };
        var collection = new Collection
        {
            Name = "Test Collection",
            Variables = new Dictionary<string, string>
            {
                ["API_URL"] = "https://api.override.com"
            }
        };
        var input = "{{ API_URL }}/users";

        // Act
        var result = _variableResolver.ResolveVariables(input, environment, collection);

        // Assert
        Assert.Equal("https://api.override.com/users", result);
    }

    [Fact]
    public void ResolveVariables_WithMissingVariable_ShouldKeepOriginal()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>()
        };
        var input = "{{ MISSING_VAR }}/users";

        // Act
        var result = _variableResolver.ResolveVariables(input, environment);

        // Assert
        Assert.Equal("{{ MISSING_VAR }}/users", result);
    }

    [Fact]
    public void ResolveVariables_WithMultipleVariables_ShouldResolveAll()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>
            {
                ["API_URL"] = "https://api.example.com",
                ["VERSION"] = "v1",
                ["RESOURCE"] = "users"
            }
        };
        var input = "{{ API_URL }}/{{ VERSION }}/{{ RESOURCE }}";

        // Act
        var result = _variableResolver.ResolveVariables(input, environment);

        // Assert
        Assert.Equal("https://api.example.com/v1/users", result);
    }

    [Fact]
    public void ResolveVariables_WithWhitespace_ShouldHandleCorrectly()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>
            {
                ["API_URL"] = "https://api.example.com"
            }
        };
        var input = "{{  API_URL  }}/users";

        // Act
        var result = _variableResolver.ResolveVariables(input, environment);

        // Assert
        Assert.Equal("https://api.example.com/users", result);
    }

    [Fact]
    public void ResolveVariables_WithEmptyInput_ShouldReturnEmpty()
    {
        // Arrange
        var environment = new DomainEnvironment { Name = "Test" };
        var input = "";

        // Act
        var result = _variableResolver.ResolveVariables(input, environment);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void ContainsVariables_WithVariables_ShouldReturnTrue()
    {
        // Arrange
        var input = "{{ API_URL }}/users";

        // Act
        var result = _variableResolver.ContainsVariables(input);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ContainsVariables_WithoutVariables_ShouldReturnFalse()
    {
        // Arrange
        var input = "https://api.example.com/users";

        // Act
        var result = _variableResolver.ContainsVariables(input);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ContainsVariables_WithEmptyInput_ShouldReturnFalse()
    {
        // Arrange
        var input = "";

        // Act
        var result = _variableResolver.ContainsVariables(input);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ExtractVariableNames_WithMultipleVariables_ShouldExtractAll()
    {
        // Arrange
        var input = "{{ API_URL }}/{{ VERSION }}/{{ RESOURCE }}";

        // Act
        var result = _variableResolver.ExtractVariableNames(input).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("API_URL", result);
        Assert.Contains("VERSION", result);
        Assert.Contains("RESOURCE", result);
    }

    [Fact]
    public void ExtractVariableNames_WithDuplicateVariables_ShouldReturnUnique()
    {
        // Arrange
        var input = "{{ API_URL }}/{{ API_URL }}";

        // Act
        var result = _variableResolver.ExtractVariableNames(input).ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains("API_URL", result);
    }

    [Fact]
    public void ExtractVariableNames_WithEmptyInput_ShouldReturnEmpty()
    {
        // Arrange
        var input = "";

        // Act
        var result = _variableResolver.ExtractVariableNames(input).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetVariableValue_FromEnvironment_ShouldReturnValue()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>
            {
                ["API_URL"] = "https://api.example.com"
            }
        };

        // Act
        var result = _variableResolver.GetVariableValue("API_URL", environment);

        // Assert
        Assert.Equal("https://api.example.com", result);
    }

    [Fact]
    public void GetVariableValue_FromCollection_ShouldReturnValue()
    {
        // Arrange
        var environment = new DomainEnvironment { Name = "Test" };
        var collection = new Collection
        {
            Name = "Test Collection",
            Variables = new Dictionary<string, string>
            {
                ["API_KEY"] = "secret123"
            }
        };

        // Act
        var result = _variableResolver.GetVariableValue("API_KEY", environment, collection);

        // Assert
        Assert.Equal("secret123", result);
    }

    [Fact]
    public void GetVariableValue_CollectionOverridesEnvironment_ShouldReturnCollectionValue()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>
            {
                ["API_URL"] = "https://api.example.com"
            }
        };
        var collection = new Collection
        {
            Name = "Test Collection",
            Variables = new Dictionary<string, string>
            {
                ["API_URL"] = "https://api.override.com"
            }
        };

        // Act
        var result = _variableResolver.GetVariableValue("API_URL", environment, collection);

        // Assert
        Assert.Equal("https://api.override.com", result);
    }

    [Fact]
    public void GetVariableValue_MissingVariable_ShouldReturnNull()
    {
        // Arrange
        var environment = new DomainEnvironment { Name = "Test" };

        // Act
        var result = _variableResolver.GetVariableValue("MISSING_VAR", environment);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SetVariableValue_ToEnvironment_ShouldSetVariable()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>()
        };

        // Act
        _variableResolver.SetVariableValue("NEW_VAR", "test_value", environment);

        // Assert
        Assert.Equal("test_value", environment.Variables["NEW_VAR"]);
    }

    [Fact]
    public void SetVariableValue_ToEnvironment_ShouldOverwriteExisting()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>
            {
                ["EXISTING_VAR"] = "old_value"
            }
        };

        // Act
        _variableResolver.SetVariableValue("EXISTING_VAR", "new_value", environment);

        // Assert
        Assert.Equal("new_value", environment.Variables["EXISTING_VAR"]);
    }

    [Fact]
    public void SetVariableValue_ToCollection_ShouldSetInCollection()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>()
        };
        var collection = new Collection
        {
            Name = "Test Collection",
            Variables = new Dictionary<string, string>()
        };

        // Act
        _variableResolver.SetVariableValue("COLL_VAR", "coll_value", environment, collection, saveToCollection: true);

        // Assert
        Assert.Equal("coll_value", collection.Variables["COLL_VAR"]);
        Assert.False(environment.Variables.ContainsKey("COLL_VAR"));
    }

    [Fact]
    public void SetVariableValue_ToCollectionFalse_ShouldSetInEnvironment()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>()
        };
        var collection = new Collection
        {
            Name = "Test Collection",
            Variables = new Dictionary<string, string>()
        };

        // Act
        _variableResolver.SetVariableValue("ENV_VAR", "env_value", environment, collection, saveToCollection: false);

        // Assert
        Assert.Equal("env_value", environment.Variables["ENV_VAR"]);
        Assert.False(collection.Variables.ContainsKey("ENV_VAR"));
    }
}
