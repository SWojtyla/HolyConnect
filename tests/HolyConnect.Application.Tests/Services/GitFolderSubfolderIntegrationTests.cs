using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;
using Xunit;

namespace HolyConnect.Application.Tests.Services;

/// <summary>
/// Integration test demonstrating git subfolder support at the application layer
/// </summary>
public class GitFolderSubfolderIntegrationTests
{
    [Fact]
    public async Task AddGitFolder_WithSubfolderPath_ShouldDiscoverAndStoreRepositoryRoot()
    {
        // Arrange
        var mockSettingsService = new Mock<ISettingsService>();
        var mockGitService = new Mock<IGitService>();
        var testSettings = new AppSettings
        {
            GitFolders = new List<GitFolder>()
        };
        
        mockSettingsService.Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(testSettings);
        
        // Simulate discovering a parent repository
        var subfolderPath = @"D:\Projects\BlazorMusic\HolyConnect";
        var repositoryRootPath = @"D:\Projects\BlazorMusic";
        
        mockGitService.Setup(g => g.DiscoverRepositoryAsync(subfolderPath))
            .ReturnsAsync(repositoryRootPath);
        
        var gitFolderService = new GitFolderService(mockSettingsService.Object, mockGitService.Object);

        // Act
        var gitFolder = await gitFolderService.AddAsync("My API Tests", subfolderPath);

        // Assert
        Assert.NotNull(gitFolder);
        Assert.Equal("My API Tests", gitFolder.Name);
        Assert.Equal(subfolderPath, gitFolder.Path);
        Assert.Equal(repositoryRootPath, gitFolder.RepositoryPath);
        
        // Verify the git service was called to discover the repository
        mockGitService.Verify(g => g.DiscoverRepositoryAsync(subfolderPath), Times.Once);
    }

    [Fact]
    public async Task AddGitFolder_WithRepoRootPath_ShouldAlsoWork()
    {
        // Arrange
        var mockSettingsService = new Mock<ISettingsService>();
        var mockGitService = new Mock<IGitService>();
        var testSettings = new AppSettings
        {
            GitFolders = new List<GitFolder>()
        };
        
        mockSettingsService.Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(testSettings);
        
        // When the path IS the repository root, DiscoverRepositoryAsync returns the same path
        var repositoryPath = @"D:\Projects\BlazorMusic";
        
        mockGitService.Setup(g => g.DiscoverRepositoryAsync(repositoryPath))
            .ReturnsAsync(repositoryPath);
        
        var gitFolderService = new GitFolderService(mockSettingsService.Object, mockGitService.Object);

        // Act
        var gitFolder = await gitFolderService.AddAsync("Main Project", repositoryPath);

        // Assert
        Assert.NotNull(gitFolder);
        Assert.Equal("Main Project", gitFolder.Name);
        Assert.Equal(repositoryPath, gitFolder.Path);
        Assert.Equal(repositoryPath, gitFolder.RepositoryPath);
    }

    [Fact]
    public async Task AddGitFolder_MultipleSubfoldersInSameRepo_ShouldStoreCorrectly()
    {
        // Arrange
        var mockSettingsService = new Mock<ISettingsService>();
        var mockGitService = new Mock<IGitService>();
        var testSettings = new AppSettings
        {
            GitFolders = new List<GitFolder>()
        };
        
        mockSettingsService.Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(testSettings);
        
        var repositoryRootPath = @"D:\Projects\BlazorMusic";
        var subfolder1 = @"D:\Projects\BlazorMusic\HolyConnect";
        var subfolder2 = @"D:\Projects\BlazorMusic\AnotherProject";
        
        mockGitService.Setup(g => g.DiscoverRepositoryAsync(It.IsAny<string>()))
            .ReturnsAsync(repositoryRootPath);
        
        var gitFolderService = new GitFolderService(mockSettingsService.Object, mockGitService.Object);

        // Act
        var gitFolder1 = await gitFolderService.AddAsync("HolyConnect", subfolder1);
        var gitFolder2 = await gitFolderService.AddAsync("Another Project", subfolder2);

        // Assert
        Assert.Equal(2, testSettings.GitFolders.Count);
        
        // Both should have the same repository root
        Assert.Equal(repositoryRootPath, gitFolder1.RepositoryPath);
        Assert.Equal(repositoryRootPath, gitFolder2.RepositoryPath);
        
        // But different working directories
        Assert.Equal(subfolder1, gitFolder1.Path);
        Assert.Equal(subfolder2, gitFolder2.Path);
    }

    [Fact]
    public async Task AddGitFolder_WhenDiscoveryReturnsNull_ShouldStillCreate()
    {
        // Arrange
        var mockSettingsService = new Mock<ISettingsService>();
        var mockGitService = new Mock<IGitService>();
        var testSettings = new AppSettings
        {
            GitFolders = new List<GitFolder>()
        };
        
        mockSettingsService.Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(testSettings);
        
        var pathWithoutGit = @"D:\Projects\NoGit";
        
        // Discovery returns null when no repository is found
        mockGitService.Setup(g => g.DiscoverRepositoryAsync(pathWithoutGit))
            .ReturnsAsync((string?)null);
        
        var gitFolderService = new GitFolderService(mockSettingsService.Object, mockGitService.Object);

        // Act
        var gitFolder = await gitFolderService.AddAsync("No Git Here", pathWithoutGit);

        // Assert
        Assert.NotNull(gitFolder);
        Assert.Equal(pathWithoutGit, gitFolder.Path);
        Assert.Null(gitFolder.RepositoryPath);
    }
}
