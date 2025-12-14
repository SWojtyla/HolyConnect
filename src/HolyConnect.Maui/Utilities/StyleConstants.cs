namespace HolyConnect.Maui.Utilities;

/// <summary>
/// Provides constants for common CSS styles and dimensions used throughout the application.
/// Centralizes magic values to ensure consistency and ease of maintenance.
/// </summary>
public static class StyleConstants
{
    // Height constants
    public const string FullHeightWithHeader = "calc(100vh - 4rem)";
    public const string FullHeightWithToolbar = "calc(100vh - 11.25rem)";
    
    // Common flex layouts
    public const string FlexColumn = "display: flex; flex-direction: column;";
    public const string FlexRow = "display: flex; flex-direction: row;";
    public const string FlexGrow = "flex: 1;";
    
    // Overflow patterns
    public const string OverflowHidden = "overflow: hidden;";
    public const string OverflowYAuto = "overflow-y: auto;";
    public const string OverflowAuto = "overflow: auto;";
    
    // Combined patterns for common use cases
    public const string FlexColumnWithOverflow = "display: flex; flex-direction: column; overflow: hidden;";
    public const string FullHeightFlexColumn = "height: 100%; display: flex; flex-direction: column;";
    public const string FullHeightFlexColumnWithOverflow = "height: 100%; display: flex; flex-direction: column; overflow: hidden;";
}
