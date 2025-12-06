using MudBlazor;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Maui.Services;

public static class ThemeProvider
{
    public static MudTheme GetTheme(ThemePreset preset)
    {
        return preset switch
        {
            ThemePreset.LightContrast => CreateLightContrastTheme(),
            ThemePreset.LightSoft => CreateLightSoftTheme(),
            ThemePreset.Dark => CreateDarkTheme(),
            ThemePreset.DarkContrast => CreateDarkContrastTheme(),
            ThemePreset.DarkHighSaturation => CreateDarkHighSaturationTheme(),
            _ => CreateDefaultTheme()
        };
    }

    public static bool IsThemeDark(ThemePreset preset)
    {
        return preset == ThemePreset.Dark ||
               preset == ThemePreset.DarkContrast ||
               preset == ThemePreset.DarkHighSaturation;
    }

    public static string GetThemeName(ThemePreset preset)
    {
        return preset switch
        {
            ThemePreset.Default => "Default Light",
            ThemePreset.LightContrast => "Light Contrast",
            ThemePreset.LightSoft => "Light Soft",
            ThemePreset.Dark => "Dark",
            ThemePreset.DarkContrast => "Dark Contrast",
            ThemePreset.DarkHighSaturation => "Dark Vibrant",
            _ => "Default"
        };
    }

    public static string GetThemeDescription(ThemePreset preset)
    {
        return preset switch
        {
            ThemePreset.Default => "Clean and minimal light theme with standard colors",
            ThemePreset.LightContrast => "High contrast light theme for better readability",
            ThemePreset.LightSoft => "Soft, muted light theme with gentle colors",
            ThemePreset.Dark => "Classic dark theme with balanced colors",
            ThemePreset.DarkContrast => "High contrast dark theme for enhanced visibility",
            ThemePreset.DarkHighSaturation => "Vibrant dark theme with rich, saturated colors",
            _ => "Default theme"
        };
    }

    private static MudTheme CreateDefaultTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#594AE2",
                Secondary = "#FF4081",
                Success = "#4CAF50",
                Info = "#2196F3",
                Warning = "#FF9800",
                Error = "#F44336",
                AppbarBackground = "#594AE2",
                Background = "#FFFFFF",
                Surface = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "rgba(0,0,0, 0.87)",
                TextPrimary = "rgba(0,0,0, 0.87)",
                TextSecondary = "rgba(0,0,0, 0.54)"
            }
        };
    }

    private static MudTheme CreateLightContrastTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#0050C8",
                Secondary = "#E91E63",
                Success = "#2E7D32",
                Info = "#01579B",
                Warning = "#E65100",
                Error = "#C62828",
                AppbarBackground = "#0050C8",
                Background = "#FFFFFF",
                Surface = "#F5F5F5",
                DrawerBackground = "#FAFAFA",
                DrawerText = "rgba(0,0,0, 0.95)",
                TextPrimary = "rgba(0,0,0, 0.95)",
                TextSecondary = "rgba(0,0,0, 0.70)"
            }
        };
    }

    private static MudTheme CreateLightSoftTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#7986CB",
                Secondary = "#F48FB1",
                Success = "#81C784",
                Info = "#64B5F6",
                Warning = "#FFB74D",
                Error = "#E57373",
                AppbarBackground = "#7986CB",
                Background = "#FAFAFA",
                Surface = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "rgba(0,0,0, 0.75)",
                TextPrimary = "rgba(0,0,0, 0.75)",
                TextSecondary = "rgba(0,0,0, 0.45)"
            }
        };
    }

    private static MudTheme CreateDarkTheme()
    {
        return new MudTheme
        {
            PaletteDark = new PaletteDark
            {
                Primary = "#776BE7",
                Secondary = "#FF4081",
                Success = "#66BB6A",
                Info = "#42A5F5",
                Warning = "#FFA726",
                Error = "#EF5350",
                AppbarBackground = "#1E1E1E",
                Background = "#121212",
                Surface = "#1E1E1E",
                DrawerBackground = "#1E1E1E",
                DrawerText = "rgba(255,255,255, 0.87)",
                TextPrimary = "rgba(255,255,255, 0.87)",
                TextSecondary = "rgba(255,255,255, 0.54)"
            }
        };
    }

    private static MudTheme CreateDarkContrastTheme()
    {
        return new MudTheme
        {
            PaletteDark = new PaletteDark
            {
                Primary = "#90CAF9",
                Secondary = "#F48FB1",
                Success = "#81C784",
                Info = "#64B5F6",
                Warning = "#FFB74D",
                Error = "#FF8A80",
                AppbarBackground = "#000000",
                Background = "#000000",
                Surface = "#121212",
                DrawerBackground = "#0A0A0A",
                DrawerText = "rgba(255,255,255, 0.95)",
                TextPrimary = "rgba(255,255,255, 0.95)",
                TextSecondary = "rgba(255,255,255, 0.70)"
            }
        };
    }

    private static MudTheme CreateDarkHighSaturationTheme()
    {
        return new MudTheme
        {
            PaletteDark = new PaletteDark
            {
                Primary = "#BB86FC",
                Secondary = "#FF4081",
                Success = "#00E676",
                Info = "#00B8D4",
                Warning = "#FFD740",
                Error = "#FF5252",
                AppbarBackground = "#1A1A2E",
                Background = "#0F0F1E",
                Surface = "#1A1A2E",
                DrawerBackground = "#16162A",
                DrawerText = "rgba(255,255,255, 0.90)",
                TextPrimary = "rgba(255,255,255, 0.90)",
                TextSecondary = "rgba(255,255,255, 0.60)"
            }
        };
    }
}
