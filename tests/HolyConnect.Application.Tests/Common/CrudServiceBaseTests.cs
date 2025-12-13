using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using Moq;

namespace HolyConnect.Application.Tests.Common;

public class CrudServiceBaseTests
{
    private readonly Mock<IRepository<TestEntity>> _mockRepository;
    private readonly Mock<ISecretVariablesService> _mockSecretVariablesService;
    private readonly Mock<ITestSecretService> _mockTestSecretService;
    private readonly TestCrudService _service;

    public CrudServiceBaseTests()
    {
        _mockRepository = new Mock<IRepository<TestEntity>>();
        _mockSecretVariablesService = new Mock<ISecretVariablesService>();
        _mockTestSecretService = new Mock<ITestSecretService>();
        _service = new TestCrudService(
            _mockRepository.Object,
            _mockSecretVariablesService.Object,
            _mockTestSecretService.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity1" },
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity2" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityExists_ShouldReturnEntityWithSecrets()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity
        {
            Id = entityId,
            Name = "Test",
            Variables = new Dictionary<string, string> { { "key1", "value1" } },
            SecretVariableNames = new HashSet<string> { "secret1" }
        };
        var secrets = new Dictionary<string, string> { { "secret1", "secretValue1" } };

        _mockRepository.Setup(r => r.GetByIdAsync(entityId)).ReturnsAsync(entity);
        _mockTestSecretService.Setup(s => s.GetSecretsAsync(entityId)).ReturnsAsync(secrets);

        // Act
        var result = await _service.GetByIdAsync(entityId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entityId, result.Id);
        Assert.Equal(2, result.Variables.Count); // Original + secret
        Assert.Contains("secret1", result.Variables.Keys);
        Assert.Equal("secretValue1", result.Variables["secret1"]);
        _mockRepository.Verify(r => r.GetByIdAsync(entityId), Times.Once);
        _mockTestSecretService.Verify(s => s.GetSecretsAsync(entityId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(entityId)).ReturnsAsync((TestEntity?)null);

        // Act
        var result = await _service.GetByIdAsync(entityId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(entityId), Times.Once);
        _mockTestSecretService.Verify(s => s.GetSecretsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldSeparateSecretsAndMergeBack()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity
        {
            Id = entityId,
            Name = "Test",
            Variables = new Dictionary<string, string>
            {
                { "normalVar", "normalValue" },
                { "secretVar", "secretValue" }
            },
            SecretVariableNames = new HashSet<string> { "secretVar" }
        };

        TestEntity? capturedEntity = null;
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<TestEntity>()))
            .Callback<TestEntity>(e => capturedEntity = e)
            .ReturnsAsync((TestEntity e) => new TestEntity
            {
                Id = e.Id,
                Name = e.Name,
                Variables = new Dictionary<string, string>(e.Variables),
                SecretVariableNames = e.SecretVariableNames
            });

        // Act
        var result = await _service.UpdateAsync(entity);

        // Assert
        Assert.NotNull(capturedEntity);
        // Captured entity should have only non-secret variables
        Assert.Single(capturedEntity.Variables);
        Assert.Contains("normalVar", capturedEntity.Variables.Keys);
        Assert.DoesNotContain("secretVar", capturedEntity.Variables.Keys);

        // Result should have both variables merged
        Assert.Equal(2, result.Variables.Count);
        Assert.Contains("normalVar", result.Variables.Keys);
        Assert.Contains("secretVar", result.Variables.Keys);

        _mockTestSecretService.Verify(
            s => s.SaveSecretsAsync(entityId, It.Is<Dictionary<string, string>>(d => d.ContainsKey("secretVar"))),
            Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TestEntity>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNoSecrets_ShouldUpdateNormally()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new TestEntity
        {
            Id = entityId,
            Name = "Test",
            Variables = new Dictionary<string, string> { { "key1", "value1" } },
            SecretVariableNames = new HashSet<string>()
        };

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<TestEntity>()))
            .ReturnsAsync((TestEntity e) => e);

        // Act
        var result = await _service.UpdateAsync(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Single(result.Variables);
        _mockTestSecretService.Verify(
            s => s.SaveSecretsAsync(entityId, It.Is<Dictionary<string, string>>(d => d.Count == 0)),
            Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TestEntity>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteSecretsAndEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        _mockTestSecretService.Setup(s => s.DeleteSecretsAsync(entityId)).Returns(Task.CompletedTask);
        _mockRepository.Setup(r => r.DeleteAsync(entityId)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(entityId);

        // Assert
        _mockTestSecretService.Verify(s => s.DeleteSecretsAsync(entityId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(entityId), Times.Once);
    }

    #region Test Entity and Service

    public class TestEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, string> Variables { get; set; } = new();
        public HashSet<string> SecretVariableNames { get; set; } = new();
    }

    public interface ITestSecretService
    {
        Task<Dictionary<string, string>> GetSecretsAsync(Guid id);
        Task SaveSecretsAsync(Guid id, Dictionary<string, string> secrets);
        Task DeleteSecretsAsync(Guid id);
    }

    public class TestCrudService : CrudServiceBase<TestEntity>
    {
        private readonly ITestSecretService _testSecretService;

        public TestCrudService(
            IRepository<TestEntity> repository,
            ISecretVariablesService secretVariablesService,
            ITestSecretService testSecretService)
            : base(repository, secretVariablesService)
        {
            _testSecretService = testSecretService;
        }

        protected override Guid GetEntityId(TestEntity entity) => entity.Id;

        protected override Dictionary<string, string> GetEntityVariables(TestEntity entity) => entity.Variables;

        protected override void SetEntityVariables(TestEntity entity, Dictionary<string, string> variables)
        {
            entity.Variables = variables;
        }

        protected override HashSet<string> GetEntitySecretNames(TestEntity entity) => entity.SecretVariableNames;

        protected override async Task LoadAndMergeSecretsAsync(Guid id, TestEntity entity)
        {
            await SecretVariableHelper.LoadAndMergeSecretsAsync(
                id,
                entity.Variables,
                _testSecretService.GetSecretsAsync);
        }

        protected override async Task SaveSecretsAsync(Guid id, Dictionary<string, string> secrets)
        {
            await _testSecretService.SaveSecretsAsync(id, secrets);
        }

        protected override async Task DeleteSecretsAsync(Guid id)
        {
            await _testSecretService.DeleteSecretsAsync(id);
        }
    }

    #endregion
}
