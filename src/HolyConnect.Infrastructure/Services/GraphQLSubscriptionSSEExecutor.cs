using System.Diagnostics;
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

        var stopwatch = Stopwatch.StartNew();
        var response = new RequestResponse
        {
            Timestamp = DateTime.UtcNow,
            IsStreaming = true
        };

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
            response.SentRequest = HttpRequestHelper.CreateSentRequest(
                httpRequest, 
                graphQLRequest.Url, 
                "GRAPHQL_SUBSCRIPTION_SSE", 
                json);

            // Send the request with ResponseHeadersRead to start reading stream immediately
            var httpResponse = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead);

            stopwatch.Stop();
            response.ResponseTime = stopwatch.ElapsedMilliseconds;
            response.StatusCode = (int)httpResponse.StatusCode;
            response.StatusMessage = httpResponse.ReasonPhrase ?? string.Empty;

            ResponseHelper.CaptureHeaders(response.Headers, httpResponse.Headers);
            ResponseHelper.CaptureHeaders(response.Headers, httpResponse.Content.Headers);

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
                                response.StreamEvents.Add(new StreamEvent
                                {
                                    Timestamp = DateTime.UtcNow,
                                    Data = eventData.ToString(),
                                    EventType = eventType
                                });

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
                        response.StreamEvents.Add(new StreamEvent
                        {
                            Timestamp = DateTime.UtcNow,
                            Data = "Timeout reached, closing connection",
                            EventType = "timeout"
                        });
                        break;
                    }
                }

                // Add any remaining event data
                if (eventData.Length > 0)
                {
                    response.StreamEvents.Add(new StreamEvent
                    {
                        Timestamp = DateTime.UtcNow,
                        Data = eventData.ToString(),
                        EventType = eventType
                    });
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

        return response;
    }
}
