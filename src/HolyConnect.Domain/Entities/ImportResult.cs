namespace HolyConnect.Domain.Entities;

/// <summary>
/// Represents the result of an import operation
/// </summary>
public class ImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Request? ImportedRequest { get; set; }
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Supported import source formats
/// </summary>
public enum ImportSource
{
    Curl,
    Bruno
}
