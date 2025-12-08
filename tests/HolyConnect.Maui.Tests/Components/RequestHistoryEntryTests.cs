using HolyConnect.Domain.Entities;

namespace HolyConnect.Maui.Tests.Components;

/// <summary>
/// Tests for RequestHistoryEntry entity used in UI components
/// </summary>
public class RequestHistoryEntryTests
{
    [Fact]
    public void RequestHistoryEntry_CanBeCreated_WithBasicProperties()
    {
        // Arrange & Act
        var history = new RequestHistoryEntry
        {
            Id = Guid.NewGuid(),
            RequestId = Guid.NewGuid(),
            RequestName = "Test Request",
            RequestType = RequestType.Rest,
            Timestamp = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, history.Id);
        Assert.NotNull(history.RequestId);
        Assert.Equal("Test Request", history.RequestName);
        Assert.Equal(RequestType.Rest, history.RequestType);
    }

    [Fact]
    public void RequestHistoryEntry_TracksExecutionTime()
    {
        // Arrange
        var beforeExecution = DateTime.UtcNow;
        
        // Act
        var history = new RequestHistoryEntry
        {
            Id = Guid.NewGuid(),
            RequestId = Guid.NewGuid(),
            RequestName = "Test",
            RequestType = RequestType.Rest,
            Timestamp = DateTime.UtcNow
        };
        
        var afterExecution = DateTime.UtcNow;

        // Assert
        Assert.True(history.Timestamp >= beforeExecution);
        Assert.True(history.Timestamp <= afterExecution);
    }

    [Fact]
    public void RequestHistoryEntry_HasSentRequest()
    {
        // Arrange & Act
        var history = new RequestHistoryEntry
        {
            Id = Guid.NewGuid(),
            RequestId = Guid.NewGuid(),
            RequestName = "Test",
            RequestType = RequestType.Rest,
            Timestamp = DateTime.UtcNow,
            SentRequest = new SentRequest
            {
                Method = "GET",
                Url = "https://api.example.com",
                Headers = new Dictionary<string, string>(),
                Body = string.Empty,
                QueryParameters = new Dictionary<string, string>()
            }
        };

        // Assert
        Assert.NotNull(history.SentRequest);
        Assert.Equal("GET", history.SentRequest.Method);
        Assert.Equal("https://api.example.com", history.SentRequest.Url);
    }

    [Fact]
    public void RequestHistoryEntry_HasResponse()
    {
        // Arrange & Act
        var history = new RequestHistoryEntry
        {
            Id = Guid.NewGuid(),
            RequestId = Guid.NewGuid(),
            RequestName = "Test",
            RequestType = RequestType.Rest,
            Timestamp = DateTime.UtcNow,
            Response = new RequestResponse
            {
                StatusCode = 200,
                StatusMessage = "OK",
                Body = "{\"success\": true}",
                ResponseTime = 150,
                Size = 100,
                Headers = new Dictionary<string, string>()
            }
        };

        // Assert
        Assert.NotNull(history.Response);
        Assert.Equal(200, history.Response.StatusCode);
        Assert.Equal("OK", history.Response.StatusMessage);
        Assert.Equal(150, history.Response.ResponseTime);
    }

    [Theory]
    [InlineData(RequestType.Rest)]
    [InlineData(RequestType.GraphQL)]
    [InlineData(RequestType.WebSocket)]
    [InlineData(RequestType.Soap)]
    public void RequestHistoryEntry_SupportsAllRequestTypes(RequestType requestType)
    {
        // Arrange & Act
        var history = new RequestHistoryEntry
        {
            Id = Guid.NewGuid(),
            RequestName = "Test",
            RequestType = requestType,
            Timestamp = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(requestType, history.RequestType);
    }

    [Fact]
    public void RequestHistoryEntry_CanLinkToOriginalRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid();

        // Act
        var history = new RequestHistoryEntry
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            RequestName = "Test",
            RequestType = RequestType.Rest,
            Timestamp = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(requestId, history.RequestId);
    }

