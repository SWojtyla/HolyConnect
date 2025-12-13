using HolyConnect.Application.Common;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Tests.Common;

public class CollectionHierarchyHelperTests
{
    [Fact]
    public void BuildHierarchy_WithFlatList_ShouldPopulateSubCollections()
    {
        // Arrange
        var rootId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var grandchildId = Guid.NewGuid();
        
        var collections = new List<Collection>
        {
            new Collection { Id = rootId, Name = "Root", ParentCollectionId = null },
            new Collection { Id = childId, Name = "Child", ParentCollectionId = rootId },
            new Collection { Id = grandchildId, Name = "Grandchild", ParentCollectionId = childId }
        };

        // Act
        var result = CollectionHierarchyHelper.BuildHierarchy(collections);

        // Assert
        Assert.Single(result); // Only one root collection
        var root = result.First();
        Assert.Equal("Root", root.Name);
        Assert.Single(root.SubCollections);
        
        var child = root.SubCollections.First();
        Assert.Equal("Child", child.Name);
        Assert.Single(child.SubCollections);
        
        var grandchild = child.SubCollections.First();
        Assert.Equal("Grandchild", grandchild.Name);
        Assert.Empty(grandchild.SubCollections);
    }

    [Fact]
    public void BuildHierarchy_WithMultipleRoots_ShouldReturnAllRoots()
    {
        // Arrange
        var root1Id = Guid.NewGuid();
        var root2Id = Guid.NewGuid();
        var child1Id = Guid.NewGuid();
        
        var collections = new List<Collection>
        {
            new Collection { Id = root1Id, Name = "Root1", ParentCollectionId = null },
            new Collection { Id = root2Id, Name = "Root2", ParentCollectionId = null },
            new Collection { Id = child1Id, Name = "Child1", ParentCollectionId = root1Id }
        };

        // Act
        var result = CollectionHierarchyHelper.BuildHierarchy(collections);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Name == "Root1");
        Assert.Contains(result, c => c.Name == "Root2");
        
        var root1 = result.First(c => c.Name == "Root1");
        Assert.Single(root1.SubCollections);
        Assert.Equal("Child1", root1.SubCollections.First().Name);
        
