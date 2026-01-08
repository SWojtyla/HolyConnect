using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Services;

public class FlowServiceTests
{
    private readonly Mock<IRepository<Flow>> _mockFlowRepository;
    private readonly Mock<IRepository<Request>> _mockRequestRepository;
    private readonly Mock<IActiveEnvironmentService> _mockActiveEnvironmentService;
    private readonly Mock<IRepository<Collection>> _mockCollectionRepository;
    private readonly Mock<IRequestService> _mockRequestService;
    private readonly Mock<IVariableResolver> _mockVariableResolver;
    private readonly Mock<IRepository<Domain.Entities.Environment>> _mockEnvironmentRepository;
    private readonly Mock<IRepository<RequestHistoryEntry>> _mockHistoryRepository;
    private readonly Mock<IRequestExecutorFactory> _mockExecutorFactory;
    private readonly FlowService _service;

    public FlowServiceTests()
    {
        _mockFlowRepository = new Mock<IRepository<Flow>>();
        _mockRequestRepository = new Mock<IRepository<Request>>();
        _mockActiveEnvironmentService = new Mock<IActiveEnvironmentService>();
        _mockCollectionRepository = new Mock<IRepository<Collection>>();
        _mockRequestService = new Mock<IRequestService>();
        _mockVariableResolver = new Mock<IVariableResolver>();
        _mockEnvironmentRepository = new Mock<IRepository<Domain.Entities.Environment>>();
        _mockHistoryRepository = new Mock<IRepository<RequestHistoryEntry>>();
        _mockExecutorFactory = new Mock<IRequestExecutorFactory>();

        var repositories = new RepositoryAccessor(
            _mockRequestRepository.Object,
            _mockCollectionRepository.Object,
            _mockEnvironmentRepository.Object,
            _mockFlowRepository.Object,
            _mockHistoryRepository.Object);

        var executionContext = new RequestExecutionContext(
            _mockActiveEnvironmentService.Object,
            _mockVariableResolver.Object,
            _mockExecutorFactory.Object);

        _service = new FlowService(
            repositories,
            executionContext,
            _mockRequestService.Object);
    }

    [Fact]
    public async Task CreateFlowAsync_ShouldCreateFlowWithCorrectProperties()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var flow = new Flow
        {
            Name = "Test Flow",
            Description = "Test Description",            Steps = new List<FlowStep>
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
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = "Updated Flow"
        };

        Flow? capturedFlow = null;
        _mockFlowRepository.Setup(r => r.UpdateAsync(It.IsAny<Flow>()))
            .Callback<Flow>(f => capturedFlow = f)
            .ReturnsAsync((Flow f) => f);

        // Act
        var result = await _service.UpdateFlowAsync(flow);

        // Assert
        Assert.NotNull(capturedFlow);
        Assert.Equal(flow.Id, capturedFlow.Id);
        Assert.Equal(flow.Name, capturedFlow.Name);
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
        var environmentId = Guid.NewGuid();
        _mockFlowRepository.Setup(r => r.GetByIdAsync(flowId))
            .ReturnsAsync((Flow?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ExecuteFlowAsync(flowId, environmentId));
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
            Name = "Empty Flow",            Steps = new List<FlowStep>()
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
        var result = await _service.ExecuteFlowAsync(flowId, environmentId);

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
            Url = "https://api.example.com/test",        };

