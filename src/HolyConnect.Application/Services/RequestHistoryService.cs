using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

/// <summary>
/// Service for managing request execution history
/// </summary>
public class RequestHistoryService : IRequestHistoryService
{
    private readonly IRepository<RequestHistoryEntry> _historyRepository;
    private const int MaxHistorySize = 10;

    public RequestHistoryService(IRepository<RequestHistoryEntry> historyRepository)
    {
        _historyRepository = historyRepository;
    }

    public async Task AddHistoryEntryAsync(RequestHistoryEntry entry)
    {
        entry.Id = Guid.NewGuid();
        entry.Timestamp = DateTime.UtcNow;
        
        await _historyRepository.AddAsync(entry);
        
        // Keep only the last 10 entries
        await TrimHistoryAsync();
    }

    public async Task<IEnumerable<RequestHistoryEntry>> GetHistoryAsync(int maxCount = MaxHistorySize)
    {
        var allEntries = await _historyRepository.GetAllAsync();
        return allEntries
            .OrderByDescending(e => e.Timestamp)
            .Take(Math.Min(maxCount, MaxHistorySize));
    }

    public async Task ClearHistoryAsync()
    {
        var allEntries = await _historyRepository.GetAllAsync();
        foreach (var entry in allEntries)
        {
            await _historyRepository.DeleteAsync(entry.Id);
        }
    }

    private async Task TrimHistoryAsync()
    {
        var allEntries = await _historyRepository.GetAllAsync();
        var entriesToRemove = allEntries
            .OrderByDescending(e => e.Timestamp)
            .Skip(MaxHistorySize);
        
        foreach (var entry in entriesToRemove)
        {
            await _historyRepository.DeleteAsync(entry.Id);
        }
    }
}
