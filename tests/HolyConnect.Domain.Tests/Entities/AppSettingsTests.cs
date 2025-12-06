using HolyConnect.Domain.Entities;

namespace HolyConnect.Domain.Tests.Entities;

public class AppSettingsTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var settings = new AppSettings();

        // Assert
        Assert.Equal(string.Empty, settings.StoragePath);
        Assert.False(settings.IsDarkMode);
        Assert.Equal(RequestLayout.Horizontal, settings.Layout);
        Assert.Equal(ThemePreset.Default, settings.SelectedTheme);
        Assert.NotNull(settings.CustomTheme);
    }

    [Fact]
    public void StoragePath_ShouldBeSettable()
    {
        // Arrange
        var settings = new AppSettings();
        var path = "/test/storage/path";

        // Act
        settings.StoragePath = path;

        // Assert
        Assert.Equal(path, settings.StoragePath);
    }

    [Fact]
    public void IsDarkMode_ShouldBeSettable()
    {
        // Arrange
        var settings = new AppSettings();

        // Act
        settings.IsDarkMode = true;

        // Assert
        Assert.True(settings.IsDarkMode);
    }

    [Fact]
    public void Properties_ShouldBeIndependent()
    {
        // Arrange
        var settings1 = new AppSettings { StoragePath = "/path1", IsDarkMode = true, Layout = RequestLayout.Vertical };
        var settings2 = new AppSettings { StoragePath = "/path2", IsDarkMode = false, Layout = RequestLayout.Horizontal };

        // Assert
        Assert.NotEqual(settings1.StoragePath, settings2.StoragePath);
        Assert.NotEqual(settings1.IsDarkMode, settings2.IsDarkMode);
        Assert.NotEqual(settings1.Layout, settings2.Layout);
    }

    [Fact]
    public void Layout_ShouldBeSettable()
    {
        // Arrange
        var settings = new AppSettings();

        // Act
        settings.Layout = RequestLayout.Vertical;

        // Assert
        Assert.Equal(RequestLayout.Vertical, settings.Layout);
    }

    [Theory]
    [InlineData(RequestLayout.Horizontal)]
    [InlineData(RequestLayout.Vertical)]
    public void Layout_ShouldSupportAllLayoutTypes(RequestLayout layout)
    {
        // Arrange
        var settings = new AppSettings { Layout = layout };

        // Assert
        Assert.Equal(layout, settings.Layout);
    }

    [Fact]
    public void SelectedTheme_ShouldBeSettable()
    {
        // Arrange
        var settings = new AppSettings();

        // Act
        settings.SelectedTheme = ThemePreset.Ocean;

        // Assert
        Assert.Equal(ThemePreset.Ocean, settings.SelectedTheme);
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
    public void SelectedTheme_ShouldSupportAllThemePresets(ThemePreset preset)
    {
        // Arrange
        var settings = new AppSettings { SelectedTheme = preset };

        // Assert
        Assert.Equal(preset, settings.SelectedTheme);
    }

    [Fact]
    public void CustomTheme_ShouldBeSettable()
    {
        // Arrange
        var settings = new AppSettings();
        var customTheme = new CustomTheme
        {
            Primary = "#0077be",
            IsDarkMode = true
        };

        // Act
        settings.CustomTheme = customTheme;

        // Assert
        Assert.Equal(customTheme, settings.CustomTheme);
        Assert.Equal("#0077be", settings.CustomTheme.Primary);
        Assert.True(settings.CustomTheme.IsDarkMode);
    }
}
