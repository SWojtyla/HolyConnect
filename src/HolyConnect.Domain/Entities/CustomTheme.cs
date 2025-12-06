namespace HolyConnect.Domain.Entities;

public class CustomTheme
{
    public string Primary { get; set; } = "#594AE2";
    public string Secondary { get; set; } = "#FF4081";
    public string Background { get; set; } = "#FFFFFF";
    public string Surface { get; set; } = "#FFFFFF";
    public string AppbarBackground { get; set; } = "#594AE2";
    public string AppbarText { get; set; } = "#FFFFFF";
    public string DrawerBackground { get; set; } = "#FFFFFF";
    public string DrawerText { get; set; } = "#000000";
    public bool IsDarkMode { get; set; } = false;
}
