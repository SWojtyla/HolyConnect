using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Service for importing requests from various formats (curl, Bruno, etc.)
/// </summary>
public interface IImportService
{
    /// <summary>
    /// Import a request from curl command
    /// </summary>
    /// <param name="curlCommand">The curl command string</param>
    /// <param name="environmentId">Target environment ID</param>
    /// <param name="collectionId">Optional target collection ID</param>
    /// <param name="customName">Optional custom name for the request. If not provided, name will be auto-generated from URL</param>
    /// <returns>Import result containing the imported request or error details</returns>
    Task<ImportResult> ImportFromCurlAsync(string curlCommand, Guid environmentId, Guid? collectionId = null, string? customName = null);
    
    /// <summary>
    /// Check if the service can handle a specific import source
    /// </summary>
    /// <param name="source">Import source type</param>
    /// <returns>True if the source is supported</returns>
    bool CanImport(ImportSource source);
}
