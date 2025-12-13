using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services;
using HolyConnect.Infrastructure.Services.ImportStrategies;
using Moq;

namespace HolyConnect.Infrastructure.Tests.Services;

public class ImportServiceTests
{
    private readonly Mock<IRequestService> _mockRequestService;
    private readonly Mock<IEnvironmentService> _mockEnvironmentService;
    private readonly ImportService _service;

    public ImportServiceTests()
    {
        _mockRequestService = new Mock<IRequestService>();
        var mockCollectionService = new Mock<ICollectionService>();
        _mockEnvironmentService = new Mock<IEnvironmentService>();
        var strategies = new List<IImportStrategy>
        {
            new CurlImportStrategy(),
            new BrunoImportStrategy()
        };
        _service = new ImportService(_mockRequestService.Object, mockCollectionService.Object, _mockEnvironmentService.Object, strategies);
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
    public void CanImport_WithBrunoSource_ReturnsTrue()
    {
        // Act
        var result = _service.CanImport(ImportSource.Bruno);

        // Assert
        Assert.True(result);
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
        var result = await _service.ImportFromCurlAsync(curlCommand, collectionId);

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

    [Fact]
    public async Task ImportFromCurlAsync_WithCustomName_UsesCustomName()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl 'https://api.example.com/users'";
        var customName = "My Custom Request Name";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromCurlAsync(curlCommand, null, customName);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(customName, capturedRequest.Name);
    }

    [Fact]
    public async Task ImportFromCurlAsync_WithoutCustomName_AutoGeneratesName()
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

    #region Bruno Import Tests

    [Fact]
    public async Task ImportFromBrunoAsync_WithEmptyContent_ReturnsError()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = "";

        // Act
        var result = await _service.ImportFromBrunoAsync(brunoContent, environmentId);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Failed to parse Bruno file", result.ErrorMessage);
    }

    [Fact]
    public async Task ImportFromBrunoAsync_WithSimpleGetRequest_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Get Users List
  type: http
  seq: 1
}

get {
  url: https://api.example.com/users
  body: none
  auth: none
}";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromBrunoAsync(brunoContent, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.ImportedRequest);
        Assert.NotNull(capturedRequest);
        Assert.Equal("Get Users List", capturedRequest.Name);
        Assert.Equal("https://api.example.com/users", capturedRequest.Url);
        Assert.Equal(Domain.Entities.HttpMethod.Get, capturedRequest.Method);
        _mockRequestService.Verify(s => s.CreateRequestAsync(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromBrunoAsync_WithPostRequest_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Create User
  type: http
  seq: 2
}

post {
  url: https://api.example.com/users
  body: json
  auth: none
}

body:json {
  {
    ""name"": ""John Doe"",
    ""email"": ""john@example.com""
  }
}";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromBrunoAsync(brunoContent, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal("Create User", capturedRequest.Name);
        Assert.Equal(Domain.Entities.HttpMethod.Post, capturedRequest.Method);
        Assert.NotNull(capturedRequest.Body);
        Assert.Contains("John Doe", capturedRequest.Body);
        Assert.Contains("john@example.com", capturedRequest.Body);
        Assert.Equal(BodyType.Json, capturedRequest.BodyType);
    }

    [Fact]
    public async Task ImportFromBrunoAsync_WithHeaders_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Get Users
  type: http
}

get {
  url: https://api.example.com/users
}

headers {
  Content-Type: application/json
  Accept: application/json
  X-Custom-Header: custom-value
}";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromBrunoAsync(brunoContent, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(3, capturedRequest.Headers.Count);
        Assert.Equal("application/json", capturedRequest.Headers["Content-Type"]);
        Assert.Equal("application/json", capturedRequest.Headers["Accept"]);
        Assert.Equal("custom-value", capturedRequest.Headers["X-Custom-Header"]);
    }

    [Fact]
    public async Task ImportFromBrunoAsync_WithBearerAuth_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Get Protected Data
  type: http
}

get {
  url: https://api.example.com/protected
  auth: bearer
}

auth:bearer {
  token: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9
}";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromBrunoAsync(brunoContent, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(AuthenticationType.BearerToken, capturedRequest.AuthType);
        Assert.Equal("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9", capturedRequest.BearerToken);
    }

    [Fact]
    public async Task ImportFromBrunoAsync_WithBasicAuth_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Get Protected Data
  type: http
}

