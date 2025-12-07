using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Maui.Tests.Components;

/// <summary>
/// Tests for Flows page component logic.
/// </summary>
public class FlowsPageTests
{
    [Fact]
    public async Task GetAllFlowsAsync_ReturnsFlows_WhenFlowsExist()
    {
        // Arrange
        var mockFlowService = new Mock<IFlowService>();
        var testFlows = new List<Flow>
        {
            new Flow
            {
                Id = Guid.NewGuid(),
                Name = "Test Flow 1",
                Description = "First test flow",
                EnvironmentId = Guid.NewGuid(),
                Steps = new List<FlowStep>
                {
                    new FlowStep { Id = Guid.NewGuid(), Order = 1, RequestId = Guid.NewGuid() }
                },
                CreatedAt = DateTime.UtcNow,
            },
            new Flow
            {
                Id = Guid.NewGuid(),
                Name = "Test Flow 2",
                Description = "Second test flow",
                EnvironmentId = Guid.NewGuid(),
                Steps = new List<FlowStep>
                {
                    new FlowStep { Id = Guid.NewGuid(), Order = 1, RequestId = Guid.NewGuid() },
                    new FlowStep { Id = Guid.NewGuid(), Order = 2, RequestId = Guid.NewGuid() }
                },
                CreatedAt = DateTime.UtcNow,
            }
        };

        mockFlowService
            .Setup(s => s.GetAllFlowsAsync())
            .ReturnsAsync(testFlows);

        // Act
        var result = await mockFlowService.Object.GetAllFlowsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("Test Flow 1", result.First().Name);
        Assert.Equal(2, result.Last().Steps.Count);
    }

    [Fact]
    public async Task GetAllFlowsAsync_ReturnsEmptyList_WhenNoFlowsExist()
    {
        // Arrange
        var mockFlowService = new Mock<IFlowService>();
        mockFlowService
            .Setup(s => s.GetAllFlowsAsync())
            .ReturnsAsync(new List<Flow>());

        // Act
        var result = await mockFlowService.Object.GetAllFlowsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task DeleteFlowAsync_CallsServiceWithCorrectId()
    {
        // Arrange
        var mockFlowService = new Mock<IFlowService>();
        var flowId = Guid.NewGuid();

        // Act
        await mockFlowService.Object.DeleteFlowAsync(flowId);

        // Assert
        mockFlowService.Verify(s => s.DeleteFlowAsync(flowId), Times.Once);
    }

    [Fact]
    public async Task GetFlowByIdAsync_ReturnsFlow_WhenFlowExists()
    {
        // Arrange
        var mockFlowService = new Mock<IFlowService>();
        var flowId = Guid.NewGuid();
        var testFlow = new Flow
        {
            Id = flowId,
            Name = "Test Flow",
            Description = "Test Description",
            EnvironmentId = Guid.NewGuid(),
            Steps = new List<FlowStep>(),
            CreatedAt = DateTime.UtcNow,
        };

        mockFlowService
            .Setup(s => s.GetFlowByIdAsync(flowId))
            .ReturnsAsync(testFlow);

        // Act
        var result = await mockFlowService.Object.GetFlowByIdAsync(flowId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(flowId, result.Id);
        Assert.Equal("Test Flow", result.Name);
    }

    [Fact]
    public async Task UpdateFlowAsync_UpdatesFlow_WhenValidDataProvided()
    {
        // Arrange
        var mockFlowService = new Mock<IFlowService>();
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = "Updated Flow",
            Description = "Updated Description",
            EnvironmentId = Guid.NewGuid(),
            Steps = new List<FlowStep>
            {
                new FlowStep { Id = Guid.NewGuid(), Order = 1, RequestId = Guid.NewGuid() }
            },
            CreatedAt = DateTime.UtcNow.AddDays(-1),
        };

        mockFlowService
            .Setup(s => s.UpdateFlowAsync(It.IsAny<Flow>()))
            .ReturnsAsync(flow);

        // Act
        var result = await mockFlowService.Object.UpdateFlowAsync(flow);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Flow", result.Name);
        mockFlowService.Verify(s => s.UpdateFlowAsync(It.IsAny<Flow>()), Times.Once);
    }
}
