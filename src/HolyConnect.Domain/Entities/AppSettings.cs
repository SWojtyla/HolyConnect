namespace HolyConnect.Domain.Entities;

public class AppSettings
{
    public string StoragePath { get; set; } = string.Empty;
    public bool IsDarkMode { get; set; } = false;
    public RequestLayout Layout { get; set; } = RequestLayout.Horizontal;
    public ThemePreset Theme { get; set; } = ThemePreset.Default;
}

public enum RequestLayout
{
    Horizontal,
    Vertical
}

public enum ThemePreset
{
    Default,
    Ocean,
    Forest,
    Sunset,
    Monochrome,
    Dark,
    OceanDark,
    ForestDark,
    SunsetDark,
    MonochromeDark
}
