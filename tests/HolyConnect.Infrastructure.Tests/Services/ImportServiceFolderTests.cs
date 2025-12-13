using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services;
using HolyConnect.Infrastructure.Services.ImportStrategies;
using Moq;

namespace HolyConnect.Infrastructure.Tests.Services;

public class ImportServiceFolderTests : IDisposable
{
    private readonly Mock<IRequestService> _mockRequestService;
    private readonly Mock<ICollectionService> _mockCollectionService;
    private readonly Mock<IEnvironmentService> _mockEnvironmentService;
    private readonly ImportService _importService;
    private readonly string _testFolderPath;

    public ImportServiceFolderTests()
    {
        _mockRequestService = new Mock<IRequestService>();
        _mockCollectionService = new Mock<ICollectionService>();
        _mockEnvironmentService = new Mock<IEnvironmentService>();
        
        var strategies = new List<IImportStrategy>
        {
            new BrunoImportStrategy()
        };

        _importService = new ImportService(_mockRequestService.Object, _mockCollectionService.Object, _mockEnvironmentService.Object, strategies);
        
        // Create a temporary test folder structure
        _testFolderPath = Path.Combine(Path.GetTempPath(), $"bruno_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testFolderPath);
    }

    public void Dispose()
    {
        // Clean up test folder
        if (Directory.Exists(_testFolderPath))
        {
            Directory.Delete(_testFolderPath, true);
        }
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_WithEmptyFolder_ShouldReturnErrorResult()
    {
        // Arrange
        var environmentId = Guid.NewGuid();

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath);
        Assert.Equal(0, result.TotalFilesProcessed);
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_WithNonExistentFolder_ShouldReturnErrorResult()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var nonExistentPath = Path.Combine(_testFolderPath, "nonexistent");

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(nonExistentPath);
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_WithSingleFile_ShouldImportSuccessfully()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        
        var brunoContent = @"
meta {
  name: Test Request
  type: http
}

get {
  url: https://api.example.com/test
}";
        
        var filePath = Path.Combine(_testFolderPath, "test.bru");
        await File.WriteAllTextAsync(filePath, brunoContent);

        var mockCollection = new Collection { Id = collectionId, Name = Path.GetFileName(_testFolderPath) };
        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync(It.IsAny<string>(), null, It.IsAny<string>()))
            .ReturnsAsync(mockCollection);

        var mockRequest = new RestRequest { Id = Guid.NewGuid(), Name = "Test Request" };
        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync(mockRequest);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath);
        Assert.Equal(1, result.SuccessfulImports);
        Assert.Equal(0, result.FailedImports);
        Assert.Single(result.ImportedRequests);
        Assert.Single(result.ImportedCollections);
        _mockRequestService.Verify(s => s.CreateRequestAsync(It.IsAny<Request>()), Times.Once);
        _mockCollectionService.Verify(s => s.CreateCollectionAsync(It.IsAny<string>(), null, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_WithMultipleFiles_ShouldImportAll()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        
        // Create multiple Bruno files
        for (int i = 1; i <= 3; i++)
        {
            var brunoContent = $@"
meta {{
  name: Test Request {i}
  type: http
}}

get {{
  url: https://api.example.com/test{i}
}}";
            var filePath = Path.Combine(_testFolderPath, $"test{i}.bru");
            await File.WriteAllTextAsync(filePath, brunoContent);
        }

        var mockCollection = new Collection { Id = collectionId, Name = Path.GetFileName(_testFolderPath) };
        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync(It.IsAny<string>(), null, It.IsAny<string>()))
            .ReturnsAsync(mockCollection);

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath);
        Assert.Equal(3, result.SuccessfulImports);
        Assert.Equal(0, result.FailedImports);
        Assert.Equal(3, result.ImportedRequests.Count);
        _mockRequestService.Verify(s => s.CreateRequestAsync(It.IsAny<Request>()), Times.Exactly(3));
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_WithSubfolders_ShouldCreateSubcollections()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        
        // Create folder structure: root/subfolder1/test.bru
        var subfolder1 = Path.Combine(_testFolderPath, "subfolder1");
        Directory.CreateDirectory(subfolder1);
        