        var root2 = result.First(c => c.Name == "Root2");
        Assert.Empty(root2.SubCollections);
    }

    [Fact]
    public void BuildHierarchy_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var collections = new List<Collection>();

        // Act
        var result = CollectionHierarchyHelper.BuildHierarchy(collections);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void BuildHierarchy_WithOrphanedChild_ShouldNotIncludeInRoots()
    {
        // Arrange
        var rootId = Guid.NewGuid();
        var orphanId = Guid.NewGuid();
        var nonExistentParentId = Guid.NewGuid();
        
        var collections = new List<Collection>
        {
            new Collection { Id = rootId, Name = "Root", ParentCollectionId = null },
            new Collection { Id = orphanId, Name = "Orphan", ParentCollectionId = nonExistentParentId }
        };

        // Act
        var result = CollectionHierarchyHelper.BuildHierarchy(collections);

        // Assert
        Assert.Single(result); // Only the root, orphan should be excluded from roots
        Assert.Equal("Root", result.First().Name);
    }

    [Fact]
    public void FlattenHierarchy_WithNestedCollections_ShouldReturnAllCollections()
    {
        // Arrange
        var rootId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var grandchildId = Guid.NewGuid();
        
        var collections = new List<Collection>
        {
            new Collection { Id = rootId, Name = "Root", ParentCollectionId = null },
            new Collection { Id = childId, Name = "Child", ParentCollectionId = rootId },
            new Collection { Id = grandchildId, Name = "Grandchild", ParentCollectionId = childId }
        };
        
        var hierarchy = CollectionHierarchyHelper.BuildHierarchy(collections);

        // Act
        var flattened = CollectionHierarchyHelper.FlattenHierarchy(hierarchy);

        // Assert
        Assert.Equal(3, flattened.Count);
        Assert.Contains(flattened, c => c.Name == "Root");
        Assert.Contains(flattened, c => c.Name == "Child");
        Assert.Contains(flattened, c => c.Name == "Grandchild");
    }

    [Fact]
    public void FlattenHierarchy_WithMultipleRoots_ShouldReturnAllCollections()
    {
        // Arrange
        var root1Id = Guid.NewGuid();
        var root2Id = Guid.NewGuid();
        var child1Id = Guid.NewGuid();
        var child2Id = Guid.NewGuid();
        
        var collections = new List<Collection>
        {
            new Collection { Id = root1Id, Name = "Root1", ParentCollectionId = null },
            new Collection { Id = root2Id, Name = "Root2", ParentCollectionId = null },
            new Collection { Id = child1Id, Name = "Child1", ParentCollectionId = root1Id },
            new Collection { Id = child2Id, Name = "Child2", ParentCollectionId = root2Id }
        };
        
        var hierarchy = CollectionHierarchyHelper.BuildHierarchy(collections);

        // Act
        var flattened = CollectionHierarchyHelper.FlattenHierarchy(hierarchy);

        // Assert
        Assert.Equal(4, flattened.Count);
        Assert.Contains(flattened, c => c.Name == "Root1");
        Assert.Contains(flattened, c => c.Name == "Root2");
        Assert.Contains(flattened, c => c.Name == "Child1");
        Assert.Contains(flattened, c => c.Name == "Child2");
    }

    [Fact]
    public void BuildHierarchy_ClearsExistingSubCollections_BeforeBuilding()
    {
        // Arrange
        var rootId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        
        var root = new Collection 
        { 
            Id = rootId, 
            Name = "Root", 
            ParentCollectionId = null,
            SubCollections = new List<Collection>
            {
                new Collection { Id = Guid.NewGuid(), Name = "OldChild", ParentCollectionId = rootId }
            }
        };
        
        var child = new Collection { Id = childId, Name = "Child", ParentCollectionId = rootId };
        
        var collections = new List<Collection> { root, child };

        // Act
        var result = CollectionHierarchyHelper.BuildHierarchy(collections);

        // Assert
        var resultRoot = result.First();
        Assert.Single(resultRoot.SubCollections);
        Assert.Equal("Child", resultRoot.SubCollections.First().Name);
    }

    [Fact]
    public void BuildHierarchy_WithDuplicateIds_ShouldTakeFirstOccurrence()
    {
        // Arrange
        var duplicateId = Guid.NewGuid();
        
        var collections = new List<Collection>
        {
            new Collection { Id = duplicateId, Name = "First", ParentCollectionId = null },
            new Collection { Id = duplicateId, Name = "Duplicate", ParentCollectionId = null }
        };

        // Act
        var result = CollectionHierarchyHelper.BuildHierarchy(collections);

        // Assert
        Assert.Single(result);
        Assert.Equal("First", result.First().Name);
    }

    [Fact]
    public void PopulateRequests_WithRequestsInCollections_ShouldPopulateRequestsProperty()
    {
        // Arrange
        var collection1Id = Guid.NewGuid();
        var collection2Id = Guid.NewGuid();
        
        var collections = new List<Collection>
        {
            new Collection { Id = collection1Id, Name = "Collection1", ParentCollectionId = null },
            new Collection { Id = collection2Id, Name = "Collection2", ParentCollectionId = null }
        };
        
        var requests = new List<Request>
        {
            new RestRequest { Id = Guid.NewGuid(), Name = "Request1", CollectionId = collection1Id },
            new RestRequest { Id = Guid.NewGuid(), Name = "Request2", CollectionId = collection1Id },
            new RestRequest { Id = Guid.NewGuid(), Name = "Request3", CollectionId = collection2Id }
        };

        // Act
        CollectionHierarchyHelper.PopulateRequests(collections, requests);

        // Assert
        var col1 = collections.First(c => c.Id == collection1Id);
        var col2 = collections.First(c => c.Id == collection2Id);
        
        Assert.Equal(2, col1.Requests.Count);
        Assert.Single(col2.Requests);
        Assert.All(col1.Requests, r => Assert.Equal(collection1Id, r.CollectionId));
        Assert.All(col2.Requests, r => Assert.Equal(collection2Id, r.CollectionId));
    }

    [Fact]
    public void PopulateRequests_WithCollectionWithoutRequests_ShouldHaveEmptyList()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        
        var collections = new List<Collection>
        {
            new Collection { Id = collectionId, Name = "EmptyCollection", ParentCollectionId = null }
        };
        
        var requests = new List<Request>();

        // Act
        CollectionHierarchyHelper.PopulateRequests(collections, requests);

        // Assert
        var collection = collections.First();
        Assert.NotNull(collection.Requests);
        Assert.Empty(collection.Requests);
    }

    [Fact]
    public void PopulateRequests_WithRequestsWithoutCollectionId_ShouldNotIncludeOrphans()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        
        var collections = new List<Collection>
        {
            new Collection { Id = collectionId, Name = "Collection", ParentCollectionId = null }
        };
        
        var requests = new List<Request>
        {
            new RestRequest { Id = Guid.NewGuid(), Name = "Request1", CollectionId = collectionId },
            new RestRequest { Id = Guid.NewGuid(), Name = "OrphanRequest", CollectionId = null }
        };

        // Act
        CollectionHierarchyHelper.PopulateRequests(collections, requests);

        // Assert
        var collection = collections.First();
        Assert.Single(collection.Requests);
        Assert.Equal("Request1", collection.Requests.First().Name);
    }
}
