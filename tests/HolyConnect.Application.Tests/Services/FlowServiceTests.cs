using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Services;

public class FlowServiceTests
{
    private readonly Mock<IRepository<Flow>> _mockFlowRepository;
    private readonly Mock<IRepository<Request>> _mockRequestRepository;
    private readonly Mock<IRepository<Domain.Entities.Environment>> _mockEnvironmentRepository;
    private readonly Mock<IRepository<Collection>> _mockCollectionRepository;
    private readonly Mock<IRequestService> _mockRequestService;
    private readonly Mock<IVariableResolver> _mockVariableResolver;
    private readonly FlowService _service;

    public FlowServiceTests()
    {
        _mockFlowRepository = new Mock<IRepository<Flow>>();
        _mockRequestRepository = new Mock<IRepository<Request>>();
        _mockEnvironmentRepository = new Mock<IRepository<Domain.Entities.Environment>>();
        _mockCollectionRepository = new Mock<IRepository<Collection>>();
        _mockRequestService = new Mock<IRequestService>();
        _mockVariableResolver = new Mock<IVariableResolver>();

        _service = new FlowService(
            _mockFlowRepository.Object,
            _mockRequestRepository.Object,
            _mockEnvironmentRepository.Object,
            _mockCollectionRepository.Object,
            _mockRequestService.Object,
            _mockVariableResolver.Object);
    }

    [Fact]
    public async Task CreateFlowAsync_ShouldCreateFlowWithCorrectProperties()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var flow = new Flow
        {
            Name = "Test Flow",
            Description = "Test Description",
            EnvironmentId = environmentId,
            Steps = new List<FlowStep>
            {
                new FlowStep { Order = 1, RequestId = Guid.NewGuid() },
                new FlowStep { Order = 2, RequestId = Guid.NewGuid() }
            }
        };

        Flow? capturedFlow = null;
        _mockFlowRepository.Setup(r => r.AddAsync(It.IsAny<Flow>()))
            .Callback<Flow>(f => capturedFlow = f)
            .ReturnsAsync((Flow f) => f);

        // Act
        var result = await _service.CreateFlowAsync(flow);

