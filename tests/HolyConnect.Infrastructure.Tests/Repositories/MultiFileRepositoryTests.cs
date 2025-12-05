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

        // Verify file was created with readable name pattern
        var directoryPath = Path.Combine(_testDirectory, "test-entities");
        var filePattern = $"*__{entity.Id}.json";
        var files = Directory.GetFiles(directoryPath, filePattern);
        Assert.True(files.Length > 0, "Expected at least one file matching the pattern");
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

    private class TestEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
