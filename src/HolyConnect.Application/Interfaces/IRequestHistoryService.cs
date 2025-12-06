using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Service for managing request execution history
/// </summary>
public interface IRequestHistoryService
{
    /// <summary>
    /// Adds a new history entry
    /// </summary>
    Task AddHistoryEntryAsync(RequestHistoryEntry entry);
    
    /// <summary>
    /// Gets the most recent history entries (up to maxCount)
    /// </summary>
    Task<IEnumerable<RequestHistoryEntry>> GetHistoryAsync(int maxCount = 10);
    
    /// <summary>
    /// Clears all history entries
    /// </summary>
    Task ClearHistoryAsync();
}
