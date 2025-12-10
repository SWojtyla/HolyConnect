using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services;
using Moq;

namespace HolyConnect.Infrastructure.Tests.Services;

public class ImportServiceTests
{
    private readonly Mock<IRequestService> _mockRequestService;
    private readonly ImportService _service;

    public ImportServiceTests()
    {
        _mockRequestService = new Mock<IRequestService>();
        _service = new ImportService(_mockRequestService.Object);
    }

    [Fact]
    public void CanImport_WithCurlSource_ReturnsTrue()
    {
        // Act
        var result = _service.CanImport(ImportSource.Curl);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanImport_WithBrunoSource_ReturnsFalse()
    {
        // Act
        var result = _service.CanImport(ImportSource.Bruno);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ImportFromCurlAsync_WithInvalidCommand_ReturnsError()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "not a curl command";

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, environmentId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("curl", result.ErrorMessage);
    }

    [Fact]
    public async Task ImportFromCurlAsync_WithSimpleGetRequest_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl 'https://api.example.com/users'";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.ImportedRequest);
        Assert.NotNull(capturedRequest);
        Assert.Equal("https://api.example.com/users", capturedRequest.Url);
        Assert.Equal(Domain.Entities.HttpMethod.Get, capturedRequest.Method);
        Assert.Equal(environmentId, capturedRequest.EnvironmentId);
        _mockRequestService.Verify(s => s.CreateRequestAsync(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromCurlAsync_WithPostRequest_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl -X POST 'https://api.example.com/users'";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(Domain.Entities.HttpMethod.Post, capturedRequest.Method);
    }

    [Fact]
    public async Task ImportFromCurlAsync_WithHeaders_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl 'https://api.example.com/users' -H 'Content-Type: application/json' -H 'Accept: application/json'";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(2, capturedRequest.Headers.Count);
        Assert.Equal("application/json", capturedRequest.Headers["Content-Type"]);
        Assert.Equal("application/json", capturedRequest.Headers["Accept"]);
    }

    [Fact]
    public async Task ImportFromCurlAsync_WithJsonBody_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl -X POST 'https://api.example.com/users' -H 'Content-Type: application/json' -d '{\"name\":\"John\",\"email\":\"john@example.com\"}'";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(Domain.Entities.HttpMethod.Post, capturedRequest.Method);
        Assert.NotNull(capturedRequest.Body);
        Assert.Contains("John", capturedRequest.Body);
        Assert.Contains("john@example.com", capturedRequest.Body);
        Assert.Equal(BodyType.Json, capturedRequest.BodyType);
    }

    [Fact]
    public async Task ImportFromCurlAsync_WithBasicAuth_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl -u username:password 'https://api.example.com/protected'";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(AuthenticationType.Basic, capturedRequest.AuthType);
        Assert.Equal("username", capturedRequest.BasicAuthUsername);
        Assert.Equal("password", capturedRequest.BasicAuthPassword);
    }

    [Fact]
    public async Task ImportFromCurlAsync_WithBearerToken_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl 'https://api.example.com/protected' -H 'Authorization: Bearer my-secret-token'";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(AuthenticationType.BearerToken, capturedRequest.AuthType);
        Assert.Equal("my-secret-token", capturedRequest.BearerToken);
        Assert.False(capturedRequest.Headers.ContainsKey("Authorization"));
    }

    [Fact]
    public async Task ImportFromCurlAsync_WithCollectionId_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        var curlCommand = "curl 'https://api.example.com/users'";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, environmentId, collectionId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(collectionId, capturedRequest.CollectionId);
    }

    [Fact]
    public async Task ImportFromCurlAsync_WithMultilineCommand_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = @"curl -X POST 'https://api.example.com/users' \
  -H 'Content-Type: application/json' \
  -d '{""name"":""John""}'";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(Domain.Entities.HttpMethod.Post, capturedRequest.Method);
        Assert.Equal("application/json", capturedRequest.Headers["Content-Type"]);
        Assert.NotNull(capturedRequest.Body);
    }

    [Fact]
    public async Task ImportFromCurlAsync_WithPutMethod_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl -X PUT 'https://api.example.com/users/123'";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(Domain.Entities.HttpMethod.Put, capturedRequest.Method);
    }

    [Fact]
    public async Task ImportFromCurlAsync_WithDeleteMethod_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl -X DELETE 'https://api.example.com/users/123'";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(Domain.Entities.HttpMethod.Delete, capturedRequest.Method);
    }

    [Fact]
    public async Task ImportFromCurlAsync_WithPatchMethod_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl -X PATCH 'https://api.example.com/users/123'";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(Domain.Entities.HttpMethod.Patch, capturedRequest.Method);
    }

    [Fact]
    public async Task ImportFromCurlAsync_GeneratesAppropriateRequestName()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl 'https://api.example.com/users'";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Name);
        Assert.NotEqual("Imported Request", capturedRequest.Name);
        Assert.Contains("Request", capturedRequest.Name);
    }

    [Fact]
    public async Task ImportFromCurlAsync_WithRequestFlag_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl --request GET 'https://api.example.com/users'";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(Domain.Entities.HttpMethod.Get, capturedRequest.Method);
    }

    [Fact]
    public async Task ImportFromCurlAsync_WithDataRawFlag_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl -X POST 'https://api.example.com/users' --data-raw '{\"name\":\"John\"}'";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Body);
        Assert.Contains("John", capturedRequest.Body);
    }
}