get {
  url: https://api.example.com/protected
  auth: basic
}

auth:basic {
  username: testuser
  password: testpass123
}";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromBrunoAsync(brunoContent, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(AuthenticationType.Basic, capturedRequest.AuthType);
        Assert.Equal("testuser", capturedRequest.BasicAuthUsername);
        Assert.Equal("testpass123", capturedRequest.BasicAuthPassword);
    }

    [Fact]
    public async Task ImportFromBrunoAsync_WithGraphQLQuery_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Get User
  type: graphql
  seq: 1
}

post {
  url: https://api.example.com/graphql
  body: graphql
  auth: none
}

body:graphql {
  query GetUser($id: ID!) {
    user(id: $id) {
      id
      name
      email
    }
  }
}

body:graphql:vars {
  {
    ""id"": ""123""
  }
}";
        GraphQLRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as GraphQLRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromBrunoAsync(brunoContent, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal("Get User", capturedRequest.Name);
        Assert.Equal("https://api.example.com/graphql", capturedRequest.Url);
        Assert.Contains("GetUser", capturedRequest.Query);
        Assert.Contains("user(id: $id)", capturedRequest.Query);
        Assert.NotNull(capturedRequest.Variables);
        Assert.Contains("123", capturedRequest.Variables);
        Assert.Equal(GraphQLOperationType.Query, capturedRequest.OperationType);
    }

    [Fact]
    public async Task ImportFromBrunoAsync_WithGraphQLMutation_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Create User
  type: graphql
}

post {
  url: https://api.example.com/graphql
  body: graphql
}

body:graphql {
  mutation CreateUser($name: String!, $email: String!) {
    createUser(name: $name, email: $email) {
      id
      name
    }
  }
}";
        GraphQLRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as GraphQLRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromBrunoAsync(brunoContent, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal("Create User", capturedRequest.Name);
        Assert.Contains("mutation", capturedRequest.Query);
        Assert.Equal(GraphQLOperationType.Mutation, capturedRequest.OperationType);
    }

    [Fact]
    public async Task ImportFromBrunoAsync_WithPutMethod_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Update User
  type: http
}

put {
  url: https://api.example.com/users/123
}";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromBrunoAsync(brunoContent, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(Domain.Entities.HttpMethod.Put, capturedRequest.Method);
    }

    [Fact]
    public async Task ImportFromBrunoAsync_WithDeleteMethod_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Delete User
  type: http
}

delete {
  url: https://api.example.com/users/123
}";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromBrunoAsync(brunoContent, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(Domain.Entities.HttpMethod.Delete, capturedRequest.Method);
    }

    [Fact]
    public async Task ImportFromBrunoAsync_WithPatchMethod_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Partial Update
  type: http
}

patch {
  url: https://api.example.com/users/123
}";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromBrunoAsync(brunoContent, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(Domain.Entities.HttpMethod.Patch, capturedRequest.Method);
    }

    [Fact]
    public async Task ImportFromBrunoAsync_WithCustomName_UsesCustomName()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var customName = "My Custom Request";
        var brunoContent = @"
meta {
  name: Original Name
  type: http
}

get {
  url: https://api.example.com/users
}";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromBrunoAsync(brunoContent, null, customName);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(customName, capturedRequest.Name);
    }

    [Fact]
    public async Task ImportFromBrunoAsync_WithCollectionId_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Get Users
  type: http
}

get {
  url: https://api.example.com/users
}";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromBrunoAsync(brunoContent, collectionId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.Equal(collectionId, capturedRequest.CollectionId);
    }

    [Fact]
    public async Task ImportFromBrunoAsync_WithXmlBody_SuccessfullyImports()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Create Item
  type: http
}

post {
  url: https://api.example.com/items
  body: xml
}

body:xml {
  <item>
    <name>Test Item</name>
    <value>123</value>
  </item>
}";
        RestRequest? capturedRequest = null;

        _mockRequestService.Setup(s => s.CreateRequestAsync(It.IsAny<Request>()))
            .Callback<Request>(r => capturedRequest = r as RestRequest)
            .ReturnsAsync((Request r) => r);

        // Act
        var result = await _service.ImportFromBrunoAsync(brunoContent, environmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Body);
        Assert.Contains("<item>", capturedRequest.Body);
        Assert.Contains("Test Item", capturedRequest.Body);
        Assert.Equal(BodyType.Xml, capturedRequest.BodyType);
    }

    #endregion
}
