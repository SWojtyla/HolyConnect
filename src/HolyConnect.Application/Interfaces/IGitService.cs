namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Provides basic git operations for managing collections in version control
/// </summary>
public interface IGitService
{
    /// <summary>
    /// Gets the current branch name
    /// </summary>
    Task<string?> GetCurrentBranchAsync();

    /// <summary>
    /// Gets all local branches
    /// </summary>
    Task<IEnumerable<string>> GetBranchesAsync();

    /// <summary>
    /// Switches to the specified branch
    /// </summary>
    Task<bool> CheckoutBranchAsync(string branchName);

    /// <summary>
    /// Creates a new branch from current HEAD
    /// </summary>
    Task<bool> CreateBranchAsync(string branchName);

    /// <summary>
    /// Fetches changes from remote
    /// </summary>
    Task<bool> FetchAsync();

    /// <summary>
    /// Pulls changes from remote to current branch
    /// </summary>
    Task<bool> PullAsync();

    /// <summary>
    /// Commits all changes with the specified message
    /// </summary>
    Task<bool> CommitAllAsync(string message);

    /// <summary>
    /// Pushes current branch to remote
    /// </summary>
    Task<bool> PushAsync();

    /// <summary>
    /// Gets the repository status (pending changes)
    /// </summary>
    Task<GitStatus> GetStatusAsync();

    /// <summary>
    /// Initializes a new git repository at the specified path
    /// </summary>
    Task<bool> InitRepositoryAsync(string path);

    /// <summary>
    /// Checks if the storage path is a git repository
    /// </summary>
    Task<bool> IsRepositoryAsync();
}

/// <summary>
/// Represents the status of a git repository
/// </summary>
public class GitStatus
{
    public bool HasChanges { get; set; }
    public int ModifiedFiles { get; set; }
    public int AddedFiles { get; set; }
    public int DeletedFiles { get; set; }
    public int UntrackedFiles { get; set; }
    public List<string> Changes { get; set; } = new();
}
