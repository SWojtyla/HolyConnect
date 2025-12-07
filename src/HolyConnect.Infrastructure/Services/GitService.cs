using HolyConnect.Application.Interfaces;
using LibGit2Sharp;

namespace HolyConnect.Infrastructure.Services;

/// <summary>
/// Implementation of git operations using LibGit2Sharp
/// </summary>
public class GitService : IGitService
{
    private const int SHORT_SHA_LENGTH = 7;
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
        var repoPath = GetRepositoryPath();
        if (!Repository.IsValid(repoPath))
            return Task.FromResult(false);

        using var repo = new Repository(repoPath);
        
        // Check if there's a remote configured
        var remote = repo.Network.Remotes.FirstOrDefault();
        if (remote == null)
            return Task.FromResult(false);

        var options = new FetchOptions
        {
            CredentialsProvider = GetCredentialsProvider()
        };
        
        var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
        Commands.Fetch(repo, remote.Name, refSpecs, options, "fetch");
        return Task.FromResult(true);
    }

    public Task<bool> PullAsync()
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
        var options = new PullOptions
        {
            FetchOptions = new FetchOptions
            {
                CredentialsProvider = GetCredentialsProvider()
            }
        };
        
        Commands.Pull(repo, signature, options);
        return Task.FromResult(true);
    }

    public Task<bool> CommitAllAsync(string message)
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult(false);

            using var repo = new Repository(repoPath);
            
            // Stage all changes (respects .gitignore if present)
            // Using "*" to stage all modified, added, and deleted files
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

        // Set up push options with credentials provider
        var options = new PushOptions
        {
            CredentialsProvider = GetCredentialsProvider()
        };
        
        // If branch doesn't have upstream, set it
        if (branch.TrackedBranch == null)
        {
            repo.Branches.Update(branch,
                b => b.Remote = remote.Name,
                b => b.UpstreamBranch = branch.CanonicalName);
        }
        
        repo.Network.Push(branch, options);
        return Task.FromResult(true);
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

    private LibGit2Sharp.Handlers.CredentialsHandler GetCredentialsProvider()
    {
        return (url, usernameFromUrl, types) =>
        {
            // Try to use git's credential helper to get credentials
            // This is the same mechanism command-line git uses
            if (types.HasFlag(SupportedCredentialTypes.UsernamePassword))
            {
                try
                {
                    var credentials = GetCredentialsFromGit(url);
                    if (credentials != null)
                    {
                        return new UsernamePasswordCredentials
                        {
                            Username = credentials.Username,
                            Password = credentials.Password
                        };
                    }
                }
                catch
                {
                    // Fall through to default
                }
            }
            
            // For SSH, return default which should use SSH agent or default keys
            return new DefaultCredentials();
        };
    }

    private class GitCredentials
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    private GitCredentials? GetCredentialsFromGit(string url)
    {
        try
        {
            // Use git credential fill to get credentials from system's credential helpers
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "credential fill",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            // Send the URL to git credential fill
            using (var writer = process.StandardInput)
            {
                writer.WriteLine($"url={url}");
                writer.WriteLine();
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
            {
                var lines = output.Split('\n');
                string? username = null;
                string? password = null;

                foreach (var line in lines)
                {
                    if (line.StartsWith("username="))
                        username = line.Substring("username=".Length).Trim();
                    else if (line.StartsWith("password="))
                        password = line.Substring("password=".Length).Trim();
                }

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    return new GitCredentials { Username = username, Password = password };
                }
            }
        }
        catch
        {
            // If git credential helper fails, return null
        }

        return null;
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

    public Task<bool> DeleteBranchAsync(string branchName)
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

            // Cannot delete current branch
            if (branch.IsCurrentRepositoryHead)
                return Task.FromResult(false);

            repo.Branches.Remove(branch);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<IEnumerable<GitCommitInfo>> GetIncomingCommitsAsync()
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult(Enumerable.Empty<GitCommitInfo>());

            using var repo = new Repository(repoPath);
            var currentBranch = repo.Head;
            
            // Check if there's a tracked branch
            if (currentBranch.TrackedBranch == null)
                return Task.FromResult(Enumerable.Empty<GitCommitInfo>());

            // Get commits that are in remote but not in local
            var filter = new CommitFilter
            {
                IncludeReachableFrom = currentBranch.TrackedBranch,
                ExcludeReachableFrom = currentBranch
            };

            var incomingCommits = repo.Commits.QueryBy(filter)
                .Select(c => new GitCommitInfo
                {
                    Sha = c.Sha,
                    ShortSha = c.Sha.Length >= SHORT_SHA_LENGTH ? c.Sha.Substring(0, SHORT_SHA_LENGTH) : c.Sha,
                    Message = c.MessageShort,
                    Author = c.Author.Name,
                    Date = c.Author.When
                })
                .ToList();

            return Task.FromResult<IEnumerable<GitCommitInfo>>(incomingCommits);
        }
        catch
        {
            return Task.FromResult(Enumerable.Empty<GitCommitInfo>());
        }
    }

    public Task<IEnumerable<GitCommitInfo>> GetOutgoingCommitsAsync()
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult(Enumerable.Empty<GitCommitInfo>());

            using var repo = new Repository(repoPath);
            var currentBranch = repo.Head;
            
            // Check if there's a tracked branch
            if (currentBranch.TrackedBranch == null)
                return Task.FromResult(Enumerable.Empty<GitCommitInfo>());

            // Get commits that are in local but not in remote
            var filter = new CommitFilter
            {
                IncludeReachableFrom = currentBranch,
                ExcludeReachableFrom = currentBranch.TrackedBranch
            };

            var outgoingCommits = repo.Commits.QueryBy(filter)
                .Select(c => new GitCommitInfo
                {
                    Sha = c.Sha,
                    ShortSha = c.Sha.Length >= SHORT_SHA_LENGTH ? c.Sha.Substring(0, SHORT_SHA_LENGTH) : c.Sha,
                    Message = c.MessageShort,
                    Author = c.Author.Name,
                    Date = c.Author.When
                })
                .ToList();

            return Task.FromResult<IEnumerable<GitCommitInfo>>(outgoingCommits);
        }
        catch
        {
            return Task.FromResult(Enumerable.Empty<GitCommitInfo>());
        }
    }

    public Task<IEnumerable<GitCommitInfo>> GetCommitHistoryAsync(int maxCount = 50)
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult(Enumerable.Empty<GitCommitInfo>());

            using var repo = new Repository(repoPath);
            var commits = repo.Commits
                .Take(maxCount)
                .Select(c => new GitCommitInfo
                {
                    Sha = c.Sha,
                    ShortSha = c.Sha.Length >= SHORT_SHA_LENGTH ? c.Sha.Substring(0, SHORT_SHA_LENGTH) : c.Sha,
                    Message = c.MessageShort,
                    Author = c.Author.Name,
                    Date = c.Author.When
                })
                .ToList();

            return Task.FromResult<IEnumerable<GitCommitInfo>>(commits);
        }
        catch
        {
            return Task.FromResult(Enumerable.Empty<GitCommitInfo>());
        }
    }

    public Task<IEnumerable<GitFileChange>> GetFileChangesAsync()
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult(Enumerable.Empty<GitFileChange>());

            using var repo = new Repository(repoPath);
            var status = repo.RetrieveStatus();
            
            var fileChanges = new List<GitFileChange>();

            foreach (var item in status)
            {
                fileChanges.Add(new GitFileChange
                {
                    FilePath = item.FilePath,
                    Status = GetChangeType(item.State),
                    IsStaged = item.State.HasFlag(FileStatus.NewInIndex) || 
                               item.State.HasFlag(FileStatus.ModifiedInIndex) || 
                               item.State.HasFlag(FileStatus.DeletedFromIndex) ||
                               item.State.HasFlag(FileStatus.RenamedInIndex)
                });
            }

            return Task.FromResult<IEnumerable<GitFileChange>>(fileChanges);
        }
        catch
        {
            return Task.FromResult(Enumerable.Empty<GitFileChange>());
        }
    }

    public Task<bool> StageFileAsync(string filePath)
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult(false);

            using var repo = new Repository(repoPath);
            Commands.Stage(repo, filePath);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<bool> UnstageFileAsync(string filePath)
    {
        try
        {
            var repoPath = GetRepositoryPath();
            if (!Repository.IsValid(repoPath))
                return Task.FromResult(false);

            using var repo = new Repository(repoPath);
            Commands.Unstage(repo, filePath);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
