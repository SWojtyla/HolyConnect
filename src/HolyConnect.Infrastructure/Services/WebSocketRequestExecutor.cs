using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Common;

namespace HolyConnect.Infrastructure.Services;

public class WebSocketRequestExecutor : IRequestExecutor
{
    private const int MaxBufferSize = 4096;
    private const int DefaultTimeoutSeconds = 30;

    public bool CanExecute(Request request)
    {
        return request is WebSocketRequest wsRequest && 
               wsRequest.ConnectionType == WebSocketConnectionType.Standard;
    }

    public async Task<RequestResponse> ExecuteAsync(Request request)
    {
        if (request is not WebSocketRequest webSocketRequest)
        {
            throw new ArgumentException("Request must be of type WebSocketRequest", nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        var response = new RequestResponse
        {
            Timestamp = DateTime.UtcNow,
            IsStreaming = true
        };

        ClientWebSocket? webSocket = null;

        try
        {
            webSocket = new ClientWebSocket();

            // Add protocols
            foreach (var protocol in webSocketRequest.Protocols.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                webSocket.Options.AddSubProtocol(protocol);
            }

            // Add User-Agent header (only if not explicitly disabled)
            if (!webSocketRequest.DisabledHeaders.Contains(HttpConstants.Headers.UserAgent))
            {
                webSocket.Options.SetRequestHeader(HttpConstants.Headers.UserAgent, HttpConstants.Defaults.UserAgent);
            }

            // Apply authentication using helper
            HttpAuthenticationHelper.ApplyAuthentication(webSocket.Options, webSocketRequest);

            // Apply custom headers
            var failedHeaders = WebSocketHelper.ApplyHeaders(webSocket.Options, webSocketRequest);

            // Capture sent request
            var sentRequest = new SentRequest
            {
                Url = webSocketRequest.Url,
                Method = "WEBSOCKET",
                Headers = new Dictionary<string, string>(webSocketRequest.Headers
                    .Where(h => !webSocketRequest.DisabledHeaders.Contains(h.Key))),
                Body = webSocketRequest.Message,
                QueryParameters = new Dictionary<string, string>()
            };

            response.SentRequest = sentRequest;

            // Add warning about failed headers if any
            if (failedHeaders.Count > 0)
            {
                response.StreamEvents.Add(new StreamEvent
                {
                    Timestamp = DateTime.UtcNow,
                    Data = $"Warning: Failed to set {failedHeaders.Count} header(s): {string.Join(", ", failedHeaders)}",
                    EventType = "warning"
                });
            }

            // Connect
            var uri = new Uri(webSocketRequest.Url);
            await webSocket.ConnectAsync(uri, CancellationToken.None);

            stopwatch.Stop();
            response.ResponseTime = stopwatch.ElapsedMilliseconds;
            response.StatusCode = 101; // Switching Protocols
            response.StatusMessage = "WebSocket connection established";

            // Send message if provided
            if (!string.IsNullOrEmpty(webSocketRequest.Message))
            {
                var messageBytes = Encoding.UTF8.GetBytes(webSocketRequest.Message);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(messageBytes),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    CancellationToken.None);

                response.StreamEvents.Add(new StreamEvent
                {
                    Timestamp = DateTime.UtcNow,
                    Data = $"Sent: {webSocketRequest.Message}",
                    EventType = "sent"
                });
            }

            // Receive messages for a limited time or until connection closes
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DefaultTimeoutSeconds));
            var buffer = new byte[MaxBufferSize];
            var messageBuilder = new StringBuilder();

            while (webSocket.State == WebSocketState.Open && !cts.Token.IsCancellationRequested)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        cts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Client closing",
                            CancellationToken.None);

                        response.StreamEvents.Add(new StreamEvent
                        {
                            Timestamp = DateTime.UtcNow,
                            Data = "Connection closed by server",
                            EventType = "close"
                        });
                        break;
                    }

                    var messageData = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    messageBuilder.Append(messageData);

                    if (result.EndOfMessage)
                    {
                        var fullMessage = messageBuilder.ToString();
                        response.StreamEvents.Add(new StreamEvent
                        {
                            Timestamp = DateTime.UtcNow,
                            Data = fullMessage,
                            EventType = "message"
                        });

                        messageBuilder.Clear();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Timeout reached
                    response.StreamEvents.Add(new StreamEvent
                    {
                        Timestamp = DateTime.UtcNow,
                        Data = "Timeout reached, closing connection",
                        EventType = "timeout"
                    });
                    break;
                }
            }

            // Build response body from all events
            ResponseHelper.FinalizeStreamingResponse(response);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ResponseHelper.HandleException(response, ex, stopwatch.ElapsedMilliseconds);
        }
        finally
        {
            if (webSocket != null)
            {
                await WebSocketHelper.SafeCloseAsync(webSocket, response);
                webSocket.Dispose();
            }
        }

        return response;
    }

}
