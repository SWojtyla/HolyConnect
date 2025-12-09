using HolyConnect.Infrastructure.Services;
using LibGit2Sharp;
using Xunit;

namespace HolyConnect.Infrastructure.Tests.Services;

public class GitServiceTests : IDisposable
{
    private readonly string _testRepoPath;
    private readonly GitService _gitService;

    public GitServiceTests()
    {
        // Create a temporary directory for test repository
        _testRepoPath = Path.Combine(Path.GetTempPath(), $"HolyConnect_GitTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testRepoPath);
        _gitService = new GitService(() => _testRepoPath);
    }

    public void Dispose()
    {
        // Clean up test repository
        if (Directory.Exists(_testRepoPath))
        {
            // Remove read-only attributes from .git directory
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

    private string CreateRemoteRepository()
    {
        var remoteRepoPath = Path.Combine(Path.GetTempPath(), $"HolyConnect_RemoteTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(remoteRepoPath);
        Repository.Init(remoteRepoPath, isBare: true);
        return remoteRepoPath;
    }

    private void SetupRemoteTracking(Repository localRepo, string remoteRepoPath)
    {
        localRepo.Network.Remotes.Add("origin", remoteRepoPath);
        var branch = localRepo.Head;
        var pushOptions = new PushOptions();
        localRepo.Branches.Update(branch,
            b => b.Remote = "origin",
            b => b.UpstreamBranch = branch.CanonicalName);
        localRepo.Network.Push(branch, pushOptions);
    }

    [Fact]
    public async Task IsRepositoryAsync_WithoutGitInit_ShouldReturnFalse()
    {
        // Act
        var isRepo = await _gitService.IsRepositoryAsync();

        // Assert
        Assert.False(isRepo);
    }

    [Fact]
    public async Task InitRepositoryAsync_ShouldCreateGitRepository()
    {
        // Act
        var success = await _gitService.InitRepositoryAsync(_testRepoPath);

        // Assert
        Assert.True(success);
        Assert.True(await _gitService.IsRepositoryAsync());
    }

    [Fact]
    public async Task GetCurrentBranchAsync_AfterInit_ShouldReturnMasterOrMain()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);

        // Act
        var branch = await _gitService.GetCurrentBranchAsync();

        // Assert
        Assert.NotNull(branch);
        Assert.True(branch == "master" || branch == "main");
    }

    [Fact]
    public async Task GetBranchesAsync_AfterInit_ShouldReturnDefaultBranch()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();

        // Act
        var branches = await _gitService.GetBranchesAsync();

        // Assert
        Assert.NotEmpty(branches);
        Assert.Contains(branches, b => b == "master" || b == "main");
    }

    [Fact]
    public async Task CreateBranchAsync_WithValidName_ShouldCreateAndCheckoutBranch()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();

        // Act
        var success = await _gitService.CreateBranchAsync("feature-branch");

        // Assert
        Assert.True(success);
        var currentBranch = await _gitService.GetCurrentBranchAsync();
        Assert.Equal("feature-branch", currentBranch);
        
        var branches = await _gitService.GetBranchesAsync();
        Assert.Contains("feature-branch", branches);
    }

    [Fact]
    public async Task CreateBranchAsync_WithExistingBranch_ShouldReturnFalse()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        await _gitService.CreateBranchAsync("test-branch");

        // Act
        var success = await _gitService.CreateBranchAsync("test-branch");

