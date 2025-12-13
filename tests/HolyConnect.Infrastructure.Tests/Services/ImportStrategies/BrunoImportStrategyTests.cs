using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services.ImportStrategies;
using HttpMethod = HolyConnect.Domain.Entities.HttpMethod;

namespace HolyConnect.Infrastructure.Tests.Services.ImportStrategies;

public class BrunoImportStrategyTests
{
    private readonly BrunoImportStrategy _strategy;

    public BrunoImportStrategyTests()
    {
        _strategy = new BrunoImportStrategy();
    }

    [Fact]
    public void Source_ShouldReturnBruno()
    {
        // Assert
        Assert.Equal(ImportSource.Bruno, _strategy.Source);
    }

    [Fact]
    public void Parse_WithValidGetRequest_ShouldReturnRestRequest()
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
}";

        // Act
        var result = _strategy.Parse(brunoContent, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal("Get Users", restRequest.Name);
        Assert.Equal("https://api.example.com/users", restRequest.Url);
        Assert.Equal(HttpMethod.Get, restRequest.Method);
    }

    [Fact]
    public void Parse_WithEmptyContent_ShouldReturnNull()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var emptyContent = "";

        // Act
        var result = _strategy.Parse(emptyContent, null, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Parse_WithPostRequestAndJsonBody_ShouldReturnRestRequestWithBody()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Create User
  type: http
}

post {
  url: https://api.example.com/users
}

body:json {
  {
    ""name"": ""John Doe""
  }
}";

        // Act
        var result = _strategy.Parse(brunoContent, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal(HttpMethod.Post, restRequest.Method);
        Assert.NotNull(restRequest.Body);
        Assert.Contains("John Doe", restRequest.Body);
        Assert.Equal(BodyType.Json, restRequest.BodyType);
    }

    [Fact]
    public void Parse_WithCustomName_ShouldUseCustomName()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Original Name
  type: http
}

get {
  url: https://api.example.com/users
}";
        var customName = "My Custom Request";

        // Act
        var result = _strategy.Parse(brunoContent, null, customName);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal(customName, restRequest.Name);
    }

    [Fact]
    public void Parse_WithBearerAuth_ShouldSetAuthentication()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Get Protected
  type: http
}

get {
  url: https://api.example.com/protected
}

auth:bearer {
  token: my-secret-token
}";

        // Act
        var result = _strategy.Parse(brunoContent, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal(AuthenticationType.BearerToken, restRequest.AuthType);
        Assert.Equal("my-secret-token", restRequest.BearerToken);
    }

    [Fact]
    public void Parse_WithBasicAuth_ShouldSetAuthentication()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Get Protected
  type: http
}

get {
  url: https://api.example.com/protected
}

auth:basic {
  username: testuser
  password: testpass
}";

        // Act
        var result = _strategy.Parse(brunoContent, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal(AuthenticationType.Basic, restRequest.AuthType);
        Assert.Equal("testuser", restRequest.BasicAuthUsername);
        Assert.Equal("testpass", restRequest.BasicAuthPassword);
    }

    [Fact]
    public void Parse_WithGraphQLQuery_ShouldReturnGraphQLRequest()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var brunoContent = @"
meta {
  name: Get User
  type: graphql
}

post {
  url: https://api.example.com/graphql
}

body:graphql {
  query GetUser {
    user {
      id
      name
    }
  }
}";

        // Act
        var result = _strategy.Parse(brunoContent, null, null);

        // Assert
        Assert.NotNull(result);
        var graphqlRequest = Assert.IsType<GraphQLRequest>(result);
        Assert.Equal("Get User", graphqlRequest.Name);
        Assert.Equal("https://api.example.com/graphql", graphqlRequest.Url);
        Assert.Contains("GetUser", graphqlRequest.Query);
        Assert.Equal(GraphQLOperationType.Query, graphqlRequest.OperationType);
    }

    [Fact]
    public void Parse_WithGraphQLMutation_ShouldReturnGraphQLRequestWithMutationType()
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
}

body:graphql {
  mutation CreateUser {
    createUser(name: ""John"") {
      id
    }
  }
}";

        // Act
        var result = _strategy.Parse(brunoContent, null, null);

        // Assert
        Assert.NotNull(result);
        var graphqlRequest = Assert.IsType<GraphQLRequest>(result);
        Assert.Contains("mutation", graphqlRequest.Query);
        Assert.Equal(GraphQLOperationType.Mutation, graphqlRequest.OperationType);
    }

    [Fact]
    public void ParseEnvironment_WithValidEnvironmentFile_ShouldReturnEnvironment()
    {
        // Arrange
        var brunoContent = @"
vars {
  baseUrl: https://api.example.com
  apiKey: secret-key-123
  timeout: 5000
}

vars:secret [
  apiKey
]";

        // Act
        var result = _strategy.ParseEnvironment(brunoContent, "Development");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Development", result.Name);
        Assert.Equal(3, result.Variables.Count);
        Assert.Equal("https://api.example.com", result.Variables["baseUrl"]);
        Assert.Equal("secret-key-123", result.Variables["apiKey"]);
        Assert.Equal("5000", result.Variables["timeout"]);
        Assert.Single(result.SecretVariableNames);
        Assert.Contains("apiKey", result.SecretVariableNames);
    }

    [Fact]
    public void ParseEnvironment_WithEmptyContent_ShouldReturnNull()
    {
        // Arrange
        var emptyContent = "";

        // Act
        var result = _strategy.ParseEnvironment(emptyContent, "Test");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseEnvironment_WithOnlyVariables_ShouldReturnEnvironmentWithoutSecrets()
    {
        // Arrange
        var brunoContent = @"
vars {
  var1: value1
  var2: value2
}";

        // Act
        var result = _strategy.ParseEnvironment(brunoContent, "Production");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Production", result.Name);
        Assert.Equal(2, result.Variables.Count);
        Assert.Empty(result.SecretVariableNames);
    }

    [Fact]
    public void ParseEnvironment_WithMultipleSecretVariables_ShouldReturnAllSecrets()
    {
        // Arrange
        var brunoContent = @"
vars {
  baseUrl: https://api.example.com
  apiKey: secret1
  authToken: secret2
  publicVar: public
}

vars:secret [
  apiKey
  authToken
]";

        // Act
        var result = _strategy.ParseEnvironment(brunoContent, "Staging");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Variables.Count);
        Assert.Equal(2, result.SecretVariableNames.Count);
        Assert.Contains("apiKey", result.SecretVariableNames);
        Assert.Contains("authToken", result.SecretVariableNames);
    }

    [Fact]
    public void ParseCollectionVariables_WithValidContent_ShouldReturnVariables()
    {
        // Arrange
        var brunoContent = @"
vars {
  collectionVar1: value1
  sharedEndpoint: /api/v1
}";

        // Act
        var result = _strategy.ParseCollectionVariables(brunoContent);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("value1", result["collectionVar1"]);
        Assert.Equal("/api/v1", result["sharedEndpoint"]);
    }

    [Fact]
    public void ParseCollectionVariables_WithEmptyContent_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var emptyContent = "";

        // Act
        var result = _strategy.ParseCollectionVariables(emptyContent);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseCollectionSecretVariables_WithValidContent_ShouldReturnSecrets()
    {
        // Arrange
        var brunoContent = @"
vars {
  var1: value1
  secret1: secretvalue
}

vars:secret [
  secret1
]";

        // Act
        var result = _strategy.ParseCollectionSecretVariables(brunoContent);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("secret1", result);
    }
}
