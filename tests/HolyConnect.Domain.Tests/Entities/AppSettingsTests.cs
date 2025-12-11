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
        var settings1 = new AppSettings { StoragePath = "/path1", IsDarkMode = true };
        var settings2 = new AppSettings { StoragePath = "/path2", IsDarkMode = false };

        // Assert
        Assert.NotEqual(settings1.StoragePath, settings2.StoragePath);
        Assert.NotEqual(settings1.IsDarkMode, settings2.IsDarkMode);
    }
}