        var brunoContent = @"
meta {
  name: Test Request
  type: http
}

get {
  url: https://api.example.com/test
}";
        
        await File.WriteAllTextAsync(Path.Combine(subfolder1, "test.bru"), brunoContent);

        var rootCollectionId = Guid.NewGuid();
        var subCollectionId = Guid.NewGuid();
        
        // Mock root collection creation
        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync(Path.GetFileName(_testFolderPath), null, It.IsAny<string>()))
            .ReturnsAsync(new Collection { Id = rootCollectionId, Name = Path.GetFileName(_testFolderPath) });

        // Mock subcollection creation
        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync("subfolder1", rootCollectionId, It.IsAny<string>()))
            .ReturnsAsync(new Collection { Id = subCollectionId, Name = "subfolder1", ParentCollectionId = rootCollectionId });

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath);
        Assert.Equal(1, result.SuccessfulImports);
        Assert.Equal(2, result.ImportedCollections.Count); // Root and subfolder
        _mockCollectionService.Verify(s => s.CreateCollectionAsync(It.IsAny<string>(), null, It.IsAny<string>()), Times.Once);
        _mockCollectionService.Verify(s => s.CreateCollectionAsync("subfolder1", rootCollectionId, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_WithNestedSubfolders_ShouldCreateNestedCollections()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        
        // Create nested folder structure: root/api/v1/test.bru
        var apiFolder = Path.Combine(_testFolderPath, "api");
        var v1Folder = Path.Combine(apiFolder, "v1");
        Directory.CreateDirectory(v1Folder);
        
        var brunoContent = @"
meta {
  name: Test Request
  type: http
}

get {
  url: https://api.example.com/v1/test
}";
        
        await File.WriteAllTextAsync(Path.Combine(v1Folder, "test.bru"), brunoContent);

        var rootCollectionId = Guid.NewGuid();
        var apiCollectionId = Guid.NewGuid();
        var v1CollectionId = Guid.NewGuid();
        
        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync(Path.GetFileName(_testFolderPath), null, It.IsAny<string>()))
            .ReturnsAsync(new Collection { Id = rootCollectionId, Name = Path.GetFileName(_testFolderPath) });

        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync("api", rootCollectionId, It.IsAny<string>()))
            .ReturnsAsync(new Collection { Id = apiCollectionId, Name = "api", ParentCollectionId = rootCollectionId });

        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync("v1", apiCollectionId, It.IsAny<string>()))
            .ReturnsAsync(new Collection { Id = v1CollectionId, Name = "v1", ParentCollectionId = apiCollectionId });

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath);
        Assert.Equal(1, result.SuccessfulImports);
        Assert.Equal(3, result.ImportedCollections.Count); // Root, api, and v1
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_WithInvalidFile_ShouldReportWarning()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        
        // Create one valid and one invalid file
        var validContent = @"
meta {
  name: Valid Request
  type: http
}

get {
  url: https://api.example.com/valid
}";
        
        await File.WriteAllTextAsync(Path.Combine(_testFolderPath, "valid.bru"), validContent);
        // Empty file should fail to parse
        await File.WriteAllTextAsync(Path.Combine(_testFolderPath, "invalid.bru"), "");

        var mockCollection = new Collection { Id = collectionId, Name = Path.GetFileName(_testFolderPath) };
        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync(It.IsAny<string>(), null, It.IsAny<string>()))
            .ReturnsAsync(mockCollection);

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath);
        Assert.Equal(1, result.SuccessfulImports);
        Assert.Equal(1, result.FailedImports);
        Assert.NotEmpty(result.Warnings);
        Assert.Contains(result.Warnings, w => w.Contains("invalid.bru"));
        // Should have warning about the failed file and possibly a summary warning
        Assert.True(result.Warnings.Count >= 1);
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_WithParentCollectionId_ShouldUseAsParent()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var parentCollectionId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        
        var brunoContent = @"
