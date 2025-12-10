using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;
using Xunit;

namespace HolyConnect.Application.Tests.Services;

public class GitFolderIntegrationTests
{
    [Fact]
    public async Task MultipleGitFolders_WorkflowScenario_ShouldWorkCorrectly()
    {
        // Arrange
        var mockSettingsService = new Mock<ISettingsService>();
        var testSettings = new AppSettings
        {
            GitFolders = new List<GitFolder>(),
            StoragePath = "/default/path"
        };
        mockSettingsService.Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(testSettings);
        
        var gitFolderService = new GitFolderService(mockSettingsService.Object);

        // Act & Assert - Add first repository
        var repo1 = await gitFolderService.AddAsync("Project A", "/path/to/project-a");
        Assert.NotNull(repo1);
        Assert.True(repo1.IsActive, "First repository should be set as active");
        Assert.Equal(repo1.Id, testSettings.ActiveGitFolderId);

        // Act & Assert - Add second repository
        var repo2 = await gitFolderService.AddAsync("Project B", "/path/to/project-b");
        Assert.NotNull(repo2);
        Assert.False(repo2.IsActive, "Second repository should not be active by default");
        Assert.Equal(repo1.Id, testSettings.ActiveGitFolderId);

        // Act & Assert - Get all repositories
        var allRepos = await gitFolderService.GetAllAsync();
        Assert.Equal(2, allRepos.Count());

        // Act & Assert - Get active repository
        var activeRepo = await gitFolderService.GetActiveAsync();
        Assert.NotNull(activeRepo);
        Assert.Equal(repo1.Id, activeRepo.Id);
        Assert.Equal("Project A", activeRepo.Name);

        // Act & Assert - Switch active repository
        var switched = await gitFolderService.SetActiveAsync(repo2.Id);
        Assert.True(switched);
        Assert.True(repo2.IsActive);
        Assert.False(repo1.IsActive);
        Assert.Equal(repo2.Id, testSettings.ActiveGitFolderId);

        // Act & Assert - Verify new active
        activeRepo = await gitFolderService.GetActiveAsync();
        Assert.NotNull(activeRepo);
        Assert.Equal(repo2.Id, activeRepo.Id);
        Assert.Equal("Project B", activeRepo.Name);

        // Act & Assert - Update repository
        repo2.Name = "Project B Updated";
        var updated = await gitFolderService.UpdateAsync(repo2);
        Assert.True(updated);
        Assert.Equal("Project B Updated", repo2.Name);

        // Act & Assert - Delete active repository (should set new active)
        var deleted = await gitFolderService.DeleteAsync(repo2.Id);
        Assert.True(deleted);
        Assert.Single(testSettings.GitFolders);
        Assert.Equal(repo1.Id, testSettings.ActiveGitFolderId);
        Assert.True(repo1.IsActive);

        // Act & Assert - Delete last repository
        deleted = await gitFolderService.DeleteAsync(repo1.Id);
        Assert.True(deleted);
        Assert.Empty(testSettings.GitFolders);
        Assert.Null(testSettings.ActiveGitFolderId);
    }

    [Fact]
    public async Task ActiveGitFolder_WithNoFolders_ShouldReturnNull()
    {
        // Arrange
        var mockSettingsService = new Mock<ISettingsService>();
        var testSettings = new AppSettings
        {
            GitFolders = new List<GitFolder>()
        };
        mockSettingsService.Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(testSettings);
        
        var gitFolderService = new GitFolderService(mockSettingsService.Object);

        // Act
        var activeRepo = await gitFolderService.GetActiveAsync();

        // Assert
        Assert.Null(activeRepo);
    }

    [Fact]
    public async Task SwitchingGitFolders_ShouldUpdateLastAccessedTime()
    {
        // Arrange
        var mockSettingsService = new Mock<ISettingsService>();
        var testSettings = new AppSettings
        {
            GitFolders = new List<GitFolder>()
        };
        mockSettingsService.Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(testSettings);
        
        var gitFolderService = new GitFolderService(mockSettingsService.Object);

        // Act
        var repo1 = await gitFolderService.AddAsync("Repo 1", "/path/1");
        var repo2 = await gitFolderService.AddAsync("Repo 2", "/path/2");
        
        // Wait a bit to ensure time difference
        await Task.Delay(10);
        
        var beforeSwitch = DateTimeOffset.UtcNow;
        await gitFolderService.SetActiveAsync(repo2.Id);

        // Assert
        Assert.NotNull(repo2.LastAccessedAt);
        Assert.True(repo2.LastAccessedAt >= beforeSwitch);
    }
}
