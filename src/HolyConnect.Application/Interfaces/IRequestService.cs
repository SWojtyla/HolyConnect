using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

public interface IRequestService
{
    Task<Request> CreateRequestAsync(Request request);
    Task<IEnumerable<Request>> GetAllRequestsAsync();
    Task<Request?> GetRequestByIdAsync(Guid id);
    Task<IEnumerable<Request>> GetRequestsByCollectionIdAsync(Guid collectionId);
    Task<Request> UpdateRequestAsync(Request request);
    Task DeleteRequestAsync(Guid id);
    Task<RequestResponse> ExecuteRequestAsync(Request request);
    Task<RequestResponse> ExecuteRequestAsync(Request request, Domain.Entities.Environment? environment, Collection? collection);
    
    /// <summary>
    /// Updates the order index of multiple requests in a single operation.
    /// </summary>
    Task UpdateRequestOrderAsync(IEnumerable<(Guid RequestId, int OrderIndex)> requestOrders);
    
    /// <summary>
    /// Moves a request up or down in the order (within the same collection/environment).
    /// </summary>
    Task MoveRequestAsync(Guid requestId, bool moveUp);
}
