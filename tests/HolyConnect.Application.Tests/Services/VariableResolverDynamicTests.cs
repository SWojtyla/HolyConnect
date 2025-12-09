using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;
using DomainEnvironment = HolyConnect.Domain.Entities.Environment;

namespace HolyConnect.Application.Tests.Services;

public class VariableResolverDynamicTests
{
    private readonly Mock<IDataGeneratorService> _mockDataGenerator;
    private readonly VariableResolver _variableResolver;

    public VariableResolverDynamicTests()
    {
        _mockDataGenerator = new Mock<IDataGeneratorService>();
        _variableResolver = new VariableResolver(_mockDataGenerator.Object);
    }

    [Fact]
    public void ResolveVariables_WithDynamicVariableInEnvironment_ShouldGenerateValue()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>(),
            DynamicVariables = new List<DynamicVariable>
            {
                new()
                {
                    Name = "firstName",
                    GeneratorType = DataGeneratorType.FirstName
                }
            }
        };

        _mockDataGenerator
            .Setup(g => g.GenerateValue(It.IsAny<DynamicVariable>()))
            .Returns("John");

        var input = "User: {{ firstName }}";

        // Act
        var result = _variableResolver.ResolveVariables(input, environment);

        // Assert
        Assert.Equal("User: John", result);
        _mockDataGenerator.Verify(g => g.GenerateValue(It.IsAny<DynamicVariable>()), Times.Once);
    }

    [Fact]
    public void ResolveVariables_WithDynamicVariableInCollection_ShouldGenerateValue()
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
            DynamicVariables = new List<DynamicVariable>
            {
                new()
                {
                    Name = "email",
                    GeneratorType = DataGeneratorType.Email
                }
            }
        };

        _mockDataGenerator
            .Setup(g => g.GenerateValue(It.IsAny<DynamicVariable>()))
            .Returns("test@example.com");

        var input = "Email: {{ email }}";

        // Act
        var result = _variableResolver.ResolveVariables(input, environment, collection);

        // Assert
        Assert.Equal("Email: test@example.com", result);
    }

    [Fact]
    public void ResolveVariables_WithDynamicVariableInRequest_ShouldGenerateValue()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>()
        };

        var request = new RestRequest
        {
            Name = "Test Request",
            DynamicVariables = new List<DynamicVariable>
            {
                new()
                {
                    Name = "userId",
                    GeneratorType = DataGeneratorType.Guid
                }
            }
        };

        _mockDataGenerator
            .Setup(g => g.GenerateValue(It.IsAny<DynamicVariable>()))
            .Returns("123e4567-e89b-12d3-a456-426614174000");

        var input = "UserId: {{ userId }}";

        // Act
        var result = _variableResolver.ResolveVariables(input, environment, null, request);

        // Assert
        Assert.Equal("UserId: 123e4567-e89b-12d3-a456-426614174000", result);
    }

    [Fact]
    public void ResolveVariables_StaticVariableTakesPrecedenceOverDynamic_ShouldUseStatic()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>
            {
                ["name"] = "StaticName"
            },
            DynamicVariables = new List<DynamicVariable>
            {
                new()
                {
                    Name = "name",
                    GeneratorType = DataGeneratorType.FirstName
                }
            }
        };

        _mockDataGenerator
            .Setup(g => g.GenerateValue(It.IsAny<DynamicVariable>()))
            .Returns("DynamicName");

        var input = "Name: {{ name }}";

        // Act
        var result = _variableResolver.ResolveVariables(input, environment);

        // Assert
        Assert.Equal("Name: StaticName", result);
        _mockDataGenerator.Verify(g => g.GenerateValue(It.IsAny<DynamicVariable>()), Times.Never);
    }

    [Fact]
    public void ResolveVariables_CollectionDynamicTakesPrecedenceOverEnvironmentDynamic()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            DynamicVariables = new List<DynamicVariable>
            {
                new()
                {
                    Name = "value",
                    GeneratorType = DataGeneratorType.Integer
                }
            }
        };

        var collection = new Collection
        {
            Name = "Test Collection",
            DynamicVariables = new List<DynamicVariable>
            {
                new()
                {
                    Name = "value",
                    GeneratorType = DataGeneratorType.Decimal
                }
            }
        };

        // First call should be for collection variable (takes precedence)
        _mockDataGenerator
            .Setup(g => g.GenerateValue(It.Is<DynamicVariable>(dv => dv.GeneratorType == DataGeneratorType.Decimal)))
            .Returns("99.99");

        var input = "Value: {{ value }}";

        // Act
        var result = _variableResolver.ResolveVariables(input, environment, collection);

        // Assert
        Assert.Equal("Value: 99.99", result);
    }

    [Fact]
    public void ResolveVariables_RequestDynamicTakesPrecedenceOverCollectionAndEnvironment()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            DynamicVariables = new List<DynamicVariable>
            {
                new()
                {
                    Name = "id",
                    GeneratorType = DataGeneratorType.Integer
                }
            }
        };

        var collection = new Collection
        {
            Name = "Test Collection",
            DynamicVariables = new List<DynamicVariable>
            {
                new()
                {
                    Name = "id",
                    GeneratorType = DataGeneratorType.Decimal
                }
            }
        };

        var request = new RestRequest
        {
            Name = "Test Request",
            DynamicVariables = new List<DynamicVariable>
            {
                new()
                {
                    Name = "id",
                    GeneratorType = DataGeneratorType.Guid
                }
            }
        };

        _mockDataGenerator
            .Setup(g => g.GenerateValue(It.Is<DynamicVariable>(dv => dv.GeneratorType == DataGeneratorType.Guid)))
            .Returns("request-guid");

        var input = "ID: {{ id }}";

        // Act
        var result = _variableResolver.ResolveVariables(input, environment, collection, request);

        // Assert
        Assert.Equal("ID: request-guid", result);
    }

    [Fact]
    public void ResolveVariables_WithMultipleDynamicVariables_ShouldResolveAll()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            DynamicVariables = new List<DynamicVariable>
            {
                new()
                {
                    Name = "firstName",
                    GeneratorType = DataGeneratorType.FirstName
                },
                new()
                {
                    Name = "lastName",
                    GeneratorType = DataGeneratorType.LastName
                },
                new()
                {
                    Name = "email",
                    GeneratorType = DataGeneratorType.Email
                }
            }
        };

        _mockDataGenerator
            .Setup(g => g.GenerateValue(It.Is<DynamicVariable>(dv => dv.Name == "firstName")))
            .Returns("John");
        _mockDataGenerator
            .Setup(g => g.GenerateValue(It.Is<DynamicVariable>(dv => dv.Name == "lastName")))
            .Returns("Doe");
        _mockDataGenerator
            .Setup(g => g.GenerateValue(It.Is<DynamicVariable>(dv => dv.Name == "email")))
            .Returns("john.doe@example.com");

        var input = "{{ firstName }} {{ lastName }} <{{ email }}>";

        // Act
        var result = _variableResolver.ResolveVariables(input, environment);

        // Assert
        Assert.Equal("John Doe <john.doe@example.com>", result);
    }

    [Fact]
    public void GetVariableValue_WithDynamicVariable_ShouldGenerateValue()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Name = "Test",
            DynamicVariables = new List<DynamicVariable>
            {
                new()
                {
                    Name = "randomNumber",
                    GeneratorType = DataGeneratorType.Integer
                }
            }
        };

        _mockDataGenerator
            .Setup(g => g.GenerateValue(It.IsAny<DynamicVariable>()))
            .Returns("42");

        // Act
        var result = _variableResolver.GetVariableValue("randomNumber", environment);

        // Assert
        Assert.Equal("42", result);
    }

    [Fact]
    public void ResolveVariables_WithoutDataGenerator_ShouldOnlyResolveStaticVariables()
    {
        // Arrange
        var resolverWithoutGenerator = new VariableResolver(null);
        var environment = new DomainEnvironment
        {
            Name = "Test",
            Variables = new Dictionary<string, string>
            {
                ["static"] = "StaticValue"
            },
            DynamicVariables = new List<DynamicVariable>
            {
                new()
                {
                    Name = "dynamic",
                    GeneratorType = DataGeneratorType.FirstName
                }
            }
        };

        var input = "Static: {{ static }}, Dynamic: {{ dynamic }}";

        // Act
        var result = resolverWithoutGenerator.ResolveVariables(input, environment);

        // Assert
        Assert.Equal("Static: StaticValue, Dynamic: {{ dynamic }}", result);
    }
}
