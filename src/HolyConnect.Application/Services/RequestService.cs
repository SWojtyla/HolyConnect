using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

public class RequestService : IRequestService
{
    private readonly RepositoryAccessor _repositories;
    private readonly RequestExecutionContext _executionContext;
    private readonly IEnvironmentService _environmentService;
    private readonly ICollectionService _collectionService;
    private readonly IRequestHistoryService? _historyService;

    public RequestService(
        RepositoryAccessor repositories,
        RequestExecutionContext executionContext,
        IEnvironmentService environmentService,
        ICollectionService collectionService,
        IRequestHistoryService? historyService = null)
    {
        _repositories = repositories;
        _executionContext = executionContext;
        _environmentService = environmentService;
        _collectionService = collectionService;
        _historyService = historyService;
    }

    public async Task<Request> CreateRequestAsync(Request request)
    {
        request.Id = Guid.NewGuid();
        request.CreatedAt = DateTime.UtcNow;
        return await _repositories.Requests.AddAsync(request);
    }

    public async Task<IEnumerable<Request>> GetAllRequestsAsync()
    {
        return await _repositories.Requests.GetAllAsync();
    }

    public async Task<Request?> GetRequestByIdAsync(Guid id)
    {
        return await _repositories.Requests.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Request>> GetRequestsByCollectionIdAsync(Guid collectionId)
    {
        var allRequests = await _repositories.Requests.GetAllAsync();
        return allRequests.Where(r => r.CollectionId == collectionId);
    }

    public async Task<Request> UpdateRequestAsync(Request request)
    {
        return await _repositories.Requests.UpdateAsync(request);
    }

    public async Task DeleteRequestAsync(Guid id)
    {
        await _repositories.Requests.DeleteAsync(id);
    }

    public async Task ReorderRequestsAsync(IEnumerable<Guid> requestIds)
    {
        var orderedIds = requestIds.ToList();
        
        // Load all requests in parallel
        var loadTasks = orderedIds.Select(id => _repositories.Requests.GetByIdAsync(id));
        var loadedRequests = await Task.WhenAll(loadTasks);
        
        // Filter out nulls and create list
        var requests = loadedRequests.Where(r => r != null).Cast<Request>().ToList();
        
        // Assign new order values and collect items that need updating
        var updateTasks = new List<Task<Request>>();
        for (int i = 0; i < requests.Count; i++)
        {
            if (requests[i].Order != i)
            {
                requests[i].Order = i;
                updateTasks.Add(_repositories.Requests.UpdateAsync(requests[i]));
            }
        }
        
        // Update all changed items in parallel
        if (updateTasks.Any())
        {
            await Task.WhenAll(updateTasks);
        }
    }

    public async Task<RequestResponse> ExecuteRequestAsync(Request request)
    {
        var executor = _executionContext.ExecutorFactory.GetExecutor(request);

        // Resolve variables before execution using active environment
        var resolvedRequest = await ResolveRequestVariablesAsync(request);

        var response = await executor.ExecuteAsync(resolvedRequest);

        // Apply response extractions if configured
        if (_executionContext.ResponseExtractor != null && request.ResponseExtractions.Any(e => e.IsEnabled))
        {
            await ApplyResponseExtractionsAsync(request, response);
        }

        // Save to history if history service is available
        if (_historyService != null && response.SentRequest != null)
        {
            var activeEnvId = await _executionContext.ActiveEnvironment.GetActiveEnvironmentIdAsync();
            var historyEntry = new RequestHistoryEntry
            {
                RequestName = request.Name,
                RequestType = request.Type,
                SentRequest = response.SentRequest,
                Response = response,
                RequestId = request.Id,
                EnvironmentId = activeEnvId,  // Store which environment was active during execution
                CollectionId = request.CollectionId
            };
            
            await _historyService.AddHistoryEntryAsync(historyEntry);
        }

        return response;
    }

    private async Task<Request> ResolveRequestVariablesAsync(Request request)
    {
        // Load active environment with secrets merged
        var activeEnvId = await _executionContext.ActiveEnvironment.GetActiveEnvironmentIdAsync();
        if (!activeEnvId.HasValue)
        {
            // No active environment - continue without variable resolution
            // Variables will remain as placeholders in the request
            return request;
        }
        
        var environment = await _environmentService.GetEnvironmentByIdAsync(activeEnvId.Value);
        if (environment == null)
        {
            return request;
        }

        Collection? collection = null;
        if (request.CollectionId.HasValue)
        {
            // Use service to load collection with secrets merged
            collection = await _collectionService.GetCollectionByIdAsync(request.CollectionId.Value);
        }

        // Create a clone to avoid modifying the original request
        var resolvedRequest = RequestCloner.Clone(request);

        // Resolve all variables using the helper
        VariableResolutionHelper.ResolveAllVariables(resolvedRequest, _executionContext.VariableResolver, environment, collection);

        return resolvedRequest;
    }



    private async Task ApplyResponseExtractionsAsync(Request request, RequestResponse response)
    {
        if (_executionContext.ResponseExtractor == null || string.IsNullOrWhiteSpace(response.Body))
        {
            return;
        }

        // Load active environment with secrets for variable saving
        var activeEnvId = await _executionContext.ActiveEnvironment.GetActiveEnvironmentIdAsync();
        if (!activeEnvId.HasValue)
        {
            return;
        }
        
        var environment = await _environmentService.GetEnvironmentByIdAsync(activeEnvId.Value);
        if (environment == null)
        {
            return;
        }

        Collection? collection = null;
        if (request.CollectionId.HasValue)
        {
            // Use service to load collection with secrets merged
            collection = await _collectionService.GetCollectionByIdAsync(request.CollectionId.Value);
        }

        // Determine content type from headers
        var contentType = response.Headers.FirstOrDefault(h => 
            h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)).Value ?? "application/json";

        // Apply each enabled extraction
        foreach (var extraction in request.ResponseExtractions.Where(e => e.IsEnabled))
        {
            try
            {
                var extractedValue = _executionContext.ResponseExtractor.ExtractValue(response.Body, extraction.Pattern, contentType);
                
                if (extractedValue != null && !string.IsNullOrEmpty(extraction.VariableName))
                {
                    // Save to variable
                    _executionContext.VariableResolver.SetVariableValue(
                        extraction.VariableName, 
                        extractedValue, 
                        environment, 
                        collection, 
                        extraction.SaveToCollection);

                    // Persist the updated environment
                    await _environmentService.UpdateEnvironmentAsync(environment);
                    
                    if (collection != null && extraction.SaveToCollection)
                    {
                        await _collectionService.UpdateCollectionAsync(collection);
                    }
                }
            }
            catch
            {
                // Silently fail for individual extraction errors
                // This allows other extractions to proceed
            }
        }
    }
}