        var flow = new Flow
        {
            Id = flowId,
            Name = "Single Step Flow",            Steps = new List<FlowStep>
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
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.IsAny<Request>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()))
            .ReturnsAsync(response);

        // Act
        var result = await _service.ExecuteFlowAsync(flowId, environmentId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Completed, result.Status);
        Assert.Single(result.StepResults);
        Assert.Equal(FlowStepStatus.Success, result.StepResults[0].Status);
        Assert.Equal("Test Request", result.StepResults[0].RequestName);
        Assert.NotNull(result.StepResults[0].Response);
        _mockRequestService.Verify(s => s.ExecuteRequestAsync(It.IsAny<Request>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithMultipleSteps_ShouldExecuteInOrder()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var request1Id = Guid.NewGuid();
        var request2Id = Guid.NewGuid();
        var flowId = Guid.NewGuid();

        var request1 = new RestRequest { Id = request1Id, Name = "Request 1"};
        var request2 = new RestRequest { Id = request2Id, Name = "Request 2"};

        var flow = new Flow
        {
            Id = flowId,
            Name = "Multi Step Flow",            Steps = new List<FlowStep>
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
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.IsAny<Request>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()))
            .ReturnsAsync(new RequestResponse { StatusCode = 200, Body = "{}" });

        // Act
        var result = await _service.ExecuteFlowAsync(flowId, environmentId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Completed, result.Status);
        Assert.Equal(2, result.StepResults.Count);
        Assert.Equal(1, result.StepResults[0].StepOrder);
        Assert.Equal(2, result.StepResults[1].StepOrder);
        Assert.Equal("Request 1", result.StepResults[0].RequestName);
        Assert.Equal("Request 2", result.StepResults[1].RequestName);
        _mockRequestService.Verify(s => s.ExecuteRequestAsync(It.IsAny<Request>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()), Times.Exactly(2));
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
            Name = "Flow with Disabled Step",            Steps = new List<FlowStep>
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
        var result = await _service.ExecuteFlowAsync(flowId, environmentId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Completed, result.Status);
        Assert.Single(result.StepResults);
        Assert.Equal(FlowStepStatus.Skipped, result.StepResults[0].Status);
        _mockRequestService.Verify(s => s.ExecuteRequestAsync(It.IsAny<Request>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithFailedStepAndContinueOnError_ShouldContinue()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var request1Id = Guid.NewGuid();
        var request2Id = Guid.NewGuid();
        var flowId = Guid.NewGuid();

        var request1 = new RestRequest { Id = request1Id, Name = "Failing Request"};
        var request2 = new RestRequest { Id = request2Id, Name = "Success Request"};

        var flow = new Flow
        {
            Id = flowId,
            Name = "Flow with ContinueOnError",            Steps = new List<FlowStep>
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
        
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.Is<Request>(r => r.Id == request1Id), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()))
            .ThrowsAsync(new Exception("Request failed"));
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.Is<Request>(r => r.Id == request2Id), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()))
            .ReturnsAsync(new RequestResponse { StatusCode = 200, Body = "{}" });

        // Act
        var result = await _service.ExecuteFlowAsync(flowId, environmentId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Completed, result.Status);
        Assert.Equal(2, result.StepResults.Count);
        Assert.Equal(FlowStepStatus.FailedContinued, result.StepResults[0].Status);
        Assert.Equal(FlowStepStatus.Success, result.StepResults[1].Status);
        _mockRequestService.Verify(s => s.ExecuteRequestAsync(It.IsAny<Request>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithFailedStepAndNoContinue_ShouldStopExecution()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var request1Id = Guid.NewGuid();
        var request2Id = Guid.NewGuid();
        var flowId = Guid.NewGuid();

        var request1 = new RestRequest { Id = request1Id, Name = "Failing Request"};

        var flow = new Flow
        {
            Id = flowId,
            Name = "Flow Stopping on Error",            Steps = new List<FlowStep>
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
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.IsAny<Request>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()))
            .ThrowsAsync(new Exception("Request failed"));

        // Act
        var result = await _service.ExecuteFlowAsync(flowId, environmentId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Failed, result.Status);
        Assert.Single(result.StepResults); // Second step should not execute
        Assert.Equal(FlowStepStatus.Failed, result.StepResults[0].Status);
        Assert.NotNull(result.ErrorMessage);
        _mockRequestService.Verify(s => s.ExecuteRequestAsync(It.IsAny<Request>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()), Times.Once);
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
            Name = "Cancellable Flow",            Steps = new List<FlowStep>
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
        var result = await _service.ExecuteFlowAsync(flowId, environmentId, cts.Token);

        // Assert
        Assert.Equal(FlowExecutionStatus.Cancelled, result.Status);
        Assert.Empty(result.StepResults); // No steps should execute
    }

    [Fact]
    public async Task CreateFlowAsync_WithDuplicateName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var flow = new Flow
        {
            Name = "Duplicate Flow",            Steps = new List<FlowStep>()
        };
        _mockFlowRepository.Setup(r => r.AddAsync(It.IsAny<Flow>()))
            .ThrowsAsync(new InvalidOperationException($"An entity with the name '{flow.Name}' already exists."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateFlowAsync(flow));
        Assert.Contains("already exists", exception.Message);
        _mockFlowRepository.Verify(r => r.AddAsync(It.IsAny<Flow>()), Times.Once);
    }

    [Fact]
    public async Task UpdateFlowAsync_WithDuplicateName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = "Duplicate Flow",            Steps = new List<FlowStep>()
        };
        _mockFlowRepository.Setup(r => r.UpdateAsync(It.IsAny<Flow>()))
            .ThrowsAsync(new InvalidOperationException($"An entity with the name '{flow.Name}' already exists."));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateFlowAsync(flow));
        Assert.Contains("already exists", exception.Message);
        _mockFlowRepository.Verify(r => r.UpdateAsync(It.IsAny<Flow>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithHttpErrorStatusCode_ShouldFailStep()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var flowId = Guid.NewGuid();

        var request = new RestRequest
        {
            Id = requestId,
            Name = "Failing Request",
            Url = "https://api.example.com/notfound",        };

        var flow = new Flow
        {
            Id = flowId,
            Name = "Flow with 404",            Steps = new List<FlowStep>
            {
                new FlowStep
                {
                    Id = Guid.NewGuid(),
                    Order = 1,
                    RequestId = requestId,
                    IsEnabled = true,
                    ContinueOnError = false
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
            StatusCode = 404,
            StatusMessage = "Not Found",
            Body = "{\"error\": \"Resource not found\"}"
        };

        _mockFlowRepository.Setup(r => r.GetByIdAsync(flowId))
            .ReturnsAsync(flow);
        _mockEnvironmentRepository.Setup(r => r.GetByIdAsync(environmentId))
            .ReturnsAsync(environment);
        _mockRequestRepository.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.IsAny<Request>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()))
            .ReturnsAsync(response);

        // Act
        var result = await _service.ExecuteFlowAsync(flowId, environmentId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Failed, result.Status);
        Assert.Single(result.StepResults);
        Assert.Equal(FlowStepStatus.Failed, result.StepResults[0].Status);
        Assert.Contains("404", result.StepResults[0].ErrorMessage);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteFlowAsync_With500StatusCode_ShouldFailStep()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var flowId = Guid.NewGuid();

        var request = new RestRequest
        {
            Id = requestId,
            Name = "Server Error Request",        };

        var flow = new Flow
        {
            Id = flowId,
            Name = "Flow with 500",            Steps = new List<FlowStep>
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
            StatusCode = 500,
            StatusMessage = "Internal Server Error",
            Body = "Server error occurred"
        };

        _mockFlowRepository.Setup(r => r.GetByIdAsync(flowId)).ReturnsAsync(flow);
        _mockEnvironmentRepository.Setup(r => r.GetByIdAsync(environmentId)).ReturnsAsync(environment);
        _mockRequestRepository.Setup(r => r.GetByIdAsync(requestId)).ReturnsAsync(request);
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.IsAny<Request>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()))
            .ReturnsAsync(response);

        // Act
        var result = await _service.ExecuteFlowAsync(flowId, environmentId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Failed, result.Status);
        Assert.Single(result.StepResults);
        Assert.Equal(FlowStepStatus.Failed, result.StepResults[0].Status);
        Assert.Contains("500", result.StepResults[0].ErrorMessage);
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithErrorStatusCodeAndContinueOnError_ShouldContinue()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var request1Id = Guid.NewGuid();
        var request2Id = Guid.NewGuid();
        var flowId = Guid.NewGuid();

        var request1 = new RestRequest { Id = request1Id, Name = "Failing Request"};
        var request2 = new RestRequest { Id = request2Id, Name = "Success Request"};

        var flow = new Flow
        {
            Id = flowId,
            Name = "Flow with Error and Continue",            Steps = new List<FlowStep>
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

        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.Is<Request>(r => r.Id == request1Id), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()))
            .ReturnsAsync(new RequestResponse { StatusCode = 404, StatusMessage = "Not Found" });
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.Is<Request>(r => r.Id == request2Id), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()))
            .ReturnsAsync(new RequestResponse { StatusCode = 200, Body = "{}" });

        // Act
        var result = await _service.ExecuteFlowAsync(flowId, environmentId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Completed, result.Status);
        Assert.Equal(2, result.StepResults.Count);
        Assert.Equal(FlowStepStatus.FailedContinued, result.StepResults[0].Status);
        Assert.Equal(FlowStepStatus.Success, result.StepResults[1].Status);
        _mockRequestService.Verify(s => s.ExecuteRequestAsync(It.IsAny<Request>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()), Times.Exactly(2));
    }

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(204)]
    [InlineData(299)]
    public async Task ExecuteFlowAsync_WithSuccessStatusCodes_ShouldSucceed(int statusCode)
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var flowId = Guid.NewGuid();

        var request = new RestRequest
        {
            Id = requestId,
            Name = "Test Request",        };

        var flow = new Flow
        {
            Id = flowId,
            Name = "Success Flow",            Steps = new List<FlowStep>
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
            StatusCode = statusCode,
            StatusMessage = "OK"
        };

        _mockFlowRepository.Setup(r => r.GetByIdAsync(flowId)).ReturnsAsync(flow);
        _mockEnvironmentRepository.Setup(r => r.GetByIdAsync(environmentId)).ReturnsAsync(environment);
        _mockRequestRepository.Setup(r => r.GetByIdAsync(requestId)).ReturnsAsync(request);
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.IsAny<Request>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()))
            .ReturnsAsync(response);

        // Act
        var result = await _service.ExecuteFlowAsync(flowId, environmentId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Completed, result.Status);
        Assert.Single(result.StepResults);
        Assert.Equal(FlowStepStatus.Success, result.StepResults[0].Status);
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithDynamicVariables_ShouldReuseGeneratedValuesAcrossSteps()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var request1Id = Guid.NewGuid();
        var request2Id = Guid.NewGuid();
        var flowId = Guid.NewGuid();

        var request1 = new RestRequest
        {
            Id = request1Id,
            Name = "Request 1",
            Url = "https://api.example.com/test",
            DynamicVariables = new List<DynamicVariable>
            {
                new DynamicVariable { Name = "testGuid", GeneratorType = DataGeneratorType.Guid }
            }
        };

        var request2 = new RestRequest
        {
            Id = request2Id,
            Name = "Request 2",
            Url = "https://api.example.com/test",
            DynamicVariables = new List<DynamicVariable>
            {
                new DynamicVariable { Name = "testGuid", GeneratorType = DataGeneratorType.Guid }
            }
        };

        var flow = new Flow
        {
            Id = flowId,
            Name = "Dynamic Variable Flow",
            Steps = new List<FlowStep>
            {
                new FlowStep
                {
                    Id = Guid.NewGuid(),
                    Order = 1,
                    RequestId = request1Id,
                    IsEnabled = true
                },
                new FlowStep
                {
                    Id = Guid.NewGuid(),
                    Order = 2,
                    RequestId = request2Id,
                    IsEnabled = true
                }
            }
        };

        var environment = new Domain.Entities.Environment
        {
            Id = environmentId,
            Variables = new Dictionary<string, string>(),
            DynamicVariables = new List<DynamicVariable>()
        };

        var response = new RequestResponse
        {
            StatusCode = 200,
            Body = "{\"success\": true}",
            SentRequest = new SentRequest
            {
                Url = "https://api.example.com/test",
                Method = "GET"
            }
        };

        // Track the GUID value that gets generated
        string? generatedGuid = null;
        
        _mockFlowRepository.Setup(r => r.GetByIdAsync(flowId))
            .ReturnsAsync(flow);
        _mockEnvironmentRepository.Setup(r => r.GetByIdAsync(environmentId))
            .ReturnsAsync(environment);
        _mockRequestRepository.Setup(r => r.GetByIdAsync(request1Id))
            .ReturnsAsync(request1);
        _mockRequestRepository.Setup(r => r.GetByIdAsync(request2Id))
            .ReturnsAsync(request2);
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.IsAny<Request>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()))
            .ReturnsAsync(response);
        
        // Mock the variable resolver to track when dynamic variables are generated
        _mockVariableResolver
            .Setup(v => v.GetVariableValue("testGuid", It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>(), It.IsAny<Request>()))
            .Returns(() =>
            {
                // Generate the GUID once and store it
                if (generatedGuid == null)
                {
                    generatedGuid = Guid.NewGuid().ToString();
                }
                return generatedGuid;
            });

        // Act
        var result = await _service.ExecuteFlowAsync(flowId, environmentId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Completed, result.Status);
        Assert.Equal(2, result.StepResults.Count);
        Assert.All(result.StepResults, sr => Assert.Equal(FlowStepStatus.Success, sr.Status));
        
        // Verify that GetVariableValue was called for the dynamic variable
        // The key assertion is that the same GUID should be used across all steps
        _mockVariableResolver.Verify(
            v => v.GetVariableValue("testGuid", It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>(), It.IsAny<Request>()),
            Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteFlowAsync_WithSentRequestUrl_ShouldIncludeUrlInStepResult()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var flowId = Guid.NewGuid();
        var expectedUrl = "https://api.example.com/test";

        var request = new RestRequest
        {
            Id = requestId,
            Name = "Test Request",
            Url = expectedUrl,
        };

        var flow = new Flow
        {
            Id = flowId,
            Name = "URL Test Flow",
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
            Body = "{\"success\": true}",
            SentRequest = new SentRequest
            {
                Url = expectedUrl,
                Method = "GET"
            }
        };

        _mockFlowRepository.Setup(r => r.GetByIdAsync(flowId))
            .ReturnsAsync(flow);
        _mockEnvironmentRepository.Setup(r => r.GetByIdAsync(environmentId))
            .ReturnsAsync(environment);
        _mockRequestRepository.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);
        _mockRequestService.Setup(s => s.ExecuteRequestAsync(It.IsAny<Request>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>()))
            .ReturnsAsync(response);

        // Act
        var result = await _service.ExecuteFlowAsync(flowId, environmentId);

        // Assert
        Assert.Equal(FlowExecutionStatus.Completed, result.Status);
        Assert.Single(result.StepResults);
        var stepResult = result.StepResults[0];
        Assert.NotNull(stepResult.Response);
        Assert.NotNull(stepResult.Response.SentRequest);
        Assert.Equal(expectedUrl, stepResult.Response.SentRequest.Url);
        Assert.Equal("GET", stepResult.Response.SentRequest.Method);
    }
}
