using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services.ImportStrategies;
using HttpMethod = HolyConnect.Domain.Entities.HttpMethod;

namespace HolyConnect.Infrastructure.Tests.Services.ImportStrategies;

public class CurlImportStrategyTests
{
    private readonly CurlImportStrategy _strategy;

    public CurlImportStrategyTests()
    {
        _strategy = new CurlImportStrategy();
    }

    [Fact]
    public void Source_ShouldReturnCurl()
    {
        // Assert
        Assert.Equal(ImportSource.Curl, _strategy.Source);
    }

    [Fact]
    public void Parse_WithValidGetRequest_ShouldReturnRestRequest()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl 'https://api.example.com/users'";

        // Act
        var result = _strategy.Parse(curlCommand, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal("https://api.example.com/users", restRequest.Url);
        Assert.Equal(HttpMethod.Get, restRequest.Method);    }

    [Fact]
    public void Parse_WithInvalidCommand_ShouldReturnNull()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var invalidCommand = "not a curl command";

        // Act
        var result = _strategy.Parse(invalidCommand, null, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Parse_WithPostRequestAndBody_ShouldReturnRestRequestWithBody()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl -X POST 'https://api.example.com/users' -d '{\"name\":\"John\"}'";

        // Act
        var result = _strategy.Parse(curlCommand, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal(HttpMethod.Post, restRequest.Method);
        Assert.NotNull(restRequest.Body);
        Assert.Contains("John", restRequest.Body);
    }

    [Fact]
    public void Parse_WithCustomName_ShouldUseCustomName()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl 'https://api.example.com/users'";
        var customName = "My Custom Request";

        // Act
        var result = _strategy.Parse(curlCommand, null, customName);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal(customName, restRequest.Name);
    }

    [Fact]
    public void Parse_WithBasicAuth_ShouldSetAuthentication()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl -u username:password 'https://api.example.com/protected'";

        // Act
        var result = _strategy.Parse(curlCommand, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal(AuthenticationType.Basic, restRequest.AuthType);
        Assert.Equal("username", restRequest.BasicAuthUsername);
        Assert.Equal("password", restRequest.BasicAuthPassword);
    }

    [Fact]
    public void Parse_WithBearerToken_ShouldSetAuthentication()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl 'https://api.example.com/protected' -H 'Authorization: Bearer my-token'";

        // Act
        var result = _strategy.Parse(curlCommand, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal(AuthenticationType.BearerToken, restRequest.AuthType);
        Assert.Equal("my-token", restRequest.BearerToken);
    }

    [Fact]
    public void Parse_WithHeaders_ShouldParseHeaders()
    {
        // Arrange
        var environmentId = Guid.NewGuid();
        var curlCommand = "curl 'https://api.example.com/users' -H 'Content-Type: application/json' -H 'Accept: application/json'";

        // Act
        var result = _strategy.Parse(curlCommand, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal(2, restRequest.Headers.Count);
        Assert.Equal("application/json", restRequest.Headers["Content-Type"]);
        Assert.Equal("application/json", restRequest.Headers["Accept"]);
    }
}
