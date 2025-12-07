using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Service for fetching and caching GraphQL schemas through introspection queries
/// </summary>
public interface IGraphQLSchemaService
{
    /// <summary>
    /// Fetches the GraphQL schema from the specified endpoint using introspection
    /// </summary>
    /// <param name="url">The GraphQL endpoint URL</param>
    /// <param name="request">The GraphQL request containing authentication and headers</param>
    /// <returns>The schema as a JSON string, or null if introspection fails</returns>
    Task<string?> FetchSchemaAsync(string url, GraphQLRequest request);

    /// <summary>
    /// Gets the cached schema for the specified endpoint, if available
    /// </summary>
    /// <param name="url">The GraphQL endpoint URL</param>
    /// <returns>The cached schema as a JSON string, or null if not cached</returns>
    string? GetCachedSchema(string url);

    /// <summary>
    /// Clears the schema cache for the specified endpoint
    /// </summary>
    /// <param name="url">The GraphQL endpoint URL</param>
    void ClearCache(string url);

    /// <summary>
    /// Clears all cached schemas
    /// </summary>
    void ClearAllCaches();
}
