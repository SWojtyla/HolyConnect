using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

public class ThemeService
{
    public CustomTheme GetThemePreset(ThemePreset preset)
    {
        var theme = new CustomTheme();
        
        switch (preset)
        {
            case ThemePreset.Default:
                // Use default values already set in CustomTheme constructor
                break;
                
            case ThemePreset.Ocean:
                theme.Primary = "#0077be";
                theme.Secondary = "#00a8cc";
                theme.Background = "#e6f3f9";
                theme.Surface = "#ffffff";
                theme.AppbarBackground = "#005f99";
                theme.AppbarText = "#ffffff";
                theme.DrawerBackground = "#b3ddf2";
                theme.DrawerText = "#003d66";
                theme.IsDarkMode = false;
                break;
                
            case ThemePreset.Forest:
                theme.Primary = "#2d5016";
                theme.Secondary = "#6b8e23";
                theme.Background = "#f0f4e8";
                theme.Surface = "#ffffff";
                theme.AppbarBackground = "#2d5016";
                theme.AppbarText = "#ffffff";
                theme.DrawerBackground = "#d4e4c0";
                theme.DrawerText = "#1a2e0d";
                theme.IsDarkMode = false;
                break;
                
            case ThemePreset.Sunset:
                theme.Primary = "#ff6b35";
                theme.Secondary = "#f7931e";
                theme.Background = "#fff3e0";
                theme.Surface = "#ffffff";
                theme.AppbarBackground = "#d84315";
                theme.AppbarText = "#ffffff";
                theme.DrawerBackground = "#ffe0b2";
                theme.DrawerText = "#8b2e0b";
                theme.IsDarkMode = false;
                break;
                
            case ThemePreset.Monochrome:
                theme.Primary = "#424242";
                theme.Secondary = "#757575";
                theme.Background = "#fafafa";
                theme.Surface = "#ffffff";
                theme.AppbarBackground = "#212121";
                theme.AppbarText = "#ffffff";
                theme.DrawerBackground = "#e0e0e0";
                theme.DrawerText = "#212121";
                theme.IsDarkMode = false;
                break;
                
            case ThemePreset.OceanDark:
                theme.Primary = "#4fc3f7";
                theme.Secondary = "#29b6f6";
                theme.Background = "#0d1117";
                theme.Surface = "#1e2530";
                theme.AppbarBackground = "#0a4d6e";
                theme.AppbarText = "#e0f7ff";
                theme.DrawerBackground = "#1a2332";
                theme.DrawerText = "#b3e5fc";
                theme.IsDarkMode = true;
                break;
                
            case ThemePreset.ForestDark:
                theme.Primary = "#81c784";
                theme.Secondary = "#aed581";
                theme.Background = "#1a1a1a";
                theme.Surface = "#2d2d2d";
                theme.AppbarBackground = "#1b3a0f";
                theme.AppbarText = "#c8e6c9";
                theme.DrawerBackground = "#212121";
                theme.DrawerText = "#c8e6c9";
                theme.IsDarkMode = true;
                break;
                
            case ThemePreset.SunsetDark:
                theme.Primary = "#ff8a65";
                theme.Secondary = "#ffab91";
                theme.Background = "#1a1a1a";
                theme.Surface = "#2d2d2d";
                theme.AppbarBackground = "#8b2e0b";
                theme.AppbarText = "#ffe0b2";
                theme.DrawerBackground = "#212121";
                theme.DrawerText = "#ffccbc";
                theme.IsDarkMode = true;
                break;
                
            case ThemePreset.MonochromeDark:
                theme.Primary = "#9e9e9e";
                theme.Secondary = "#bdbdbd";
                theme.Background = "#121212";
                theme.Surface = "#1e1e1e";
                theme.AppbarBackground = "#0d0d0d";
                theme.AppbarText = "#e0e0e0";
                theme.DrawerBackground = "#1a1a1a";
                theme.DrawerText = "#e0e0e0";
                theme.IsDarkMode = true;
                break;
        }
        
        return theme;
    }
}
