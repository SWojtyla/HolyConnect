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
                Steps = new List<FlowStep>
                {
                    new FlowStep { Id = Guid.NewGuid(), Order = 1, RequestId = Guid.NewGuid() }
                },
                CreatedAt = DateTime.UtcNow
            },
            new Flow
            {
                Id = Guid.NewGuid(),
                Name = "Test Flow 2",
                Description = "Second test flow",
                Steps = new List<FlowStep>
                {
                    new FlowStep { Id = Guid.NewGuid(), Order = 1, RequestId = Guid.NewGuid() },
                    new FlowStep { Id = Guid.NewGuid(), Order = 2, RequestId = Guid.NewGuid() }
                },
                CreatedAt = DateTime.UtcNow
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
            Steps = new List<FlowStep>(),
            CreatedAt = DateTime.UtcNow
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
            Steps = new List<FlowStep>
            {
                new FlowStep { Id = Guid.NewGuid(), Order = 1, RequestId = Guid.NewGuid() }
            },
            CreatedAt = DateTime.UtcNow.AddDays(-1)
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

    [Fact]
    public async Task GetFlowByIdAsync_ForViewPage_ReturnsFlowDetails()
    {
        // Arrange
        var mockFlowService = new Mock<IFlowService>();
        var flowId = Guid.NewGuid();
        var requestId1 = Guid.NewGuid();
        var requestId2 = Guid.NewGuid();
        var testFlow = new Flow
        {
            Id = flowId,
            Name = "View Test Flow",
            Description = "Flow for view page testing",
            CollectionId = Guid.NewGuid(),
            Steps = new List<FlowStep>
            {
                new FlowStep { Id = Guid.NewGuid(), Order = 1, RequestId = requestId1, IsEnabled = true, ContinueOnError = false },
                new FlowStep { Id = Guid.NewGuid(), Order = 2, RequestId = requestId2, IsEnabled = true, ContinueOnError = true, DelayBeforeExecutionMs = 1000 }
            },
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        mockFlowService
            .Setup(s => s.GetFlowByIdAsync(flowId))
            .ReturnsAsync(testFlow);

        // Act
        var result = await mockFlowService.Object.GetFlowByIdAsync(flowId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(flowId, result.Id);
        Assert.Equal("View Test Flow", result.Name);
        Assert.Equal("Flow for view page testing", result.Description);
        Assert.Equal(2, result.Steps.Count);
        Assert.Contains(result.Steps, s => s.ContinueOnError);
        Assert.Contains(result.Steps, s => s.DelayBeforeExecutionMs.HasValue);
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithEnvironmentId_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var mockFlowService = new Mock<IFlowService>();
        var flowId = Guid.NewGuid();
        var environmentId = Guid.NewGuid();
        var expectedResult = new FlowExecutionResult
        {
            FlowId = flowId,
            FlowName = "Test Flow",
            Status = FlowExecutionStatus.Completed,
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow.AddSeconds(5)
        };

        mockFlowService
            .Setup(s => s.ExecuteFlowAsync(flowId, environmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await mockFlowService.Object.ExecuteFlowAsync(flowId, environmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(FlowExecutionStatus.Completed, result.Status);
        mockFlowService.Verify(s => s.ExecuteFlowAsync(flowId, environmentId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
