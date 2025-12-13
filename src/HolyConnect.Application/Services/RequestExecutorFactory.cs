using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

/// <summary>
/// Factory for selecting the appropriate request executor based on request type.
/// Caches executors for better performance.
/// </summary>
public class RequestExecutorFactory : IRequestExecutorFactory
{
    private readonly IEnumerable<IRequestExecutor> _executors;
    private readonly Dictionary<Type, IRequestExecutor> _executorCache;

    public RequestExecutorFactory(IEnumerable<IRequestExecutor> executors)
    {
        _executors = executors ?? throw new ArgumentNullException(nameof(executors));
        _executorCache = new Dictionary<Type, IRequestExecutor>();
    }

    public IRequestExecutor GetExecutor(Request request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var requestType = request.GetType();

        // Check cache first
        if (_executorCache.TryGetValue(requestType, out var cachedExecutor))
        {
            return cachedExecutor;
        }

        // Find executor that can handle this request type
        var executor = _executors.FirstOrDefault(e => e.CanExecute(request));

        if (executor == null)
        {
            throw new NotSupportedException($"No executor found for request type: {request.Type}");
        }

        // Cache the executor for this request type
        _executorCache[requestType] = executor;

        return executor;
    }
}
