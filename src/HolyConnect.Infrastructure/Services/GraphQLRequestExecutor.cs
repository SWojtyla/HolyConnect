using System.Diagnostics;
using System.Text;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Common;
using Newtonsoft.Json;

namespace HolyConnect.Infrastructure.Services;

public class GraphQLRequestExecutor : IRequestExecutor
{
    private readonly HttpClient _httpClient;

    public GraphQLRequestExecutor(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public bool CanExecute(Request request)
    {
        return request is GraphQLRequest;
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
            Timestamp = DateTime.UtcNow
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
            var httpRequest = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, graphQLRequest.Url);
            
            // Set content with or without Content-Type based on DisabledHeaders
            if (graphQLRequest.DisabledHeaders.Contains(HttpConstants.Headers.ContentType))
            {
                httpRequest.Content = new StringContent(json, Encoding.UTF8);
                httpRequest.Content.Headers.ContentType = null;
            }
            else
            {
                httpRequest.Content = new StringContent(json, Encoding.UTF8, HttpConstants.MediaTypes.ApplicationJson);
            }

            // Add User-Agent header by default (can be overridden by custom headers)
            // Only add if not explicitly disabled
            if (!graphQLRequest.DisabledHeaders.Contains(HttpConstants.Headers.UserAgent))
            {
                httpRequest.Headers.TryAddWithoutValidation(HttpConstants.Headers.UserAgent, HttpConstants.Defaults.UserAgent);
            }

            // Apply authentication and headers using helpers
            HttpAuthenticationHelper.ApplyAuthentication(httpRequest, graphQLRequest);
            HttpAuthenticationHelper.ApplyHeaders(httpRequest, graphQLRequest);

            // Capture the sent request details
            var sentRequest = new SentRequest
            {
                Url = graphQLRequest.Url,
                Method = "POST",
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

            var httpResponse = await _httpClient.SendAsync(httpRequest);

            stopwatch.Stop();
            response.ResponseTime = stopwatch.ElapsedMilliseconds;
            response.StatusCode = (int)httpResponse.StatusCode;
            response.StatusMessage = httpResponse.ReasonPhrase ?? string.Empty;

            foreach (var header in httpResponse.Headers)
            {
                response.Headers[header.Key] = string.Join(", ", header.Value);
            }

            if (httpResponse.Content != null)
            {
                response.Body = await httpResponse.Content.ReadAsStringAsync();
                response.Size = response.Body.Length;

                foreach (var header in httpResponse.Content.Headers)
                {
                    response.Headers[header.Key] = string.Join(", ", header.Value);
                }
            }
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

}
