using HolyConnect.Domain.Entities;

namespace HolyConnect.Domain.Tests.Entities;

public class FlowTests
{
    [Fact]
    public void Flow_ShouldInitializeWithEmptySteps()
    {
        // Arrange & Act
        var flow = new Flow();

        // Assert
        Assert.NotNull(flow.Steps);
        Assert.Empty(flow.Steps);
    }

    [Fact]
    public void Flow_ShouldHaveCorrectProperties()
    {
        // Arrange
        var flowId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var flow = new Flow
        {
            Id = flowId,
            Name = "Test Flow",
            Description = "Test Description",
            CollectionId = collectionId,
            CreatedAt = createdAt
        };

        // Assert
        Assert.Equal(flowId, flow.Id);
        Assert.Equal("Test Flow", flow.Name);
        Assert.Equal("Test Description", flow.Description);
        Assert.Equal(collectionId, flow.CollectionId);
        Assert.Equal(createdAt, flow.CreatedAt);
    }

    [Fact]
    public void Flow_ShouldAllowAddingSteps()
    {
        // Arrange
        var flow = new Flow { Id = Guid.NewGuid() };
        var step1 = new FlowStep { Id = Guid.NewGuid(), Order = 1, RequestId = Guid.NewGuid() };
        var step2 = new FlowStep { Id = Guid.NewGuid(), Order = 2, RequestId = Guid.NewGuid() };

        // Act
        flow.Steps.Add(step1);
        flow.Steps.Add(step2);

        // Assert
        Assert.Equal(2, flow.Steps.Count);
        Assert.Contains(step1, flow.Steps);
        Assert.Contains(step2, flow.Steps);
    }
}
