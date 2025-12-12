using System.Net.Http;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

/// <summary>
/// Service for managing and executing flows (sequences of requests).
/// </summary>
public class FlowService : IFlowService
{
    private readonly IRepository<Flow> _flowRepository;
    private readonly IRepository<Request> _requestRepository;
    private readonly IActiveEnvironmentService _activeEnvironmentService;
    private readonly IRepository<Collection> _collectionRepository;
    private readonly IRequestService _requestService;
    private readonly IVariableResolver _variableResolver;

    public FlowService(
        IRepository<Flow> flowRepository,
        IRepository<Request> requestRepository,
        IActiveEnvironmentService activeEnvironmentService,
        IRepository<Collection> collectionRepository,
        IRequestService requestService,
        IVariableResolver variableResolver)
    {
        _flowRepository = flowRepository;
        _requestRepository = requestRepository;
        _activeEnvironmentService = activeEnvironmentService;
        _collectionRepository = collectionRepository;
        _requestService = requestService;
        _variableResolver = variableResolver;
    }

    public async Task<Flow> CreateFlowAsync(Flow flow)
    {
        flow.Id = Guid.NewGuid();
        flow.CreatedAt = DateTime.UtcNow;
        
        // Assign IDs to steps
        foreach (var step in flow.Steps)
        {
            step.Id = Guid.NewGuid();
            step.FlowId = flow.Id;
        }
        
        return await _flowRepository.AddAsync(flow);
    }

    public async Task<IEnumerable<Flow>> GetAllFlowsAsync()
    {
        return await _flowRepository.GetAllAsync();
    }

    public async Task<Flow?> GetFlowByIdAsync(Guid id)
    {
        return await _flowRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Flow>> GetFlowsByCollectionIdAsync(Guid collectionId)
    {
        var allFlows = await _flowRepository.GetAllAsync();
        return allFlows.Where(f => f.CollectionId == collectionId);
    }

    public async Task<Flow> UpdateFlowAsync(Flow flow)
    {
        return await _flowRepository.UpdateAsync(flow);
    }

    public async Task DeleteFlowAsync(Guid id)
    {
        await _flowRepository.DeleteAsync(id);
    }

    public async Task<FlowExecutionResult> ExecuteFlowAsync(Guid flowId, CancellationToken cancellationToken = default)
    {
        var flow = await _flowRepository.GetByIdAsync(flowId);
        if (flow == null)
        {
            throw new InvalidOperationException($"Flow with ID {flowId} not found.");
        }

        var result = new FlowExecutionResult
        {
            FlowId = flow.Id,
            FlowName = flow.Name,
            StartedAt = DateTime.UtcNow,
            Status = FlowExecutionStatus.Running
        };

        try
        {
            // Get active environment for variable management
            var environment = await _activeEnvironmentService.GetActiveEnvironmentAsync();
            if (environment == null)
            {
                throw new InvalidOperationException("No active environment set. Please select an environment before executing the flow.");
            }

            // Get collection if specified
            Collection? collection = null;
            if (flow.CollectionId.HasValue)
            {
                collection = await _collectionRepository.GetByIdAsync(flow.CollectionId.Value);
            }

            // Create a temporary variables dictionary for flow execution
            // This allows variables to be passed between steps without permanently modifying the environment
            var flowVariables = new Dictionary<string, string>(environment.Variables);
            if (collection != null)
            {
                foreach (var kvp in collection.Variables)
                {
                    flowVariables[kvp.Key] = kvp.Value;
                }
            }

            // Execute steps in order
            var sortedSteps = flow.Steps.OrderBy(s => s.Order).ToList();
            
            foreach (var step in sortedSteps)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    result.Status = FlowExecutionStatus.Cancelled;
                    result.CompletedAt = DateTime.UtcNow;
                    return result;
                }

                var stepResult = await ExecuteStepAsync(step, environment, collection, flowVariables, cancellationToken);
                result.StepResults.Add(stepResult);

                // If step failed and should not continue, stop execution
                if (stepResult.Status == FlowStepStatus.Failed)
                {
                    result.Status = FlowExecutionStatus.Failed;
                    result.ErrorMessage = $"Step {stepResult.StepOrder} ({stepResult.RequestName}) failed: {stepResult.ErrorMessage}";
                    result.CompletedAt = DateTime.UtcNow;
                    return result;
                }
            }

            result.Status = FlowExecutionStatus.Completed;
            result.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            result.Status = FlowExecutionStatus.Failed;
            result.ErrorMessage = ex.Message;
            result.CompletedAt = DateTime.UtcNow;
        }

