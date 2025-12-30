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

    [Fact]
    public void GenerateValue_DateTime_WithDaysOffset_ShouldGenerateCorrectDate()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "futureDate",
            GeneratorType = DataGeneratorType.DateTime,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.DaysOffset, Value = "1" }
            }
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(DateTime.TryParse(result, out var generatedDate));
        var expectedDate = DateTime.Now.AddDays(1);
        // Allow some tolerance for test execution time
        Assert.True(Math.Abs((generatedDate.Date - expectedDate.Date).TotalDays) < 1);
    }

    [Fact]
    public void GenerateValue_DateTime_WithNegativeDaysOffset_ShouldGeneratePastDate()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "pastDate",
            GeneratorType = DataGeneratorType.DateTime,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.DaysOffset, Value = "-1" }
            }
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(DateTime.TryParse(result, out var generatedDate));
        var expectedDate = DateTime.Now.AddDays(-1);
        Assert.True(Math.Abs((generatedDate.Date - expectedDate.Date).TotalDays) < 1);
    }

    [Fact]
    public void GenerateValue_DateTime_WithZeroDaysOffset_ShouldGenerateToday()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "today",
            GeneratorType = DataGeneratorType.DateTime,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.DaysOffset, Value = "0" }
            }
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(DateTime.TryParse(result, out var generatedDate));
        Assert.Equal(DateTime.Now.Date, generatedDate.Date);
    }

    [Fact]
    public void GenerateValue_DateTime_WithHoursOffset_ShouldGenerateCorrectTime()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "futureTime",
            GeneratorType = DataGeneratorType.DateTime,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.HoursOffset, Value = "2" }
            }
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(DateTime.TryParse(result, out var generatedDate));
        var expectedDate = DateTime.Now.AddHours(2);
        // Allow 1 minute tolerance for test execution
        Assert.True(Math.Abs((generatedDate - expectedDate).TotalMinutes) < 1);
    }

    [Fact]
    public void GenerateValue_DateTime_WithMinutesOffset_ShouldGenerateCorrectTime()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "futureTime",
            GeneratorType = DataGeneratorType.DateTime,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.MinutesOffset, Value = "30" }
            }
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(DateTime.TryParse(result, out var generatedDate));
        var expectedDate = DateTime.Now.AddMinutes(30);
        // Allow 1 minute tolerance for test execution
        Assert.True(Math.Abs((generatedDate - expectedDate).TotalMinutes) < 1);
    }

    [Fact]
    public void GenerateValue_DateTime_WithSecondsOffset_ShouldGenerateCorrectTime()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "futureTime",
            GeneratorType = DataGeneratorType.DateTime,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.SecondsOffset, Value = "45" }
            }
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(DateTime.TryParse(result, out var generatedDate));
        var expectedDate = DateTime.Now.AddSeconds(45);
        // Allow 1 second tolerance for test execution
        Assert.True(Math.Abs((generatedDate - expectedDate).TotalSeconds) < 1);
    }

    [Fact]
    public void GenerateValue_DateTime_WithMultipleOffsets_ShouldApplyAll()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "complexDate",
            GeneratorType = DataGeneratorType.DateTime,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.DaysOffset, Value = "1" },
                new() { Type = ConstraintType.HoursOffset, Value = "2" },
                new() { Type = ConstraintType.MinutesOffset, Value = "30" }
            }
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(DateTime.TryParse(result, out var generatedDate));
        var expectedDate = DateTime.Now.AddDays(1).AddHours(2).AddMinutes(30);
        // Allow 1 minute tolerance
        Assert.True(Math.Abs((generatedDate - expectedDate).TotalMinutes) < 1);
    }

    [Fact]
    public void GenerateValue_DateTime_WithCustomFormat_ShouldUseFormat()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "customFormatDate",
            GeneratorType = DataGeneratorType.DateTime,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.DaysOffset, Value = "0" },
                new() { Type = ConstraintType.Format, Value = "yyyy-MM-dd HH:mm" }
            }
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        // Check format matches yyyy-MM-dd HH:mm (no seconds)
        Assert.Matches(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}$", result);
    }

    [Fact]
    public void GenerateValue_Date_WithDaysOffset_ShouldGenerateDateOnly()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "dateFuture",
            GeneratorType = DataGeneratorType.Date,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.DaysOffset, Value = "7" }
            }
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(DateTime.TryParse(result, out var generatedDate));
        var expectedDate = DateTime.Now.AddDays(7);
        Assert.True(Math.Abs((generatedDate.Date - expectedDate.Date).TotalDays) < 1);
        // Check format is date-only (yyyy-MM-dd)
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}$", result);
    }

    [Fact]
    public void GenerateValue_DatePast_WithDaysOffset_ShouldGenerateOffsetDate()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "pastDateOffset",
            GeneratorType = DataGeneratorType.DatePast,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.DaysOffset, Value = "-5" }
            }
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(DateTime.TryParse(result, out var generatedDate));
        var expectedDate = DateTime.Now.AddDays(-5);
        Assert.True(Math.Abs((generatedDate.Date - expectedDate.Date).TotalDays) < 1);
    }

    [Fact]
    public void GenerateValue_DateFuture_WithDaysOffset_ShouldGenerateOffsetDate()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "futureDateOffset",
            GeneratorType = DataGeneratorType.DateFuture,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.DaysOffset, Value = "10" }
            }
        };

        // Act
        var result = _service.GenerateValue(dynamicVariable);

        // Assert
        Assert.True(DateTime.TryParse(result, out var generatedDate));
        var expectedDate = DateTime.Now.AddDays(10);
        Assert.True(Math.Abs((generatedDate.Date - expectedDate.Date).TotalDays) < 1);
    }

    [Fact]
    public void ValidateConfiguration_WithValidDaysOffset_ShouldReturnTrue()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "offsetDate",
            GeneratorType = DataGeneratorType.DateTime,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.DaysOffset, Value = "7" }
            }
        };

        // Act
        var result = _service.ValidateConfiguration(dynamicVariable);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateConfiguration_WithInvalidDaysOffset_ShouldReturnFalse()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "offsetDate",
            GeneratorType = DataGeneratorType.DateTime,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.DaysOffset, Value = "not_a_number" }
            }
        };

        // Act
        var result = _service.ValidateConfiguration(dynamicVariable);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateConfiguration_WithValidHoursOffset_ShouldReturnTrue()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "offsetTime",
            GeneratorType = DataGeneratorType.DateTime,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.HoursOffset, Value = "-2" }
            }
        };

        // Act
        var result = _service.ValidateConfiguration(dynamicVariable);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateConfiguration_WithInvalidMinutesOffset_ShouldReturnFalse()
    {
        // Arrange
        var dynamicVariable = new DynamicVariable
        {
            Name = "offsetTime",
            GeneratorType = DataGeneratorType.DateTime,
            Constraints = new List<ConstraintRule>
            {
                new() { Type = ConstraintType.MinutesOffset, Value = "invalid" }
            }
        };

        // Act
        var result = _service.ValidateConfiguration(dynamicVariable);

        // Assert
        Assert.False(result);
    }
}
