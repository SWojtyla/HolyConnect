using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Tests.Services;

public class ThemeServiceTests
{
    private readonly ThemeService _themeService;

    public ThemeServiceTests()
    {
        _themeService = new ThemeService();
    }

    [Fact]
    public void GetThemePreset_WithDefaultPreset_ShouldReturnDefaultTheme()
    {
        // Act
        var theme = _themeService.GetThemePreset(ThemePreset.Default);

        // Assert
        Assert.NotNull(theme);
        Assert.Equal("#594AE2", theme.Primary);
        Assert.False(theme.IsDarkMode);
    }

    [Fact]
    public void GetThemePreset_WithOceanPreset_ShouldReturnOceanTheme()
    {
        // Act
        var theme = _themeService.GetThemePreset(ThemePreset.Ocean);

        // Assert
        Assert.NotNull(theme);
        Assert.Equal("#0077be", theme.Primary);
        Assert.Equal("#00a8cc", theme.Secondary);
        Assert.Equal("#e6f3f9", theme.Background);
        Assert.Equal("#ffffff", theme.Surface);
        Assert.Equal("#005f99", theme.AppbarBackground);
        Assert.Equal("#ffffff", theme.AppbarText);
        Assert.Equal("#b3ddf2", theme.DrawerBackground);
        Assert.Equal("#003d66", theme.DrawerText);
        Assert.False(theme.IsDarkMode);
    }

    [Fact]
    public void GetThemePreset_WithForestPreset_ShouldReturnForestTheme()
    {
        // Act
        var theme = _themeService.GetThemePreset(ThemePreset.Forest);

        // Assert
        Assert.NotNull(theme);
        Assert.Equal("#2d5016", theme.Primary);
        Assert.Equal("#6b8e23", theme.Secondary);
        Assert.False(theme.IsDarkMode);
    }

    [Fact]
    public void GetThemePreset_WithSunsetPreset_ShouldReturnSunsetTheme()
    {
        // Act
        var theme = _themeService.GetThemePreset(ThemePreset.Sunset);

        // Assert
        Assert.NotNull(theme);
        Assert.Equal("#ff6b35", theme.Primary);
        Assert.Equal("#f7931e", theme.Secondary);
        Assert.False(theme.IsDarkMode);
    }

    [Fact]
    public void GetThemePreset_WithMonochromePreset_ShouldReturnMonochromeTheme()
    {
        // Act
        var theme = _themeService.GetThemePreset(ThemePreset.Monochrome);

        // Assert
        Assert.NotNull(theme);
        Assert.Equal("#424242", theme.Primary);
        Assert.Equal("#757575", theme.Secondary);
        Assert.False(theme.IsDarkMode);
    }

    [Fact]
    public void GetThemePreset_WithOceanDarkPreset_ShouldReturnOceanDarkTheme()
    {
        // Act
        var theme = _themeService.GetThemePreset(ThemePreset.OceanDark);

        // Assert
        Assert.NotNull(theme);
        Assert.Equal("#4fc3f7", theme.Primary);
        Assert.Equal("#29b6f6", theme.Secondary);
        Assert.Equal("#0d1117", theme.Background);
        Assert.Equal("#1e2530", theme.Surface);
        Assert.Equal("#0a4d6e", theme.AppbarBackground);
        Assert.Equal("#e0f7ff", theme.AppbarText);
        Assert.Equal("#1a2332", theme.DrawerBackground);
        Assert.Equal("#b3e5fc", theme.DrawerText);
        Assert.True(theme.IsDarkMode);
    }

    [Fact]
    public void GetThemePreset_WithForestDarkPreset_ShouldReturnForestDarkTheme()
    {
        // Act
        var theme = _themeService.GetThemePreset(ThemePreset.ForestDark);

        // Assert
        Assert.NotNull(theme);
        Assert.Equal("#81c784", theme.Primary);
        Assert.Equal("#aed581", theme.Secondary);
        Assert.True(theme.IsDarkMode);
    }

    [Fact]
    public void GetThemePreset_WithSunsetDarkPreset_ShouldReturnSunsetDarkTheme()
    {
        // Act
        var theme = _themeService.GetThemePreset(ThemePreset.SunsetDark);

        // Assert
        Assert.NotNull(theme);
        Assert.Equal("#ff8a65", theme.Primary);
        Assert.Equal("#ffab91", theme.Secondary);
        Assert.True(theme.IsDarkMode);
    }

    [Fact]
    public void GetThemePreset_WithMonochromeDarkPreset_ShouldReturnMonochromeDarkTheme()
    {
        // Act
        var theme = _themeService.GetThemePreset(ThemePreset.MonochromeDark);

        // Assert
        Assert.NotNull(theme);
        Assert.Equal("#9e9e9e", theme.Primary);
        Assert.Equal("#bdbdbd", theme.Secondary);
        Assert.True(theme.IsDarkMode);
    }

    [Theory]
    [InlineData(ThemePreset.Default)]
    [InlineData(ThemePreset.Ocean)]
    [InlineData(ThemePreset.Forest)]
    [InlineData(ThemePreset.Sunset)]
    [InlineData(ThemePreset.Monochrome)]
    [InlineData(ThemePreset.OceanDark)]
    [InlineData(ThemePreset.ForestDark)]
    [InlineData(ThemePreset.SunsetDark)]
    [InlineData(ThemePreset.MonochromeDark)]
    public void GetThemePreset_WithAnyPreset_ShouldReturnNonNullTheme(ThemePreset preset)
    {
        // Act
        var theme = _themeService.GetThemePreset(preset);

        // Assert
        Assert.NotNull(theme);
        Assert.NotNull(theme.Primary);
        Assert.NotNull(theme.Secondary);
        Assert.NotNull(theme.Background);
        Assert.NotNull(theme.Surface);
        Assert.NotNull(theme.AppbarBackground);
        Assert.NotNull(theme.AppbarText);
        Assert.NotNull(theme.DrawerBackground);
        Assert.NotNull(theme.DrawerText);
    }
}
