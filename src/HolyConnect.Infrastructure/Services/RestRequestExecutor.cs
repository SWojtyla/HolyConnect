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

        var builder = RequestResponseBuilder.Create();

        try
        {
            var httpRequest = CreateHttpRequestMessage(restRequest);
            
            // Capture the sent request details (only enabled parameters)
            builder.WithSentRequest(
                httpRequest,
                restRequest.Url,
                restRequest.Method.ToString(),
                restRequest.Body,
                GetEnabledQueryParameters(restRequest));

            var httpResponse = await _httpClient.SendAsync(httpRequest);

            builder.StopTiming()
                .WithStatus(httpResponse)
                .WithHeaders(httpResponse.Headers);

            await builder.WithBodyFromContentAsync(httpResponse.Content);
        }
        catch (Exception ex)
        {
            builder.WithException(ex);
        }

        return builder.Build();
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

        // Add User-Agent header by default (can be overridden by custom headers)
        HttpRequestHelper.AddUserAgentHeader(httpRequest, request);

        // Apply authentication using helper
        HttpAuthenticationHelper.ApplyAuthentication(httpRequest, request);

        // Apply enabled headers using helper
        HttpAuthenticationHelper.ApplyHeaders(httpRequest, request);

        // Set content based on body type
        if (request.BodyType == BodyType.FormData)
        {
            // Build multipart/form-data content
            var multipartContent = CreateMultipartFormDataContent(request);
            httpRequest.Content = multipartContent;
        }
        else if (!string.IsNullOrEmpty(request.Body))
        {
            var contentType = GetContentType(request);
            HttpRequestHelper.SetContent(httpRequest, request.Body, contentType, request);
        }

        return httpRequest;
    }

    private MultipartFormDataContent CreateMultipartFormDataContent(RestRequest request)
    {
        var multipartContent = new MultipartFormDataContent();

        // Add text fields
        foreach (var field in request.FormDataFields.Where(f => f.Enabled && !string.IsNullOrEmpty(f.Key)))
        {
            multipartContent.Add(new StringContent(field.Value), field.Key);
        }

        // Add file attachments
        foreach (var file in request.FormDataFiles.Where(f => f.Enabled && !string.IsNullOrEmpty(f.Key) && !string.IsNullOrEmpty(f.FilePath)))
        {
            if (File.Exists(file.FilePath))
            {
                // StreamContent will dispose the stream when it's disposed
                var fileStream = File.OpenRead(file.FilePath);
                var fileName = Path.GetFileName(file.FilePath);
                var fileContent = new StreamContent(fileStream);
                
                // Set content type if specified, otherwise let HttpClient infer it
                if (!string.IsNullOrEmpty(file.ContentType))
                {
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                }
                
                multipartContent.Add(fileContent, file.Key, fileName);
            }
        }

        return multipartContent;
    }

    private string GetContentType(RestRequest request)
    {
        // If user explicitly set ContentType, use it
        if (!string.IsNullOrEmpty(request.ContentType))
        {
            return request.ContentType;
        }

        // Otherwise, infer from BodyType
        return request.BodyType switch
        {
            BodyType.Json => HttpConstants.MediaTypes.ApplicationJson,
            BodyType.Xml => HttpConstants.MediaTypes.ApplicationXml,
            BodyType.Html => HttpConstants.MediaTypes.TextHtml,
            BodyType.JavaScript => HttpConstants.MediaTypes.ApplicationJavaScript,
            BodyType.Text => HttpConstants.MediaTypes.TextPlain,
            BodyType.FormData => HttpConstants.MediaTypes.MultipartFormData,
            _ => HttpConstants.MediaTypes.TextPlain
        };
    }
}
