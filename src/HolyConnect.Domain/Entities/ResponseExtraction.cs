namespace HolyConnect.Domain.Entities;

/// <summary>
/// Represents a rule for extracting values from a response body.
/// </summary>
public class ResponseExtraction
{
    /// <summary>
    /// Unique identifier for the extraction rule.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name or description of what is being extracted.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The pattern/path used to extract the value.
    /// For JSON/GraphQL: JSONPath expression (e.g., "$.data.user.id")
    /// For XML: XPath expression (e.g., "//user/id")
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// The variable name to save the extracted value to.
    /// If null, the value is only available for clipboard copy.
    /// </summary>
    public string? VariableName { get; set; }

    /// <summary>
    /// Whether to save the extracted value to a collection variable (true) or environment variable (false).
    /// </summary>
    public bool SaveToCollection { get; set; } = false;

    /// <summary>
    /// Whether this extraction rule is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
