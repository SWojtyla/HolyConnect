using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;
using Xunit;

namespace HolyConnect.Application.Tests.Services;

public class GitFolderServiceTests
{
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly GitFolderService _service;
    private readonly AppSettings _testSettings;

    public GitFolderServiceTests()
    {
        _mockSettingsService = new Mock<ISettingsService>();
        _testSettings = new AppSettings
        {
            GitFolders = new List<GitFolder>()
        };
        _mockSettingsService.Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(_testSettings);
        _service = new GitFolderService(_mockSettingsService.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoFolders_ShouldReturnEmptyList()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveAsync_WhenNoActiveFolderId_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetActiveAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_WithValidData_ShouldAddFolder()
    {
        // Arrange
        var name = "Test Repo";
        var path = "/test/path";

        // Act
        var result = await _service.AddAsync(name, path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
        Assert.Equal(path, result.Path);
        Assert.NotEqual(Guid.Empty, result.Id);
        _mockSettingsService.Verify(s => s.SaveSettingsAsync(It.IsAny<AppSettings>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_WhenFirstFolder_ShouldSetAsActive()
    {
        // Arrange
        var name = "Test Repo";
        var path = "/test/path";

        // Act
        var result = await _service.AddAsync(name, path);

        // Assert
        Assert.True(result.IsActive);
        Assert.Equal(result.Id, _testSettings.ActiveGitFolderId);
    }

    [Fact]
    public async Task SetActiveAsync_WithValidId_ShouldSetActive()
    {
        // Arrange
        var folder1 = await _service.AddAsync("Repo 1", "/path1");
        var folder2 = await _service.AddAsync("Repo 2", "/path2");

        // Act
        var result = await _service.SetActiveAsync(folder2.Id);

        // Assert
        Assert.True(result);
        Assert.False(folder1.IsActive);
        Assert.True(folder2.IsActive);
        Assert.Equal(folder2.Id, _testSettings.ActiveGitFolderId);
    }

    [Fact]
    public async Task SetActiveAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _service.SetActiveAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldRemoveFolder()
    {
        // Arrange
        var folder = await _service.AddAsync("Test Repo", "/test/path");
        var folderId = folder.Id;

        // Act
        var result = await _service.DeleteAsync(folderId);

        // Assert
        Assert.True(result);
        Assert.Empty(_testSettings.GitFolders);
    }

    [Fact]
    public async Task DeleteAsync_WhenDeletingActiveFolder_ShouldSetNewActive()
    {
        // Arrange
        var folder1 = await _service.AddAsync("Repo 1", "/path1");
        var folder2 = await _service.AddAsync("Repo 2", "/path2");
        await _service.SetActiveAsync(folder1.Id);

        // Act
        var result = await _service.DeleteAsync(folder1.Id);

        // Assert
        Assert.True(result);
        Assert.Equal(folder2.Id, _testSettings.ActiveGitFolderId);
        Assert.True(folder2.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_WithValidFolder_ShouldUpdateProperties()
    {
        // Arrange
        var folder = await _service.AddAsync("Old Name", "/old/path");
        folder.Name = "New Name";
        folder.Path = "/new/path";

        // Act
        var result = await _service.UpdateAsync(folder);

        // Assert
        Assert.True(result);
        Assert.Equal("New Name", folder.Name);
        Assert.Equal("/new/path", folder.Path);
        Assert.NotNull(folder.LastAccessedAt);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var folder = new GitFolder
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Path = "/test"
        };

        // Act
        var result = await _service.UpdateAsync(folder);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnFolder()
    {
        // Arrange
        var folder = await _service.AddAsync("Test Repo", "/test/path");

        // Act
        var result = await _service.GetByIdAsync(folder.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(folder.Id, result.Id);
        Assert.Equal(folder.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }
}
