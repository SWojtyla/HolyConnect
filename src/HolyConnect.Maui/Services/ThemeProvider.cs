using MudBlazor;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Maui.Services;

public static class ThemeProvider
{
    public static MudTheme GetTheme(ThemePreset preset)
    {
        return preset switch
        {
            ThemePreset.Ocean => CreateOceanTheme(),
            ThemePreset.Forest => CreateForestTheme(),
            ThemePreset.Sunset => CreateSunsetTheme(),
            ThemePreset.Monochrome => CreateMonochromeTheme(),
            ThemePreset.Dark => CreateDarkTheme(),
            ThemePreset.OceanDark => CreateOceanDarkTheme(),
            ThemePreset.ForestDark => CreateForestDarkTheme(),
            ThemePreset.SunsetDark => CreateSunsetDarkTheme(),
            ThemePreset.MonochromeDark => CreateMonochromeDarkTheme(),
            _ => CreateDefaultTheme()
        };
    }

    public static bool IsThemeDark(ThemePreset preset)
    {
        return preset == ThemePreset.Dark ||
               preset == ThemePreset.OceanDark ||
               preset == ThemePreset.ForestDark ||
               preset == ThemePreset.SunsetDark ||
               preset == ThemePreset.MonochromeDark;
    }

    public static string GetThemeName(ThemePreset preset)
    {
        return preset switch
        {
            ThemePreset.Default => "Default Light",
            ThemePreset.Ocean => "Ocean",
            ThemePreset.Forest => "Forest",
            ThemePreset.Sunset => "Sunset",
            ThemePreset.Monochrome => "Monochrome",
            ThemePreset.Dark => "Dark",
            ThemePreset.OceanDark => "Ocean Dark",
            ThemePreset.ForestDark => "Forest Dark",
            ThemePreset.SunsetDark => "Sunset Dark",
            ThemePreset.MonochromeDark => "Monochrome Dark",
            _ => "Default"
        };
    }

