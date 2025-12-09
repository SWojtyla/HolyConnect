using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services;
using Xunit;

namespace HolyConnect.Infrastructure.Tests.Services;

public class DataGeneratorServiceTests
{
    private readonly DataGeneratorService _service;

    public DataGeneratorServiceTests()
    {
        _service = new DataGeneratorService();
    }

    [Fact]
    public void GenerateValue_FirstName_ShouldReturnNonEmptyString()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "firstName",
            GeneratorType = DataGeneratorType.FirstName
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GenerateValue_Email_ShouldReturnValidEmailFormat()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "email",
            GeneratorType = DataGeneratorType.Email
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("@", result);
    }

    [Fact]
    public void GenerateValue_Guid_ShouldReturnValidGuid()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "id",
            GeneratorType = DataGeneratorType.Guid
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(Guid.TryParse(result, out _));
    }

    [Fact]
    public void GenerateValue_Integer_WithoutConstraints_ShouldReturnInteger()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "count",
            GeneratorType = DataGeneratorType.Integer
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(int.TryParse(result, out _));
    }

    [Fact]
    public void GenerateValue_Integer_WithMinMaxConstraints_ShouldBeInRange()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "count",
            GeneratorType = DataGeneratorType.Integer,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.Minimum, Value = "1" },
                new() { Type = ConstraintType.Maximum, Value = "10" }
            }
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(int.TryParse(result, out var value));
        Assert.InRange(value, 1, 10);
    }

    [Fact]
    public void GenerateValue_Date_WithMinimumAgeConstraint_ShouldGenerateValidDate()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "birthdate",
            GeneratorType = DataGeneratorType.Date,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.MinimumAge, Value = "18" }
            }
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(DateTime.TryParse(result, out var date));
        var age = DateTime.Today.Year - date.Year - (DateTime.Today.DayOfYear < date.DayOfYear ? 1 : 0);
        Assert.True(age >= 18);
    }

    [Fact]
    public void GenerateValue_Date_WithMaximumAgeConstraint_ShouldGenerateValidDate()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "birthdate",
            GeneratorType = DataGeneratorType.Date,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.MaximumAge, Value = "65" }
            }
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(DateTime.TryParse(result, out var date));
        var age = DateTime.Today.Year - date.Year - (DateTime.Today.DayOfYear < date.DayOfYear ? 1 : 0);
        Assert.True(age <= 65);
    }

    [Fact]
    public void GenerateValue_Date_WithBothAgeConstraints_ShouldGenerateValidDate()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "birthdate",
            GeneratorType = DataGeneratorType.Date,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.MinimumAge, Value = "18" },
                new() { Type = ConstraintType.MaximumAge, Value = "65" }
            }
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(DateTime.TryParse(result, out var date));
        var age = DateTime.Today.Year - date.Year - (DateTime.Today.DayOfYear < date.DayOfYear ? 1 : 0);
        Assert.InRange(age, 18, 65);
    }

    [Fact]
    public void GenerateValue_Boolean_ShouldReturnTrueOrFalse()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "isActive",
            GeneratorType = DataGeneratorType.Boolean
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(result == "true" || result == "false");
    }

    [Fact]
    public void ValidateConfiguration_ValidConfiguration_ShouldReturnTrue()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "count",
            GeneratorType = DataGeneratorType.Integer,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.Minimum, Value = "1" },
                new() { Type = ConstraintType.Maximum, Value = "10" }
            }
        };

        // Act
        var result = _service.ValidateConfiguration(dynamicVariable);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateConfiguration_EmptyName_ShouldReturnFalse()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "",
            GeneratorType = DataGeneratorType.Integer
        };

        // Act
        var result = _service.ValidateConfiguration(dynamicVariable);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateConfiguration_InvalidMinimumConstraint_ShouldReturnFalse()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "count",
            GeneratorType = DataGeneratorType.Integer,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.Minimum, Value = "not_a_number" }
            }
        };

        // Act
        var result = _service.ValidateConfiguration(dynamicVariable);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GenerateValue_MultipleCallsShouldGenerateDifferentValues()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "firstName",
            GeneratorType = DataGeneratorType.FirstName
        };

        // Act
        var results = new HashSet<string>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(_service.GenerateValue(dynamicVariable));
        }

        // Assert - At least some values should be different (probabilistic, but very likely)
        Assert.True(results.Count > 1);
    }
}
