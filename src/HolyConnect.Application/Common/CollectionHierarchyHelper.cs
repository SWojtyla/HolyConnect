using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Common;

/// <summary>
/// Helper class for building collection hierarchy from flat collection lists.
/// </summary>
public static class CollectionHierarchyHelper
{
    /// <summary>
    /// Builds a hierarchical tree structure from a flat list of collections.
    /// Populates the SubCollections navigation property for each collection.
    /// </summary>
    /// <param name="collections">Flat list of all collections</param>
    /// <returns>List of root-level collections with SubCollections populated</returns>
    public static List<Collection> BuildHierarchy(IEnumerable<Collection> collections)
    {
        var collectionList = collections.ToList();
        
        // Use GroupBy to handle potential duplicates, taking the first occurrence
        var collectionDict = collectionList
            .GroupBy(c => c.Id)
            .ToDictionary(g => g.Key, g => g.First());
        
        // Clear existing subcollections to avoid duplicates
        foreach (var collection in collectionDict.Values)
        {
            collection.SubCollections = new List<Collection>();
        }
        
        // Build the hierarchy by populating SubCollections
        foreach (var collection in collectionDict.Values)
        {
            if (collection.ParentCollectionId.HasValue && 
                collectionDict.TryGetValue(collection.ParentCollectionId.Value, out var parent))
            {
                parent.SubCollections.Add(collection);
            }
        }
        
        // Return only root-level collections (those without a parent)
        return collectionDict.Values.Where(c => !c.ParentCollectionId.HasValue).ToList();
    }
    
    /// <summary>
    /// Populates the Requests navigation property for each collection in the hierarchy.
    /// </summary>
    /// <param name="collections">Collections to populate</param>
    /// <param name="requests">All requests to assign to collections</param>
    public static void PopulateRequests(IEnumerable<Collection> collections, IEnumerable<Request> requests)
    {
        var requestsByCollectionId = requests
            .Where(r => r.CollectionId.HasValue)
            .GroupBy(r => r.CollectionId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        foreach (var collection in collections)
        {
            collection.Requests = requestsByCollectionId.TryGetValue(collection.Id, out var collectionRequests)
                ? collectionRequests
                : new List<Request>();
        }
    }
    
    /// <summary>
    /// Gets all collections in a hierarchy including descendants.
    /// </summary>
    /// <param name="rootCollections">Root-level collections with hierarchy populated</param>
    /// <returns>Flattened list of all collections</returns>
    public static List<Collection> FlattenHierarchy(IEnumerable<Collection> rootCollections)
    {
        var result = new List<Collection>();
        
        foreach (var collection in rootCollections)
        {
            AddCollectionAndDescendants(collection, result);
        }
        
        return result;
    }
    
    private static void AddCollectionAndDescendants(Collection collection, List<Collection> result)
    {
        result.Add(collection);
        
        if (collection.SubCollections != null && collection.SubCollections.Any())
        {
            foreach (var subCollection in collection.SubCollections)
            {
                AddCollectionAndDescendants(subCollection, result);
            }
        }
    }
}
