using System.Text;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Infrastructure.Common;

/// <summary>
/// Helper class for building HTTP request messages
/// </summary>
public static class HttpRequestHelper
{
    /// <summary>
    /// Adds User-Agent header to an HTTP request if not disabled
    /// </summary>
    public static void AddUserAgentHeader(HttpRequestMessage httpRequest, Request request)
    {
        if (!request.DisabledHeaders.Contains(HttpConstants.Headers.UserAgent))
        {
            httpRequest.Headers.TryAddWithoutValidation(HttpConstants.Headers.UserAgent, HttpConstants.Defaults.UserAgent);
        }
    }

    /// <summary>
    /// Sets content on HTTP request with optional Content-Type header
    /// </summary>
    public static void SetContent(HttpRequestMessage httpRequest, string body, string contentType, Request request)
    {
        if (string.IsNullOrEmpty(body)) return;

        if (request.DisabledHeaders.Contains(HttpConstants.Headers.ContentType))
        {
            httpRequest.Content = new StringContent(body, Encoding.UTF8);
            httpRequest.Content.Headers.ContentType = null;
        }
        else
        {
            httpRequest.Content = new StringContent(body, Encoding.UTF8, contentType);
        }
    }

    /// <summary>
    /// Creates a SentRequest object from an HTTP request message
    /// </summary>
    public static SentRequest CreateSentRequest(HttpRequestMessage httpRequest, string url, string method, string? body = null, Dictionary<string, string>? queryParameters = null)
    {
        var sentRequest = new SentRequest
        {
            Url = httpRequest.RequestUri?.ToString() ?? url,
            Method = method,
            Headers = new Dictionary<string, string>(),
            Body = body,
            QueryParameters = queryParameters ?? new Dictionary<string, string>()
        };

        ResponseHelper.CaptureRequestHeaders(sentRequest.Headers, httpRequest);
        return sentRequest;
    }
}