    [Fact]
    public void RequestHistoryEntry_CanLinkToEnvironment()
    {
        // Arrange
        var environmentId = Guid.NewGuid();

        // Act
        var history = new RequestHistoryEntry
        {
            Id = Guid.NewGuid(),
            RequestName = "Test",
            RequestType = RequestType.Rest,
            Timestamp = DateTime.UtcNow,
            EnvironmentId = environmentId
        };

        // Assert
        Assert.Equal(environmentId, history.EnvironmentId);
    }

    [Fact]
    public void RequestHistoryEntry_CanLinkToCollection()
    {
        // Arrange
        var collectionId = Guid.NewGuid();

        // Act
        var history = new RequestHistoryEntry
        {
            Id = Guid.NewGuid(),
            RequestName = "Test",
            RequestType = RequestType.Rest,
            Timestamp = DateTime.UtcNow,
            CollectionId = collectionId
        };

        // Assert
        Assert.Equal(collectionId, history.CollectionId);
    }

    [Fact]
    public void RequestHistoryEntry_CanHaveNullLinks()
    {
        // Arrange & Act
        var history = new RequestHistoryEntry
        {
            Id = Guid.NewGuid(),
            RequestName = "Test",
            RequestType = RequestType.Rest,
            Timestamp = DateTime.UtcNow,
            RequestId = null,
            EnvironmentId = null,
            CollectionId = null
        };

        // Assert
        Assert.Null(history.RequestId);
        Assert.Null(history.EnvironmentId);
        Assert.Null(history.CollectionId);
    }

    [Fact]
    public void RequestHistoryEntry_ResponseCanContainHeaders()
    {
        // Arrange & Act
        var history = new RequestHistoryEntry
        {
            Id = Guid.NewGuid(),
            RequestName = "Test",
            RequestType = RequestType.Rest,
            Timestamp = DateTime.UtcNow,
            Response = new RequestResponse
            {
                StatusCode = 200,
                StatusMessage = "OK",
                Body = "{}",
                ResponseTime = 100,
                Size = 50,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Cache-Control", "no-cache" }
                }
            }
        };

        // Assert
        Assert.Equal(2, history.Response.Headers.Count);
        Assert.Equal("application/json", history.Response.Headers["Content-Type"]);
        Assert.Equal("no-cache", history.Response.Headers["Cache-Control"]);
    }

    [Fact]
    public void RequestHistoryEntry_SentRequestCanContainHeaders()
    {
        // Arrange & Act
        var history = new RequestHistoryEntry
        {
            Id = Guid.NewGuid(),
            RequestName = "Test",
            RequestType = RequestType.Rest,
            Timestamp = DateTime.UtcNow,
            SentRequest = new SentRequest
            {
                Method = "POST",
                Url = "https://api.example.com",
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer token123" },
                    { "User-Agent", "HolyConnect/1.0" }
                },
                Body = "{\"test\":\"data\"}",
                QueryParameters = new Dictionary<string, string>()
            }
        };

        // Assert
        Assert.Equal(2, history.SentRequest.Headers.Count);
        Assert.Equal("Bearer token123", history.SentRequest.Headers["Authorization"]);
        Assert.Equal("HolyConnect/1.0", history.SentRequest.Headers["User-Agent"]);
    }

    [Fact]
    public void RequestHistoryEntry_SentRequestCanContainQueryParameters()
    {
        // Arrange & Act
        var history = new RequestHistoryEntry
        {
            Id = Guid.NewGuid(),
            RequestName = "Test",
            RequestType = RequestType.Rest,
            Timestamp = DateTime.UtcNow,
            SentRequest = new SentRequest
            {
                Method = "GET",
                Url = "https://api.example.com",
                Headers = new Dictionary<string, string>(),
                Body = string.Empty,
                QueryParameters = new Dictionary<string, string>
                {
                    { "page", "1" },
                    { "limit", "10" }
                }
            }
        };

        // Assert
        Assert.Equal(2, history.SentRequest.QueryParameters.Count);
        Assert.Equal("1", history.SentRequest.QueryParameters["page"]);
        Assert.Equal("10", history.SentRequest.QueryParameters["limit"]);
    }
}
