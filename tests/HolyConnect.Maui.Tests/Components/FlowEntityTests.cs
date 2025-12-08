using HolyConnect.Domain.Entities;

namespace HolyConnect.Maui.Tests.Components;

/// <summary>
/// Tests for Flow entity used in UI components
/// </summary>
public class FlowEntityTests
{
    [Fact]
    public void Flow_CanBeCreated_WithBasicProperties()
    {
        // Arrange & Act
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = "Login Flow",
            Description = "Automated login workflow"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, flow.Id);
        Assert.Equal("Login Flow", flow.Name);
        Assert.Equal("Automated login workflow", flow.Description);
    }

    [Fact]
    public void Flow_InitializesEmptyStepsList()
    {
        // Arrange & Act
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = "Test Flow"
        };

        // Assert
        Assert.NotNull(flow.Steps);
        Assert.Empty(flow.Steps);
    }

    [Fact]
    public void Flow_SupportsAddingSteps()
    {
        // Arrange
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = "Test Flow"
        };

        // Act
        flow.Steps.Add(new FlowStep
        {
            Id = Guid.NewGuid(),
            Order = 1,
            RequestId = Guid.NewGuid()
        });
        flow.Steps.Add(new FlowStep
        {
            Id = Guid.NewGuid(),
            Order = 2,
            RequestId = Guid.NewGuid()
        });

        // Assert
        Assert.Equal(2, flow.Steps.Count);
        Assert.Equal(1, flow.Steps[0].Order);
        Assert.Equal(2, flow.Steps[1].Order);
    }

    [Fact]
    public void Flow_StepsCanBeOrdered()
    {
        // Arrange
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = "Test Flow"
        };

        // Act
        flow.Steps.Add(new FlowStep { Id = Guid.NewGuid(), Order = 3, RequestId = Guid.NewGuid() });
        flow.Steps.Add(new FlowStep { Id = Guid.NewGuid(), Order = 1, RequestId = Guid.NewGuid() });
        flow.Steps.Add(new FlowStep { Id = Guid.NewGuid(), Order = 2, RequestId = Guid.NewGuid() });

        var orderedSteps = flow.Steps.OrderBy(s => s.Order).ToList();

        // Assert
        Assert.Equal(1, orderedSteps[0].Order);
        Assert.Equal(2, orderedSteps[1].Order);
        Assert.Equal(3, orderedSteps[2].Order);
    }

    [Fact]
    public void Flow_CanHaveNullDescription()
    {
        // Arrange & Act
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = "Test Flow",
            Description = null
        };

        // Assert
        Assert.Null(flow.Description);
    }

    [Fact]
    public void Flow_TracksCreationDate()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        
        // Act
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = "Test Flow",
            CreatedAt = DateTime.UtcNow
        };
        
        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.True(flow.CreatedAt >= beforeCreate);
        Assert.True(flow.CreatedAt <= afterCreate);
    }

    [Fact]
    public void Flow_CanBeAssociatedWithEnvironment()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = "Test Flow"
        };

        // Act
        flow.EnvironmentId = environmentId;

        // Assert
        Assert.Equal(environmentId, flow.EnvironmentId);
    }

    [Fact]
    public void Flow_SupportsRemovingSteps()
    {
        // Arrange
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = "Test Flow"
        };
        var step1 = new FlowStep { Id = Guid.NewGuid(), Order = 1, RequestId = Guid.NewGuid() };
        var step2 = new FlowStep { Id = Guid.NewGuid(), Order = 2, RequestId = Guid.NewGuid() };
        flow.Steps.Add(step1);
        flow.Steps.Add(step2);

        // Act
        flow.Steps.Remove(step1);

        // Assert
        Assert.Single(flow.Steps);
        Assert.Equal(step2.Id, flow.Steps[0].Id);
    }

    [Fact]
    public void FlowStep_HasRequiredProperties()
    {
        // Arrange & Act
        var step = new FlowStep
        {
            Id = Guid.NewGuid(),
            Order = 1,
            RequestId = Guid.NewGuid()
        };

        // Assert
        Assert.NotEqual(Guid.Empty, step.Id);
        Assert.Equal(1, step.Order);
        Assert.NotEqual(Guid.Empty, step.RequestId);
    }

    [Fact]
    public void FlowStep_SupportsDelay()
    {
        // Arrange & Act
        var step = new FlowStep
        {
            Id = Guid.NewGuid(),
            Order = 1,
            RequestId = Guid.NewGuid(),
            DelayBeforeExecutionMs = 1000
        };

        // Assert
        Assert.Equal(1000, step.DelayBeforeExecutionMs);
    }

    [Fact]
    public void FlowStep_DefaultDelayIsNull()
    {
        // Arrange & Act
        var step = new FlowStep
        {
            Id = Guid.NewGuid(),
            Order = 1,
            RequestId = Guid.NewGuid()
        };

        // Assert
        Assert.Null(step.DelayBeforeExecutionMs);
    }

    [Theory]
    [InlineData("User Login Flow")]
    [InlineData("Data Migration")]
    [InlineData("API Integration Test")]
    [InlineData("Smoke Test Suite")]
    public void Flow_SupportsCommonFlowNames(string name)
    {
        // Arrange & Act
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        // Assert
        Assert.Equal(name, flow.Name);
    }

    [Fact]
    public void Flow_SupportsLongName()
    {
        // Arrange
        var longName = new string('a', 200);

        // Act
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = longName
        };

        // Assert
        Assert.Equal(200, flow.Name.Length);
        Assert.Equal(longName, flow.Name);
    }

    [Fact]
    public void Flow_CanHaveManySteps()
    {
        // Arrange
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = "Complex Flow"
        };

        // Act
        for (int i = 1; i <= 50; i++)
        {
            flow.Steps.Add(new FlowStep
            {
                Id = Guid.NewGuid(),
                Order = i,
                RequestId = Guid.NewGuid()
            });
        }

        // Assert
        Assert.Equal(50, flow.Steps.Count);
    }

    [Fact]
    public void FlowStep_CanReferenceRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid();

        // Act
        var step = new FlowStep
        {
            Id = Guid.NewGuid(),
            Order = 1,
            RequestId = requestId
        };

        // Assert
        Assert.Equal(requestId, step.RequestId);
    }

    [Fact]
    public void FlowStep_SupportsContinueOnError()
    {
        // Arrange & Act
        var step = new FlowStep
        {
            Id = Guid.NewGuid(),
            Order = 1,
            RequestId = Guid.NewGuid(),
            ContinueOnError = true
        };

        // Assert
        Assert.True(step.ContinueOnError);
    }

    [Fact]
    public void FlowStep_DefaultContinueOnErrorIsFalse()
    {
        // Arrange & Act
        var step = new FlowStep
        {
            Id = Guid.NewGuid(),
            Order = 1,
            RequestId = Guid.NewGuid()
        };

        // Assert
        Assert.False(step.ContinueOnError);
    }

    [Fact]
    public void FlowStep_SupportsIsEnabled()
    {
        // Arrange & Act
        var step = new FlowStep
        {
            Id = Guid.NewGuid(),
            Order = 1,
            RequestId = Guid.NewGuid(),
            IsEnabled = false
        };

        // Assert
        Assert.False(step.IsEnabled);
    }

    [Fact]
    public void FlowStep_DefaultIsEnabledIsTrue()
    {
        // Arrange & Act
        var step = new FlowStep
        {
            Id = Guid.NewGuid(),
            Order = 1,
            RequestId = Guid.NewGuid()
        };

        // Assert
        Assert.True(step.IsEnabled);
    }
}
