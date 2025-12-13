using System.Text;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Common;

namespace HolyConnect.Infrastructure.Services;

public class GraphQLSubscriptionSSEExecutor : IRequestExecutor
{
    private readonly HttpClient _httpClient;
    private const int DefaultTimeoutSeconds = 60;

    public GraphQLSubscriptionSSEExecutor(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public bool CanExecute(Request request)
    {
        return request is GraphQLRequest graphQLRequest &&
               graphQLRequest.OperationType == GraphQLOperationType.Subscription &&
               graphQLRequest.SubscriptionProtocol == GraphQLSubscriptionProtocol.ServerSentEvents;
    }

    public async Task<RequestResponse> ExecuteAsync(Request request)
    {
        if (request is not GraphQLRequest graphQLRequest)
        {
            throw new ArgumentException("Request must be of type GraphQLRequest", nameof(request));
        }

        var builder = RequestResponseBuilder.CreateStreaming();

        try
        {
            var json = GraphQLHelper.SerializePayload(graphQLRequest);
            var httpRequest = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, graphQLRequest.Url);
            
            // Set content with or without Content-Type based on DisabledHeaders
            HttpRequestHelper.SetContent(httpRequest, json, HttpConstants.MediaTypes.ApplicationJson, graphQLRequest);

            // Add User-Agent header by default
            HttpRequestHelper.AddUserAgentHeader(httpRequest, graphQLRequest);

            // Accept SSE content type
            httpRequest.Headers.TryAddWithoutValidation("Accept", "text/event-stream");

            // Apply authentication and headers using helpers
            HttpAuthenticationHelper.ApplyAuthentication(httpRequest, graphQLRequest);
            HttpAuthenticationHelper.ApplyHeaders(httpRequest, graphQLRequest);

            // Capture the sent request details
            builder.WithSentRequest(httpRequest, graphQLRequest.Url, "GRAPHQL_SUBSCRIPTION_SSE", json);

            // Send the request with ResponseHeadersRead to start reading stream immediately
            var httpResponse = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead);

            builder.StopTiming()
                .WithStatus(httpResponse)
                .WithHeaders(httpResponse.Headers)
                .WithHeaders(httpResponse.Content.Headers);

            // Read the SSE stream
            if (httpResponse.IsSuccessStatusCode)
            {
                using var stream = await httpResponse.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DefaultTimeoutSeconds));

                var eventData = new StringBuilder();
                var eventType = "message";

                while (!cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var line = await reader.ReadLineAsync();

                        if (line == null)
                            break;

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            // Empty line indicates end of event
                            if (eventData.Length > 0)
                            {
                                builder.AddStreamEvent(eventData.ToString(), eventType);
                                eventData.Clear();
                                eventType = "message";
                            }
                            continue;
                        }

                        if (line.StartsWith("event:"))
                        {
                            eventType = line.Substring(6).Trim();
                        }
                        else if (line.StartsWith("data:"))
                        {
                            var data = line.Substring(5).Trim();
                            if (eventData.Length > 0)
                                eventData.AppendLine();
                            eventData.Append(data);
                        }
                        else if (line.StartsWith(":"))
                        {
                            // Comment, ignore
                            continue;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        builder.AddStreamEvent("Timeout reached, closing connection", "timeout");
                        break;
                    }
                }

                // Add any remaining event data
                if (eventData.Length > 0)
                {
                    builder.AddStreamEvent(eventData.ToString(), eventType);
                }
            }

            // Build response body from all events
            builder.FinalizeStreaming();
        }
        catch (Exception ex)
        {
            builder.WithException(ex);
        }

        return builder.Build();
    }
}
