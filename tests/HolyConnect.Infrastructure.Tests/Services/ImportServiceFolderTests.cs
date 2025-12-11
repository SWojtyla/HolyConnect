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
    private readonly ImportService _importService;
    private readonly string _testFolderPath;

    public ImportServiceFolderTests()
    {
        _mockRequestService = new Mock<IRequestService>();
        _mockCollectionService = new Mock<ICollectionService>();
        
        var strategies = new List<IImportStrategy>
        {
            new BrunoImportStrategy()
        };

        _importService = new ImportService(_mockRequestService.Object, _mockCollectionService.Object, strategies);
        
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
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath, environmentId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("No Bruno files", result.ErrorMessage);
        Assert.Equal(0, result.TotalFilesProcessed);
    }

    [Fact]
    public async Task ImportFromBrunoFolderAsync_WithNonExistentFolder_ShouldReturnErrorResult()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var nonExistentPath = Path.Combine(_testFolderPath, "nonexistent");

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(nonExistentPath, environmentId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("does not exist", result.ErrorMessage);
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
            .Setup(s => s.CreateCollectionAsync(It.IsAny<string>(), environmentId, null, It.IsAny<string>()))
            .ReturnsAsync(mockCollection);

        var mockRequest = new RestRequest { Id = Guid.NewGuid(), Name = "Test Request" };
        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync(mockRequest);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.TotalFilesProcessed);
        Assert.Equal(1, result.SuccessfulImports);
        Assert.Equal(0, result.FailedImports);
        Assert.Single(result.ImportedRequests);
        Assert.Single(result.ImportedCollections);
        _mockRequestService.Verify(s => s.CreateRequestAsync(It.IsAny<Request>()), Times.Once);
        _mockCollectionService.Verify(s => s.CreateCollectionAsync(It.IsAny<string>(), environmentId, null, It.IsAny<string>()), Times.Once);
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
            .Setup(s => s.CreateCollectionAsync(It.IsAny<string>(), environmentId, null, It.IsAny<string>()))
            .ReturnsAsync(mockCollection);

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(3, result.TotalFilesProcessed);
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
            .Setup(s => s.CreateCollectionAsync(Path.GetFileName(_testFolderPath), environmentId, null, It.IsAny<string>()))
            .ReturnsAsync(new Collection { Id = rootCollectionId, Name = Path.GetFileName(_testFolderPath) });

        // Mock subcollection creation
        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync("subfolder1", environmentId, rootCollectionId, It.IsAny<string>()))
            .ReturnsAsync(new Collection { Id = subCollectionId, Name = "subfolder1", ParentCollectionId = rootCollectionId });

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.TotalFilesProcessed);
        Assert.Equal(1, result.SuccessfulImports);
        Assert.Equal(2, result.ImportedCollections.Count); // Root and subfolder
        _mockCollectionService.Verify(s => s.CreateCollectionAsync(It.IsAny<string>(), environmentId, null, It.IsAny<string>()), Times.Once);
        _mockCollectionService.Verify(s => s.CreateCollectionAsync("subfolder1", environmentId, rootCollectionId, It.IsAny<string>()), Times.Once);
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
            .Setup(s => s.CreateCollectionAsync(Path.GetFileName(_testFolderPath), environmentId, null, It.IsAny<string>()))
            .ReturnsAsync(new Collection { Id = rootCollectionId, Name = Path.GetFileName(_testFolderPath) });

        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync("api", environmentId, rootCollectionId, It.IsAny<string>()))
            .ReturnsAsync(new Collection { Id = apiCollectionId, Name = "api", ParentCollectionId = rootCollectionId });

        _mockCollectionService
            .Setup(s => s.CreateCollectionAsync("v1", environmentId, apiCollectionId, It.IsAny<string>()))
            .ReturnsAsync(new Collection { Id = v1CollectionId, Name = "v1", ParentCollectionId = apiCollectionId });

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.TotalFilesProcessed);
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
            .Setup(s => s.CreateCollectionAsync(It.IsAny<string>(), environmentId, null, It.IsAny<string>()))
            .ReturnsAsync(mockCollection);

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath, environmentId);

        // Assert
        Assert.True(result.Success); // Should still succeed as one file was imported
        Assert.Equal(2, result.TotalFilesProcessed);
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
            .Setup(s => s.CreateCollectionAsync(It.IsAny<string>(), environmentId, parentCollectionId, It.IsAny<string>()))
            .ReturnsAsync(mockCollection);

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath, environmentId, parentCollectionId);

        // Assert
        Assert.True(result.Success);
        _mockCollectionService.Verify(s => s.CreateCollectionAsync(
            It.IsAny<string>(), 
            environmentId, 
            parentCollectionId, 
            It.IsAny<string>()), Times.Once);
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
            .Setup(s => s.CreateCollectionAsync(It.IsAny<string>(), environmentId, null, It.IsAny<string>()))
            .ReturnsAsync(mockCollection);

        _mockRequestService
            .Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _importService.ImportFromBrunoFolderAsync(_testFolderPath, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.TotalFilesProcessed);
        Assert.Equal(2, result.SuccessfulImports);
        Assert.Equal(2, result.ImportedRequests.Count);
        Assert.Contains(result.ImportedRequests, r => r is RestRequest);
        Assert.Contains(result.ImportedRequests, r => r is GraphQLRequest);
    }
}
