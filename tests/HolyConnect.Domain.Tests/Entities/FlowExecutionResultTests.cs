using HolyConnect.Domain.Entities;

namespace HolyConnect.Domain.Tests.Entities;

public class FlowExecutionResultTests
{
    [Fact]
    public void FlowExecutionResult_ShouldInitializeWithEmptyStepResults()
    {
        // Arrange & Act
        var result = new FlowExecutionResult();

        // Assert
        Assert.NotNull(result.StepResults);
        Assert.Empty(result.StepResults);
    }

    [Fact]
    public void FlowExecutionResult_TotalDurationMs_ShouldCalculateCorrectly()
    {
        // Arrange
        var startedAt = DateTime.UtcNow;
        var completedAt = startedAt.AddSeconds(5);

        var result = new FlowExecutionResult
        {
            StartedAt = startedAt,
            CompletedAt = completedAt
        };

        // Act
        var duration = result.TotalDurationMs;

        // Assert
        Assert.True(duration >= 5000 && duration <= 5100); // Allow some tolerance
    }

    [Fact]
    public void FlowExecutionResult_TotalDurationMs_ShouldBeZeroWhenNotCompleted()
    {
        // Arrange
        var result = new FlowExecutionResult
        {
            StartedAt = DateTime.UtcNow,
            CompletedAt = null
        };

        // Act
        var duration = result.TotalDurationMs;

        // Assert
        Assert.Equal(0, duration);
    }

    [Fact]
    public void FlowStepResult_DurationMs_ShouldCalculateCorrectly()
    {
        // Arrange
        var startedAt = DateTime.UtcNow;
        var completedAt = startedAt.AddSeconds(2);

        var stepResult = new FlowStepResult
        {
            StartedAt = startedAt,
            CompletedAt = completedAt
        };

        // Act
        var duration = stepResult.DurationMs;

        // Assert
        Assert.True(duration >= 2000 && duration <= 2100); // Allow some tolerance
    }

    [Fact]
    public void FlowStepResult_DurationMs_ShouldBeZeroWhenNotCompleted()
    {
        // Arrange
        var stepResult = new FlowStepResult
        {
            StartedAt = DateTime.UtcNow,
            CompletedAt = null
        };

        // Act
        var duration = stepResult.DurationMs;

        // Assert
        Assert.Equal(0, duration);
    }

    [Theory]
    [InlineData(FlowExecutionStatus.Running)]
    [InlineData(FlowExecutionStatus.Completed)]
    [InlineData(FlowExecutionStatus.Failed)]
    [InlineData(FlowExecutionStatus.Cancelled)]
    public void FlowExecutionResult_Status_ShouldAcceptAllStatuses(FlowExecutionStatus status)
    {
        // Arrange & Act
        var result = new FlowExecutionResult { Status = status };

        // Assert
        Assert.Equal(status, result.Status);
    }

    [Theory]
    [InlineData(FlowStepStatus.Pending)]
    [InlineData(FlowStepStatus.Running)]
    [InlineData(FlowStepStatus.Success)]
    [InlineData(FlowStepStatus.FailedContinued)]
    [InlineData(FlowStepStatus.Failed)]
    [InlineData(FlowStepStatus.Skipped)]
    public void FlowStepResult_Status_ShouldAcceptAllStatuses(FlowStepStatus status)
    {
        // Arrange & Act
        var stepResult = new FlowStepResult { Status = status };

        // Assert
        Assert.Equal(status, stepResult.Status);
    }
}
