using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Persistence;

namespace HolyConnect.Infrastructure.Tests.Repositories;

public class InMemoryRepositoryTests
{
    private class TestEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        var repository = new InMemoryRepository<TestEntity>(e => e.Id);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };

        // Act
        var result = await repository.AddAsync(entity);

        // Assert
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
    {
        // Arrange
        var repository = new InMemoryRepository<TestEntity>(e => e.Id);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        await repository.AddAsync(entity);

        // Act
        var result = await repository.GetByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var repository = new InMemoryRepository<TestEntity>(e => e.Id);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        var repository = new InMemoryRepository<TestEntity>(e => e.Id);
        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "Test1" };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "Test2" };
        await repository.AddAsync(entity1);
        await repository.AddAsync(entity2);

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        // Arrange
        var repository = new InMemoryRepository<TestEntity>(e => e.Id);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        await repository.AddAsync(entity);

        // Act
        entity.Name = "Updated";
        await repository.UpdateAsync(entity);
        var result = await repository.GetByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        // Arrange
        var repository = new InMemoryRepository<TestEntity>(e => e.Id);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test" };
        await repository.AddAsync(entity);

        // Act
        await repository.DeleteAsync(entity.Id);
        var result = await repository.GetByIdAsync(entity.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldOverwriteExistingEntity_WithSameId()
    {
        // Arrange
        var repository = new InMemoryRepository<TestEntity>(e => e.Id);
        var id = Guid.NewGuid();
        var entity1 = new TestEntity { Id = id, Name = "First" };
        var entity2 = new TestEntity { Id = id, Name = "Second" };

        // Act
        await repository.AddAsync(entity1);
        await repository.AddAsync(entity2);
        var result = await repository.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Second", result.Name);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        // Arrange
        var repository = new InMemoryRepository<TestEntity>(e => e.Id);
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "Test1" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Test2" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Test3" }
        };

        // Act
        var results = await repository.AddRangeAsync(entities);
        var all = await repository.GetAllAsync();

        // Assert
        Assert.Equal(3, results.Count());
        Assert.Equal(3, all.Count());
    }

    [Fact]
    public async Task UpdateRangeAsync_ShouldUpdateMultipleEntities()
    {
        // Arrange
        var repository = new InMemoryRepository<TestEntity>(e => e.Id);
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "Test1" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Test2" }
        };
        await repository.AddRangeAsync(entities);

        // Act
        entities[0].Name = "Updated1";
        entities[1].Name = "Updated2";
        var results = await repository.UpdateRangeAsync(entities);
        var all = await repository.GetAllAsync();

        // Assert
        Assert.Equal(2, results.Count());
        Assert.Contains(all, e => e.Name == "Updated1");
        Assert.Contains(all, e => e.Name == "Updated2");
    }

    [Fact]
    public async Task DeleteRangeAsync_ShouldDeleteMultipleEntities()
    {
        // Arrange
        var repository = new InMemoryRepository<TestEntity>(e => e.Id);
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "Test1" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Test2" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Test3" }
        };
        await repository.AddRangeAsync(entities);

        // Act
        var idsToDelete = entities.Take(2).Select(e => e.Id);
        await repository.DeleteRangeAsync(idsToDelete);
        var all = await repository.GetAllAsync();

        // Assert
        Assert.Single(all);
        Assert.Contains(all, e => e.Name == "Test3");
    }

    [Fact]
    public async Task AddRangeAsync_WithEmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var repository = new InMemoryRepository<TestEntity>(e => e.Id);
        var entities = new List<TestEntity>();

        // Act
        var results = await repository.AddRangeAsync(entities);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task DeleteRangeAsync_WithNonExistentIds_ShouldNotThrow()
    {
        // Arrange
        var repository = new InMemoryRepository<TestEntity>(e => e.Id);
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act & Assert - Should not throw
        await repository.DeleteRangeAsync(ids);
    }
}
