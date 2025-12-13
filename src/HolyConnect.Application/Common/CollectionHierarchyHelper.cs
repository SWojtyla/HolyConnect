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
        var collectionDict = collectionList.ToDictionary(c => c.Id);
        
        // Clear existing subcollections to avoid duplicates
        foreach (var collection in collectionList)
        {
            collection.SubCollections = new List<Collection>();
        }
        
        // Build the hierarchy by populating SubCollections
        foreach (var collection in collectionList)
        {
            if (collection.ParentCollectionId.HasValue && 
                collectionDict.TryGetValue(collection.ParentCollectionId.Value, out var parent))
            {
                parent.SubCollections.Add(collection);
            }
        }
        
        // Return only root-level collections (those without a parent)
        return collectionList.Where(c => !c.ParentCollectionId.HasValue).ToList();
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
