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

            foreach (var header in graphQLRequest.Headers)
            {
                httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

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
