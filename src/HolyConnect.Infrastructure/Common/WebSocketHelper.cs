using System.Net.WebSockets;
using System.Text;
using HolyConnect.Domain.Entities;
using Newtonsoft.Json;

namespace HolyConnect.Infrastructure.Common;

/// <summary>
/// Helper class for WebSocket operations
/// </summary>
public static class WebSocketHelper
{
    /// <summary>
    /// Safely closes a WebSocket connection if it's open
    /// </summary>
    public static async Task SafeCloseAsync(ClientWebSocket? webSocket, RequestResponse response)
    {
        if (webSocket == null) return;

        if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
        {
            try
            {
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Client closing",
                    CancellationToken.None);
            }
            catch (Exception cleanupEx)
            {
                response.StreamEvents.Add(new StreamEvent
                {
                    Timestamp = DateTime.UtcNow,
                    Data = $"Warning: Failed to close WebSocket cleanly: {cleanupEx.Message}",
                    EventType = "warning"
                });
            }
        }
    }

    /// <summary>
    /// Sends a JSON message over WebSocket
    /// </summary>
    public static async Task SendJsonMessageAsync(ClientWebSocket webSocket, object message)
    {
        var json = JsonConvert.SerializeObject(message);
        var bytes = Encoding.UTF8.GetBytes(json);
        await webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            endOfMessage: true,
            CancellationToken.None);
    }

    /// <summary>
    /// Applies custom headers to WebSocket options, tracking failures
    /// </summary>
    public static List<string> ApplyHeaders(ClientWebSocketOptions options, Request request)
    {
        var failedHeaders = new List<string>();
        
        foreach (var header in request.Headers.Where(h => !request.DisabledHeaders.Contains(h.Key)))
        {
            try
            {
                options.SetRequestHeader(header.Key, header.Value);
            }
            catch (ArgumentException ex)
            {
                // Some headers cannot be set directly (e.g., restricted headers like Host, Content-Length)
                failedHeaders.Add($"{header.Key}: {ex.Message}");
            }
        }
        
        return failedHeaders;
    }

    /// <summary>
    /// Converts HTTP/HTTPS URL to WebSocket URL (WS/WSS)
    /// </summary>
    public static string ConvertToWebSocketUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            // If not a valid absolute URI, assume it needs wss:// prefix
            return $"wss://{url}";
        }

        // Already a WebSocket URL
        if (uri.Scheme == "ws" || uri.Scheme == "wss")
        {
            return url;
        }

        // Convert HTTP to WebSocket
        if (uri.Scheme == "http")
        {
            return $"ws://{uri.Host}{(uri.IsDefaultPort ? "" : $":{uri.Port}")}{uri.PathAndQuery}";
        }

        // Convert HTTPS to secure WebSocket
        if (uri.Scheme == "https")
        {
            return $"wss://{uri.Host}{(uri.IsDefaultPort ? "" : $":{uri.Port}")}{uri.PathAndQuery}";
        }

        // Default to wss for secure connections
        return $"wss://{url}";
    }
}
