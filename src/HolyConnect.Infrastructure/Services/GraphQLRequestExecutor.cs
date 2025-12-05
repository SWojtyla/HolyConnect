using System.Diagnostics;
using System.Text;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
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
            var httpRequest = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, graphQLRequest.Url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // Apply authentication
            ApplyAuthentication(httpRequest, graphQLRequest);

            foreach (var header in graphQLRequest.Headers)
            {
                httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

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
