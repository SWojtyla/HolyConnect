using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;
using DomainEnvironment = HolyConnect.Domain.Entities.Environment;
using DomainHttpMethod = HolyConnect.Domain.Entities.HttpMethod;

namespace HolyConnect.Application.Tests.Services;

public class GlobalSearchServiceTests
{
    private readonly Mock<IEnvironmentService> _mockEnvironmentService;
    private readonly Mock<ICollectionService> _mockCollectionService;
    private readonly Mock<IRequestService> _mockRequestService;
    private readonly Mock<IFlowService> _mockFlowService;
    private readonly GlobalSearchService _service;

    public GlobalSearchServiceTests()
    {
        _mockEnvironmentService = new Mock<IEnvironmentService>();
        _mockCollectionService = new Mock<ICollectionService>();
        _mockRequestService = new Mock<IRequestService>();
        _mockFlowService = new Mock<IFlowService>();
        
        _service = new GlobalSearchService(
            _mockEnvironmentService.Object,
            _mockCollectionService.Object,
            _mockRequestService.Object,
            _mockFlowService.Object
        );
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_ShouldReturnEmptyResults()
    {
        // Act
        var results = await _service.SearchAsync("");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_WithNullQuery_ShouldReturnEmptyResults()
    {
        // Act
        var results = await _service.SearchAsync(null!);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_WithMatchingEnvironment_ShouldReturnResult()
    {
        // Arrange
        var environment = new DomainEnvironment
        {
            Id = Guid.NewGuid(),
            Name = "Production",
            Description = "Production environment"
        };
        
        _mockEnvironmentService.Setup(s => s.GetAllEnvironmentsAsync())
            .ReturnsAsync(new[] { environment });
        _mockCollectionService.Setup(s => s.GetAllCollectionsAsync())
            .ReturnsAsync(Array.Empty<Collection>());
        _mockRequestService.Setup(s => s.GetAllRequestsAsync())
            .ReturnsAsync(Array.Empty<Request>());
        _mockFlowService.Setup(s => s.GetAllFlowsAsync())
            .ReturnsAsync(Array.Empty<Flow>());

        // Act
        var results = await _service.SearchAsync("prod");

        // Assert
        Assert.Single(results);
        var result = results.First();
        Assert.Equal(SearchResultType.Environment, result.Type);
        Assert.Equal("Production", result.Name);
        Assert.Equal(environment.Id, result.Id);
    }

    [Fact]
    public async Task SearchAsync_WithMatchingCollection_ShouldReturnResult()
    {
        // Arrange
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "API Tests",
            Description = "Collection of API tests"
        };
        
        _mockEnvironmentService.Setup(s => s.GetAllEnvironmentsAsync())
            .ReturnsAsync(Array.Empty<DomainEnvironment>());
        _mockCollectionService.Setup(s => s.GetAllCollectionsAsync())
            .ReturnsAsync(new[] { collection });
        _mockRequestService.Setup(s => s.GetAllRequestsAsync())
            .ReturnsAsync(Array.Empty<Request>());
        _mockFlowService.Setup(s => s.GetAllFlowsAsync())
            .ReturnsAsync(Array.Empty<Flow>());

        // Act
        var results = await _service.SearchAsync("api");

        // Assert
        Assert.Single(results);
        var result = results.First();
        Assert.Equal(SearchResultType.Collection, result.Type);
        Assert.Equal("API Tests", result.Name);
        Assert.Equal(collection.Id, result.Id);
    }

    [Fact]
    public async Task SearchAsync_WithMatchingRequest_ShouldReturnResult()
    {
        // Arrange
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            CollectionId = Guid.NewGuid(),
            Name = "Get Users",
            Url = "https://api.example.com/users",
            Method = DomainHttpMethod.Get
        };
        
        _mockEnvironmentService.Setup(s => s.GetAllEnvironmentsAsync())
            .ReturnsAsync(Array.Empty<DomainEnvironment>());
        _mockCollectionService.Setup(s => s.GetAllCollectionsAsync())
            .ReturnsAsync(Array.Empty<Collection>());
        _mockRequestService.Setup(s => s.GetAllRequestsAsync())
            .ReturnsAsync(new Request[] { request });
        _mockFlowService.Setup(s => s.GetAllFlowsAsync())
            .ReturnsAsync(Array.Empty<Flow>());

        // Act
        var results = await _service.SearchAsync("users");

        // Assert
        Assert.Single(results);
        var result = results.First();
        Assert.Equal(SearchResultType.Request, result.Type);
        Assert.Equal("Get Users", result.Name);
        Assert.Equal(request.Id, result.Id);
    }

    [Fact]
    public async Task SearchAsync_WithMatchingFlow_ShouldReturnResult()
    {
        // Arrange
        var flow = new Flow
        {
            Id = Guid.NewGuid(),
            Name = "Authentication Flow",
            Description = "Login and get token"
        };
        
        _mockEnvironmentService.Setup(s => s.GetAllEnvironmentsAsync())
            .ReturnsAsync(Array.Empty<DomainEnvironment>());
        _mockCollectionService.Setup(s => s.GetAllCollectionsAsync())
            .ReturnsAsync(Array.Empty<Collection>());
        _mockRequestService.Setup(s => s.GetAllRequestsAsync())
            .ReturnsAsync(Array.Empty<Request>());
        _mockFlowService.Setup(s => s.GetAllFlowsAsync())
            .ReturnsAsync(new[] { flow });

        // Act
        var results = await _service.SearchAsync("auth");

        // Assert
        Assert.Single(results);
        var result = results.First();
        Assert.Equal(SearchResultType.Flow, result.Type);
        Assert.Equal("Authentication Flow", result.Name);
        Assert.Equal(flow.Id, result.Id);
    }

    [Fact]
    public async Task SearchAsync_WithExactMatch_ShouldHaveHigherRelevanceScore()
    {
        // Arrange
        var env1 = new DomainEnvironment { Id = Guid.NewGuid(), Name = "Production", Description = "" };
        var env2 = new DomainEnvironment { Id = Guid.NewGuid(), Name = "Production Environment", Description = "" };
        
        _mockEnvironmentService.Setup(s => s.GetAllEnvironmentsAsync())
            .ReturnsAsync(new[] { env1, env2 });
        _mockCollectionService.Setup(s => s.GetAllCollectionsAsync())
            .ReturnsAsync(Array.Empty<Collection>());
        _mockRequestService.Setup(s => s.GetAllRequestsAsync())
            .ReturnsAsync(Array.Empty<Request>());
        _mockFlowService.Setup(s => s.GetAllFlowsAsync())
            .ReturnsAsync(Array.Empty<Flow>());

        // Act
        var results = await _service.SearchAsync("production");

        // Assert
        Assert.Equal(2, results.Count());
        var orderedResults = results.ToList();
        Assert.Equal("Production", orderedResults[0].Name); // Exact match comes first
        Assert.True(orderedResults[0].RelevanceScore > orderedResults[1].RelevanceScore);
    }

    [Fact]
    public async Task SearchAsync_WithMultipleMatches_ShouldReturnAllSortedByRelevance()
    {
        // Arrange
        var env = new DomainEnvironment { Id = Guid.NewGuid(), Name = "Test Environment", Description = "" };
        var collection = new Collection { Id = Guid.NewGuid(), Name = "Test Collection", Description = "" };
        var request = new RestRequest { Id = Guid.NewGuid(), CollectionId = Guid.NewGuid(), Name = "Test Request", Url = "", Method = DomainHttpMethod.Get };
        
        _mockEnvironmentService.Setup(s => s.GetAllEnvironmentsAsync())
            .ReturnsAsync(new[] { env });
        _mockCollectionService.Setup(s => s.GetAllCollectionsAsync())
            .ReturnsAsync(new[] { collection });
        _mockRequestService.Setup(s => s.GetAllRequestsAsync())
            .ReturnsAsync(new Request[] { request });
        _mockFlowService.Setup(s => s.GetAllFlowsAsync())
            .ReturnsAsync(Array.Empty<Flow>());

        // Act
        var results = await _service.SearchAsync("test");

        // Assert
        Assert.Equal(3, results.Count());
        Assert.All(results, r => Assert.True(r.RelevanceScore > 0));
    }

    [Fact]
    public async Task SearchAsync_WithNoMatches_ShouldReturnEmptyResults()
    {
        // Arrange
        var env = new DomainEnvironment { Id = Guid.NewGuid(), Name = "Production", Description = "" };
        
        _mockEnvironmentService.Setup(s => s.GetAllEnvironmentsAsync())
            .ReturnsAsync(new[] { env });
        _mockCollectionService.Setup(s => s.GetAllCollectionsAsync())
            .ReturnsAsync(Array.Empty<Collection>());
        _mockRequestService.Setup(s => s.GetAllRequestsAsync())
            .ReturnsAsync(Array.Empty<Request>());
        _mockFlowService.Setup(s => s.GetAllFlowsAsync())
            .ReturnsAsync(Array.Empty<Flow>());

        // Act
        var results = await _service.SearchAsync("nonexistent");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_WithDescriptionMatch_ShouldReturnResult()
    {
        // Arrange
        var env = new DomainEnvironment 
        { 
            Id = Guid.NewGuid(), 
            Name = "Prod", 
            Description = "Production environment for live services" 
        };
        
        _mockEnvironmentService.Setup(s => s.GetAllEnvironmentsAsync())
            .ReturnsAsync(new[] { env });
        _mockCollectionService.Setup(s => s.GetAllCollectionsAsync())
            .ReturnsAsync(Array.Empty<Collection>());
        _mockRequestService.Setup(s => s.GetAllRequestsAsync())
            .ReturnsAsync(Array.Empty<Request>());
        _mockFlowService.Setup(s => s.GetAllFlowsAsync())
            .ReturnsAsync(Array.Empty<Flow>());

        // Act
        var results = await _service.SearchAsync("live");

        // Assert
        Assert.Single(results);
        Assert.Equal("Prod", results.First().Name);
    }

    [Fact]
    public async Task SearchAsync_WithCaseInsensitiveQuery_ShouldMatch()
    {
        // Arrange
        var env = new DomainEnvironment { Id = Guid.NewGuid(), Name = "Production", Description = "" };
        
        _mockEnvironmentService.Setup(s => s.GetAllEnvironmentsAsync())
            .ReturnsAsync(new[] { env });
        _mockCollectionService.Setup(s => s.GetAllCollectionsAsync())
            .ReturnsAsync(Array.Empty<Collection>());
        _mockRequestService.Setup(s => s.GetAllRequestsAsync())
            .ReturnsAsync(Array.Empty<Request>());
        _mockFlowService.Setup(s => s.GetAllFlowsAsync())
            .ReturnsAsync(Array.Empty<Flow>());

        // Act
        var results = await _service.SearchAsync("PRODUCTION");

        // Assert
        Assert.Single(results);
        Assert.Equal("Production", results.First().Name);
    }

    [Fact]
    public async Task SearchAsync_WithNestedCollection_ShouldIncludeParentContext()
    {
        // Arrange
        var parentCollection = new Collection 
        { 
            Id = Guid.NewGuid(), 
            Name = "Parent Collection",
            ParentCollectionId = null
        };
        
        var childCollection = new Collection 
        { 
            Id = Guid.NewGuid(), 
            Name = "Child Collection",
            ParentCollectionId = parentCollection.Id
        };
        
        _mockEnvironmentService.Setup(s => s.GetAllEnvironmentsAsync())
            .ReturnsAsync(Array.Empty<DomainEnvironment>());
        _mockCollectionService.Setup(s => s.GetAllCollectionsAsync())
            .ReturnsAsync(new[] { parentCollection, childCollection });
        _mockRequestService.Setup(s => s.GetAllRequestsAsync())
            .ReturnsAsync(Array.Empty<Request>());
        _mockFlowService.Setup(s => s.GetAllFlowsAsync())
            .ReturnsAsync(Array.Empty<Flow>());

        // Act
        var results = await _service.SearchAsync("child");

        // Assert
        Assert.Single(results);
        var result = results.First();
        Assert.Equal("Child Collection", result.Name);
        Assert.Equal("Parent Collection", result.ParentContext);
    }
}
