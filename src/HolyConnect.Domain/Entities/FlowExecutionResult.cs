namespace HolyConnect.Domain.Entities;

/// <summary>
/// Represents the result of executing a flow.
/// Contains the results of each step and overall flow status.
/// </summary>
public class FlowExecutionResult
{
    public Guid FlowId { get; set; }
    public string FlowName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public FlowExecutionStatus Status { get; set; }
    public List<FlowStepResult> StepResults { get; set; } = new();
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Total duration of the flow execution in milliseconds.
    /// </summary>
    public long TotalDurationMs => CompletedAt.HasValue 
        ? (long)(CompletedAt.Value - StartedAt).TotalMilliseconds 
        : 0;
}

/// <summary>
/// Represents the result of executing a single step in a flow.
/// </summary>
public class FlowStepResult
{
    public Guid StepId { get; set; }
    public int StepOrder { get; set; }
    public string RequestName { get; set; } = string.Empty;
    public Guid RequestId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public FlowStepStatus Status { get; set; }
    public RequestResponse? Response { get; set; }
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Duration of this step execution in milliseconds.
    /// </summary>
    public long DurationMs => CompletedAt.HasValue 
        ? (long)(CompletedAt.Value - StartedAt).TotalMilliseconds 
        : 0;
}

public enum FlowExecutionStatus
{
    /// <summary>
    /// Flow execution is in progress.
    /// </summary>
    Running,
    
    /// <summary>
    /// Flow execution completed successfully.
    /// </summary>
    Completed,
    
    /// <summary>
    /// Flow execution failed due to an error.
    /// </summary>
    Failed,
    
    /// <summary>
    /// Flow execution was cancelled by the user.
    /// </summary>
    Cancelled
}

public enum FlowStepStatus
{
    /// <summary>
    /// Step has not been executed yet.
    /// </summary>
    Pending,
    
    /// <summary>
    /// Step is currently executing.
    /// </summary>
    Running,
    
    /// <summary>
    /// Step completed successfully.
    /// </summary>
    Success,
    
    /// <summary>
    /// Step failed but flow continued (ContinueOnError = true).
    /// </summary>
    FailedContinued,
    
    /// <summary>
    /// Step failed and flow stopped.
    /// </summary>
    Failed,
    
    /// <summary>
    /// Step was skipped (disabled or previous step failed without continue).
    /// </summary>
    Skipped
}
