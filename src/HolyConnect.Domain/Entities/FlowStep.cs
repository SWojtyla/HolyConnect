namespace HolyConnect.Domain.Entities;

/// <summary>
/// Represents a single step in a flow execution sequence.
/// Each step executes a request and can extract values to be used in subsequent steps.
/// </summary>
public class FlowStep
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public Guid RequestId { get; set; }
    public Request? Request { get; set; }
    public Guid FlowId { get; set; }
    public Flow? Flow { get; set; }
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// If true, the flow will continue even if this step fails.
    /// If false, the flow will stop on error.
    /// </summary>
    public bool ContinueOnError { get; set; } = false;
    
    /// <summary>
    /// Optional delay in milliseconds to wait before executing this step.
    /// Useful for rate limiting or waiting for async operations to complete.
    /// </summary>
    public int? DelayBeforeExecutionMs { get; set; }
}
