using HolyConnect.Infrastructure.Persistence;
using Xunit;

namespace HolyConnect.Infrastructure.Tests.Repositories;

public class MultiFileRepositoryTests : IDisposable
{
    private readonly string _testDirectory;

    public MultiFileRepositoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"HolyConnectTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public async Task AddAsync_WithValidEntity_ShouldPersistToFile()
    {
        // Arrange
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities");
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test Entity" };

        // Act
        var result = await repository.AddAsync(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Name, result.Name);

        // Verify file was created (without name selector, should use ID as filename)
        var directoryPath = Path.Combine(_testDirectory, "test-entities");
        var expectedFileName = $"{entity.Id}.json";
        var filePath = Path.Combine(directoryPath, expectedFileName);
        Assert.True(File.Exists(filePath), $"Expected file {expectedFileName} to exist");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingEntity_ShouldReturnEntity()
    {
        // Arrange
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities");
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test Entity" };
        await repository.AddAsync(entity);

        // Act
        var result = await repository.GetByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingEntity_ShouldReturnNull()
    {
        // Arrange
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities");
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await repository.GetByIdAsync(nonExistingId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleEntities_ShouldReturnAllEntities()
    {
        // Arrange
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities");
        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "Entity 1" };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "Entity 2" };
        var entity3 = new TestEntity { Id = Guid.NewGuid(), Name = "Entity 3" };
        
        await repository.AddAsync(entity1);
        await repository.AddAsync(entity2);
        await repository.AddAsync(entity3);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        var entities = result.ToList();
        Assert.Equal(3, entities.Count);
        Assert.Contains(entities, e => e.Id == entity1.Id);
        Assert.Contains(entities, e => e.Id == entity2.Id);
        Assert.Contains(entities, e => e.Id == entity3.Id);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyDirectory_ShouldReturnEmptyList()
    {
        // Arrange
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities");

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingEntity_ShouldUpdateEntity()
    {
        // Arrange
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities");
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Original Name" };
        await repository.AddAsync(entity);

        // Act
        entity.Name = "Updated Name";
        await repository.UpdateAsync(entity);

        // Assert
        var result = await repository.GetByIdAsync(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingEntity_ShouldRemoveEntity()
    {
        // Arrange
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities");
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test Entity" };
        await repository.AddAsync(entity);

        // Act
        await repository.DeleteAsync(entity.Id);

        // Assert
        var result = await repository.GetByIdAsync(entity.Id);
        Assert.Null(result);

        // Verify file was deleted
        var expectedFilePath = Path.Combine(_testDirectory, "test-entities", $"{entity.Id}.json");
        Assert.False(File.Exists(expectedFilePath));
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingEntity_ShouldNotThrow()
    {
        // Arrange
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities");
        var nonExistingId = Guid.NewGuid();

        // Act & Assert (should not throw)
        await repository.DeleteAsync(nonExistingId);
    }

    [Fact]
    public async Task MultiFileRepository_WithManyEntities_ShouldHandleEachInSeparateFile()
    {
        // Arrange
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities");
        var entities = Enumerable.Range(1, 10)
            .Select(i => new TestEntity { Id = Guid.NewGuid(), Name = $"Entity {i}" })
            .ToList();

        // Act
        foreach (var entity in entities)
        {
            await repository.AddAsync(entity);
        }

        // Assert
        var directoryPath = Path.Combine(_testDirectory, "test-entities");
        var files = Directory.GetFiles(directoryPath, "*.json");
        Assert.Equal(10, files.Length);

        var allEntities = await repository.GetAllAsync();
        Assert.Equal(10, allEntities.Count());
    }

    [Fact]
    public async Task AddAsync_WithDuplicateName_ShouldThrowException()
    {
        // Arrange
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities",
            e => e.Name); // Provide name selector
        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "Test Entity" };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "Test Entity" }; // Same name, different ID
        await repository.AddAsync(entity1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.AddAsync(entity2));
        Assert.Contains("An entity with the name 'Test Entity' already exists", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ShouldThrowException()
    {
        // Arrange
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities",
            e => e.Name); // Provide name selector
        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "Entity 1" };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "Entity 2" };
        await repository.AddAsync(entity1);
        await repository.AddAsync(entity2);

        // Act - Try to rename entity2 to have the same name as entity1
        entity2.Name = "Entity 1";
        
        // Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdateAsync(entity2));
        Assert.Contains("An entity with the name 'Entity 1' already exists", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_WithSameEntityRenamed_ShouldSucceed()
    {
        // Arrange
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities",
            e => e.Name); // Provide name selector
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Original Name" };
        await repository.AddAsync(entity);

        // Act - Rename the same entity (should not throw)
        entity.Name = "New Name";
        await repository.UpdateAsync(entity);

        // Assert
        var result = await repository.GetByIdAsync(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        
        // Verify old file is deleted and new file exists
        var directoryPath = Path.Combine(_testDirectory, "test-entities");
        var oldFilePath = Path.Combine(directoryPath, "Original Name.json");
        var newFilePath = Path.Combine(directoryPath, "New Name.json");
        Assert.False(File.Exists(oldFilePath), "Old file should be deleted");
        Assert.True(File.Exists(newFilePath), "New file should exist");
    }

    [Fact]
    public async Task AddAsync_WithSameName_WithoutNameSelector_ShouldSucceed()
    {
        // Arrange - Repository without name selector uses ID as filename
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities"); // No name selector
        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "Get List objects" };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "Get List objects" }; // Same name, different ID

        // Act - Both should succeed because filenames are based on ID, not name
        var result1 = await repository.AddAsync(entity1);
        var result2 = await repository.AddAsync(entity2);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(entity1.Id, result1.Id);
        Assert.Equal(entity2.Id, result2.Id);
        Assert.Equal("Get List objects", result1.Name);
        Assert.Equal("Get List objects", result2.Name);

        // Verify both files exist with ID-based names
        var directoryPath = Path.Combine(_testDirectory, "test-entities");
        var file1 = Path.Combine(directoryPath, $"{entity1.Id}.json");
        var file2 = Path.Combine(directoryPath, $"{entity2.Id}.json");
        Assert.True(File.Exists(file1), $"Expected file {entity1.Id}.json to exist");
        Assert.True(File.Exists(file2), $"Expected file {entity2.Id}.json to exist");

        // Verify we can retrieve both entities
        var retrieved1 = await repository.GetByIdAsync(entity1.Id);
        var retrieved2 = await repository.GetByIdAsync(entity2.Id);
        Assert.NotNull(retrieved1);
        Assert.NotNull(retrieved2);
        Assert.Equal(entity1.Id, retrieved1.Id);
        Assert.Equal(entity2.Id, retrieved2.Id);
    }

    [Fact]
    public async Task AddAsync_WithHierarchicalPath_ShouldStoreInSubdirectory()
    {
        // Arrange - Repository with hierarchical path provider
        var parent1Id = Guid.NewGuid();
        var parent2Id = Guid.NewGuid();
        
        Task<string> GetHierarchicalPath(TestEntity e)
        {
            if (!e.ParentId.HasValue) return Task.FromResult(string.Empty);
            // Simulate parent path lookup
            if (e.ParentId == parent1Id) return Task.FromResult("folder1");
            if (e.ParentId == parent2Id) return Task.FromResult("folder2");
            return Task.FromResult(string.Empty);
        }
        
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities",
            e => e.Name,
            GetHierarchicalPath,
            e => e.ParentId);
        
        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "GetUser", ParentId = parent1Id };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "GetUser", ParentId = parent2Id };

        // Act
        await repository.AddAsync(entity1);
        await repository.AddAsync(entity2);

        // Assert - Both should exist in different folders
        var file1 = Path.Combine(_testDirectory, "test-entities", "folder1", "GetUser.json");
        var file2 = Path.Combine(_testDirectory, "test-entities", "folder2", "GetUser.json");
        Assert.True(File.Exists(file1), $"Expected file to exist at {file1}");
        Assert.True(File.Exists(file2), $"Expected file to exist at {file2}");
        
        // Verify both can be retrieved
        var retrieved1 = await repository.GetByIdAsync(entity1.Id);
        var retrieved2 = await repository.GetByIdAsync(entity2.Id);
        Assert.NotNull(retrieved1);
        Assert.NotNull(retrieved2);
        Assert.Equal("GetUser", retrieved1.Name);
        Assert.Equal("GetUser", retrieved2.Name);
    }

