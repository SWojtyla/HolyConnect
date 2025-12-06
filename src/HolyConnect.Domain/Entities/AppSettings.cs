namespace HolyConnect.Domain.Entities;

public class AppSettings
{
    public string StoragePath { get; set; } = string.Empty;
    public bool IsDarkMode { get; set; } = false;
    public RequestLayout Layout { get; set; } = RequestLayout.Horizontal;
    public ThemePreset SelectedTheme { get; set; } = ThemePreset.Default;
    public CustomTheme CustomTheme { get; set; } = new CustomTheme();
}

public enum RequestLayout
{
    Horizontal,
    Vertical
}
