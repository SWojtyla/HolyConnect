namespace HolyConnect.Domain.Entities;

public class AppSettings
{
    public string StoragePath { get; set; } = string.Empty;
    public bool IsDarkMode { get; set; } = false;
    public ThemePreset Theme { get; set; } = ThemePreset.Default;
    public List<GitFolder> GitFolders { get; set; } = new();
    public Guid? ActiveGitFolderId { get; set; }
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
