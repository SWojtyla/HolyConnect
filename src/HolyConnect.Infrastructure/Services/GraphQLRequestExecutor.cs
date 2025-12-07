using System.Diagnostics;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Common;

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
            var json = GraphQLHelper.SerializePayload(graphQLRequest);
            var httpRequest = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, graphQLRequest.Url);
            
            // Set content with or without Content-Type based on DisabledHeaders
            HttpRequestHelper.SetContent(httpRequest, json, HttpConstants.MediaTypes.ApplicationJson, graphQLRequest);

            // Add User-Agent header by default
            HttpRequestHelper.AddUserAgentHeader(httpRequest, graphQLRequest);

            // Apply authentication and headers using helpers
            HttpAuthenticationHelper.ApplyAuthentication(httpRequest, graphQLRequest);
            HttpAuthenticationHelper.ApplyHeaders(httpRequest, graphQLRequest);

            // Capture the sent request details
            response.SentRequest = HttpRequestHelper.CreateSentRequest(httpRequest, graphQLRequest.Url, "POST", json);

            var httpResponse = await _httpClient.SendAsync(httpRequest);

            stopwatch.Stop();
            response.ResponseTime = stopwatch.ElapsedMilliseconds;
            response.StatusCode = (int)httpResponse.StatusCode;
            response.StatusMessage = httpResponse.ReasonPhrase ?? string.Empty;

            ResponseHelper.CaptureHeaders(response.Headers, httpResponse.Headers);

            if (httpResponse.Content != null)
            {
                response.Body = await httpResponse.Content.ReadAsStringAsync();
                response.Size = response.Body.Length;
                ResponseHelper.CaptureHeaders(response.Headers, httpResponse.Content.Headers);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ResponseHelper.HandleException(response, ex, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }

}
