using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Persistence;

namespace HolyConnect.Infrastructure.Tests.Repositories;

public class FileBasedRepositoryTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly FileBasedRepository<TestEntity> _repository;

    public FileBasedRepositoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"HolyConnectTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        _repository = new FileBasedRepository<TestEntity>(
            entity => entity.Id,
            () => _testDirectory,
            "test-entities.json"
        );
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
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test Entity" };

        // Act
        var result = await _repository.AddAsync(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal("Test Entity", result.Name);

        var filePath = Path.Combine(_testDirectory, "test-entities.json");
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingEntity_ShouldReturnEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test Entity" };
        await _repository.AddAsync(entity);

        // Act
        var result = await _repository.GetByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal("Test Entity", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingEntity_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyRepository_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleEntities_ShouldReturnAllEntities()
    {
        // Arrange
        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "Entity 1" };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "Entity 2" };
        var entity3 = new TestEntity { Id = Guid.NewGuid(), Name = "Entity 3" };

        await _repository.AddAsync(entity1);
        await _repository.AddAsync(entity2);
        await _repository.AddAsync(entity3);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task UpdateAsync_WithExistingEntity_ShouldUpdateEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Original Name" };
        await _repository.AddAsync(entity);

        entity.Name = "Updated Name";

        // Act
        var result = await _repository.UpdateAsync(entity);

        // Assert
        Assert.Equal("Updated Name", result.Name);

        var retrieved = await _repository.GetByIdAsync(entity.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Updated Name", retrieved.Name);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingEntity_ShouldRemoveEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test Entity" };
        await _repository.AddAsync(entity);

        // Act
        await _repository.DeleteAsync(entity.Id);

        // Assert
        var result = await _repository.GetByIdAsync(entity.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingEntity_ShouldNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await _repository.DeleteAsync(nonExistentId); // Should not throw
    }

    [Fact]
    public async Task AddAsync_WithSameId_ShouldOverwrite()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity { Id = id, Name = "First" };
        var entity2 = new TestEntity { Id = id, Name = "Second" };

        // Act
        await _repository.AddAsync(entity1);
        await _repository.AddAsync(entity2);

        // Assert
        var result = await _repository.GetByIdAsync(id);
        Assert.NotNull(result);
        Assert.Equal("Second", result.Name);

        var all = await _repository.GetAllAsync();
        Assert.Single(all);
    }

    [Fact]
    public async Task Repository_WithEmptyStoragePath_ShouldUseDefaultPath()
    {
        // Arrange
        var repo = new FileBasedRepository<TestEntity>(
            entity => entity.Id,
            () => "",
            "test.json"
        );
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        await repo.AddAsync(entity);
        var result = await repo.GetByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }

    [Fact]
    public async Task Repository_WithWhitespaceStoragePath_ShouldUseDefaultPath()
    {
        // Arrange
        var repo = new FileBasedRepository<TestEntity>(
            entity => entity.Id,
            () => "   ",
            "test.json"
        );
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        await repo.AddAsync(entity);
        var result = await repo.GetByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }

    [Fact]
    public async Task Repository_WithCorruptedFile_ShouldReturnEmptyList()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test-entities.json");
        await File.WriteAllTextAsync(filePath, "{ invalid json }");

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Repository_ShouldPersistAcrossInstances()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Persisted Entity" };
        await _repository.AddAsync(entity);

        // Act - Create new repository instance
        var newRepository = new FileBasedRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities.json"
        );
        var result = await newRepository.GetByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal("Persisted Entity", result.Name);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity1" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity2" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity3" }
        };

        // Act
        var results = await _repository.AddRangeAsync(entities);
        var all = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(3, results.Count());
        Assert.Equal(3, all.Count());
    }

    [Fact]
    public async Task UpdateRangeAsync_ShouldUpdateMultipleEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "Original1" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Original2" }
        };
        await _repository.AddRangeAsync(entities);

        // Act
        entities[0].Name = "Updated1";
        entities[1].Name = "Updated2";
        await _repository.UpdateRangeAsync(entities);
        var all = await _repository.GetAllAsync();

        // Assert
        Assert.Contains(all, e => e.Name == "Updated1");
        Assert.Contains(all, e => e.Name == "Updated2");
        Assert.DoesNotContain(all, e => e.Name == "Original1");
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldDeleteMultipleEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity1" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity2" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity3" }
        };
        await _repository.AddRangeAsync(entities);

        // Act
        var idsToDelete = entities.Take(2).Select(e => e.Id);
        await _repository.DeleteRangeAsync(idsToDelete);
        var all = await _repository.GetAllAsync();

        // Assert
        Assert.Single(all);
        Assert.Contains(all, e => e.Name == "Entity3");
    }

    [Fact]
    public async Task AddRangeAsync_ShouldPersistToFile()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity1" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity2" }
        };

        // Act
        await _repository.AddRangeAsync(entities);

        // Assert
        var filePath = Path.Combine(_testDirectory, "test-entities.json");
        Assert.True(File.Exists(filePath));

        // Create new instance to verify persistence
        var newRepo = new FileBasedRepository<TestEntity>(
            e => e.Id,
            () => _testDirectory,
            "test-entities.json"
        );
        var all = await newRepo.GetAllAsync();
        Assert.Equal(2, all.Count());
    }

    // Test entity class
    private class TestEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
