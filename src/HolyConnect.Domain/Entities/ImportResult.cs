namespace HolyConnect.Domain.Entities;

/// <summary>
/// Represents the result of an import operation
/// </summary>
public class ImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Request? ImportedRequest { get; set; }
    public List<Request> ImportedRequests { get; set; } = new();
    public List<Collection> ImportedCollections { get; set; } = new();
    public List<Environment> ImportedEnvironments { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public int TotalFilesProcessed { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
}

/// <summary>
/// Supported import source formats
/// </summary>
public enum ImportSource
{
    Curl,
    Bruno
}
