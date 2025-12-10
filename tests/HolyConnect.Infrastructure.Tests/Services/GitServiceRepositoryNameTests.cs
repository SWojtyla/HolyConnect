using HolyConnect.Infrastructure.Services;
using LibGit2Sharp;
using Xunit;

namespace HolyConnect.Infrastructure.Tests.Services;

public class GitServiceRepositoryNameTests : IDisposable
{
    private readonly string _testRepoPath;
    private readonly GitService _gitService;

    public GitServiceRepositoryNameTests()
    {
        // Create a temporary directory for test repository
        _testRepoPath = Path.Combine(Path.GetTempPath(), $"HolyConnect_GitNameTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testRepoPath);
        _gitService = new GitService(() => _testRepoPath);
    }

    public void Dispose()
    {
        // Clean up test repository
        if (Directory.Exists(_testRepoPath))
        {
            RemoveReadOnlyAttributes(_testRepoPath);
            Directory.Delete(_testRepoPath, true);
        }
    }

    private void RemoveReadOnlyAttributes(string path)
    {
        var directoryInfo = new DirectoryInfo(path);
        foreach (var file in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
        {
            file.Attributes = FileAttributes.Normal;
        }
    }

    [Fact]
    public async Task GetRepositoryNameAsync_WithoutGitInit_ShouldReturnNull()
    {
        // Act
        var name = await _gitService.GetRepositoryNameAsync();

        // Assert
        Assert.Null(name);
    }

    [Fact]
    public async Task GetRepositoryNameAsync_WithLocalRepo_ShouldReturnDirectoryName()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);

        // Act
        var name = await _gitService.GetRepositoryNameAsync();

        // Assert
        Assert.NotNull(name);
        Assert.Equal(Path.GetFileName(_testRepoPath), name);
    }

    [Fact]
    public async Task GetRepositoryNameAsync_WithRemoteUrl_ShouldExtractRepoName()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        using (var repo = new Repository(_testRepoPath))
        {
            repo.Network.Remotes.Add("origin", "https://github.com/testuser/test-repo.git");
        }

        // Act
        var name = await _gitService.GetRepositoryNameAsync();

        // Assert
        Assert.Equal("test-repo", name);
    }

    [Fact]
    public async Task GetRepositoryNameAsync_WithDifferentPath_ShouldUseSpecifiedPath()
    {
        // Arrange
        var otherPath = Path.Combine(Path.GetTempPath(), $"HolyConnect_GitOtherTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(otherPath);
        try
        {
            Repository.Init(otherPath);

            // Act
            var name = await _gitService.GetRepositoryNameAsync(otherPath);

            // Assert
            Assert.NotNull(name);
            Assert.Equal(Path.GetFileName(otherPath), name);
        }
        finally
        {
            if (Directory.Exists(otherPath))
            {
                RemoveReadOnlyAttributes(otherPath);
                Directory.Delete(otherPath, true);
            }
        }
    }

    [Fact]
    public async Task IsRepositoryAsync_WithDifferentPath_ShouldCheckSpecifiedPath()
    {
        // Arrange
        var otherPath = Path.Combine(Path.GetTempPath(), $"HolyConnect_GitPathTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(otherPath);
        try
        {
            Repository.Init(otherPath);

            // Act
            var isRepo = await _gitService.IsRepositoryAsync(otherPath);
            var defaultIsRepo = await _gitService.IsRepositoryAsync();

            // Assert
            Assert.True(isRepo);
            Assert.False(defaultIsRepo);
        }
        finally
        {
            if (Directory.Exists(otherPath))
            {
                RemoveReadOnlyAttributes(otherPath);
                Directory.Delete(otherPath, true);
            }
        }
    }

    [Fact]
    public async Task GetCurrentBranchAsync_WithDifferentPath_ShouldUseSpecifiedPath()
    {
        // Arrange
        var otherPath = Path.Combine(Path.GetTempPath(), $"HolyConnect_GitBranchTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(otherPath);
        try
        {
            Repository.Init(otherPath);

            // Act
            var branch = await _gitService.GetCurrentBranchAsync(otherPath);

            // Assert
            Assert.NotNull(branch);
            Assert.True(branch == "master" || branch == "main");
        }
        finally
        {
            if (Directory.Exists(otherPath))
            {
                RemoveReadOnlyAttributes(otherPath);
                Directory.Delete(otherPath, true);
            }
        }
    }
}