        return result;
    }

    private async Task<FlowStepResult> ExecuteStepAsync(
        FlowStep step, 
        Domain.Entities.Environment environment, 
        Collection? collection,
        Dictionary<string, string> flowVariables,
        CancellationToken cancellationToken)
    {
        var stepResult = new FlowStepResult
        {
            StepId = step.Id,
            StepOrder = step.Order,
            StartedAt = DateTime.UtcNow,
            Status = FlowStepStatus.Running
        };

        try
        {
            // Check if step is enabled
            if (!step.IsEnabled)
            {
                stepResult.Status = FlowStepStatus.Skipped;
                stepResult.CompletedAt = DateTime.UtcNow;
                return stepResult;
            }

            // Apply delay if specified
            if (step.DelayBeforeExecutionMs.HasValue && step.DelayBeforeExecutionMs.Value > 0)
            {
                await Task.Delay(step.DelayBeforeExecutionMs.Value, cancellationToken);
            }

            // Get the request
            var request = await _requestRepository.GetByIdAsync(step.RequestId);
            if (request == null)
            {
                throw new InvalidOperationException($"Request with ID {step.RequestId} not found.");
            }

            stepResult.RequestName = request.Name;
            stepResult.RequestId = request.Id;

            // Temporarily update environment/collection variables with flow variables
            var originalEnvVariables = new Dictionary<string, string>(environment.Variables);
            var originalCollectionVariables = collection != null 
                ? new Dictionary<string, string>(collection.Variables) 
                : null;

            try
            {
                // Merge flow variables into environment (temporary)
                foreach (var kvp in flowVariables)
                {
                    environment.Variables[kvp.Key] = kvp.Value;
                }

                // Execute the request
                var response = await _requestService.ExecuteRequestAsync(request);
                stepResult.Response = response;

                // Extract the current state of variables after request execution
                // (RequestService may have updated them through ResponseExtractions)
                foreach (var kvp in environment.Variables)
                {
                    flowVariables[kvp.Key] = kvp.Value;
                }
                if (collection != null)
                {
                    foreach (var kvp in collection.Variables)
                    {
                        flowVariables[kvp.Key] = kvp.Value;
                    }
                }

                // Check if the response status code indicates success
                // Status codes in the 2xx range (200-299) are considered successful
                // Note: This logic is also implemented in ResponseHelper.IsSuccessStatusCode()
                // but we don't use it here to avoid a dependency from Application to Infrastructure layer
                if (response.StatusCode >= 200 && response.StatusCode <= 299)
                {
                    stepResult.Status = FlowStepStatus.Success;
                }
                else if (response.StatusCode == 0)
                {
                    // Status code 0 indicates an error (network error, exception, etc.)
                    // This should be handled as a failure
                    throw new HttpRequestException($"Request failed: {response.StatusMessage}");
                }
                else
                {
                    // Non-success HTTP status codes (4xx, 5xx, etc.) should be treated as failures
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {response.StatusMessage}");
                }
            }
            finally
            {
                // Restore original variables
                environment.Variables = originalEnvVariables;
                if (collection != null && originalCollectionVariables != null)
                {
                    collection.Variables = originalCollectionVariables;
                }
            }

            stepResult.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            stepResult.ErrorMessage = ex.Message;
            
            if (step.ContinueOnError)
            {
                stepResult.Status = FlowStepStatus.FailedContinued;
            }
            else
            {
                stepResult.Status = FlowStepStatus.Failed;
            }
            
            stepResult.CompletedAt = DateTime.UtcNow;
        }

        return stepResult;
    }
}
