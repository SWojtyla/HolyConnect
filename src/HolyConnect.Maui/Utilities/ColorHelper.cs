namespace HolyConnect.Maui.Utilities;

/// <summary>
/// Provides helper methods for determining colors based on various criteria.
/// </summary>
public static class ColorHelper
{
    /// <summary>
    /// Gets the appropriate MudBlazor color based on HTTP status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code</param>
    /// <returns>The appropriate MudBlazor Color</returns>
    public static MudBlazor.Color GetStatusColor(int statusCode) => statusCode switch
    {
        >= 200 and < 300 => MudBlazor.Color.Success,
        >= 300 and < 400 => MudBlazor.Color.Info,
        >= 400 and < 500 => MudBlazor.Color.Warning,
        >= 500 => MudBlazor.Color.Error,
        _ => MudBlazor.Color.Default
    };

    /// <summary>
    /// Gets a human-readable text description of the HTTP status code category.
    /// </summary>
    /// <param name="statusCode">The HTTP status code</param>
    /// <returns>A text description of the status category</returns>
    public static string GetStatusText(int statusCode) => statusCode switch
    {
        >= 200 and < 300 => "Success",
        >= 300 and < 400 => "Redirect",
        >= 400 and < 500 => "Client Error",
        >= 500 => "Server Error",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets a detailed description of the HTTP status code for accessibility purposes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code</param>
    /// <returns>A detailed description suitable for screen readers</returns>
    public static string GetStatusDescription(int statusCode) => statusCode switch
    {
        >= 200 and < 300 => $"HTTP {statusCode}: Request successful",
        >= 300 and < 400 => $"HTTP {statusCode}: Request redirected",
        >= 400 and < 500 => $"HTTP {statusCode}: Client error occurred",
        >= 500 => $"HTTP {statusCode}: Server error occurred",
        _ => $"HTTP {statusCode}: Unknown status"
    };
}
