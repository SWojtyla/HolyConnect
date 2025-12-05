using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

public class RequestService
{
    private readonly IRepository<Request> _requestRepository;
    private readonly IEnumerable<IRequestExecutor> _executors;

    public RequestService(IRepository<Request> requestRepository, IEnumerable<IRequestExecutor> executors)
    {
        _requestRepository = requestRepository;
        _executors = executors;
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

        return await executor.ExecuteAsync(request);
    }
}
