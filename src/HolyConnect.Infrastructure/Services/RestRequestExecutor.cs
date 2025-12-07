using System.Diagnostics;
using System.Text;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Common;

namespace HolyConnect.Infrastructure.Services;

public class RestRequestExecutor : IRequestExecutor
{
    private readonly HttpClient _httpClient;

    public RestRequestExecutor(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public bool CanExecute(Request request)
    {
        return request is RestRequest;
    }

    public async Task<RequestResponse> ExecuteAsync(Request request)
    {
        if (request is not RestRequest restRequest)
        {
            throw new ArgumentException("Request must be of type RestRequest", nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        var response = new RequestResponse
        {
            Timestamp = DateTime.UtcNow
        };

        try
        {
            var httpRequest = CreateHttpRequestMessage(restRequest);
            
            // Capture the sent request details (only enabled parameters)
            var sentRequest = new SentRequest
            {
                Url = httpRequest.RequestUri?.ToString() ?? restRequest.Url,
                Method = restRequest.Method.ToString(),
                Headers = new Dictionary<string, string>(),
                Body = restRequest.Body,
                QueryParameters = GetEnabledQueryParameters(restRequest)
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

    private Dictionary<string, string> GetEnabledQueryParameters(RestRequest request)
    {
        return request.QueryParameters
            .Where(kvp => !request.DisabledQueryParameters.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private HttpRequestMessage CreateHttpRequestMessage(RestRequest request)
    {
        var url = request.Url;
        
        // Only include enabled query parameters
        var enabledQueryParams = GetEnabledQueryParameters(request);
        
        if (enabledQueryParams.Any())
        {
            var queryString = string.Join("&", enabledQueryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            url = $"{url}?{queryString}";
        }

        var httpMethod = request.Method switch
        {
            Domain.Entities.HttpMethod.Get => System.Net.Http.HttpMethod.Get,
            Domain.Entities.HttpMethod.Post => System.Net.Http.HttpMethod.Post,
            Domain.Entities.HttpMethod.Put => System.Net.Http.HttpMethod.Put,
            Domain.Entities.HttpMethod.Delete => System.Net.Http.HttpMethod.Delete,
            Domain.Entities.HttpMethod.Patch => System.Net.Http.HttpMethod.Patch,
            Domain.Entities.HttpMethod.Head => System.Net.Http.HttpMethod.Head,
            Domain.Entities.HttpMethod.Options => System.Net.Http.HttpMethod.Options,
            _ => System.Net.Http.HttpMethod.Get
        };

        var httpRequest = new HttpRequestMessage(httpMethod, url);

        // Apply authentication using helper
        HttpAuthenticationHelper.ApplyAuthentication(httpRequest, request);

        // Apply enabled headers using helper
        HttpAuthenticationHelper.ApplyHeaders(httpRequest, request);

        if (!string.IsNullOrEmpty(request.Body))
        {
            var contentType = string.IsNullOrEmpty(request.ContentType) 
                ? HttpConstants.MediaTypes.TextPlain 
                : request.ContentType;
            httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, contentType);
        }

        return httpRequest;
    }
}
