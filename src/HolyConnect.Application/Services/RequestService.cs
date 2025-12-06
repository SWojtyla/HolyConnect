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

    public RequestService(
        IRepository<Request> requestRepository,
        IRepository<Domain.Entities.Environment> environmentRepository,
        IRepository<Collection> collectionRepository,
        IEnumerable<IRequestExecutor> executors,
        IVariableResolver variableResolver,
        IRequestHistoryService? historyService = null)
    {
        _requestRepository = requestRepository;
        _environmentRepository = environmentRepository;
        _collectionRepository = collectionRepository;
        _executors = executors;
        _variableResolver = variableResolver;
        _historyService = historyService;
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

        // Save to history if history service is available
        if (_historyService != null && response.SentRequest != null)
        {
            var historyEntry = new RequestHistoryEntry
            {
                RequestName = request.Name,
                RequestType = request.Type,
                SentRequest = response.SentRequest,
                Response = response
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
        Request resolvedRequest = request switch
        {
            RestRequest restRequest => CloneRestRequest(restRequest),
            GraphQLRequest graphQLRequest => CloneGraphQLRequest(graphQLRequest),
            _ => throw new NotSupportedException($"Request type {request.Type} is not supported for variable resolution")
        };

        // Resolve variables in common properties
        resolvedRequest.Url = _variableResolver.ResolveVariables(resolvedRequest.Url, environment, collection);

        var resolvedHeaders = new Dictionary<string, string>();
        foreach (var header in resolvedRequest.Headers)
        {
            var resolvedKey = _variableResolver.ResolveVariables(header.Key, environment, collection);
            var resolvedValue = _variableResolver.ResolveVariables(header.Value, environment, collection);
            resolvedHeaders[resolvedKey] = resolvedValue;
        }
        resolvedRequest.Headers = resolvedHeaders;

        // Resolve variables in authentication fields
        if (!string.IsNullOrEmpty(resolvedRequest.BasicAuthUsername))
        {
            resolvedRequest.BasicAuthUsername = _variableResolver.ResolveVariables(resolvedRequest.BasicAuthUsername, environment, collection);
        }
        if (!string.IsNullOrEmpty(resolvedRequest.BasicAuthPassword))
        {
            resolvedRequest.BasicAuthPassword = _variableResolver.ResolveVariables(resolvedRequest.BasicAuthPassword, environment, collection);
        }
        if (!string.IsNullOrEmpty(resolvedRequest.BearerToken))
        {
            resolvedRequest.BearerToken = _variableResolver.ResolveVariables(resolvedRequest.BearerToken, environment, collection);
        }

        // Resolve variables in request-specific properties
        if (resolvedRequest is RestRequest resolvedRestRequest)
        {
            if (!string.IsNullOrEmpty(resolvedRestRequest.Body))
            {
                resolvedRestRequest.Body = _variableResolver.ResolveVariables(resolvedRestRequest.Body, environment, collection);
            }

            var resolvedQueryParams = new Dictionary<string, string>();
            foreach (var param in resolvedRestRequest.QueryParameters)
            {
                var resolvedKey = _variableResolver.ResolveVariables(param.Key, environment, collection);
                var resolvedValue = _variableResolver.ResolveVariables(param.Value, environment, collection);
                resolvedQueryParams[resolvedKey] = resolvedValue;
            }
            resolvedRestRequest.QueryParameters = resolvedQueryParams;
        }
        else if (resolvedRequest is GraphQLRequest resolvedGraphQLRequest)
        {
            if (!string.IsNullOrEmpty(resolvedGraphQLRequest.Query))
            {
                resolvedGraphQLRequest.Query = _variableResolver.ResolveVariables(resolvedGraphQLRequest.Query, environment, collection);
            }

            if (!string.IsNullOrEmpty(resolvedGraphQLRequest.Variables))
            {
                resolvedGraphQLRequest.Variables = _variableResolver.ResolveVariables(resolvedGraphQLRequest.Variables, environment, collection);
            }

            if (!string.IsNullOrEmpty(resolvedGraphQLRequest.OperationName))
            {
                resolvedGraphQLRequest.OperationName = _variableResolver.ResolveVariables(resolvedGraphQLRequest.OperationName, environment, collection);
            }
        }

        return resolvedRequest;
    }

    private RestRequest CloneRestRequest(RestRequest source)
    {
        return new RestRequest
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            Url = source.Url,
            Headers = new Dictionary<string, string>(source.Headers),
            DisabledHeaders = new HashSet<string>(source.DisabledHeaders),
            CollectionId = source.CollectionId,
            Collection = source.Collection,
            EnvironmentId = source.EnvironmentId,
            Environment = source.Environment,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            Method = source.Method,
            Body = source.Body,
            ContentType = source.ContentType,
            BodyType = source.BodyType,
            QueryParameters = new Dictionary<string, string>(source.QueryParameters),
            DisabledQueryParameters = new HashSet<string>(source.DisabledQueryParameters),
            AuthType = source.AuthType,
            BasicAuthUsername = source.BasicAuthUsername,
            BasicAuthPassword = source.BasicAuthPassword,
            BearerToken = source.BearerToken
        };
    }

    private GraphQLRequest CloneGraphQLRequest(GraphQLRequest source)
    {
        return new GraphQLRequest
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            Url = source.Url,
            Headers = new Dictionary<string, string>(source.Headers),
            DisabledHeaders = new HashSet<string>(source.DisabledHeaders),
            CollectionId = source.CollectionId,
            Collection = source.Collection,
            EnvironmentId = source.EnvironmentId,
            Environment = source.Environment,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            Query = source.Query,
            Variables = source.Variables,
            OperationName = source.OperationName,
            AuthType = source.AuthType,
            BasicAuthUsername = source.BasicAuthUsername,
            BasicAuthPassword = source.BasicAuthPassword,
            BearerToken = source.BearerToken
        };
    }
}
