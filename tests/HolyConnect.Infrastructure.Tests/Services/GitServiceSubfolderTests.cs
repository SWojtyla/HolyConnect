using HolyConnect.Infrastructure.Services;
using LibGit2Sharp;
using Xunit;

namespace HolyConnect.Infrastructure.Tests.Services;

/// <summary>
/// Tests for GitService subfolder support functionality
/// </summary>
public class GitServiceSubfolderTests : IDisposable
{
    private readonly string _testRepoPath;
    private readonly string _testSubfolderPath;
    private readonly GitService _gitService;

    public GitServiceSubfolderTests()
    {
        // Create a temporary directory for test repository
        _testRepoPath = Path.Combine(Path.GetTempPath(), $"HolyConnect_SubfolderTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testRepoPath);
        
        // Create a subfolder within the repository
        _testSubfolderPath = Path.Combine(_testRepoPath, "subfolder", "nested");
        Directory.CreateDirectory(_testSubfolderPath);
        
        _gitService = new GitService(() => _testSubfolderPath);
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
    public async Task IsRepositoryAsync_WithSubfolderInGitRepo_ShouldReturnTrue()
    {
        // Arrange
        Repository.Init(_testRepoPath);

        // Act
        var isRepo = await _gitService.IsRepositoryAsync(_testSubfolderPath);

        // Assert
        Assert.True(isRepo);
    }

    [Fact]
    public async Task IsRepositoryAsync_WithSubfolderNotInGitRepo_ShouldReturnFalse()
    {
        // Act - No git init, so this is not a repository
        var isRepo = await _gitService.IsRepositoryAsync(_testSubfolderPath);

        // Assert
        Assert.False(isRepo);
    }

    [Fact]
    public async Task DiscoverRepositoryAsync_WithSubfolder_ShouldReturnRepoRoot()
    {
        // Arrange
        Repository.Init(_testRepoPath);

        // Act
        var discoveredPath = await _gitService.DiscoverRepositoryAsync(_testSubfolderPath);

        // Assert
        Assert.NotNull(discoveredPath);
        // Normalize paths for comparison (handle different path separators)
        var normalizedDiscovered = Path.GetFullPath(discoveredPath);
        var normalizedExpected = Path.GetFullPath(_testRepoPath);
        Assert.Equal(normalizedExpected, normalizedDiscovered);
    }

    [Fact]
    public async Task DiscoverRepositoryAsync_WithNonRepoPath_ShouldReturnNull()
    {
        // Act - No git init
        var discoveredPath = await _gitService.DiscoverRepositoryAsync(_testSubfolderPath);

        // Assert
        Assert.Null(discoveredPath);
    }

    [Fact]
    public async Task GetCurrentBranchAsync_WithSubfolder_ShouldReturnBranch()
    {
        // Arrange
        Repository.Init(_testRepoPath);

        // Act
        var branch = await _gitService.GetCurrentBranchAsync(_testSubfolderPath);

        // Assert
        Assert.NotNull(branch);
        Assert.True(branch == "master" || branch == "main");
    }

    [Fact]
    public async Task GetRepositoryNameAsync_WithSubfolder_ShouldReturnRepoName()
    {
        // Arrange
        Repository.Init(_testRepoPath);
        var expectedRepoName = Path.GetFileName(_testRepoPath);

        // Act
        var repoName = await _gitService.GetRepositoryNameAsync(_testSubfolderPath);

        // Assert
        Assert.NotNull(repoName);
        Assert.Equal(expectedRepoName, repoName);
    }

    [Fact]
    public async Task CommitAllAsync_WithSubfolder_ShouldCommitSuccessfully()
    {
        // Arrange
        Repository.Init(_testRepoPath);
        
        // Create a file in the subfolder
        var testFilePath = Path.Combine(_testSubfolderPath, "test.txt");
        File.WriteAllText(testFilePath, "test content");

        // Initialize git config
        using (var repo = new Repository(_testRepoPath))
        {
            repo.Config.Set("user.name", "Test User");
            repo.Config.Set("user.email", "test@test.com");
        }

        // Act
        var committed = await _gitService.CommitAllAsync("Test commit");

        // Assert
        Assert.True(committed);
    }

    [Fact]
    public async Task GetStatusAsync_WithSubfolder_ShouldShowChanges()
    {
        // Arrange
        Repository.Init(_testRepoPath);
        
        // Create a file in the subfolder
        var testFilePath = Path.Combine(_testSubfolderPath, "test.txt");
        File.WriteAllText(testFilePath, "test content");

        // Act
        var status = await _gitService.GetStatusAsync();

        // Assert
        Assert.True(status.HasChanges);
        Assert.True(status.UntrackedFiles > 0);
    }

    [Fact]
    public async Task CreateBranchAsync_WithSubfolder_ShouldCreateBranch()
    {
        // Arrange
        Repository.Init(_testRepoPath);
        
        // Create initial commit
        var testFilePath = Path.Combine(_testRepoPath, "initial.txt");
        File.WriteAllText(testFilePath, "initial content");
        
        using (var repo = new Repository(_testRepoPath))
        {
            repo.Config.Set("user.name", "Test User");
            repo.Config.Set("user.email", "test@test.com");
            Commands.Stage(repo, "*");
            var signature = new Signature("Test User", "test@test.com", DateTimeOffset.Now);
            repo.Commit("Initial commit", signature, signature);
        }

        // Act
        var created = await _gitService.CreateBranchAsync("feature-branch");

        // Assert
        Assert.True(created);
        
        // Verify branch was created and checked out
        var currentBranch = await _gitService.GetCurrentBranchAsync(_testSubfolderPath);
        Assert.Equal("feature-branch", currentBranch);
    }
}
