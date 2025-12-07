using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Service for managing and executing flows (sequences of requests).
/// </summary>
public interface IFlowService
{
    /// <summary>
    /// Creates a new flow.
    /// </summary>
    Task<Flow> CreateFlowAsync(Flow flow);
    
    /// <summary>
    /// Gets all flows.
    /// </summary>
    Task<IEnumerable<Flow>> GetAllFlowsAsync();
    
    /// <summary>
    /// Gets a flow by ID.
    /// </summary>
    Task<Flow?> GetFlowByIdAsync(Guid id);
    
    /// <summary>
    /// Gets all flows for a specific environment.
    /// </summary>
    Task<IEnumerable<Flow>> GetFlowsByEnvironmentIdAsync(Guid environmentId);
    
    /// <summary>
    /// Gets all flows for a specific collection.
    /// </summary>
    Task<IEnumerable<Flow>> GetFlowsByCollectionIdAsync(Guid collectionId);
    
    /// <summary>
    /// Updates an existing flow.
    /// </summary>
    Task<Flow> UpdateFlowAsync(Flow flow);
    
    /// <summary>
    /// Deletes a flow.
    /// </summary>
    Task DeleteFlowAsync(Guid id);
    
    /// <summary>
    /// Executes a flow, running all enabled steps in sequence.
    /// Variables extracted from each step are made available to subsequent steps.
    /// </summary>
    Task<FlowExecutionResult> ExecuteFlowAsync(Guid flowId, CancellationToken cancellationToken = default);
}
