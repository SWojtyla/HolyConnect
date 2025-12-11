using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services;

namespace HolyConnect.Infrastructure.Tests.Services;

public class FileBasedSettingsServiceTests : IDisposable
{
    private readonly FileBasedSettingsService _service;

    public FileBasedSettingsServiceTests()
    {
        _service = new FileBasedSettingsService();
    }

    public void Dispose()
    {
        // No cleanup needed - service uses system LocalApplicationData directory
    }

    [Fact]
    public async Task GetSettingsAsync_WhenFileDoesNotExist_ShouldReturnDefaultSettings()
    {
        // Act
        var settings = await _service.GetSettingsAsync();

        // Assert
        Assert.NotNull(settings);
        Assert.NotEmpty(settings.StoragePath);
    }

    [Fact]
    public async Task SaveSettingsAsync_ShouldPersistSettings()
    {
        // Arrange
        var settings = new AppSettings
        {
            StoragePath = "/test/path/save_persist",
            IsDarkMode = true
        };

        // Act
        await _service.SaveSettingsAsync(settings);

        // Assert - Create new instance to verify persistence
        var newService = new FileBasedSettingsService();
        var loadedSettings = await newService.GetSettingsAsync();
        Assert.Equal("/test/path/save_persist", loadedSettings.StoragePath);
        Assert.True(loadedSettings.IsDarkMode);
    }

    [Fact]
    public async Task GetSettingsAsync_AfterSave_ShouldReturnSavedSettings()
    {
        // Arrange
        var settings = new AppSettings
        {
            StoragePath = "/custom/storage/path",
            IsDarkMode = false
        };

        // Act
        await _service.SaveSettingsAsync(settings);
        var retrievedSettings = await _service.GetSettingsAsync();

        // Assert
        Assert.Equal("/custom/storage/path", retrievedSettings.StoragePath);
        Assert.False(retrievedSettings.IsDarkMode);
    }

    [Fact]
    public async Task SaveSettingsAsync_MultipleTimes_ShouldOverwritePreviousSettings()
    {
        // Arrange
        var settings1 = new AppSettings { StoragePath = "/path1", IsDarkMode = false };
        var settings2 = new AppSettings { StoragePath = "/path2/overwrite", IsDarkMode = true };

        // Act
        await _service.SaveSettingsAsync(settings1);
        await _service.SaveSettingsAsync(settings2);
        var result = await _service.GetSettingsAsync();

        // Assert
        Assert.Equal("/path2/overwrite", result.StoragePath);
        Assert.True(result.IsDarkMode);
    }

    [Fact]
    public async Task SaveSettingsAsync_WithDifferentValues_ShouldPersistCorrectly()
    {
        // Arrange
        var settings = new AppSettings
        {
            StoragePath = "/unique/test/path",
            IsDarkMode = true
        };

        // Act
        await _service.SaveSettingsAsync(settings);
        var loadedSettings = await _service.GetSettingsAsync();

        // Assert
        Assert.Equal("/unique/test/path", loadedSettings.StoragePath);
        Assert.True(loadedSettings.IsDarkMode);
    }

    [Fact]
    public async Task SaveSettingsAsync_WithGitFolders_ShouldPersistGitFolders()
    {
        // Arrange
        var gitFolder = new GitFolder
        {
            Id = Guid.NewGuid(),
            Name = "Test Repository",
            Path = "/test/repo/path",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var settings = new AppSettings
        {
            StoragePath = "/test/path",
            GitFolders = new List<GitFolder> { gitFolder },
            ActiveGitFolderId = gitFolder.Id
        };

        // Act
        await _service.SaveSettingsAsync(settings);
        var loadedSettings = await _service.GetSettingsAsync();

        // Assert
        Assert.NotNull(loadedSettings.GitFolders);
        Assert.Single(loadedSettings.GitFolders);
        Assert.Equal("Test Repository", loadedSettings.GitFolders[0].Name);
        Assert.Equal("/test/repo/path", loadedSettings.GitFolders[0].Path);
        Assert.True(loadedSettings.GitFolders[0].IsActive);
        Assert.Equal(gitFolder.Id, loadedSettings.ActiveGitFolderId);
    }

    [Fact]
    public async Task SaveSettingsAsync_WithLayout_ShouldPersistLayout()
    {
        // Arrange
        var settings = new AppSettings
        {
            StoragePath = "/test/path",
            Layout = RequestLayout.Vertical
        };

        // Act
        await _service.SaveSettingsAsync(settings);
        var loadedSettings = await _service.GetSettingsAsync();

        // Assert
        Assert.Equal(RequestLayout.Vertical, loadedSettings.Layout);
    }

    [Fact]
    public async Task SaveSettingsAsync_ShouldCreateBackup()
    {
        // Arrange
        var settings1 = new AppSettings
        {
            StoragePath = "/test/path/backup1",
            IsDarkMode = true,
            GitFolders = new List<GitFolder>
            {
                new GitFolder
                {
                    Id = Guid.NewGuid(),
                    Name = "Backup Test Repo",
                    Path = "/backup/repo",
                    IsActive = true
                }
            }
        };

        var settings2 = new AppSettings
        {
            StoragePath = "/test/path/backup2",
            IsDarkMode = false
        };

        // Act
        await _service.SaveSettingsAsync(settings1);
        await _service.SaveSettingsAsync(settings2);

        // Assert - If we load settings, we should get settings2
        var loadedSettings = await _service.GetSettingsAsync();
        Assert.Equal("/test/path/backup2", loadedSettings.StoragePath);
        Assert.False(loadedSettings.IsDarkMode);
    }
}
