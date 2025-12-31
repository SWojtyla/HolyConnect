namespace HolyConnect.Domain.Entities;

/// <summary>
/// Represents a file attachment in multipart/form-data request
/// </summary>
public class FormDataFile
{
    public string Key { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public bool Enabled { get; set; } = true;
}
