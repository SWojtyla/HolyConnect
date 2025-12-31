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
        Assert.False(settings.AutoSaveOnNavigate);
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
    public void AutoSaveOnNavigate_ShouldBeSettable()
    {
        // Arrange
        var settings = new AppSettings();

        // Act
        settings.AutoSaveOnNavigate = true;

        // Assert
        Assert.True(settings.AutoSaveOnNavigate);
    }

    [Fact]
    public void AutoSaveOnNavigate_ShouldDefaultToFalse()
    {
        // Arrange & Act
        var settings = new AppSettings();

        // Assert
        Assert.False(settings.AutoSaveOnNavigate);
    }

    [Fact]
    public void EnvironmentOrder_ShouldBeInitializedAsEmptyList()
    {
        // Arrange & Act
        var settings = new AppSettings();

        // Assert
        Assert.NotNull(settings.EnvironmentOrder);
        Assert.Empty(settings.EnvironmentOrder);
    }

    [Fact]
    public void EnvironmentOrder_ShouldBeModifiable()
    {
        // Arrange
        var settings = new AppSettings();
        var env1Id = Guid.NewGuid();
        var env2Id = Guid.NewGuid();
        var env3Id = Guid.NewGuid();

        // Act
        settings.EnvironmentOrder.Add(env1Id);
        settings.EnvironmentOrder.Add(env2Id);
        settings.EnvironmentOrder.Add(env3Id);

        // Assert
        Assert.Equal(3, settings.EnvironmentOrder.Count);
        Assert.Equal(env1Id, settings.EnvironmentOrder[0]);
        Assert.Equal(env2Id, settings.EnvironmentOrder[1]);
        Assert.Equal(env3Id, settings.EnvironmentOrder[2]);
    }

    [Fact]
    public void EnvironmentOrder_ShouldMaintainOrder()
    {
        // Arrange
        var settings = new AppSettings();
        var env1Id = Guid.NewGuid();
        var env2Id = Guid.NewGuid();

        // Act
        settings.EnvironmentOrder.Add(env2Id);
        settings.EnvironmentOrder.Add(env1Id);

        // Assert
        Assert.Equal(env2Id, settings.EnvironmentOrder[0]);
        Assert.Equal(env1Id, settings.EnvironmentOrder[1]);
    }
}
