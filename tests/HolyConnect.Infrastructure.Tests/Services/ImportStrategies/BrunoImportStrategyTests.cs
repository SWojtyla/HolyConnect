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
}