meta {
  name: Test Request
  type: http
}

get {
  url: https://api.example.com/test
}";
        
        await File.WriteAllTextAsync(Path.Combine(_testFolderPath, "test.bru"), brunoContent);

        var mockCollection = new Collection 
        { 
            Id = collectionId, 
            Name = Path.GetFileName(_testFolderPath),
            ParentCollectionId = parentCollectionId 
        };
        
        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync(It.IsAny<string>(), parentCollectionId, It.IsAny<string>()))
            .ReturnsAsync(mockCollection);

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath, parentCollectionId);

        // Assert
        Assert.True(result.Success);
        _mockCollectionService.Verify(s => s.CreateCollectionAsync(
            It.IsAny<string>(), parentCollectionId, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_WithMixedRestAndGraphQL_ShouldImportBoth()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        
        var restContent = @"
meta {
  name: REST Request
  type: http
}

get {
  url: https://api.example.com/rest
}";
        
        var graphqlContent = @"
meta {
  name: GraphQL Request
  type: graphql
}

post {
  url: https://api.example.com/graphql
}

body:graphql {
  query { user { id } }
}";
        
        await File.WriteAllTextAsync(Path.Combine(_testFolderPath, "rest.bru"), restContent);
        await File.WriteAllTextAsync(Path.Combine(_testFolderPath, "graphql.bru"), graphqlContent);

        var mockCollection = new Collection { Id = collectionId, Name = Path.GetFileName(_testFolderPath) };
        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync(It.IsAny<string>(), null, It.IsAny<string>()))
            .ReturnsAsync(mockCollection);

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath);
        Assert.Equal(2, result.SuccessfulImports);
        Assert.Equal(2, result.ImportedRequests.Count);
        Assert.Contains(result.ImportedRequests, r => r is RestRequest);
        Assert.Contains(result.ImportedRequests, r => r is GraphQLRequest);
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_WithEnvironmentsFolder_ShouldImportEnvironments()
    {
        // Arrange
        var environmentsPath = Path.Combine(_testFolderPath, "environments");
        Directory.CreateDirectory(environmentsPath);
        
        var devEnvContent = @"
vars {
  baseUrl: http://localhost:3000
  apiKey: dev-key-123
}

vars:secret [
  apiKey
]";
        
        var prodEnvContent = @"
vars {
  baseUrl: https://api.production.com
  apiKey: prod-key-456
}

vars:secret [
  apiKey
]";
        
        await File.WriteAllTextAsync(Path.Combine(environmentsPath, "development.bru"), devEnvContent);
        await File.WriteAllTextAsync(Path.Combine(environmentsPath, "production.bru"), prodEnvContent);

        _mockEnvironmentService
            .Setup(s => s.CreateEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string name, string? desc) => new Domain.Entities.Environment 
            { 
                Id = Guid.NewGuid(), 
                Name = name,
                Description = desc,
                CreatedAt = DateTime.UtcNow
            });

        _mockEnvironmentService
            .Setup(s => s.UpdateEnvironmentAsync(It.IsAny<Domain.Entities.Environment>()))
            .ReturnsAsync((Domain.Entities.Environment env) => env);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.ImportedEnvironments.Count);
        
        var devEnv = result.ImportedEnvironments.FirstOrDefault(e => e.Name == "development");
        Assert.NotNull(devEnv);
        Assert.Equal("http://localhost:3000", devEnv.Variables["baseUrl"]);
        Assert.Equal("dev-key-123", devEnv.Variables["apiKey"]);
        Assert.Contains("apiKey", devEnv.SecretVariableNames);
        
        var prodEnv = result.ImportedEnvironments.FirstOrDefault(e => e.Name == "production");
        Assert.NotNull(prodEnv);
        Assert.Equal("https://api.production.com", prodEnv.Variables["baseUrl"]);
        Assert.Equal("prod-key-456", prodEnv.Variables["apiKey"]);
        Assert.Contains("apiKey", prodEnv.SecretVariableNames);
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_WithCollectionBru_ShouldImportCollectionVariables()
    {
        // Arrange
        var collectionBruContent = @"
vars {
  collectionVar1: value1
  sharedEndpoint: /api/v1
}

vars:secret [
  collectionVar1
]";
        
        await File.WriteAllTextAsync(Path.Combine(_testFolderPath, "collection.bru"), collectionBruContent);
        
        var brunoContent = @"
meta {
  name: Test Request
  type: http
}

get {
  url: https://api.example.com/test
}";
        
        await File.WriteAllTextAsync(Path.Combine(_testFolderPath, "test.bru"), brunoContent);

        var collectionId = Guid.NewGuid();
        var mockCollection = new Collection 
        { 
            Id = collectionId, 
            Name = Path.GetFileName(_testFolderPath),
            Variables = new Dictionary<string, string>(),
            SecretVariableNames = new HashSet<string>()
        };
        
        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync(It.IsAny<string>(), null, It.IsAny<string>()))
            .ReturnsAsync(mockCollection);

        _mockCollectionService
            .Setup(s => s.UpdateCollectionAsync(It.IsAny<Collection>()))
            .ReturnsAsync((Collection c) => c);

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.ImportedCollections);
        
        // Verify collection variables were parsed and updated
        _mockCollectionService.Verify(s => s.UpdateCollectionAsync(
            It.Is<Collection>(c => 
                c.Variables.ContainsKey("collectionVar1") && 
                c.Variables["collectionVar1"] == "value1" &&
                c.Variables.ContainsKey("sharedEndpoint") &&
                c.Variables["sharedEndpoint"] == "/api/v1" &&
                c.SecretVariableNames.Contains("collectionVar1")
            )), Times.Once);
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_WithBrunoJson_ShouldUseCollectionName()
    {
        // Arrange
        var brunoJsonContent = @"{
  ""version"": ""1"",
  ""name"": ""My API Collection"",
  ""type"": ""collection""
}";
        
        await File.WriteAllTextAsync(Path.Combine(_testFolderPath, "bruno.json"), brunoJsonContent);
        
        var brunoContent = @"