        // Assert
        Assert.False(success);
    }

    [Fact]
    public async Task CheckoutBranchAsync_WithValidBranch_ShouldSwitchBranch()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        await _gitService.CreateBranchAsync("new-branch");
        
        // Switch back to master/main
        using var repo = new Repository(_testRepoPath);
        var masterBranch = repo.Branches.FirstOrDefault(b => b.FriendlyName == "master" || b.FriendlyName == "main");
        Commands.Checkout(repo, masterBranch);

        // Act
        var success = await _gitService.CheckoutBranchAsync("new-branch");

        // Assert
        Assert.True(success);
        var currentBranch = await _gitService.GetCurrentBranchAsync();
        Assert.Equal("new-branch", currentBranch);
    }

    [Fact]
    public async Task GetStatusAsync_WithNoChanges_ShouldReturnNoChanges()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();

        // Act
        var status = await _gitService.GetStatusAsync();

        // Assert
        Assert.NotNull(status);
        Assert.False(status.HasChanges);
        Assert.Equal(0, status.ModifiedFiles);
        Assert.Equal(0, status.AddedFiles);
        Assert.Equal(0, status.DeletedFiles);
        Assert.Equal(0, status.UntrackedFiles);
    }

    [Fact]
    public async Task GetStatusAsync_WithUntrackedFile_ShouldDetectChanges()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        
        // Create a new file
        File.WriteAllText(Path.Combine(_testRepoPath, "newfile.txt"), "test content");

        // Act
        var status = await _gitService.GetStatusAsync();

        // Assert
        Assert.NotNull(status);
        Assert.True(status.HasChanges);
        Assert.Equal(1, status.UntrackedFiles);
    }

    [Fact]
    public async Task CommitAllAsync_WithChanges_ShouldCommitSuccessfully()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        File.WriteAllText(Path.Combine(_testRepoPath, "test.txt"), "modified content");

        // Act
        var success = await _gitService.CommitAllAsync("Test commit");

        // Assert
        Assert.True(success);
        
        var status = await _gitService.GetStatusAsync();
        Assert.False(status.HasChanges);
    }

    [Fact]
    public async Task CommitAllAsync_WithNoChanges_ShouldReturnFalse()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();

        // Act
        var success = await _gitService.CommitAllAsync("Test commit");

        // Assert
        Assert.False(success);
    }

    [Fact]
    public async Task FetchAsync_WithoutRemote_ShouldReturnFalse()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);

        // Act
        var success = await _gitService.FetchAsync();

        // Assert
        Assert.False(success);
    }

    [Fact]
    public async Task PullAsync_WithoutRemote_ShouldReturnFalse()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);

        // Act
        var success = await _gitService.PullAsync();

        // Assert
        Assert.False(success);
    }

    [Fact]
    public async Task PushAsync_WithoutRemote_ShouldReturnFalse()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);

        // Act
        var success = await _gitService.PushAsync();

        // Assert
        Assert.False(success);
    }

    [Fact]
    public async Task DeleteBranchAsync_WithValidBranch_ShouldDeleteBranch()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        await _gitService.CreateBranchAsync("branch-to-delete");
        
        // Switch back to master/main
        using var repo = new Repository(_testRepoPath);
        var masterBranch = repo.Branches.FirstOrDefault(b => b.FriendlyName == "master" || b.FriendlyName == "main");
        Commands.Checkout(repo, masterBranch);

        // Act
        var success = await _gitService.DeleteBranchAsync("branch-to-delete");

        // Assert
        Assert.True(success);
        var branches = await _gitService.GetBranchesAsync();
        Assert.DoesNotContain("branch-to-delete", branches);
    }

    [Fact]
    public async Task DeleteBranchAsync_WithCurrentBranch_ShouldReturnFalse()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        var currentBranch = await _gitService.GetCurrentBranchAsync();

        // Act
        var success = await _gitService.DeleteBranchAsync(currentBranch!);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public async Task DeleteBranchAsync_WithNonExistentBranch_ShouldReturnFalse()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();

        // Act
        var success = await _gitService.DeleteBranchAsync("non-existent-branch");

        // Assert
        Assert.False(success);
    }

    [Fact]
    public async Task GetIncomingCommitsAsync_WithoutRemote_ShouldReturnEmpty()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();

        // Act
        var incomingCommits = await _gitService.GetIncomingCommitsAsync();

        // Assert
        Assert.Empty(incomingCommits);
    }

    [Fact]
    public async Task GetOutgoingCommitsAsync_WithoutRemote_ShouldReturnEmpty()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();

        // Act
        var outgoingCommits = await _gitService.GetOutgoingCommitsAsync();

        // Assert
        Assert.Empty(outgoingCommits);
    }

    [Fact]
    public async Task GetIncomingCommitsAsync_WithRemoteAhead_ShouldReturnIncomingCommits()
    {
        // Arrange
        var remoteRepoPath = CreateRemoteRepository();
        
        try
        {
            // Create local repository
            await _gitService.InitRepositoryAsync(_testRepoPath);
            CreateInitialCommit();
            
            using var localRepo = new Repository(_testRepoPath);
            SetupRemoteTracking(localRepo, remoteRepoPath);
            
            // Create new commit in remote (simulated by creating commit locally, pushing, then resetting)
            var branch = localRepo.Head;
            var pushOptions = new PushOptions();
            File.WriteAllText(Path.Combine(_testRepoPath, "remote-file.txt"), "remote content");
            Commands.Stage(localRepo, "remote-file.txt");
            var signature = new Signature("Test User", "test@example.com", DateTimeOffset.Now);
            localRepo.Commit("Remote commit", signature, signature);
            localRepo.Network.Push(branch, pushOptions);
            
            // Reset local branch to previous commit (simulating remote ahead)
            var headCommit = localRepo.Head.Tip;
            var previousCommit = headCommit.Parents.FirstOrDefault();
            if (previousCommit != null)
            {
                localRepo.Reset(ResetMode.Hard, previousCommit);
            }
            
            // Fetch to update remote tracking branch
            var remote = localRepo.Network.Remotes["origin"];
            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(localRepo, remote.Name, refSpecs, null, "test fetch");

            // Act
            var incomingCommits = await _gitService.GetIncomingCommitsAsync();

            // Assert
            Assert.NotEmpty(incomingCommits);
            Assert.Contains(incomingCommits, c => c.Message.Contains("Remote commit"));
        }
        finally
        {
            // Clean up remote repository
            if (Directory.Exists(remoteRepoPath))
            {
                RemoveReadOnlyAttributes(remoteRepoPath);
                Directory.Delete(remoteRepoPath, true);
            }
        }
    }

    [Fact]
    public async Task GetOutgoingCommitsAsync_WithLocalAhead_ShouldReturnOutgoingCommits()
    {
        // Arrange
        var remoteRepoPath = CreateRemoteRepository();
        
        try
        {
            // Create local repository
            await _gitService.InitRepositoryAsync(_testRepoPath);
            CreateInitialCommit();
            
            using var localRepo = new Repository(_testRepoPath);
            SetupRemoteTracking(localRepo, remoteRepoPath);
            
            // Create new local commit
            File.WriteAllText(Path.Combine(_testRepoPath, "local-file.txt"), "local content");
            Commands.Stage(localRepo, "local-file.txt");
            var signature = new Signature("Test User", "test@example.com", DateTimeOffset.Now);
            localRepo.Commit("Local commit", signature, signature);

            // Act
            var outgoingCommits = await _gitService.GetOutgoingCommitsAsync();

            // Assert
            Assert.NotEmpty(outgoingCommits);
            Assert.Contains(outgoingCommits, c => c.Message.Contains("Local commit"));
        }
        finally
        {
            // Clean up remote repository
            if (Directory.Exists(remoteRepoPath))
            {
                RemoveReadOnlyAttributes(remoteRepoPath);
                Directory.Delete(remoteRepoPath, true);
            }
        }
    }

    private void CreateInitialCommit()
    {
        using var repo = new Repository(_testRepoPath);
        
        // Create a test file
        var testFile = Path.Combine(_testRepoPath, "test.txt");
        File.WriteAllText(testFile, "initial content");
        
        // Stage and commit
        Commands.Stage(repo, "test.txt");
        var signature = new Signature("Test User", "test@example.com", DateTimeOffset.Now);
        repo.Commit("Initial commit", signature, signature);
    }

    [Fact]
    public async Task GetCommitHistoryAsync_WithCommits_ShouldReturnCommitList()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();

        // Act
        var commits = await _gitService.GetCommitHistoryAsync(10);

        // Assert
        Assert.NotEmpty(commits);
        Assert.Single(commits);
        Assert.Equal("Initial commit", commits.First().Message);
    }

    [Fact]
    public async Task GetCommitHistoryAsync_WithMaxCount_ShouldLimitResults()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        
        using var repo = new Repository(_testRepoPath);
        var signature = new Signature("Test User", "test@example.com", DateTimeOffset.Now);
        
        // Create multiple commits
        for (int i = 0; i < 5; i++)
        {
            var file = Path.Combine(_testRepoPath, $"file{i}.txt");
            File.WriteAllText(file, $"content {i}");
            Commands.Stage(repo, $"file{i}.txt");
            repo.Commit($"Commit {i}", signature, signature);
        }

        // Act
        var commits = await _gitService.GetCommitHistoryAsync(3);

        // Assert
        Assert.Equal(3, commits.Count());
    }

    [Fact]
    public async Task GetFileChangesAsync_WithoutChanges_ShouldReturnEmptyList()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();

        // Act
        var changes = await _gitService.GetFileChangesAsync();

        // Assert
        Assert.Empty(changes);
    }

    [Fact]
    public async Task GetFileChangesAsync_WithUnstagedChanges_ShouldReturnChanges()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        
        var modifiedFile = Path.Combine(_testRepoPath, "test.txt");
        File.WriteAllText(modifiedFile, "modified content");

        // Act
        var changes = await _gitService.GetFileChangesAsync();

        // Assert
        Assert.Single(changes);
        Assert.Equal("test.txt", changes.First().FilePath);
        Assert.Equal("Modified", changes.First().Status);
        Assert.False(changes.First().IsStaged);
    }

    [Fact]
    public async Task StageFileAsync_WithValidFile_ShouldStageFile()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        
        var modifiedFile = Path.Combine(_testRepoPath, "test.txt");
        File.WriteAllText(modifiedFile, "modified content");

        // Act
        var success = await _gitService.StageFileAsync("test.txt");
        var changes = await _gitService.GetFileChangesAsync();

        // Assert
        Assert.True(success);
        Assert.Single(changes);
        Assert.True(changes.First().IsStaged);
    }

    [Fact]
    public async Task UnstageFileAsync_WithStagedFile_ShouldUnstageFile()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        
        var modifiedFile = Path.Combine(_testRepoPath, "test.txt");
        File.WriteAllText(modifiedFile, "modified content");
        await _gitService.StageFileAsync("test.txt");

        // Act
        var success = await _gitService.UnstageFileAsync("test.txt");
        var changes = await _gitService.GetFileChangesAsync();

        // Assert
        Assert.True(success);
        Assert.Single(changes);
        Assert.False(changes.First().IsStaged);
    }

    [Fact]
    public async Task GetFileChangesAsync_WithMultipleChanges_ShouldReturnAllChanges()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        
        // Modified file
        var modifiedFile = Path.Combine(_testRepoPath, "test.txt");
        File.WriteAllText(modifiedFile, "modified content");
        
        // New file
        var newFile = Path.Combine(_testRepoPath, "new.txt");
        File.WriteAllText(newFile, "new content");

        // Act
        var changes = await _gitService.GetFileChangesAsync();

        // Assert
        Assert.Equal(2, changes.Count());
        Assert.Contains(changes, c => c.FilePath == "test.txt" && c.Status == "Modified");
        Assert.Contains(changes, c => c.FilePath == "new.txt" && c.Status == "Untracked");
    }

    [Fact]
    public async Task IsSecretsTrackedAsync_WithoutRepository_ShouldReturnFalse()
    {
        // Act
        var isTracked = await _gitService.IsSecretsTrackedAsync();

        // Assert
        Assert.False(isTracked);
    }

    [Fact]
    public async Task IsSecretsTrackedAsync_WithSecretsInWorkdir_ShouldReturnTrue()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();

        // Create secrets folder with a file
        var secretsDir = Path.Combine(_testRepoPath, "secrets");
        Directory.CreateDirectory(secretsDir);
        var secretFile = Path.Combine(secretsDir, "test-secrets.json");
        File.WriteAllText(secretFile, "{\"key\": \"value\"}");

        // Act
        var isTracked = await _gitService.IsSecretsTrackedAsync();

        // Assert
        Assert.True(isTracked);
    }

    [Fact]
    public async Task IsSecretsTrackedAsync_WithSecretsJsonInWorkdir_ShouldReturnTrue()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();

        // Create a secrets json file
        var secretFile = Path.Combine(_testRepoPath, "environment-123-secrets.json");
        File.WriteAllText(secretFile, "{\"key\": \"value\"}");

        // Act
        var isTracked = await _gitService.IsSecretsTrackedAsync();

        // Assert
        Assert.True(isTracked);
    }

    [Fact]
    public async Task IsSecretsTrackedAsync_WithSecretsAlreadyCommitted_ShouldReturnTrue()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();

        // Create and commit secrets folder
        var secretsDir = Path.Combine(_testRepoPath, "secrets");
        Directory.CreateDirectory(secretsDir);
        var secretFile = Path.Combine(secretsDir, "test-secrets.json");
        File.WriteAllText(secretFile, "{\"key\": \"value\"}");

        using (var repo = new Repository(_testRepoPath))
        {
            Commands.Stage(repo, "secrets/test-secrets.json");
            var signature = new Signature("Test User", "test@example.com", DateTimeOffset.Now);
            repo.Commit("Add secrets", signature, signature);
        }

        // Act
        var isTracked = await _gitService.IsSecretsTrackedAsync();

        // Assert
        Assert.True(isTracked);
    }

    [Fact]
    public async Task IsSecretsTrackedAsync_WithNoSecrets_ShouldReturnFalse()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();

        // Act
        var isTracked = await _gitService.IsSecretsTrackedAsync();

        // Assert
        Assert.False(isTracked);
    }

    [Fact]
    public async Task AddSecretsToGitignoreAsync_WithoutRepository_ShouldReturnFalse()
    {
        // Act
        var success = await _gitService.AddSecretsToGitignoreAsync();

        // Assert
        Assert.False(success);
    }

    [Fact]
    public async Task AddSecretsToGitignoreAsync_WithoutExistingGitignore_ShouldCreateFile()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        var gitignorePath = Path.Combine(_testRepoPath, ".gitignore");

        // Act
        var success = await _gitService.AddSecretsToGitignoreAsync();

        // Assert
        Assert.True(success);
        Assert.True(File.Exists(gitignorePath));
        
        var content = File.ReadAllText(gitignorePath);
        Assert.Contains("secrets/", content);
        Assert.Contains("*secrets*.json", content);
        Assert.Contains("Secret variables", content);
    }

    [Fact]
    public async Task AddSecretsToGitignoreAsync_WithExistingGitignore_ShouldAppendEntries()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        var gitignorePath = Path.Combine(_testRepoPath, ".gitignore");
        
        // Create existing .gitignore with some content
        File.WriteAllText(gitignorePath, "*.log\nbin/\nobj/");

        // Act
        var success = await _gitService.AddSecretsToGitignoreAsync();

        // Assert
        Assert.True(success);
        
        var content = File.ReadAllText(gitignorePath);
        Assert.Contains("*.log", content);
        Assert.Contains("bin/", content);
        Assert.Contains("secrets/", content);
        Assert.Contains("*secrets*.json", content);
    }

    [Fact]
    public async Task AddSecretsToGitignoreAsync_WithSecretsAlreadyInGitignore_ShouldNotDuplicate()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        var gitignorePath = Path.Combine(_testRepoPath, ".gitignore");
        
        // Create .gitignore with secrets already present
        File.WriteAllText(gitignorePath, "secrets/\n*secrets*.json\n*.log");

        // Act
        var success = await _gitService.AddSecretsToGitignoreAsync();

        // Assert
        Assert.True(success);
        
        var content = File.ReadAllText(gitignorePath);
        var lines = content.Split('\n');
        
        // Count occurrences - should only be one of each
        Assert.Equal(1, lines.Count(l => l.Trim() == "secrets/"));
        Assert.Equal(1, lines.Count(l => l.Trim() == "*secrets*.json"));
    }

    [Fact]
    public async Task AddSecretsToGitignoreAsync_ShouldPreventSecretsFromBeingTracked()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();

        // Create secrets folder
        var secretsDir = Path.Combine(_testRepoPath, "secrets");
        Directory.CreateDirectory(secretsDir);
        var secretFile = Path.Combine(secretsDir, "test-secrets.json");
        File.WriteAllText(secretFile, "{\"key\": \"value\"}");

        // Add to gitignore
        await _gitService.AddSecretsToGitignoreAsync();

        // Stage all files
        using (var repo = new Repository(_testRepoPath))
        {
            Commands.Stage(repo, "*");
        }

        // Act - check if secrets are tracked
        var isTracked = await _gitService.IsSecretsTrackedAsync();

        // Assert - secrets should not be tracked because they're in .gitignore
        Assert.False(isTracked);
    }

    [Fact]
    public async Task CommitStagedAsync_WithStagedChanges_ShouldCommitSuccessfully()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        File.WriteAllText(Path.Combine(_testRepoPath, "test.txt"), "modified content");
        
        // Stage the file manually
        using (var repo = new Repository(_testRepoPath))
        {
            Commands.Stage(repo, "test.txt");
        }

        // Act
        var success = await _gitService.CommitStagedAsync("Test commit staged changes");

        // Assert
        Assert.True(success);
        
        var status = await _gitService.GetStatusAsync();
        Assert.False(status.HasChanges);
    }

    [Fact]
    public async Task CommitStagedAsync_WithOnlyStagedFiles_ShouldNotCommitUnstagedFiles()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        
        // Create two files - stage only one
        File.WriteAllText(Path.Combine(_testRepoPath, "staged.txt"), "staged content");
        File.WriteAllText(Path.Combine(_testRepoPath, "unstaged.txt"), "unstaged content");
        
        using (var repo = new Repository(_testRepoPath))
        {
            Commands.Stage(repo, "staged.txt");
        }

        // Act
        var success = await _gitService.CommitStagedAsync("Commit only staged file");

        // Assert
        Assert.True(success);
        
        // Check that unstaged file is still in working directory
        var status = await _gitService.GetStatusAsync();
        Assert.True(status.HasChanges);
        Assert.Equal(1, status.UntrackedFiles);
    }

    [Fact]
    public async Task CommitStagedAsync_WithNoStagedChanges_ShouldReturnFalse()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        
        // Create a file but don't stage it
        File.WriteAllText(Path.Combine(_testRepoPath, "unstaged.txt"), "unstaged content");

        // Act
        var success = await _gitService.CommitStagedAsync("Try to commit with no staged changes");

        // Assert
        Assert.False(success);
    }

    [Fact]
    public async Task RevertFileAsync_WithModifiedFile_ShouldRevertChanges()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        
        var testFilePath = Path.Combine(_testRepoPath, "test.txt");
        File.WriteAllText(testFilePath, "modified content");

        // Act
        var success = await _gitService.RevertFileAsync("test.txt");

        // Assert
        Assert.True(success);
        var content = File.ReadAllText(testFilePath);
        Assert.Equal("initial content", content);
    }

    [Fact]
    public async Task RevertFileAsync_WithUntrackedFile_ShouldDeleteFile()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        
        var untrackedFilePath = Path.Combine(_testRepoPath, "untracked.txt");
        File.WriteAllText(untrackedFilePath, "new file content");

        // Act
        var success = await _gitService.RevertFileAsync("untracked.txt");

        // Assert
        Assert.True(success);
        Assert.False(File.Exists(untrackedFilePath));
    }

    [Fact]
    public async Task RevertFileAsync_WithDeletedFile_ShouldRestoreFile()
    {
        // Arrange
        await _gitService.InitRepositoryAsync(_testRepoPath);
        CreateInitialCommit();
        
        var testFilePath = Path.Combine(_testRepoPath, "test.txt");
        File.Delete(testFilePath);

        // Act
        var success = await _gitService.RevertFileAsync("test.txt");

        // Assert
        Assert.True(success);
        Assert.True(File.Exists(testFilePath));
        var content = File.ReadAllText(testFilePath);
        Assert.Equal("initial content", content);
    }
}
