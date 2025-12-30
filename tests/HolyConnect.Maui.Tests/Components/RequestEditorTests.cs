using HolyConnect.Domain.Entities;

namespace HolyConnect.Maui.Tests.Components;

/// <summary>
/// Tests for RequestEditor component logic to ensure it works correctly
/// Note: Full component rendering tests require MAUI Blazor environment
/// These tests verify the data models and logic used by the component
/// </summary>
public class RequestEditorTests
{
    [Fact]
    public void RestRequest_WithBody_ShouldSupportDefaultHeaders()
    {
        // Arrange
        var restRequest = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test Request",
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Post,
            Body = "{\"test\": \"data\"}",
            BodyType = BodyType.Json
        };
        
        // Act & Assert - Verify the request can have headers
        Assert.NotNull(restRequest.Headers);
        Assert.Empty(restRequest.Headers);
        
        // Verify we can add headers
        restRequest.Headers["User-Agent"] = "HolyConnect/1.0";
        restRequest.Headers["Content-Type"] = "application/json";
        
        Assert.Equal(2, restRequest.Headers.Count);
        Assert.Equal("HolyConnect/1.0", restRequest.Headers["User-Agent"]);
        Assert.Equal("application/json", restRequest.Headers["Content-Type"]);
    }
    
    [Fact]
    public void GraphQLRequest_ShouldSupportDefaultHeaders()
    {
        // Arrange
        var graphQLRequest = new GraphQLRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test GraphQL Query",
            Url = "https://api.example.com/graphql",
            Query = "query { test }"
        };
        
        // Act & Assert - Verify the request can have headers
        Assert.NotNull(graphQLRequest.Headers);
        Assert.Empty(graphQLRequest.Headers);
        
        // Verify we can add headers
        graphQLRequest.Headers["User-Agent"] = "HolyConnect/1.0";
        graphQLRequest.Headers["Content-Type"] = "application/json";
        
        Assert.Equal(2, graphQLRequest.Headers.Count);
    }
    
    [Fact]
    public void WebSocketRequest_ShouldSupportDefaultHeaders()
    {
        // Arrange
        var webSocketRequest = new WebSocketRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test WebSocket",
            Url = "wss://api.example.com/ws"
        };
        
        // Act & Assert - Verify the request can have headers
        Assert.NotNull(webSocketRequest.Headers);
        Assert.Empty(webSocketRequest.Headers);
        
        // Verify we can add headers
        webSocketRequest.Headers["User-Agent"] = "HolyConnect/1.0";
        
        Assert.Single(webSocketRequest.Headers);
        Assert.Equal("HolyConnect/1.0", webSocketRequest.Headers["User-Agent"]);
    }
    
    [Fact]
    public void Request_ShouldSupportDisabledHeaders()
    {
        // Arrange
        var restRequest = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test Request",
            Url = "https://api.example.com/test"
        };
        
        // Act
        restRequest.Headers["User-Agent"] = "HolyConnect/1.0";
        restRequest.Headers["Content-Type"] = "application/json";
        restRequest.DisabledHeaders.Add("Content-Type");
        
        // Assert
        Assert.Equal(2, restRequest.Headers.Count);
        Assert.Single(restRequest.DisabledHeaders);
        Assert.Contains("Content-Type", restRequest.DisabledHeaders);
        
        // Verify enabled headers
        var enabledHeaders = restRequest.Headers
            .Where(h => !restRequest.DisabledHeaders.Contains(h.Key))
            .ToList();
        
        Assert.Single(enabledHeaders);
        Assert.Equal("User-Agent", enabledHeaders[0].Key);
    }
    
    [Fact]
    public void AppSettings_AutoSaveOnNavigate_ShouldControlAutoSaveBehavior()
    {
        // Arrange
        var settingsWithAutoSaveEnabled = new AppSettings
        {
            AutoSaveOnNavigate = true
        };
        
        var settingsWithAutoSaveDisabled = new AppSettings
        {
            AutoSaveOnNavigate = false
        };
        
        // Assert
        Assert.True(settingsWithAutoSaveEnabled.AutoSaveOnNavigate);
        Assert.False(settingsWithAutoSaveDisabled.AutoSaveOnNavigate);
    }
}
