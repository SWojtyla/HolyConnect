using System.Net;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Common;
using Xunit;

namespace HolyConnect.Infrastructure.Tests.Common;

public class RequestResponseBuilderTests
{
    [Fact]
    public void Create_ShouldCreateNonStreamingResponse()
    {
        // Arrange & Act
        var response = RequestResponseBuilder.Create().Build();

        // Assert
        Assert.NotNull(response);
        Assert.False(response.IsStreaming);
        Assert.NotEqual(default(DateTime), response.Timestamp);
    }

    [Fact]
    public void CreateStreaming_ShouldCreateStreamingResponse()
    {
        // Arrange & Act
        var response = RequestResponseBuilder.CreateStreaming().Build();

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsStreaming);
        Assert.NotEqual(default(DateTime), response.Timestamp);
    }

    [Fact]
    public void WithSentRequest_ShouldSetSentRequest()
    {
        // Arrange
        var sentRequest = new SentRequest
        {
            Url = "https://api.example.com",
            Method = "GET",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };

        // Act
        var response = RequestResponseBuilder.Create()
            .WithSentRequest(sentRequest)
            .Build();

        // Assert
        Assert.Equal(sentRequest, response.SentRequest);
        Assert.NotNull(response.SentRequest);
        Assert.Equal("https://api.example.com", response.SentRequest.Url);
        Assert.Equal("GET", response.SentRequest.Method);
    }

    [Fact]
    public void WithSentRequest_FromHttpRequestMessage_ShouldCreateSentRequest()
    {
        // Arrange
        var httpRequest = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://api.example.com/");
        httpRequest.Headers.Add("X-Custom-Header", "value");

        // Act
        var response = RequestResponseBuilder.Create()
            .WithSentRequest(httpRequest, "https://api.example.com/", "POST", "{\"test\": true}")
            .Build();

        // Assert
        Assert.NotNull(response.SentRequest);
        Assert.Equal("https://api.example.com/", response.SentRequest.Url);
        Assert.Equal("POST", response.SentRequest.Method);
        Assert.Equal("{\"test\": true}", response.SentRequest.Body);
    }

    [Fact]
    public void StopTiming_ShouldRecordResponseTime()
    {
        // Arrange & Act
        var response = RequestResponseBuilder.Create()
            .StopTiming()
            .Build();

        // Assert
        Assert.True(response.ResponseTime >= 0);
    }

    [Fact]
    public void WithStatus_FromHttpResponseMessage_ShouldSetStatusCodeAndMessage()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            ReasonPhrase = "OK"
        };

        // Act
        var response = RequestResponseBuilder.Create()
            .WithStatus(httpResponse)
            .Build();

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("OK", response.StatusMessage);
    }

    [Fact]
    public void WithStatus_CustomValues_ShouldSetStatusCodeAndMessage()
    {
        // Arrange & Act
        var response = RequestResponseBuilder.Create()
            .WithStatus(404, "Not Found")
            .Build();

        // Assert
        Assert.Equal(404, response.StatusCode);
        Assert.Equal("Not Found", response.StatusMessage);
    }

    [Fact]
    public void WithHeaders_FromHttpResponseHeaders_ShouldCaptureHeaders()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage();
        httpResponse.Headers.Add("X-Custom-Header", "custom-value");
        httpResponse.Headers.Add("X-Another-Header", "another-value");

        // Act
        var response = RequestResponseBuilder.Create()
            .WithHeaders(httpResponse.Headers)
            .Build();

        // Assert
        Assert.Equal(2, response.Headers.Count);
        Assert.Equal("custom-value", response.Headers["X-Custom-Header"]);
        Assert.Equal("another-value", response.Headers["X-Another-Header"]);
    }

    [Fact]
    public void WithHeaders_FromHttpContentHeaders_ShouldCaptureHeaders()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage();
        httpResponse.Content = new StringContent("test");
        httpResponse.Content.Headers.Add("Content-Encoding", "gzip");

        // Act
        var response = RequestResponseBuilder.Create()
            .WithHeaders(httpResponse.Content.Headers)
            .Build();

        // Assert
        Assert.True(response.Headers.ContainsKey("Content-Type"));
        Assert.True(response.Headers.ContainsKey("Content-Encoding"));
        Assert.Equal("gzip", response.Headers["Content-Encoding"]);
    }

    [Fact]
    public void WithHeaders_WithNullContentHeaders_ShouldNotThrow()
    {
        // Arrange & Act
        var response = RequestResponseBuilder.Create()
            .WithHeaders((System.Net.Http.Headers.HttpContentHeaders?)null)
            .Build();

        // Assert
        Assert.Empty(response.Headers);
    }

    [Fact]
    public void WithBody_ShouldSetBodyAndSize()
    {
        // Arrange
        var body = "This is a test response body";

        // Act
        var response = RequestResponseBuilder.Create()
            .WithBody(body)
            .Build();

        // Assert
        Assert.Equal(body, response.Body);
        Assert.Equal(body.Length, response.Size);
    }

    [Fact]
    public async Task WithBodyFromContentAsync_ShouldReadAndSetBody()
    {
        // Arrange
        var content = new StringContent("Response from content");

        // Act
        var builder = RequestResponseBuilder.Create();
        await builder.WithBodyFromContentAsync(content);
        var response = builder.Build();

        // Assert
        Assert.Equal("Response from content", response.Body);
        Assert.Equal("Response from content".Length, response.Size);
        Assert.True(response.Headers.ContainsKey("Content-Type"));
    }

    [Fact]
    public async Task WithBodyFromContentAsync_WithNullContent_ShouldNotSetBody()
    {
        // Arrange & Act
        var builder = RequestResponseBuilder.Create();
        await builder.WithBodyFromContentAsync(null);
        var response = builder.Build();

        // Assert
        Assert.Empty(response.Body);
        Assert.Equal(0, response.Size);
    }

    [Fact]
    public void AddStreamEvent_WithDataAndEventType_ShouldAddEvent()
    {
        // Arrange & Act
        var response = RequestResponseBuilder.CreateStreaming()
            .AddStreamEvent("Event data", "custom-event")
            .Build();

        // Assert
        Assert.Single(response.StreamEvents);
        Assert.Equal("Event data", response.StreamEvents[0].Data);
        Assert.Equal("custom-event", response.StreamEvents[0].EventType);
        Assert.NotEqual(default(DateTime), response.StreamEvents[0].Timestamp);
    }

    [Fact]
    public void AddStreamEvent_WithStreamEventObject_ShouldAddEvent()
    {
        // Arrange
        var streamEvent = new StreamEvent
        {
            Timestamp = DateTime.UtcNow,
            Data = "Test event",
            EventType = "test"
        };

        // Act
        var response = RequestResponseBuilder.CreateStreaming()
            .AddStreamEvent(streamEvent)
            .Build();

        // Assert
        Assert.Single(response.StreamEvents);
        Assert.Equal(streamEvent, response.StreamEvents[0]);
    }

    [Fact]
    public void AddStreamEvent_MultipleEvents_ShouldAddAllEvents()
    {
        // Arrange & Act
        var response = RequestResponseBuilder.CreateStreaming()
            .AddStreamEvent("Event 1", "type1")
            .AddStreamEvent("Event 2", "type2")
            .AddStreamEvent("Event 3", "type3")
            .Build();

        // Assert
        Assert.Equal(3, response.StreamEvents.Count);
        Assert.Equal("Event 1", response.StreamEvents[0].Data);
        Assert.Equal("Event 2", response.StreamEvents[1].Data);
        Assert.Equal("Event 3", response.StreamEvents[2].Data);
    }

    [Fact]
    public void FinalizeStreaming_ShouldBuildBodyFromStreamEvents()
    {
        // Arrange & Act
        var response = RequestResponseBuilder.CreateStreaming()
            .AddStreamEvent("Event 1", "info")
            .AddStreamEvent("Event 2", "data")
            .FinalizeStreaming()
            .Build();

        // Assert
        Assert.NotEmpty(response.Body);
        Assert.Contains("Event 1", response.Body);
        Assert.Contains("Event 2", response.Body);
        Assert.Equal(response.Body.Length, response.Size);
    }

    [Fact]
    public void WithException_ShouldPopulateErrorInformation()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");

        // Act
        var response = RequestResponseBuilder.Create()
            .WithException(exception)
            .Build();

        // Assert
        Assert.Equal(0, response.StatusCode);
        Assert.Contains("Test error", response.StatusMessage);
        Assert.Contains("InvalidOperationException", response.Body);
        Assert.True(response.ResponseTime >= 0);
    }

    [Fact]
    public void ElapsedMilliseconds_ShouldReturnTimingWithoutStopping()
    {
        // Arrange
        var builder = RequestResponseBuilder.Create();
        Thread.Sleep(10); // Small delay

        // Act
        var elapsed1 = builder.ElapsedMilliseconds;
        Thread.Sleep(10); // Another small delay
        var elapsed2 = builder.ElapsedMilliseconds;

        // Assert
        Assert.True(elapsed1 >= 0);
        Assert.True(elapsed2 > elapsed1);
    }

    [Fact]
    public void FluentAPI_CompleteWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        var httpRequest = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, "https://api.example.com");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        httpResponse.Content = new StringContent("{\"result\": \"success\"}");
        httpResponse.Headers.Add("X-Request-Id", "12345");

        // Act
        var response = RequestResponseBuilder.Create()
            .WithSentRequest(httpRequest, "https://api.example.com", "GET")
            .WithStatus(httpResponse)
            .WithHeaders(httpResponse.Headers)
            .WithHeaders(httpResponse.Content.Headers)
            .WithBody("{\"result\": \"success\"}")
            .StopTiming()
            .Build();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("{\"result\": \"success\"}", response.Body);
        Assert.NotEmpty(response.Headers); // Should have at least Content-Type from content
        Assert.NotNull(response.SentRequest);
        Assert.True(response.ResponseTime >= 0);
    }

    [Fact]
    public async Task FluentAPI_StreamingWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        var httpRequest = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://api.example.com/stream");
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

        // Act
        var builder = RequestResponseBuilder.CreateStreaming();
        builder.WithSentRequest(httpRequest, "https://api.example.com/stream", "POST")
            .WithStatus(httpResponse)
            .AddStreamEvent("Connected", "connect")
            .AddStreamEvent("Data received", "data")
            .AddStreamEvent("Disconnected", "disconnect")
            .FinalizeStreaming()
            .StopTiming();
        var response = builder.Build();

        // Assert
        Assert.True(response.IsStreaming);
        Assert.Equal(3, response.StreamEvents.Count);
        Assert.NotEmpty(response.Body);
        Assert.True(response.ResponseTime >= 0);
    }

    [Fact]
    public async Task FluentAPI_ErrorHandlingWorkflow_ShouldWorkCorrectly()
    {
        // Arrange
        var httpRequest = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, "https://api.example.com");
        var exception = new HttpRequestException("Network error");

        // Act
        var builder = RequestResponseBuilder.Create();
        builder.WithSentRequest(httpRequest, "https://api.example.com", "GET")
            .WithException(exception);
        var response = builder.Build();

        // Assert
        Assert.Equal(0, response.StatusCode);
        Assert.Contains("Network error", response.StatusMessage);
        Assert.Contains("HttpRequestException", response.Body);
        Assert.True(response.ResponseTime >= 0);
    }

    [Fact]
    public void Build_WhenCalledTwice_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = RequestResponseBuilder.Create();
        builder.Build(); // First call succeeds

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());
        Assert.Contains("Build() has already been called", exception.Message);
        Assert.Contains("single-use pattern", exception.Message);
    }
}
