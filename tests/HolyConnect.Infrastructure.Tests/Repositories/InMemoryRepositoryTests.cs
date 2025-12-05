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
}
