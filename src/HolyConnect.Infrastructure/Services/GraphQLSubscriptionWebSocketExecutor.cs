using System.Diagnostics;
using HolyConnect.Infrastructure.Common;
using System.Net.WebSockets;
using System.Text;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HolyConnect.Infrastructure.Services;

public class GraphQLSubscriptionWebSocketExecutor : IRequestExecutor
{
    private const int MaxBufferSize = 8192;
    private const int DefaultTimeoutSeconds = 60;

    public bool CanExecute(Request request)
    {
        return request is GraphQLRequest graphQLRequest &&
               graphQLRequest.OperationType == GraphQLOperationType.Subscription &&
               graphQLRequest.SubscriptionProtocol == GraphQLSubscriptionProtocol.WebSocket;
    }

    public async Task<RequestResponse> ExecuteAsync(Request request)
    {
        if (request is not GraphQLRequest graphQLRequest)
        {
            throw new ArgumentException("Request must be of type GraphQLRequest", nameof(request));
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

            // GraphQL subscriptions use graphql-ws or graphql-transport-ws protocol
            webSocket.Options.AddSubProtocol("graphql-transport-ws");
            webSocket.Options.AddSubProtocol("graphql-ws");

            // Add User-Agent header (only if not explicitly disabled)
            if (!graphQLRequest.DisabledHeaders.Contains(HttpConstants.Headers.UserAgent))
            {
                webSocket.Options.SetRequestHeader(HttpConstants.Headers.UserAgent, HttpConstants.Defaults.UserAgent);
            }

            // Apply authentication
            HttpAuthenticationHelper.ApplyAuthentication(webSocket.Options, graphQLRequest);

            // Apply custom headers
            var failedHeaders = WebSocketHelper.ApplyHeaders(webSocket.Options, graphQLRequest);

            // Prepare payload
            var payload = GraphQLHelper.CreatePayload(graphQLRequest);
            var payloadJson = JsonConvert.SerializeObject(payload);

            // Capture sent request
            var sentRequest = new SentRequest
            {
                Url = graphQLRequest.Url,
                Method = "GRAPHQL_SUBSCRIPTION_WS",
                Headers = new Dictionary<string, string>(graphQLRequest.Headers
                    .Where(h => !graphQLRequest.DisabledHeaders.Contains(h.Key))),
                Body = payloadJson,
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

            // Convert HTTP/HTTPS URL to WS/WSS
            var wsUrl = WebSocketHelper.ConvertToWebSocketUrl(graphQLRequest.Url);
            var uri = new Uri(wsUrl);
            await webSocket.ConnectAsync(uri, CancellationToken.None);

            stopwatch.Stop();
            response.ResponseTime = stopwatch.ElapsedMilliseconds;
            response.StatusCode = 101;
            response.StatusMessage = "WebSocket connection established";

            // Send connection_init message (graphql-transport-ws protocol)
            await WebSocketHelper.SendJsonMessageAsync(webSocket, new { type = "connection_init" });
            response.StreamEvents.Add(new StreamEvent
            {
                Timestamp = DateTime.UtcNow,
                Data = "Sent: connection_init",
                EventType = "sent"
            });

            // Wait for connection_ack
            var ackReceived = await WaitForConnectionAck(webSocket, response);
            if (!ackReceived)
            {
                response.StatusMessage = "Failed to receive connection_ack";
                return response;
            }

            // Send subscribe message
            var subscribeMessage = new
            {
                id = "1",
                type = "subscribe",
                payload = payload
            };

            await WebSocketHelper.SendJsonMessageAsync(webSocket, subscribeMessage);
            response.StreamEvents.Add(new StreamEvent
            {
                Timestamp = DateTime.UtcNow,
                Data = $"Sent: subscribe with query",
                EventType = "sent"
            });

            // Receive subscription events
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DefaultTimeoutSeconds));
            var buffer = new byte[MaxBufferSize];

            while (webSocket.State == WebSocketState.Open && !cts.Token.IsCancellationRequested)
            {
                try
                {
                    var messageBuilder = new StringBuilder();
                    WebSocketReceiveResult result;

                    do
                    {
                        result = await webSocket.ReceiveAsync(
                            new ArraySegment<byte>(buffer),
                            cts.Token);

                        var messageData = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        messageBuilder.Append(messageData);
                    }
                    while (!result.EndOfMessage);

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

                    var fullMessage = messageBuilder.ToString();
                    var messageObj = JObject.Parse(fullMessage);
                    var messageType = messageObj["type"]?.ToString();

                    if (messageType == "next")
                    {
                        var data = messageObj["payload"]?.ToString(Formatting.Indented) ?? fullMessage;
                        response.StreamEvents.Add(new StreamEvent
                        {
                            Timestamp = DateTime.UtcNow,
                            Data = data,
                            EventType = "data"
                        });
                    }
                    else if (messageType == "complete")
                    {
                        response.StreamEvents.Add(new StreamEvent
                        {
                            Timestamp = DateTime.UtcNow,
                            Data = "Subscription completed",
                            EventType = "complete"
                        });
                        break;
                    }
                    else if (messageType == "error")
                    {
                        var errors = messageObj["payload"]?.ToString(Formatting.Indented) ?? fullMessage;
                        response.StreamEvents.Add(new StreamEvent
                        {
                            Timestamp = DateTime.UtcNow,
                            Data = $"Error: {errors}",
                            EventType = "error"
                        });
                    }
                    else
                    {
                        response.StreamEvents.Add(new StreamEvent
                        {
                            Timestamp = DateTime.UtcNow,
                            Data = fullMessage,
                            EventType = messageType ?? "unknown"
                        });
                    }
                }
                catch (OperationCanceledException)
                {
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
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                {
                    try
                    {
                        // Send complete message before closing
                        await WebSocketHelper.SendJsonMessageAsync(webSocket, new { id = "1", type = "complete" });
                    }
                    catch (Exception ex)
                    {
                        // Ignore errors when sending complete message, but log to response for diagnostics
                        response.StreamEvents.Add(new StreamEvent
                        {
                            Timestamp = DateTime.UtcNow,
                            Data = $"Warning: Failed to send complete message: {ex.Message}",
                            EventType = "warning"
                        });
                    }
                }
                await WebSocketHelper.SafeCloseAsync(webSocket, response);
                webSocket.Dispose();
            }
        }

        return response;
    }

    private async Task<bool> WaitForConnectionAck(ClientWebSocket webSocket, RequestResponse response)
    {
        var buffer = new byte[MaxBufferSize];
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                cts.Token);

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var messageObj = JObject.Parse(message);
            var messageType = messageObj["type"]?.ToString();

            response.StreamEvents.Add(new StreamEvent
            {
                Timestamp = DateTime.UtcNow,
                Data = $"Received: {messageType}",
                EventType = "received"
            });

            return messageType == "connection_ack";
        }
        catch
        {
            return false;
        }
    }


}
