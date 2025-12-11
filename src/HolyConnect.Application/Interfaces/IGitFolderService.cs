using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Provides operations for managing multiple git repository folders
/// </summary>
public interface IGitFolderService
{
    /// <summary>
    /// Gets all configured git folders
    /// </summary>
    Task<IEnumerable<GitFolder>> GetAllAsync();

    /// <summary>
    /// Gets the currently active git folder
    /// </summary>
    Task<GitFolder?> GetActiveAsync();

    /// <summary>
    /// Adds a new git folder
    /// </summary>
    Task<GitFolder> AddAsync(string name, string path);

    /// <summary>
    /// Updates an existing git folder
    /// </summary>
    Task<bool> UpdateAsync(GitFolder folder);

    /// <summary>
    /// Deletes a git folder by ID
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Sets the active git folder
    /// </summary>
    Task<bool> SetActiveAsync(Guid id);

    /// <summary>
    /// Gets a git folder by ID
    /// </summary>
    Task<GitFolder?> GetByIdAsync(Guid id);
}
