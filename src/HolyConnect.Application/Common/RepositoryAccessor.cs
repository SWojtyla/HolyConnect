using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Common;

/// <summary>
/// Aggregates commonly-used repositories to reduce constructor parameter count.
/// Provides centralized access to all entity repositories.
/// </summary>
public class RepositoryAccessor
{
    public IRepository<Request> Requests { get; }
    public IRepository<Collection> Collections { get; }
    public IRepository<Domain.Entities.Environment> Environments { get; }
    public IRepository<Flow> Flows { get; }
    public IRepository<RequestHistoryEntry> History { get; }

    public RepositoryAccessor(
        IRepository<Request> requests,
        IRepository<Collection> collections,
        IRepository<Domain.Entities.Environment> environments,
        IRepository<Flow> flows,
        IRepository<RequestHistoryEntry> history)
    {
        Requests = requests;
        Collections = collections;
        Environments = environments;
        Flows = flows;
        History = history;
    }
}
