namespace HolyConnect.Domain.Entities;

/// <summary>
/// Represents a git repository configuration
/// </summary>
public class GitFolder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The working directory path (may be a subfolder within the repository)
    /// </summary>
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    /// The actual git repository root path (where .git folder is located)
    /// If null or empty, it's the same as Path
    /// </summary>
    public string? RepositoryPath { get; set; }
    
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastAccessedAt { get; set; }
}
