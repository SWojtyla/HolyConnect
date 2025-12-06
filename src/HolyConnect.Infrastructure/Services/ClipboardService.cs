using HolyConnect.Application.Interfaces;

namespace HolyConnect.Infrastructure.Services;

/// <summary>
/// Service for clipboard operations using MAUI Clipboard API.
/// </summary>
public class ClipboardService : IClipboardService
{
    public async Task SetTextAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        try
        {
            await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.SetTextAsync(text);
        }
        catch
        {
            // Silently fail if clipboard is unavailable
            // This can happen in some environments or during tests
        }
    }

    public async Task<string?> GetTextAsync()
    {
        try
        {
            return await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.GetTextAsync();
        }
        catch
        {
            return null;
        }
    }
}