        // Assert
        Assert.NotNull(capturedFlow);
        Assert.NotEqual(Guid.Empty, capturedFlow.Id);
        Assert.True(capturedFlow.CreatedAt > DateTime.MinValue);
        Assert.True(capturedFlow.UpdatedAt > DateTime.MinValue);
        Assert.Equal(2, capturedFlow.Steps.Count);
        Assert.All(capturedFlow.Steps, step =>
        {
            Assert.NotEqual(Guid.Empty, step.Id);
            Assert.Equal(capturedFlow.Id, step.FlowId);
        });
        _mockFlowRepository.Verify(r => r.AddAsync(It.IsAny<Flow>()), Times.Once);
    }

    [Fact]
    public async Task GetAllFlowsAsync_ShouldReturnAllFlows()
    {
        // Arrange
        var flows = new List<Flow>
        {
            new Flow { Id = Guid.NewGuid(), Name = "Flow 1" },
            new Flow { Id = Guid.NewGuid(), Name = "Flow 2" }
        };

        _mockFlowRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(flows);

        // Act
        var result = await _service.GetAllFlowsAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetFlowByIdAsync_ShouldReturnFlow()
    {
        // Arrange
        var id = Guid.NewGuid();
        var flow = new Flow { Id = id, Name = "Test Flow" };

        _mockFlowRepository.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(flow);

        // Act
        var result = await _service.GetFlowByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Test Flow", result.Name);
    }

    [Fact]
    public async Task GetFlowsByEnvironmentIdAsync_ShouldReturnEnvironmentFlows()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var flows = new List<Flow>
        {
            new Flow { Id = Guid.NewGuid(), Name = "Flow 1", EnvironmentId = environmentId },
            new Flow { Id = Guid.NewGuid(), Name = "Flow 2", EnvironmentId = environmentId, CollectionId = Guid.NewGuid() },
            new Flow { Id = Guid.NewGuid(), Name = "Flow 3", EnvironmentId = Guid.NewGuid() }
        };

        _mockFlowRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(flows);

        // Act
        var result = await _service.GetFlowsByEnvironmentIdAsync(environmentId);

        // Assert
        Assert.Single(result); // Only Flow 1 should be returned (no collection ID)
        Assert.Equal("Flow 1", result.First().Name);
    }

    [Fact]
    public async Task GetFlowsByCollectionIdAsync_ShouldReturnCollectionFlows()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var flows = new List<Flow>
        {
            new Flow { Id = Guid.NewGuid(), Name = "Flow 1", CollectionId = collectionId },
            new Flow { Id = Guid.NewGuid(), Name = "Flow 2", CollectionId = Guid.NewGuid() }
        };

        _mockFlowRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(flows);

        // Act
        var result = await _service.GetFlowsByCollectionIdAsync(collectionId);

        // Assert
        Assert.Single(result);
        Assert.Equal("Flow 1", result.First().Name);
    }

    [Fact]
    public async Task UpdateFlowAsync_ShouldUpdateFlow()
    {
        // Arrange
        var originalUpdatedAt = DateTime.UtcNow.AddDays(-1);
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = "Updated Flow",
            UpdatedAt = originalUpdatedAt
        };

        Flow? capturedFlow = null;
        _mockFlowRepository.Setup(r => r.UpdateAsync(It.IsAny<Flow>()))
            .Callback<Flow>(f => capturedFlow = f)
            .ReturnsAsync((Flow f) => f);

        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = await _service.UpdateFlowAsync(flow);

        // Assert
        Assert.NotNull(capturedFlow);
        Assert.True(capturedFlow.UpdatedAt > originalUpdatedAt);
        Assert.True(capturedFlow.UpdatedAt >= beforeUpdate);
        Assert.True(capturedFlow.UpdatedAt <= DateTime.UtcNow.AddSeconds(1));
        _mockFlowRepository.Verify(r => r.UpdateAsync(It.IsAny<Flow>()), Times.Once);
    }

    [Fact]
    public async Task DeleteFlowAsync_ShouldDeleteFlow()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        await _service.DeleteFlowAsync(id);

        // Assert
        _mockFlowRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithNonExistentFlow_ShouldThrowException()
    {
        // Arrange
        var flowId = Guid.NewGuid();
        _mockFlowRepository.Setup(r => r.GetByIdAsync(flowId))
            .ReturnsAsync((Flow?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ExecuteFlowAsync(flowId));
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithEmptySteps_ShouldCompleteSuccessfully()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var flowId = Guid.NewGuid();
        var flow = new Flow
        {
            Id = flowId,
            Name = "Empty Flow",
            EnvironmentId = environmentId,
            Steps = new List<FlowStep>()
        };

        var environment = new Domain.Entities.Environment
        {
            Id = environmentId,
            Variables = new Dictionary<string, string>()
        };

        _mockFlowRepository.Setup(r => r.GetByIdAsync(flowId))
            .ReturnsAsync(flow);
        _mockEnvironmentRepository.Setup(r => r.GetByIdAsync(environmentId))
            .ReturnsAsync(environment);

        // Act
        var result = await _service.ExecuteFlowAsync(flowId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Completed, result.Status);
        Assert.Empty(result.StepResults);
        Assert.NotNull(result.CompletedAt);
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithSingleStep_ShouldExecuteSuccessfully()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var flowId = Guid.NewGuid();
        
        var request = new RestRequest
        {
            Id = requestId,
            Name = "Test Request",
            Url = "https://api.example.com/test",
            EnvironmentId = environmentId
        };

        var flow = new Flow
        {
            Id = flowId,
            Name = "Single Step Flow",
            EnvironmentId = environmentId,
            Steps = new List<FlowStep>
            {
                new FlowStep
                {
                    Id = Guid.NewGuid(),
                    Order = 1,
                    RequestId = requestId,
                    IsEnabled = true
                }
            }
        };

        var environment = new Domain.Entities.Environment
        {
            Id = environmentId,
            Variables = new Dictionary<string, string>()
        };

        var response = new RequestResponse
        {
            StatusCode = 200,
            Body = "{\"success\": true}"
        };

        _mockFlowRepository.Setup(r => r.GetByIdAsync(flowId))
            .ReturnsAsync(flow);
        _mockEnvironmentRepository.Setup(r => r.GetByIdAsync(environmentId))
            .ReturnsAsync(environment);
        _mockRequestRepository.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync(response);

        // Act
        var result = await _service.ExecuteFlowAsync(flowId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Completed, result.Status);
        Assert.Single(result.StepResults);
        Assert.Equal(FlowStepStatus.Success, result.StepResults[0].Status);
        Assert.Equal("Test Request", result.StepResults[0].RequestName);
        Assert.NotNull(result.StepResults[0].Response);
        _mockRequestService.Verify(s => s.ExecuteRequestAsync(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithMultipleSteps_ShouldExecuteInOrder()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var request1Id = Guid.NewGuid();
        var request2Id = Guid.NewGuid();
        var flowId = Guid.NewGuid();

        var request1 = new RestRequest { Id = request1Id, Name = "Request 1", EnvironmentId = environmentId };
        var request2 = new RestRequest { Id = request2Id, Name = "Request 2", EnvironmentId = environmentId };

        var flow = new Flow
        {
            Id = flowId,
            Name = "Multi Step Flow",
            EnvironmentId = environmentId,
            Steps = new List<FlowStep>
            {
                new FlowStep { Id = Guid.NewGuid(), Order = 1, RequestId = request1Id, IsEnabled = true },
                new FlowStep { Id = Guid.NewGuid(), Order = 2, RequestId = request2Id, IsEnabled = true }
            }
        };

        var environment = new Domain.Entities.Environment
        {
            Id = environmentId,
            Variables = new Dictionary<string, string>()
        };

        _mockFlowRepository.Setup(r => r.GetByIdAsync(flowId)).ReturnsAsync(flow);
        _mockEnvironmentRepository.Setup(r => r.GetByIdAsync(environmentId)).ReturnsAsync(environment);
        _mockRequestRepository.Setup(r => r.GetByIdAsync(request1Id)).ReturnsAsync(request1);
        _mockRequestRepository.Setup(r => r.GetByIdAsync(request2Id)).ReturnsAsync(request2);
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync(new RequestResponse { StatusCode = 200, Body = "{}" });

        // Act
        var result = await _service.ExecuteFlowAsync(flowId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Completed, result.Status);
        Assert.Equal(2, result.StepResults.Count);
        Assert.Equal(1, result.StepResults[0].StepOrder);
        Assert.Equal(2, result.StepResults[1].StepOrder);
        Assert.Equal("Request 1", result.StepResults[0].RequestName);
        Assert.Equal("Request 2", result.StepResults[1].RequestName);
        _mockRequestService.Verify(s => s.ExecuteRequestAsync(It.IsAny<Request>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithDisabledStep_ShouldSkipStep()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var flowId = Guid.NewGuid();

        var flow = new Flow
        {
            Id = flowId,
            Name = "Flow with Disabled Step",
            EnvironmentId = environmentId,
            Steps = new List<FlowStep>
            {
                new FlowStep
                {
                    Id = Guid.NewGuid(),
                    Order = 1,
                    RequestId = requestId,
                    IsEnabled = false
                }
            }
        };

        var environment = new Domain.Entities.Environment
        {
            Id = environmentId,
            Variables = new Dictionary<string, string>()
        };

        _mockFlowRepository.Setup(r => r.GetByIdAsync(flowId)).ReturnsAsync(flow);
        _mockEnvironmentRepository.Setup(r => r.GetByIdAsync(environmentId)).ReturnsAsync(environment);

        // Act
        var result = await _service.ExecuteFlowAsync(flowId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Completed, result.Status);
        Assert.Single(result.StepResults);
        Assert.Equal(FlowStepStatus.Skipped, result.StepResults[0].Status);
        _mockRequestService.Verify(s => s.ExecuteRequestAsync(It.IsAny<Request>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithFailedStepAndContinueOnError_ShouldContinue()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var request1Id = Guid.NewGuid();
        var request2Id = Guid.NewGuid();
        var flowId = Guid.NewGuid();

        var request1 = new RestRequest { Id = request1Id, Name = "Failing Request", EnvironmentId = environmentId };
        var request2 = new RestRequest { Id = request2Id, Name = "Success Request", EnvironmentId = environmentId };

        var flow = new Flow
        {
            Id = flowId,
            Name = "Flow with ContinueOnError",
            EnvironmentId = environmentId,
            Steps = new List<FlowStep>
            {
                new FlowStep { Id = Guid.NewGuid(), Order = 1, RequestId = request1Id, IsEnabled = true, ContinueOnError = true },
                new FlowStep { Id = Guid.NewGuid(), Order = 2, RequestId = request2Id, IsEnabled = true }
            }
        };

        var environment = new Domain.Entities.Environment
        {
            Id = environmentId,
            Variables = new Dictionary<string, string>()
        };

        _mockFlowRepository.Setup(r => r.GetByIdAsync(flowId)).ReturnsAsync(flow);
        _mockEnvironmentRepository.Setup(r => r.GetByIdAsync(environmentId)).ReturnsAsync(environment);
        _mockRequestRepository.Setup(r => r.GetByIdAsync(request1Id)).ReturnsAsync(request1);
        _mockRequestRepository.Setup(r => r.GetByIdAsync(request2Id)).ReturnsAsync(request2);
        
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.Is<Request>(r => r.Id == request1Id)))
            .ThrowsAsync(new Exception("Request failed"));
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.Is<Request>(r => r.Id == request2Id)))
            .ReturnsAsync(new RequestResponse { StatusCode = 200, Body = "{}" });

        // Act
        var result = await _service.ExecuteFlowAsync(flowId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Completed, result.Status);
        Assert.Equal(2, result.StepResults.Count);
        Assert.Equal(FlowStepStatus.FailedContinued, result.StepResults[0].Status);
        Assert.Equal(FlowStepStatus.Success, result.StepResults[1].Status);
        _mockRequestService.Verify(s => s.ExecuteRequestAsync(It.IsAny<Request>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithFailedStepAndNoContinue_ShouldStopExecution()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var request1Id = Guid.NewGuid();
        var request2Id = Guid.NewGuid();
        var flowId = Guid.NewGuid();

        var request1 = new RestRequest { Id = request1Id, Name = "Failing Request", EnvironmentId = environmentId };

        var flow = new Flow
        {
            Id = flowId,
            Name = "Flow Stopping on Error",
            EnvironmentId = environmentId,
            Steps = new List<FlowStep>
            {
                new FlowStep { Id = Guid.NewGuid(), Order = 1, RequestId = request1Id, IsEnabled = true, ContinueOnError = false },
                new FlowStep { Id = Guid.NewGuid(), Order = 2, RequestId = request2Id, IsEnabled = true }
            }
        };

        var environment = new Domain.Entities.Environment
        {
            Id = environmentId,
            Variables = new Dictionary<string, string>()
        };

        _mockFlowRepository.Setup(r => r.GetByIdAsync(flowId)).ReturnsAsync(flow);
        _mockEnvironmentRepository.Setup(r => r.GetByIdAsync(environmentId)).ReturnsAsync(environment);
        _mockRequestRepository.Setup(r => r.GetByIdAsync(request1Id)).ReturnsAsync(request1);
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.IsAny<Request>()))
            .ThrowsAsync(new Exception("Request failed"));

        // Act
        var result = await _service.ExecuteFlowAsync(flowId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Failed, result.Status);
        Assert.Single(result.StepResults); // Second step should not execute
        Assert.Equal(FlowStepStatus.Failed, result.StepResults[0].Status);
        Assert.NotNull(result.ErrorMessage);
        _mockRequestService.Verify(s => s.ExecuteRequestAsync(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithCancellationToken_ShouldCancelExecution()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var flowId = Guid.NewGuid();
        var flow = new Flow
        {
            Id = flowId,
            Name = "Cancellable Flow",
            EnvironmentId = environmentId,
            Steps = new List<FlowStep>
            {
                new FlowStep { Id = Guid.NewGuid(), Order = 1, RequestId = Guid.NewGuid(), IsEnabled = true }
            }
        };

        var environment = new Domain.Entities.Environment
        {
            Id = environmentId,
            Variables = new Dictionary<string, string>()
        };

        _mockFlowRepository.Setup(r => r.GetByIdAsync(flowId)).ReturnsAsync(flow);
        _mockEnvironmentRepository.Setup(r => r.GetByIdAsync(environmentId)).ReturnsAsync(environment);

        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act
        var result = await _service.ExecuteFlowAsync(flowId, cts.Token);

        // Assert
        Assert.Equal(FlowExecutionStatus.Cancelled, result.Status);
        Assert.Empty(result.StepResults); // No steps should execute
    }
}
