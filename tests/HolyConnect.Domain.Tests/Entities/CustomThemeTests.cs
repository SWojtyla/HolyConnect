using HolyConnect.Domain.Entities;

namespace HolyConnect.Domain.Tests.Entities;

public class CustomThemeTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var theme = new CustomTheme();

        // Assert
        Assert.Equal("#594AE2", theme.Primary);
        Assert.Equal("#FF4081", theme.Secondary);
        Assert.Equal("#FFFFFF", theme.Background);
        Assert.Equal("#FFFFFF", theme.Surface);
        Assert.Equal("#594AE2", theme.AppbarBackground);
        Assert.Equal("#FFFFFF", theme.AppbarText);
        Assert.Equal("#FFFFFF", theme.DrawerBackground);
        Assert.Equal("#000000", theme.DrawerText);
        Assert.False(theme.IsDarkMode);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var theme = new CustomTheme();

        // Act
        theme.Primary = "#0077be";
        theme.Secondary = "#00a8cc";
        theme.Background = "#e6f3f9";
        theme.Surface = "#ffffff";
        theme.AppbarBackground = "#005f99";
        theme.AppbarText = "#ffffff";
        theme.DrawerBackground = "#b3ddf2";
        theme.DrawerText = "#003d66";
        theme.IsDarkMode = false;

        // Assert
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
    public void IsDarkMode_ShouldBeSettable()
    {
        // Arrange
        var theme = new CustomTheme();

        // Act
        theme.IsDarkMode = true;

        // Assert
        Assert.True(theme.IsDarkMode);
    }
}
