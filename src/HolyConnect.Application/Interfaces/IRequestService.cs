using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

public interface IRequestService
{
    Task<Request> CreateRequestAsync(Request request);
    Task<IEnumerable<Request>> GetAllRequestsAsync();
    Task<Request?> GetRequestByIdAsync(Guid id);
    Task<IEnumerable<Request>> GetRequestsByCollectionIdAsync(Guid collectionId);
    Task<IEnumerable<Request>> GetRequestsByEnvironmentIdAsync(Guid environmentId);
    Task<Request> UpdateRequestAsync(Request request);
    Task DeleteRequestAsync(Guid id);
    Task<RequestResponse> ExecuteRequestAsync(Request request);
}
