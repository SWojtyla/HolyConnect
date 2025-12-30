namespace HolyConnect.Domain.Entities;

/// <summary>
/// Represents a text field in multipart/form-data request
/// </summary>
public class FormDataField
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}
