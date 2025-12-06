namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Service for clipboard operations.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Copies text to the system clipboard.
    /// </summary>
    /// <param name="text">The text to copy</param>
    /// <returns>A task representing the async operation</returns>
    Task SetTextAsync(string text);

    /// <summary>
    /// Gets text from the system clipboard.
    /// </summary>
    /// <returns>The text from clipboard, or null if empty</returns>
    Task<string?> GetTextAsync();
}