    [Fact]
    public async Task AddAsync_WithSameNameInSameHierarchicalFolder_ShouldThrowException()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        
        Task<string> GetHierarchicalPath(TestEntity e)
        {
            if (!e.ParentId.HasValue) return Task.FromResult(string.Empty);
            if (e.ParentId == parentId) return Task.FromResult("shared-folder");
            return Task.FromResult(string.Empty);
        }
        
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities",
            e => e.Name,
            GetHierarchicalPath,
            e => e.ParentId);
        
        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "GetUser", ParentId = parentId };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "GetUser", ParentId = parentId };

        // Act
        await repository.AddAsync(entity1);

        // Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.AddAsync(entity2));
        Assert.Contains("already exists in this scope", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_WithHierarchicalPath_MovingToNewFolder_ShouldMoveFile()
    {
        // Arrange
        var parent1Id = Guid.NewGuid();
        var parent2Id = Guid.NewGuid();
        
        Task<string> GetHierarchicalPath(TestEntity e)
        {
            if (!e.ParentId.HasValue) return Task.FromResult(string.Empty);
            if (e.ParentId == parent1Id) return Task.FromResult("folder1");
            if (e.ParentId == parent2Id) return Task.FromResult("folder2");
            return Task.FromResult(string.Empty);
        }
        
        var repository = new MultiFileRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities",
            e => e.Name,
            GetHierarchicalPath,
            e => e.ParentId);
        
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "GetUser", ParentId = parent1Id };
        await repository.AddAsync(entity);

        // Act - Move to different parent
        entity.ParentId = parent2Id;
        await repository.UpdateAsync(entity);

        // Assert
        var oldFile = Path.Combine(_testDirectory, "test-entities", "folder1", "GetUser.json");
        var newFile = Path.Combine(_testDirectory, "test-entities", "folder2", "GetUser.json");
        Assert.False(File.Exists(oldFile), "Old file should be deleted");
        Assert.True(File.Exists(newFile), "New file should exist");
        
        var retrieved = await repository.GetByIdAsync(entity.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(parent2Id, retrieved.ParentId);
    }

    private class TestEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
    }
}
