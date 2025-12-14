using System.Net.Http;
using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

/// <summary>
/// Service for managing and executing flows (sequences of requests).
/// </summary>
public class FlowService : IFlowService
{
    private readonly RepositoryAccessor _repositories;
    private readonly RequestExecutionContext _executionContext;
    private readonly IRequestService _requestService;

    public FlowService(
        RepositoryAccessor repositories,
        RequestExecutionContext executionContext,
        IRequestService requestService)
    {
        _repositories = repositories;
        _executionContext = executionContext;
        _requestService = requestService;
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
        
        return await _repositories.Flows.AddAsync(flow);
    }

    public async Task<IEnumerable<Flow>> GetAllFlowsAsync()
    {
        return await _repositories.Flows.GetAllAsync();
    }

    public async Task<Flow?> GetFlowByIdAsync(Guid id)
    {
        return await _repositories.Flows.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Flow>> GetFlowsByCollectionIdAsync(Guid collectionId)
    {
        var allFlows = await _repositories.Flows.GetAllAsync();
        return allFlows.Where(f => f.CollectionId == collectionId);
    }

    public async Task<Flow> UpdateFlowAsync(Flow flow)
    {
        return await _repositories.Flows.UpdateAsync(flow);
    }

    public async Task DeleteFlowAsync(Guid id)
    {
        await _repositories.Flows.DeleteAsync(id);
    }

    public async Task<FlowExecutionResult> ExecuteFlowAsync(Guid flowId, Guid environmentId, CancellationToken cancellationToken = default)
    {
        var flow = await _repositories.Flows.GetByIdAsync(flowId);
        if (flow == null)
        {
            throw new InvalidOperationException($"Flow with ID {flowId} not found.");
        }

        // Get the specified environment
        var environment = await _repositories.Environments.GetByIdAsync(environmentId);
        if (environment == null)
        {
            throw new InvalidOperationException($"Environment with ID {environmentId} not found.");
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

            // Get collection if specified
            Collection? collection = null;
            if (flow.CollectionId.HasValue)
            {
                collection = await _repositories.Collections.GetByIdAsync(flow.CollectionId.Value);
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
        var stepResult = CreateStepResult(step);

        try
        {
            // Check if step is enabled
            if (!step.IsEnabled)
            {
                return MarkStepAsSkipped(stepResult);
            }

            // Apply delay if specified
            await ApplyStepDelayAsync(step, cancellationToken);

            // Get the request
            var request = await GetStepRequestAsync(step, stepResult);

            // Execute request with temporary variable context
            await ExecuteRequestWithVariablesAsync(
                request, 
                stepResult, 
                environment, 
                collection, 
                flowVariables);

            stepResult.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            HandleStepError(stepResult, step, ex);
        }

        return stepResult;
    }

    private FlowStepResult CreateStepResult(FlowStep step)
    {
        return new FlowStepResult
        {
            StepId = step.Id,
            StepOrder = step.Order,
            StartedAt = DateTime.UtcNow,
            Status = FlowStepStatus.Running
        };
    }

    private FlowStepResult MarkStepAsSkipped(FlowStepResult stepResult)
    {
        stepResult.Status = FlowStepStatus.Skipped;
        stepResult.CompletedAt = DateTime.UtcNow;
        return stepResult;
    }

    private async Task ApplyStepDelayAsync(FlowStep step, CancellationToken cancellationToken)
    {
        if (step.DelayBeforeExecutionMs.HasValue && step.DelayBeforeExecutionMs.Value > 0)
        {
            await Task.Delay(step.DelayBeforeExecutionMs.Value, cancellationToken);
        }
    }

    private async Task<Request> GetStepRequestAsync(FlowStep step, FlowStepResult stepResult)
    {
        var request = await _repositories.Requests.GetByIdAsync(step.RequestId);
        if (request == null)
        {
            throw new InvalidOperationException($"Request with ID {step.RequestId} not found.");
        }

        stepResult.RequestName = request.Name;
        stepResult.RequestId = request.Id;
        
        return request;
    }

    private async Task ExecuteRequestWithVariablesAsync(
        Request request,
        FlowStepResult stepResult,
        Domain.Entities.Environment environment,
        Collection? collection,
        Dictionary<string, string> flowVariables)
    {
        // Save original state
        var originalEnvVariables = new Dictionary<string, string>(environment.Variables);
        var originalCollectionVariables = collection != null 
            ? new Dictionary<string, string>(collection.Variables) 
            : null;

        try
        {
            // Merge flow variables into environment (temporary)
            MergeFlowVariables(environment, collection, flowVariables);

            // Execute the request
            var response = await _requestService.ExecuteRequestAsync(request);
            stepResult.Response = response;

            // Extract updated variables after request execution
            UpdateFlowVariablesFromResponse(environment, collection, flowVariables);

            // Validate response status
            ValidateResponseStatus(response, stepResult);
        }
        finally
        {
            // Restore original variables
            RestoreOriginalVariables(environment, collection, originalEnvVariables, originalCollectionVariables);
        }
    }

    private void MergeFlowVariables(
        Domain.Entities.Environment environment,
        Collection? collection,
        Dictionary<string, string> flowVariables)
    {
        foreach (var kvp in flowVariables)
        {
            environment.Variables[kvp.Key] = kvp.Value;
        }
    }

    private void UpdateFlowVariablesFromResponse(
        Domain.Entities.Environment environment,
        Collection? collection,
        Dictionary<string, string> flowVariables)
    {
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
    }

    private void ValidateResponseStatus(RequestResponse response, FlowStepResult stepResult)
    {
        if (HttpStatusCodeHelper.IsSuccessStatusCode(response.StatusCode))
        {
            stepResult.Status = FlowStepStatus.Success;
            return;
        }
        
        if (response.StatusCode == 0)
        {
            // Status code 0 indicates an error (network error, exception, etc.)
            throw new HttpRequestException($"Request failed: {response.StatusMessage}");
        }
        
        // Non-success HTTP status codes (4xx, 5xx, etc.) should be treated as failures
        throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {response.StatusMessage}");
    }

    private void RestoreOriginalVariables(
        Domain.Entities.Environment environment,
        Collection? collection,
        Dictionary<string, string> originalEnvVariables,
        Dictionary<string, string>? originalCollectionVariables)
    {
        environment.Variables = originalEnvVariables;
        if (collection != null && originalCollectionVariables != null)
        {
            collection.Variables = originalCollectionVariables;
        }
    }

    private void HandleStepError(FlowStepResult stepResult, FlowStep step, Exception ex)
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
}
