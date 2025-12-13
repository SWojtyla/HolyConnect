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

        var builder = RequestResponseBuilder.CreateStreaming();
        ClientWebSocket? webSocket = null;
        RequestResponse? response = null;

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

            builder.WithSentRequest(sentRequest);

            // Add warning about failed headers if any
            if (failedHeaders.Count > 0)
            {
                builder.AddStreamEvent(
                    $"Warning: Failed to set {failedHeaders.Count} header(s): {string.Join(", ", failedHeaders)}",
                    "warning");
            }

            // Convert HTTP/HTTPS URL to WS/WSS
            var wsUrl = WebSocketHelper.ConvertToWebSocketUrl(graphQLRequest.Url);
            var uri = new Uri(wsUrl);
            await webSocket.ConnectAsync(uri, CancellationToken.None);

            builder.StopTiming()
                .WithStatus(101, "WebSocket connection established");

            // Send connection_init message (graphql-transport-ws protocol)
            await WebSocketHelper.SendJsonMessageAsync(webSocket, new { type = "connection_init" });
            builder.AddStreamEvent("Sent: connection_init", "sent");

            // Wait for connection_ack
            var ackReceived = await WaitForConnectionAck(webSocket, builder);
            if (!ackReceived)
            {
                builder.WithStatus(101, "Failed to receive connection_ack");
                return builder.Build();
            }

            // Send subscribe message
            var subscribeMessage = new
            {
                id = "1",
                type = "subscribe",
                payload = payload
            };

            await WebSocketHelper.SendJsonMessageAsync(webSocket, subscribeMessage);
            builder.AddStreamEvent($"Sent: subscribe with query", "sent");

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

                        builder.AddStreamEvent("Connection closed by server", "close");
                        break;
                    }

                    var fullMessage = messageBuilder.ToString();
                    var messageObj = JObject.Parse(fullMessage);
                    var messageType = messageObj["type"]?.ToString();

                    if (messageType == "next")
                    {
                        var data = messageObj["payload"]?.ToString(Formatting.Indented) ?? fullMessage;
                        builder.AddStreamEvent(data, "data");
                    }
                    else if (messageType == "complete")
                    {
                        builder.AddStreamEvent("Subscription completed", "complete");
                        break;
                    }
                    else if (messageType == "error")
                    {
                        var errors = messageObj["payload"]?.ToString(Formatting.Indented) ?? fullMessage;
                        builder.AddStreamEvent($"Error: {errors}", "error");
                    }
                    else
                    {
                        builder.AddStreamEvent(fullMessage, messageType ?? "unknown");
                    }
                }
                catch (OperationCanceledException)
                {
                    builder.AddStreamEvent("Timeout reached, closing connection", "timeout");
                    break;
                }
            }

            // Build response body from all events
            builder.FinalizeStreaming();
        }
        catch (Exception ex)
        {
            builder.WithException(ex);
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
                        builder.AddStreamEvent($"Warning: Failed to send complete message: {ex.Message}", "warning");
                    }
                }
                
                response = builder.Build();
                await WebSocketHelper.SafeCloseAsync(webSocket, response);
                webSocket.Dispose();
            }
        }

        return response ?? builder.Build();
    }

    private async Task<bool> WaitForConnectionAck(ClientWebSocket webSocket, RequestResponseBuilder builder)
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

            builder.AddStreamEvent($"Received: {messageType}", "received");

            return messageType == "connection_ack";
        }
        catch
        {
            return false;
        }
    }


}
