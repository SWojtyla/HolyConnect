using System.Collections.Concurrent;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

/// <summary>
/// Factory for selecting the appropriate request executor based on request type.
/// Caches executors for better performance in a thread-safe manner.
/// </summary>
public class RequestExecutorFactory : IRequestExecutorFactory
{
    private readonly IEnumerable<IRequestExecutor> _executors;
    private readonly ConcurrentDictionary<Type, IRequestExecutor> _executorCache;

    public RequestExecutorFactory(IEnumerable<IRequestExecutor> executors)
    {
        _executors = executors ?? throw new ArgumentNullException(nameof(executors));
        _executorCache = new ConcurrentDictionary<Type, IRequestExecutor>();
    }

    public IRequestExecutor GetExecutor(Request request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var requestType = request.GetType();

        // GetOrAdd is thread-safe and ensures the factory function is only called once per key
        return _executorCache.GetOrAdd(requestType, _ =>
        {
            var executor = _executors.FirstOrDefault(e => e.CanExecute(request));

            if (executor == null)
            {
                throw new NotSupportedException($"No executor found for request type: {request.Type}");
            }

            return executor;
        });
    }
}