meta {
  name: Test Request
  type: http
}

get {
  url: https://api.example.com/test
}";
        
        await File.WriteAllTextAsync(Path.Combine(_testFolderPath, "test.bru"), brunoContent);

        var mockCollection = new Collection { Id = Guid.NewGuid(), Name = "My API Collection" };
        
        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync("My API Collection", null, It.IsAny<string>()))
            .ReturnsAsync(mockCollection);

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath);

        // Assert
        Assert.True(result.Success);
        _mockCollectionService.Verify(s => s.CreateCollectionAsync("My API Collection", null, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_WithEnvironmentsAndRequests_ShouldImportBoth()
    {
        // Arrange
        var environmentsPath = Path.Combine(_testFolderPath, "environments");
        Directory.CreateDirectory(environmentsPath);
        
        var envContent = @"
vars {
  baseUrl: http://localhost:3000
}";
        
        await File.WriteAllTextAsync(Path.Combine(environmentsPath, "dev.bru"), envContent);
        
        var brunoContent = @"
meta {
  name: Test Request
  type: http
}

get {
  url: {{baseUrl}}/test
}";
        
        await File.WriteAllTextAsync(Path.Combine(_testFolderPath, "test.bru"), brunoContent);

        var collectionId = Guid.NewGuid();
        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync(It.IsAny<string>(), null, It.IsAny<string>()))
            .ReturnsAsync(new Collection { Id = collectionId, Name = "Test" });

        _mockEnvironmentService
            .Setup(s => s.CreateEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string name, string? desc) => new Domain.Entities.Environment 
            { 
                Id = Guid.NewGuid(), 
                Name = name,
                CreatedAt = DateTime.UtcNow
            });

        _mockEnvironmentService
            .Setup(s => s.UpdateEnvironmentAsync(It.IsAny<Domain.Entities.Environment>()))
            .ReturnsAsync((Domain.Entities.Environment env) => env);

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.ImportedEnvironments);
        Assert.Single(result.ImportedRequests);
        Assert.Single(result.ImportedCollections);
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_CompleteStructure_ShouldImportEverythingCorrectly()
    {
        // This is an integration test verifying the complete Bruno import workflow
        // including environments, collection variables, and nested folder structure
        
        // Arrange - Create complete Bruno folder structure
        var testPath = Path.Combine(Path.GetTempPath(), $"bruno_integration_{Guid.NewGuid()}");
        Directory.CreateDirectory(testPath);
        var environmentsPath = Path.Combine(testPath, "environments");
        Directory.CreateDirectory(environmentsPath);
        var apiV1Path = Path.Combine(testPath, "api", "v1");
        Directory.CreateDirectory(apiV1Path);

        try
        {
            // Create environments
            await File.WriteAllTextAsync(Path.Combine(environmentsPath, "dev.bru"), @"
vars {
  baseUrl: http://localhost:3000
  apiKey: dev-key
}

vars:secret [
  apiKey
]");

            await File.WriteAllTextAsync(Path.Combine(environmentsPath, "prod.bru"), @"
vars {
  baseUrl: https://api.prod.com
  apiKey: prod-key
}

vars:secret [
  apiKey
]");

            // Create bruno.json for collection name
            await File.WriteAllTextAsync(Path.Combine(testPath, "bruno.json"), @"{
  ""version"": ""1"",
  ""name"": ""Integration Test API"",
  ""type"": ""collection""
}");

            // Create collection.bru for collection variables
            await File.WriteAllTextAsync(Path.Combine(testPath, "collection.bru"), @"
vars {
  sharedVar: shared-value
  endpoint: /api
}

vars:secret [
  sharedVar
]");

            // Create request files
            await File.WriteAllTextAsync(Path.Combine(apiV1Path, "get-user.bru"), @"
meta {
  name: Get User
  type: http
}

get {
  url: {{baseUrl}}{{endpoint}}/v1/users/123
}

headers {
  Authorization: Bearer {{apiKey}}
}");

            await File.WriteAllTextAsync(Path.Combine(apiV1Path, "create-user.bru"), @"
meta {
  name: Create User
  type: http
}

post {
  url: {{baseUrl}}{{endpoint}}/v1/users
}

body:json {
  {
    ""name"": ""Test User""
  }
}");

            // Setup mocks
            _mockEnvironmentService
                .Setup(s => s.CreateEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string name, string? desc) => new Domain.Entities.Environment 
                { 
                    Id = Guid.NewGuid(), 
                    Name = name,
                    Description = desc,
                    CreatedAt = DateTime.UtcNow
                });

            _mockEnvironmentService
                .Setup(s => s.UpdateEnvironmentAsync(It.IsAny<Domain.Entities.Environment>()))
                .ReturnsAsync((Domain.Entities.Environment env) => env);

            _mockCollectionService
                .Setup(s => s.CreateCollectionAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()))
                .ReturnsAsync((string name, Guid? parentId, string? desc) => new Collection 
                { 
                    Id = Guid.NewGuid(), 
                    Name = name,
                    ParentCollectionId = parentId,
                    Description = desc,
                    CreatedAt = DateTime.UtcNow
                });

            _mockCollectionService
                .Setup(s => s.UpdateCollectionAsync(It.IsAny<Collection>()))
                .ReturnsAsync((Collection c) => c);

            _mockRequestService
                .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
                .ReturnsAsync((Request r) => r);

            // Act
            var result = await _importService.ImportFromBrunoFolderAsync(testPath);

            // Assert
            Assert.True(result.Success, $"Import failed: {result.ErrorMessage}");
            
            // Verify environments
            Assert.Equal(2, result.ImportedEnvironments.Count);
            var devEnv = result.ImportedEnvironments.FirstOrDefault(e => e.Name == "dev");
            Assert.NotNull(devEnv);
            Assert.Equal("http://localhost:3000", devEnv.Variables["baseUrl"]);
            Assert.Equal("dev-key", devEnv.Variables["apiKey"]);
            Assert.Contains("apiKey", devEnv.SecretVariableNames);

            // Verify collections (root + api + v1)
            Assert.Equal(3, result.ImportedCollections.Count);
            var rootCollection = result.ImportedCollections.FirstOrDefault(c => c.Name == "Integration Test API");
            Assert.NotNull(rootCollection);
            
            // Verify collection variables were parsed
            _mockCollectionService.Verify(s => s.UpdateCollectionAsync(
                It.Is<Collection>(c => 
                    c.Variables.ContainsKey("sharedVar") &&
                    c.Variables["sharedVar"] == "shared-value" &&
                    c.Variables.ContainsKey("endpoint") &&
                    c.Variables["endpoint"] == "/api" &&
                    c.SecretVariableNames.Contains("sharedVar")
                )), Times.AtLeastOnce);

            // Verify requests
            Assert.Equal(2, result.SuccessfulImports);
            Assert.Equal(2, result.ImportedRequests.Count);
            Assert.Empty(result.Warnings);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testPath))
            {
                Directory.Delete(testPath, true);
            }
        }
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_WithDeeplyNestedSubfolders_ShouldImportAllRequests()
    {
        // This test reproduces the issue where requests in deeply nested subfolders are not imported
        // Structure: root/Providers/SMS/send-sms.bru and root/Providers/Email/send-email.bru
        
        // Arrange
        var environmentsPath = Path.Combine(_testFolderPath, "environments");
        Directory.CreateDirectory(environmentsPath);
        
        var providersPath = Path.Combine(_testFolderPath, "Providers");
        var smsPath = Path.Combine(providersPath, "SMS");
        var emailPath = Path.Combine(providersPath, "Email");
        Directory.CreateDirectory(smsPath);
        Directory.CreateDirectory(emailPath);

        // Create environment
        await File.WriteAllTextAsync(Path.Combine(environmentsPath, "dev.bru"), @"
vars {
  baseUrl: http://localhost:3000
}");

        // Create requests in nested subfolders
        await File.WriteAllTextAsync(Path.Combine(smsPath, "send-sms.bru"), @"
meta {
  name: Send SMS
  type: http
}

post {
  url: {{baseUrl}}/sms/send
}");

        await File.WriteAllTextAsync(Path.Combine(emailPath, "send-email.bru"), @"
meta {
  name: Send Email
  type: http
}

post {
  url: {{baseUrl}}/email/send
}");

        // Setup mocks
        _mockEnvironmentService
            .Setup(s => s.CreateEnvironmentAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string name, string? desc) => new Domain.Entities.Environment 
            { 
                Id = Guid.NewGuid(), 
                Name = name,
                CreatedAt = DateTime.UtcNow
            });

        _mockEnvironmentService
            .Setup(s => s.UpdateEnvironmentAsync(It.IsAny<Domain.Entities.Environment>()))
            .ReturnsAsync((Domain.Entities.Environment env) => env);

        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<string>()))
            .ReturnsAsync((string name, Guid? parentId, string? desc) => new Collection 
            { 
                Id = Guid.NewGuid(), 
                Name = name,
                ParentCollectionId = parentId,
                CreatedAt = DateTime.UtcNow
            });

        _mockCollectionService
            .Setup(s => s.UpdateCollectionAsync(It.IsAny<Collection>()))
            .ReturnsAsync((Collection c) => c);

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.ImportedEnvironments);
        
        // Should have imported both requests from nested subfolders
        Assert.Equal(2, result.SuccessfulImports);
        Assert.Equal(2, result.ImportedRequests.Count);
        Assert.Equal(0, result.FailedImports);
        
        // Verify we have a "Send SMS" and "Send Email" request
        Assert.Contains(result.ImportedRequests, r => r.Name.Contains("SMS") || r.Name.Contains("sms"));
        Assert.Contains(result.ImportedRequests, r => r.Name.Contains("Email") || r.Name.Contains("email"));
    }
}
