using System.Collections.Generic;
using System.Threading.Tasks;

namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Service for global search functionality
/// </summary>
public interface IGlobalSearchService
{
    /// <summary>
    /// Searches across all searchable items
    /// </summary>
    /// <param name="query">The search query</param>
    /// <returns>List of search results</returns>
    Task<IEnumerable<SearchResult>> SearchAsync(string query);
}

/// <summary>
/// Represents a search result
/// </summary>
public class SearchResult
{
    /// <summary>
    /// The type of item found
    /// </summary>
    public SearchResultType Type { get; set; }
    
    /// <summary>
    /// The unique identifier of the item
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// The name/title of the item
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional description or context
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// The navigation URL for the item
    /// </summary>
    public string NavigationUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Icon for the item type
    /// </summary>
    public string Icon { get; set; } = string.Empty;
    
    /// <summary>
    /// Parent context (e.g., collection name for a request)
    /// </summary>
    public string? ParentContext { get; set; }
    
    /// <summary>
    /// Search relevance score
    /// </summary>
    public double RelevanceScore { get; set; }
}

/// <summary>
/// Types of searchable items
/// </summary>
public enum SearchResultType
{
    Environment,
    Collection,
    Request,
    Flow
}
