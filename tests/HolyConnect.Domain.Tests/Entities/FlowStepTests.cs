using HolyConnect.Domain.Entities;

namespace HolyConnect.Domain.Tests.Entities;

public class FlowStepTests
{
    [Fact]
    public void FlowStep_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var step = new FlowStep();

        // Assert
        Assert.True(step.IsEnabled);
        Assert.False(step.ContinueOnError);
        Assert.Null(step.DelayBeforeExecutionMs);
    }

    [Fact]
    public void FlowStep_ShouldHaveCorrectProperties()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var flowId = Guid.NewGuid();

        // Act
        var step = new FlowStep
        {
            Id = stepId,
            Order = 1,
            RequestId = requestId,
            FlowId = flowId,
            IsEnabled = false,
            ContinueOnError = true,
            DelayBeforeExecutionMs = 1000
        };

        // Assert
        Assert.Equal(stepId, step.Id);
        Assert.Equal(1, step.Order);
        Assert.Equal(requestId, step.RequestId);
        Assert.Equal(flowId, step.FlowId);
        Assert.False(step.IsEnabled);
        Assert.True(step.ContinueOnError);
        Assert.Equal(1000, step.DelayBeforeExecutionMs);
    }

    [Fact]
    public void FlowStep_ContinueOnError_ShouldBeConfigurable()
    {
        // Arrange
        var step = new FlowStep();

        // Act
        step.ContinueOnError = true;

        // Assert
        Assert.True(step.ContinueOnError);
    }

    [Fact]
    public void FlowStep_IsEnabled_ShouldBeConfigurable()
    {
        // Arrange
        var step = new FlowStep { IsEnabled = true };

        // Act
        step.IsEnabled = false;

        // Assert
        Assert.False(step.IsEnabled);
    }
}
