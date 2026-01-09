using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

/// <summary>
/// Service for global search functionality across environments, collections, requests, and flows
/// </summary>
public class GlobalSearchService : IGlobalSearchService
{
    private readonly IEnvironmentService _environmentService;
    private readonly ICollectionService _collectionService;
    private readonly IRequestService _requestService;
    private readonly IFlowService _flowService;
    
    public GlobalSearchService(
        IEnvironmentService environmentService,
        ICollectionService collectionService,
        IRequestService requestService,
        IFlowService flowService)
    {
        _environmentService = environmentService;
        _collectionService = collectionService;
        _requestService = requestService;
        _flowService = flowService;
    }
    
    public async Task<IEnumerable<SearchResult>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Enumerable.Empty<SearchResult>();
        }
        
        var results = new List<SearchResult>();
        
        // Search environments
        var environments = await _environmentService.GetAllEnvironmentsAsync();
        results.AddRange(SearchEnvironments(environments, query));
        
        // Search collections
        var collections = await _collectionService.GetAllCollectionsAsync();
        results.AddRange(SearchCollections(collections, query));
        
        // Search requests
        var requests = await _requestService.GetAllRequestsAsync();
        results.AddRange(await SearchRequestsAsync(requests, query));
        
        // Search flows
        var flows = await _flowService.GetAllFlowsAsync();
        results.AddRange(SearchFlows(flows, query));
        
        // Sort by relevance score (descending)
        return results.OrderByDescending(r => r.RelevanceScore)
                     .ThenBy(r => r.Name);
    }
    
    private IEnumerable<SearchResult> SearchEnvironments(IEnumerable<Domain.Entities.Environment> environments, string query)
    {
        foreach (var env in environments)
        {
            var score = CalculateRelevanceScore(query, env.Name, env.Description);
            if (score > 0)
            {
                yield return new SearchResult
                {
                    Type = SearchResultType.Environment,
                    Id = env.Id,
                    Name = env.Name,
                    Description = env.Description,
                    NavigationUrl = $"/environment/{env.Id}",
                    Icon = "dns", // Material icon name
                    RelevanceScore = score
                };
            }
        }
    }
    
    private IEnumerable<SearchResult> SearchCollections(IEnumerable<Collection> collections, string query)
    {
        foreach (var collection in collections)
        {
            var score = CalculateRelevanceScore(query, collection.Name, collection.Description);
            if (score > 0)
            {
                var parentContext = GetCollectionPath(collections, collection);
                
                yield return new SearchResult
                {
                    Type = SearchResultType.Collection,
                    Id = collection.Id,
                    Name = collection.Name,
                    Description = collection.Description,
                    NavigationUrl = $"/collection/{collection.Id}",
                    Icon = "folder", // Material icon name
                    ParentContext = parentContext,
                    RelevanceScore = score
                };
            }
        }
    }
    
    private async Task<IEnumerable<SearchResult>> SearchRequestsAsync(IEnumerable<Request> requests, string query)
    {
        var results = new List<SearchResult>();
        var collections = await _collectionService.GetAllCollectionsAsync();
        
        foreach (var request in requests)
        {
            var score = CalculateRelevanceScore(query, request.Name, request.Url);
            if (score > 0)
            {
                var parentCollection = collections.FirstOrDefault(c => c.Id == request.CollectionId);
                var parentContext = parentCollection != null 
                    ? GetCollectionPath(collections, parentCollection)
                    : null;
                
                var icon = request.Type switch
                {
                    RequestType.Rest => "http",
                    RequestType.GraphQL => "graphic_eq",
                    RequestType.WebSocket => "cable",
                    _ => "api"
                };
                
                results.Add(new SearchResult
                {
                    Type = SearchResultType.Request,
                    Id = request.Id,
                    Name = request.Name,
                    Description = $"{request.Type}: {request.Url}",
                    NavigationUrl = $"/collection/{request.CollectionId}/request/{request.Id}",
                    Icon = icon,
                    ParentContext = parentContext,
                    RelevanceScore = score
                });
            }
        }
        
        return results;
    }
    
    private IEnumerable<SearchResult> SearchFlows(IEnumerable<Flow> flows, string query)
    {
        foreach (var flow in flows)
        {
            var score = CalculateRelevanceScore(query, flow.Name, flow.Description);
            if (score > 0)
            {
                yield return new SearchResult
                {
                    Type = SearchResultType.Flow,
                    Id = flow.Id,
                    Name = flow.Name,
                    Description = flow.Description,
                    NavigationUrl = $"/flow/{flow.Id}",
                    Icon = "account_tree", // Material icon name
                    RelevanceScore = score
                };
            }
        }
    }
    
    private double CalculateRelevanceScore(string query, string name, string? description = null)
    {
        var normalizedQuery = query.ToLowerInvariant();
        var normalizedName = name.ToLowerInvariant();
        var normalizedDesc = description?.ToLowerInvariant() ?? string.Empty;
        
        double score = 0;
        
        // Exact match in name (highest score)
        if (normalizedName == normalizedQuery)
        {
            score += 100;
        }
        // Starts with query in name
        else if (normalizedName.StartsWith(normalizedQuery))
        {
            score += 80;
        }
        // Contains query in name
        else if (normalizedName.Contains(normalizedQuery))
        {
            score += 50;
        }
        
        // Contains in description
        if (!string.IsNullOrEmpty(normalizedDesc) && normalizedDesc.Contains(normalizedQuery))
        {
            score += 20;
        }
        
        // Fuzzy matching - check for individual words
        var queryWords = normalizedQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in queryWords)
        {
            if (normalizedName.Contains(word))
            {
                score += 10;
            }
            if (normalizedDesc.Contains(word))
            {
                score += 5;
            }
        }
        
        return score;
    }
    
    private string? GetCollectionPath(IEnumerable<Collection> allCollections, Collection collection)
    {
        var path = new List<string>();
        var current = collection;
        
        // Walk up the parent chain
        while (current.ParentCollectionId.HasValue)
        {
            var parent = allCollections.FirstOrDefault(c => c.Id == current.ParentCollectionId.Value);
            if (parent == null) break;
            
            path.Insert(0, parent.Name);
            current = parent;
        }
        
        return path.Any() ? string.Join(" / ", path) : null;
    }
}
