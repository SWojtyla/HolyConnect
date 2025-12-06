namespace HolyConnect.Domain.Entities;

public class AppSettings
{
    public string StoragePath { get; set; } = string.Empty;
    public bool IsDarkMode { get; set; } = false;
    public RequestLayout Layout { get; set; } = RequestLayout.Horizontal;
    public bool GitEnabled { get; set; } = false;
    public string GitUserName { get; set; } = string.Empty;
    public string GitUserEmail { get; set; } = string.Empty;
}

public enum RequestLayout
{
    Horizontal,
    Vertical
}
