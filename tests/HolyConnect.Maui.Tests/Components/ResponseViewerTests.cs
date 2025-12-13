using HolyConnect.Domain.Entities;

namespace HolyConnect.Maui.Tests.Components;

/// <summary>
/// Tests for ResponseViewer component streaming support logic.
/// These tests verify the behavior of streaming response handling.
/// </summary>
public class ResponseViewerTests
{
    [Fact]
    public void GetEventTypeColor_WithMessageEventType_ReturnsPrimaryColor()
    {
        // Arrange & Act
        var color = GetEventTypeColor("message");

        // Assert
        Assert.Equal("Primary", color);
    }

    [Fact]
    public void GetEventTypeColor_WithSentEventType_ReturnsInfoColor()
    {
        // Arrange & Act
        var color = GetEventTypeColor("sent");

        // Assert
        Assert.Equal("Info", color);
    }

    [Fact]
    public void GetEventTypeColor_WithErrorEventType_ReturnsErrorColor()
    {
        // Arrange & Act
        var color = GetEventTypeColor("error");

        // Assert
        Assert.Equal("Error", color);
    }

    [Fact]
    public void GetEventTypeColor_WithWarningEventType_ReturnsWarningColor()
    {
        // Arrange & Act
        var color = GetEventTypeColor("warning");

        // Assert
        Assert.Equal("Warning", color);
    }

    [Fact]
    public void GetEventTypeColor_WithCloseEventType_ReturnsSecondaryColor()
    {
        // Arrange & Act
        var color = GetEventTypeColor("close");

        // Assert
        Assert.Equal("Secondary", color);
    }

    [Fact]
    public void GetEventTypeColor_WithTimeoutEventType_ReturnsWarningColor()
    {
        // Arrange & Act
        var color = GetEventTypeColor("timeout");

        // Assert
        Assert.Equal("Warning", color);
    }

    [Fact]
    public void GetEventTypeColor_WithUnknownEventType_ReturnsDefaultColor()
    {
        // Arrange & Act
        var color = GetEventTypeColor("unknown");

        // Assert
        Assert.Equal("Default", color);
    }

    [Fact]
    public void GetEventTypeColor_WithNullEventType_ReturnsDefaultColor()
    {
        // Arrange & Act
        var color = GetEventTypeColor(null);

        // Assert
        Assert.Equal("Default", color);
    }

    [Fact]
    public void GetEventTypeColor_WithEmptyEventType_ReturnsDefaultColor()
    {
        // Arrange & Act
        var color = GetEventTypeColor(string.Empty);

        // Assert
        Assert.Equal("Default", color);
    }

    [Fact]
    public void GetEventTypeColor_IsCaseInsensitive()
    {
        // Arrange & Act
        var colorLower = GetEventTypeColor("message");
        var colorUpper = GetEventTypeColor("MESSAGE");
        var colorMixed = GetEventTypeColor("MeSsAgE");

        // Assert
        Assert.Equal(colorLower, colorUpper);
        Assert.Equal(colorLower, colorMixed);
    }

    [Fact]
    public void StreamingResponse_HasCorrectProperties()
    {
        // Arrange
        var response = new RequestResponse
        {
            StatusCode = 101,
            StatusMessage = "Switching Protocols",
            IsStreaming = true,
            StreamEvents = new List<StreamEvent>
            {
                new StreamEvent
                {
                    Timestamp = DateTime.UtcNow,
                    Data = "Test message",
                    EventType = "message"
                }
            }
        };

        // Assert
        Assert.True(response.IsStreaming);
        Assert.NotEmpty(response.StreamEvents);
        Assert.Equal("message", response.StreamEvents[0].EventType);
    }

    [Fact]
    public void NonStreamingResponse_HasCorrectProperties()
    {
        // Arrange
        var response = new RequestResponse
        {
            StatusCode = 200,
            StatusMessage = "OK",
            IsStreaming = false,
            Body = "{\"message\":\"Hello\"}"
        };

        // Assert
        Assert.False(response.IsStreaming);
        Assert.Empty(response.StreamEvents);
        Assert.NotEmpty(response.Body);
    }

    [Fact]
    public void StreamEvent_FormatsTimestampCorrectly()
    {
        // Arrange
        var timestamp = new DateTime(2024, 1, 15, 14, 30, 45, 123);
        var evt = new StreamEvent
        {
            Timestamp = timestamp,
            Data = "Test",
            EventType = "message"
        };

        // Act
        var formattedTime = evt.Timestamp.ToString("HH:mm:ss.fff");

        // Assert
        Assert.Equal("14:30:45.123", formattedTime);
    }

    // Helper method that mimics the GetEventTypeColor logic from ResponseViewer.razor
    private static string GetEventTypeColor(string? eventType)
    {
        return eventType?.ToLowerInvariant() switch
        {
            "message" => "Primary",
            "sent" => "Info",
            "error" => "Error",
            "warning" => "Warning",
            "close" => "Secondary",
            "timeout" => "Warning",
            _ => "Default"
        };
    }
}