    public static string GetThemeDescription(ThemePreset preset)
    {
        return preset switch
        {
            ThemePreset.Default => "Clean and minimal light theme with standard colors",
            ThemePreset.Ocean => "Cool and calming blue theme inspired by the ocean",
            ThemePreset.Forest => "Natural green theme inspired by the forest",
            ThemePreset.Sunset => "Warm orange theme inspired by sunset colors",
            ThemePreset.Monochrome => "Minimalist grayscale theme with subtle contrasts",
            ThemePreset.Dark => "Classic dark theme with balanced colors",
            ThemePreset.OceanDark => "Dark theme with ocean-inspired blue accents",
            ThemePreset.ForestDark => "Dark theme with forest-inspired green accents",
            ThemePreset.SunsetDark => "Dark theme with sunset-inspired warm accents",
            ThemePreset.MonochromeDark => "Dark theme with monochrome grayscale palette",
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

    private static MudTheme CreateOceanTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#0077be",
                Secondary = "#00a8cc",
                Success = "#4CAF50",
                Info = "#2196F3",
                Warning = "#FF9800",
                Error = "#F44336",
                AppbarBackground = "#005f99",
                AppbarText = "#ffffff",
                Background = "#e6f3f9",
                Surface = "#ffffff",
                DrawerBackground = "#b3ddf2",
                DrawerText = "#003d66",
                TextPrimary = "rgba(0,0,0, 0.87)",
                TextSecondary = "rgba(0,0,0, 0.54)"
            }
        };
    }

    private static MudTheme CreateForestTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#2d5016",
                Secondary = "#6b8e23",
                Success = "#4CAF50",
                Info = "#2196F3",
                Warning = "#FF9800",
                Error = "#F44336",
                AppbarBackground = "#2d5016",
                AppbarText = "#ffffff",
                Background = "#f0f4e8",
                Surface = "#ffffff",
                DrawerBackground = "#d4e4c0",
                DrawerText = "#1a2e0d",
                TextPrimary = "rgba(0,0,0, 0.87)",
                TextSecondary = "rgba(0,0,0, 0.54)"
            }
        };
    }

    private static MudTheme CreateSunsetTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#ff6b35",
                Secondary = "#f7931e",
                Success = "#4CAF50",
                Info = "#2196F3",
                Warning = "#FF9800",
                Error = "#F44336",
                AppbarBackground = "#d84315",
                AppbarText = "#ffffff",
                Background = "#fff3e0",
                Surface = "#ffffff",
                DrawerBackground = "#ffe0b2",
                DrawerText = "#8b2e0b",
                TextPrimary = "rgba(0,0,0, 0.87)",
                TextSecondary = "rgba(0,0,0, 0.54)"
            }
        };
    }

    private static MudTheme CreateMonochromeTheme()
    {
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#424242",
                Secondary = "#757575",
                Success = "#4CAF50",
                Info = "#2196F3",
                Warning = "#FF9800",
                Error = "#F44336",
                AppbarBackground = "#212121",
                AppbarText = "#ffffff",
                Background = "#fafafa",
                Surface = "#ffffff",
                DrawerBackground = "#e0e0e0",
                DrawerText = "#212121",
                TextPrimary = "rgba(0,0,0, 0.87)",
                TextSecondary = "rgba(0,0,0, 0.54)"
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

    private static MudTheme CreateOceanDarkTheme()
    {
        return new MudTheme
        {
            PaletteDark = new PaletteDark
            {
                Primary = "#4fc3f7",
                Secondary = "#29b6f6",
                Success = "#66BB6A",
                Info = "#42A5F5",
                Warning = "#FFA726",
                Error = "#EF5350",
                AppbarBackground = "#0a4d6e",
                AppbarText = "#e0f7ff",
                Background = "#0d1117",
                Surface = "#1e2530",
                DrawerBackground = "#1a2332",
                DrawerText = "#b3e5fc",
                TextPrimary = "rgba(255,255,255, 0.87)",
                TextSecondary = "rgba(255,255,255, 0.54)"
            }
        };
    }

    private static MudTheme CreateForestDarkTheme()
    {
        return new MudTheme
        {
            PaletteDark = new PaletteDark
            {
                Primary = "#81c784",
                Secondary = "#aed581",
                Success = "#66BB6A",
                Info = "#42A5F5",
                Warning = "#FFA726",
                Error = "#EF5350",
                AppbarBackground = "#1b3a0f",
                AppbarText = "#c8e6c9",
                Background = "#1a1a1a",
                Surface = "#2d2d2d",
                DrawerBackground = "#212121",
                DrawerText = "#c8e6c9",
                TextPrimary = "rgba(255,255,255, 0.87)",
                TextSecondary = "rgba(255,255,255, 0.54)"
            }
        };
    }

    private static MudTheme CreateSunsetDarkTheme()
    {
        return new MudTheme
        {
            PaletteDark = new PaletteDark
            {
                Primary = "#ff8a65",
                Secondary = "#ffab91",
                Success = "#66BB6A",
                Info = "#42A5F5",
                Warning = "#FFA726",
                Error = "#EF5350",
                AppbarBackground = "#8b2e0b",
                AppbarText = "#ffe0b2",
                Background = "#1a1a1a",
                Surface = "#2d2d2d",
                DrawerBackground = "#212121",
                DrawerText = "#ffccbc",
                TextPrimary = "rgba(255,255,255, 0.87)",
                TextSecondary = "rgba(255,255,255, 0.54)"
            }
        };
    }

    private static MudTheme CreateMonochromeDarkTheme()
    {
        return new MudTheme
        {
            PaletteDark = new PaletteDark
            {
                Primary = "#9e9e9e",
                Secondary = "#bdbdbd",
                Success = "#66BB6A",
                Info = "#42A5F5",
                Warning = "#FFA726",
                Error = "#EF5350",
                AppbarBackground = "#0d0d0d",
                AppbarText = "#e0e0e0",
                Background = "#121212",
                Surface = "#1e1e1e",
                DrawerBackground = "#1a1a1a",
                DrawerText = "#e0e0e0",
                TextPrimary = "rgba(255,255,255, 0.87)",
                TextSecondary = "rgba(255,255,255, 0.54)"
            }
        };
    }
}
