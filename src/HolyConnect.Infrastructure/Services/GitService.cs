using HolyConnect.Application.Interfaces;
using LibGit2Sharp;
using System.Text;

namespace HolyConnect.Infrastructure.Services;

/// <summary>
/// Implementation of git operations using LibGit2Sharp
/// </summary>
public class GitService : IGitService
{
    private readonly Func<string> _getStoragePath;

    public GitService(Func<string> getStoragePath)
    {
        _getStoragePath = getStoragePath;
    }

    private string GetRepositoryPath()
    {
        return _getStoragePath();
    }

    public Task<bool> IsRepositoryAsync()
    {
        try
        {
            var path = GetRepositoryPath();
            return Task.FromResult(Repository.IsValid(path));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> InitRepositoryAsync(string path)
    {
        try
        {
            Repository.Init(path);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<string?> GetCurrentBranchAsync()
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult<string?>(null);

            using var repo = new Repository(repoPath);
            return Task.FromResult<string?>(repo.Head?.FriendlyName);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }

    public Task<IEnumerable<string>> GetBranchesAsync()
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult(Enumerable.Empty<string>());

            using var repo = new Repository(repoPath);
            var branches = repo.Branches
                .Where(b => !b.IsRemote)
                .Select(b => b.FriendlyName)
                .ToList();
            return Task.FromResult<IEnumerable<string>>(branches);
        }
        catch
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }
    }

    public Task<bool> CheckoutBranchAsync(string branchName)
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult(false);

            using var repo = new Repository(repoPath);
            var branch = repo.Branches[branchName];
            if (branch == null)
                return Task.FromResult(false);

            Commands.Checkout(repo, branch);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> CreateBranchAsync(string branchName)
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult(false);

            using var repo = new Repository(repoPath);
            
            // Check if branch already exists
            if (repo.Branches[branchName] != null)
                return Task.FromResult(false);

            var branch = repo.CreateBranch(branchName);
            Commands.Checkout(repo, branch);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> FetchAsync()
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult(false);

            using var repo = new Repository(repoPath);
            
            // Check if there's a remote configured
            var remote = repo.Network.Remotes.FirstOrDefault();
            if (remote == null)
                return Task.FromResult(false);

            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(repo, remote.Name, refSpecs, null, "fetch");
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> PullAsync()
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult(false);

            using var repo = new Repository(repoPath);
            
            // Check if there's a remote configured
            var remote = repo.Network.Remotes.FirstOrDefault();
            if (remote == null)
                return Task.FromResult(false);

            var signature = GetSignature(repo);
            var options = new PullOptions();
            
            Commands.Pull(repo, signature, options);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> CommitAllAsync(string message)
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult(false);

            using var repo = new Repository(repoPath);
            
            // Stage all changes
            Commands.Stage(repo, "*");
            
            var signature = GetSignature(repo);
            
            // Check if there are changes to commit
            var status = repo.RetrieveStatus();
            if (!status.Any())
                return Task.FromResult(false);

            repo.Commit(message, signature, signature);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> PushAsync()
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult(false);

            using var repo = new Repository(repoPath);
            
            var branch = repo.Head;
            if (branch == null)
                return Task.FromResult(false);

            // Check if there's a remote configured
            var remote = repo.Network.Remotes.FirstOrDefault();
            if (remote == null)
                return Task.FromResult(false);

            var options = new PushOptions();
            repo.Network.Push(branch, options);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<GitStatus> GetStatusAsync()
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult(new GitStatus());

            using var repo = new Repository(repoPath);
            var status = repo.RetrieveStatus();
            
            var gitStatus = new GitStatus
            {
                HasChanges = status.Any(),
                ModifiedFiles = status.Modified.Count(),
                AddedFiles = status.Added.Count(),
                DeletedFiles = status.Removed.Count(),
                UntrackedFiles = status.Untracked.Count(),
                Changes = new List<string>()
            };

            // Build list of changes
            foreach (var item in status)
            {
                var changeType = GetChangeType(item.State);
                gitStatus.Changes.Add($"{changeType}: {item.FilePath}");
            }

            return Task.FromResult(gitStatus);
        }
        catch
        {
            return Task.FromResult(new GitStatus());
        }
    }

    private Signature GetSignature(Repository repo)
    {
        // Try to get signature from config
        var name = repo.Config.Get<string>("user.name")?.Value ?? "HolyConnect User";
        var email = repo.Config.Get<string>("user.email")?.Value ?? "user@holyconnect.local";
        return new Signature(name, email, DateTimeOffset.Now);
    }

    private string GetChangeType(FileStatus status)
    {
        return status switch
        {
            FileStatus.NewInIndex => "Added",
            FileStatus.ModifiedInIndex => "Modified",
            FileStatus.DeletedFromIndex => "Deleted",
            FileStatus.NewInWorkdir => "Untracked",
            FileStatus.ModifiedInWorkdir => "Modified",
            FileStatus.DeletedFromWorkdir => "Deleted",
            FileStatus.RenamedInIndex => "Renamed",
            _ => "Changed"
        };
    }
}
