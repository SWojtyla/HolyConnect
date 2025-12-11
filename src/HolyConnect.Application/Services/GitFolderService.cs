using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

/// <summary>
/// Service for managing multiple git repository folders
/// </summary>
public class GitFolderService : IGitFolderService
{
    private readonly ISettingsService _settingsService;

    public GitFolderService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<IEnumerable<GitFolder>> GetAllAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();
        return settings.GitFolders;
    }

    public async Task<GitFolder?> GetActiveAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();
        if (settings.ActiveGitFolderId == null)
            return null;

        return settings.GitFolders.FirstOrDefault(f => f.Id == settings.ActiveGitFolderId);
    }

    public async Task<GitFolder> AddAsync(string name, string path)
    {
        var settings = await _settingsService.GetSettingsAsync();
        
        var folder = new GitFolder
        {
            Id = Guid.NewGuid(),
            Name = name,
            Path = path,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        settings.GitFolders.Add(folder);
        
        // If this is the first folder, make it active
        if (settings.GitFolders.Count == 1)
        {
            folder.IsActive = true;
            settings.ActiveGitFolderId = folder.Id;
        }

        await _settingsService.SaveSettingsAsync(settings);
        return folder;
    }

    public async Task<bool> UpdateAsync(GitFolder folder)
    {
        var settings = await _settingsService.GetSettingsAsync();
        var existingFolder = settings.GitFolders.FirstOrDefault(f => f.Id == folder.Id);
        
        if (existingFolder == null)
            return false;

        existingFolder.Name = folder.Name;
        existingFolder.Path = folder.Path;
        existingFolder.LastAccessedAt = DateTimeOffset.UtcNow;

        await _settingsService.SaveSettingsAsync(settings);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var settings = await _settingsService.GetSettingsAsync();
        var folder = settings.GitFolders.FirstOrDefault(f => f.Id == id);
        
        if (folder == null)
            return false;

        settings.GitFolders.Remove(folder);

        // If we're deleting the active folder, set a new active folder
        if (settings.ActiveGitFolderId == id)
        {
            var newActive = settings.GitFolders.FirstOrDefault();
            if (newActive != null)
            {
                newActive.IsActive = true;
                settings.ActiveGitFolderId = newActive.Id;
            }
            else
            {
                settings.ActiveGitFolderId = null;
            }
        }

        await _settingsService.SaveSettingsAsync(settings);
        return true;
    }

    public async Task<bool> SetActiveAsync(Guid id)
    {
        var settings = await _settingsService.GetSettingsAsync();
        var folder = settings.GitFolders.FirstOrDefault(f => f.Id == id);
        
        if (folder == null)
            return false;

        // Update IsActive flags
        foreach (var f in settings.GitFolders)
        {
            f.IsActive = f.Id == id;
        }

        folder.LastAccessedAt = DateTimeOffset.UtcNow;
        settings.ActiveGitFolderId = id;

        await _settingsService.SaveSettingsAsync(settings);
        return true;
    }

    public async Task<GitFolder?> GetByIdAsync(Guid id)
    {
        var settings = await _settingsService.GetSettingsAsync();
        return settings.GitFolders.FirstOrDefault(f => f.Id == id);
    }
}
