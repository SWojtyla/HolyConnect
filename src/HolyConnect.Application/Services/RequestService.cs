using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

public class RequestService : IRequestService
{
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Domain.Entities.Environment> _environmentRepository;
    private readonly IRepository<Collection> _collectionRepository;
    private readonly IEnumerable<IRequestExecutor> _executors;
    private readonly IVariableResolver _variableResolver;
    private readonly IRequestHistoryService? _historyService;
    private readonly IResponseValueExtractor? _responseValueExtractor;

    public RequestService(
        IRepository<Request> requestRepository,
        IRepository<Domain.Entities.Environment> environmentRepository,
        IRepository<Collection> collectionRepository,
        IEnumerable<IRequestExecutor> executors,
        IVariableResolver variableResolver,
        IRequestHistoryService? historyService = null,
        IResponseValueExtractor? responseValueExtractor = null)
    {
        _requestRepository = requestRepository;
        _environmentRepository = environmentRepository;
        _collectionRepository = collectionRepository;
        _executors = executors;
        _variableResolver = variableResolver;
        _historyService = historyService;
        _responseValueExtractor = responseValueExtractor;
    }

    public async Task<Request> CreateRequestAsync(Request request)
    {
        request.Id = Guid.NewGuid();
        request.CreatedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;
        return await _requestRepository.AddAsync(request);
    }

    public async Task<IEnumerable<Request>> GetAllRequestsAsync()
    {
        return await _requestRepository.GetAllAsync();
    }

    public async Task<Request?> GetRequestByIdAsync(Guid id)
    {
        return await _requestRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Request>> GetRequestsByCollectionIdAsync(Guid collectionId)
    {
        var allRequests = await _requestRepository.GetAllAsync();
        return allRequests.Where(r => r.CollectionId == collectionId);
    }

    public async Task<IEnumerable<Request>> GetRequestsByEnvironmentIdAsync(Guid environmentId)
    {
        var allRequests = await _requestRepository.GetAllAsync();
        return allRequests.Where(r => r.EnvironmentId == environmentId && r.CollectionId == null);
    }

    public async Task<Request> UpdateRequestAsync(Request request)
    {
        request.UpdatedAt = DateTime.UtcNow;
        return await _requestRepository.UpdateAsync(request);
    }

    public async Task DeleteRequestAsync(Guid id)
    {
        await _requestRepository.DeleteAsync(id);
    }

    public async Task<RequestResponse> ExecuteRequestAsync(Request request)
    {
        var executor = _executors.FirstOrDefault(e => e.CanExecute(request));
        
        if (executor == null)
        {
            throw new NotSupportedException($"No executor found for request type: {request.Type}");
        }

        // Resolve variables before execution
        var resolvedRequest = await ResolveRequestVariablesAsync(request);

        var response = await executor.ExecuteAsync(resolvedRequest);

        // Apply response extractions if configured
        if (_responseValueExtractor != null && request.ResponseExtractions.Any(e => e.IsEnabled))
        {
            await ApplyResponseExtractionsAsync(request, response);
        }

        // Save to history if history service is available
        if (_historyService != null && response.SentRequest != null)
        {
            var historyEntry = new RequestHistoryEntry
            {
                RequestName = request.Name,
                RequestType = request.Type,
                SentRequest = response.SentRequest,
                Response = response,
                RequestId = request.Id,
                EnvironmentId = request.EnvironmentId,
                CollectionId = request.CollectionId
            };
            
            await _historyService.AddHistoryEntryAsync(historyEntry);
        }

        return response;
    }

    private async Task<Request> ResolveRequestVariablesAsync(Request request)
    {
        // Load environment and collection
        var environment = await _environmentRepository.GetByIdAsync(request.EnvironmentId);
        if (environment == null)
        {
            // Environment not found - this shouldn't happen for valid requests, but we'll continue without variable resolution
            // Variables will remain as placeholders in the request
            return request;
        }

        Collection? collection = null;
        if (request.CollectionId.HasValue)
        {
            collection = await _collectionRepository.GetByIdAsync(request.CollectionId.Value);
        }

        // Create a clone to avoid modifying the original request
        var resolvedRequest = RequestCloner.Clone(request);

        // Resolve all variables using the helper
        VariableResolutionHelper.ResolveAllVariables(resolvedRequest, _variableResolver, environment, collection);

        return resolvedRequest;
    }



    private async Task ApplyResponseExtractionsAsync(Request request, RequestResponse response)
    {
        if (_responseValueExtractor == null || string.IsNullOrWhiteSpace(response.Body))
        {
            return;
        }

        // Load environment and collection for variable saving
        var environment = await _environmentRepository.GetByIdAsync(request.EnvironmentId);
        if (environment == null)
        {
            return;
        }

        Collection? collection = null;
        if (request.CollectionId.HasValue)
        {
            collection = await _collectionRepository.GetByIdAsync(request.CollectionId.Value);
        }

        // Determine content type from headers
        var contentType = response.Headers.FirstOrDefault(h => 
            h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)).Value ?? "application/json";

        // Apply each enabled extraction
        foreach (var extraction in request.ResponseExtractions.Where(e => e.IsEnabled))
        {
            try
            {
                var extractedValue = _responseValueExtractor.ExtractValue(response.Body, extraction.Pattern, contentType);
                
                if (extractedValue != null && !string.IsNullOrEmpty(extraction.VariableName))
                {
                    // Save to variable
                    _variableResolver.SetVariableValue(
                        extraction.VariableName, 
                        extractedValue, 
                        environment, 
                        collection, 
                        extraction.SaveToCollection);

                    // Update the repository to persist the changes
                    await _environmentRepository.UpdateAsync(environment);
                    if (collection != null && extraction.SaveToCollection)
                    {
                        await _collectionRepository.UpdateAsync(collection);
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
