using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Service for importing requests from various formats (curl, Bruno, etc.)
/// Requests are imported into collections and use the globally active environment for variable resolution.
/// </summary>
public interface IImportService
{
    /// <summary>
    /// Import a request from curl command
    /// </summary>
    /// <param name="curlCommand">The curl command string</param>
    /// <param name="collectionId">Optional target collection ID</param>
    /// <param name="customName">Optional custom name for the request. If not provided, name will be auto-generated from URL</param>
    /// <returns>Import result containing the imported request or error details</returns>
    Task<ImportResult> ImportFromCurlAsync(string curlCommand, Guid? collectionId = null, string? customName = null);
    
    /// <summary>
    /// Import a request from Bruno file content
    /// </summary>
    /// <param name="brunoFileContent">The Bruno file content (.bru format)</param>
    /// <param name="collectionId">Optional target collection ID</param>
    /// <param name="customName">Optional custom name for the request. If not provided, name from file will be used</param>
    /// <returns>Import result containing the imported request or error details</returns>
    Task<ImportResult> ImportFromBrunoAsync(string brunoFileContent, Guid? collectionId = null, string? customName = null);
    
    /// <summary>
    /// Import an environment from Bruno environment file (.bru file from environments/ folder)
    /// </summary>
    /// <param name="brunoEnvironmentContent">The Bruno environment file content</param>
    /// <param name="environmentName">Name for the environment</param>
    /// <returns>Import result containing the imported environment or error details</returns>
    Task<ImportResult> ImportFromBrunoEnvironmentAsync(string brunoEnvironmentContent, string environmentName);
    
    /// <summary>
    /// Import multiple requests from a Bruno folder and its subfolders
    /// Also imports environments from environments/ subfolder if present
    /// </summary>
    /// <param name="folderPath">The path to the folder containing Bruno files</param>
    /// <param name="parentCollectionId">Optional parent collection ID. If not provided, a root collection will be created</param>
    /// <returns>Import result containing all imported requests, collections, and environments</returns>
    Task<ImportResult> ImportFromBrunoFolderAsync(string folderPath, Guid? parentCollectionId = null);
    
    /// <summary>
    /// Check if the service can handle a specific import source
    /// </summary>
    /// <param name="source">Import source type</param>
    /// <returns>True if the source is supported</returns>
    bool CanImport(ImportSource source);
}
