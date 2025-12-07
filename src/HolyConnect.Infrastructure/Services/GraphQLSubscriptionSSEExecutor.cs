using System.Diagnostics;
using System.Text;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using Newtonsoft.Json;

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
            var payload = new
            {
                query = graphQLRequest.Query,
                variables = string.IsNullOrEmpty(graphQLRequest.Variables)
                    ? null
                    : JsonConvert.DeserializeObject(graphQLRequest.Variables),
                operationName = graphQLRequest.OperationName
            };

            var json = JsonConvert.SerializeObject(payload);
            var httpRequest = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, graphQLRequest.Url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // Accept SSE content type
            httpRequest.Headers.TryAddWithoutValidation("Accept", "text/event-stream");

            // Apply authentication
            ApplyAuthentication(httpRequest, graphQLRequest);

            // Only include enabled headers (skip Authorization if auth is configured)
            var skipAuthorizationHeader = graphQLRequest.AuthType != AuthenticationType.None;
            foreach (var header in graphQLRequest.Headers.Where(h => !graphQLRequest.DisabledHeaders.Contains(h.Key)))
            {
                // Skip Authorization header if authentication is configured to avoid conflicts
                if (skipAuthorizationHeader && header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Capture the sent request details
            var sentRequest = new SentRequest
            {
                Url = graphQLRequest.Url,
                Method = "GRAPHQL_SUBSCRIPTION_SSE",
                Headers = new Dictionary<string, string>(),
                Body = json,
                QueryParameters = new Dictionary<string, string>()
            };

            foreach (var header in httpRequest.Headers)
            {
                sentRequest.Headers[header.Key] = string.Join(", ", header.Value);
            }

            if (httpRequest.Content?.Headers != null)
            {
                foreach (var header in httpRequest.Content.Headers)
                {
                    sentRequest.Headers[header.Key] = string.Join(", ", header.Value);
                }
            }

            response.SentRequest = sentRequest;

            // Send the request with ResponseHeadersRead to start reading stream immediately
            var httpResponse = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead);

            stopwatch.Stop();
            response.ResponseTime = stopwatch.ElapsedMilliseconds;
            response.StatusCode = (int)httpResponse.StatusCode;
            response.StatusMessage = httpResponse.ReasonPhrase ?? string.Empty;

            foreach (var header in httpResponse.Headers)
            {
                response.Headers[header.Key] = string.Join(", ", header.Value);
            }

            if (httpResponse.Content.Headers != null)
            {
                foreach (var header in httpResponse.Content.Headers)
                {
                    response.Headers[header.Key] = string.Join(", ", header.Value);
                }
            }

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
            var bodyBuilder = new StringBuilder();
            foreach (var evt in response.StreamEvents)
            {
                bodyBuilder.AppendLine($"[{evt.Timestamp:HH:mm:ss.fff}] {evt.EventType}: {evt.Data}");
            }
            response.Body = bodyBuilder.ToString();
            response.Size = response.Body.Length;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            response.ResponseTime = stopwatch.ElapsedMilliseconds;
            response.StatusCode = 0;
            response.StatusMessage = $"Error: {ex.Message}";
            response.Body = ex.ToString();
        }

        return response;
    }

    private void ApplyAuthentication(HttpRequestMessage httpRequest, Request request)
    {
        switch (request.AuthType)
        {
            case AuthenticationType.Basic:
                if (!string.IsNullOrEmpty(request.BasicAuthUsername))
                {
                    var credentials = $"{request.BasicAuthUsername}:{request.BasicAuthPassword ?? string.Empty}";
                    var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
                    httpRequest.Headers.TryAddWithoutValidation("Authorization", $"Basic {encodedCredentials}");
                }
                break;

            case AuthenticationType.BearerToken:
                if (!string.IsNullOrEmpty(request.BearerToken))
                {
                    httpRequest.Headers.TryAddWithoutValidation("Authorization", $"Bearer {request.BearerToken}");
                }
                break;

            case AuthenticationType.None:
            default:
                // No authentication
                break;
        }
    }
}
