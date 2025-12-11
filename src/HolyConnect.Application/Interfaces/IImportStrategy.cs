using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Strategy interface for different import format parsers
/// </summary>
public interface IImportStrategy
{
    /// <summary>
    /// Gets the import source type this strategy handles
    /// </summary>
    ImportSource Source { get; }
    
    /// <summary>
    /// Parse the input content and create a request
    /// </summary>
    /// <param name="content">The content to parse (curl command or file content)</param>
    /// <param name="environmentId">Target environment ID</param>
    /// <param name="collectionId">Optional target collection ID</param>
    /// <param name="customName">Optional custom name for the request</param>
    /// <returns>The parsed request or null if parsing fails</returns>
    Request? Parse(string content, Guid environmentId, Guid? collectionId, string? customName);
}
