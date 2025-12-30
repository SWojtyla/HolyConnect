namespace HolyConnect.Domain.Entities;

public class AppSettings
{
    public string StoragePath { get; set; } = string.Empty;
    public bool IsDarkMode { get; set; } = false;
    public RequestLayout Layout { get; set; } = RequestLayout.Horizontal;
    public ThemePreset Theme { get; set; } = ThemePreset.Default;
    public List<GitFolder> GitFolders { get; set; } = new();
    public Guid? ActiveGitFolderId { get; set; }
    
    /// <summary>
    /// The globally active environment ID. All requests use this environment for variable resolution.
    /// </summary>
    public Guid? ActiveEnvironmentId { get; set; }
    
    /// <summary>
    /// The order of environments in the variables matrix. Contains environment IDs in display order.
    /// </summary>
    public List<Guid> EnvironmentOrder { get; set; } = new();
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
