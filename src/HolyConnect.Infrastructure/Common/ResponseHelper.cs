using System.Net.Http.Headers;
using System.Text;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Infrastructure.Common;

/// <summary>
/// Helper class for building and populating RequestResponse objects
/// </summary>
public static class ResponseHelper
{
    /// <summary>
    /// Captures headers from HTTP response headers
    /// </summary>
    public static void CaptureHeaders(Dictionary<string, string> target, HttpResponseHeaders headers)
    {
        foreach (var header in headers)
        {
            target[header.Key] = string.Join(", ", header.Value);
        }
    }

    /// <summary>
    /// Captures headers from HTTP content headers
    /// </summary>
    public static void CaptureHeaders(Dictionary<string, string> target, HttpContentHeaders? headers)
    {
        if (headers == null) return;
        
        foreach (var header in headers)
        {
            target[header.Key] = string.Join(", ", header.Value);
        }
    }

    /// <summary>
    /// Captures headers from HTTP request message for sent request tracking
    /// </summary>
    public static void CaptureRequestHeaders(Dictionary<string, string> target, HttpRequestMessage httpRequest)
    {
        foreach (var header in httpRequest.Headers)
        {
            target[header.Key] = string.Join(", ", header.Value);
        }

        if (httpRequest.Content?.Headers != null)
        {
            foreach (var header in httpRequest.Content.Headers)
            {
                target[header.Key] = string.Join(", ", header.Value);
            }
        }
    }

    /// <summary>
    /// Builds the response body from stream events
    /// </summary>
    public static string BuildStreamEventBody(List<StreamEvent> streamEvents)
    {
        var bodyBuilder = new StringBuilder();
        foreach (var evt in streamEvents)
        {
            bodyBuilder.AppendLine($"[{evt.Timestamp:HH:mm:ss.fff}] {evt.EventType}: {evt.Data}");
        }
        return bodyBuilder.ToString();
    }

    /// <summary>
    /// Handles common exception response formatting
    /// </summary>
    public static void HandleException(RequestResponse response, Exception ex, long elapsedMilliseconds)
    {
        response.ResponseTime = elapsedMilliseconds;
        response.StatusCode = 0;
        response.StatusMessage = $"Error: {ex.Message}";
        response.Body = ex.ToString();
    }

    /// <summary>
    /// Finalizes a streaming response by building the body from events
    /// </summary>
    public static void FinalizeStreamingResponse(RequestResponse response)
    {
        response.Body = BuildStreamEventBody(response.StreamEvents);
        response.Size = response.Body.Length;
    }
}
