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

    public async Task<RequestResponse> ExecuteRequestAsync(Request request)
    {
        return await ExecuteRequestAsync(request, null, null);
    }

    public async Task<RequestResponse> ExecuteRequestAsync(Request request, Domain.Entities.Environment? environment, Collection? collection)
    {
        var executor = _executionContext.ExecutorFactory.GetExecutor(request);

        // Resolve variables before execution using provided or active environment
        var resolvedRequest = await ResolveRequestVariablesAsync(request, environment, collection);

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

    private async Task<Request> ResolveRequestVariablesAsync(Request request, Domain.Entities.Environment? providedEnvironment = null, Collection? providedCollection = null)
    {
        Domain.Entities.Environment? environment = providedEnvironment;
        Collection? collection = providedCollection;

        // If environment not provided, load active environment with secrets merged
        if (environment == null)
        {
            var activeEnvId = await _executionContext.ActiveEnvironment.GetActiveEnvironmentIdAsync();
            if (!activeEnvId.HasValue)
            {
                // No active environment - continue without variable resolution
                // Variables will remain as placeholders in the request
                return request;
            }
            
            environment = await _environmentService.GetEnvironmentByIdAsync(activeEnvId.Value);
            if (environment == null)
            {
                return request;
            }
        }

        // If collection not provided, load it if request has a collection ID
        if (collection == null && request.CollectionId.HasValue)
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

    public async Task UpdateRequestOrderAsync(IEnumerable<(Guid RequestId, int OrderIndex)> requestOrders)
    {
        foreach (var (requestId, orderIndex) in requestOrders)
        {
            var request = await _repositories.Requests.GetByIdAsync(requestId);
            if (request != null)
            {
                request.OrderIndex = orderIndex;
                await _repositories.Requests.UpdateAsync(request);
            }
        }
    }

    public async Task MoveRequestAsync(Guid requestId, bool moveUp)
    {
        var request = await _repositories.Requests.GetByIdAsync(requestId);
        if (request == null)
        {
            throw new InvalidOperationException($"Request with ID {requestId} not found.");
        }

        // Get all requests with the same collection ID (or no collection)
        var allRequests = await _repositories.Requests.GetAllAsync();
        var siblings = allRequests
            .Where(r => r.CollectionId == request.CollectionId)
            .OrderBy(r => r.OrderIndex)
            .ThenBy(r => r.CreatedAt)
            .ToList();

        var currentIndex = siblings.FindIndex(r => r.Id == requestId);
        if (currentIndex == -1) return;

        // Determine target index based on direction
        int targetIndex = moveUp ? currentIndex - 1 : currentIndex + 1;

        // Check bounds
        if (targetIndex < 0 || targetIndex >= siblings.Count) return;

        // Swap OrderIndex values
        var targetRequest = siblings[targetIndex];
        var tempOrderIndex = request.OrderIndex;
        request.OrderIndex = targetRequest.OrderIndex;
        targetRequest.OrderIndex = tempOrderIndex;

        await _repositories.Requests.UpdateAsync(request);
        await _repositories.Requests.UpdateAsync(targetRequest);
    }
}
