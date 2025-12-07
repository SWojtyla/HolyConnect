using System.Text;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Infrastructure.Common;

/// <summary>
/// Helper class for applying authentication to HTTP requests
/// </summary>
public static class HttpAuthenticationHelper
{
    /// <summary>
    /// Applies authentication to an HTTP request message based on the request's authentication type
    /// </summary>
    /// <param name="httpRequest">The HTTP request message to apply authentication to</param>
    /// <param name="request">The domain request containing authentication details</param>
    public static void ApplyAuthentication(HttpRequestMessage httpRequest, Request request)
    {
        switch (request.AuthType)
        {
            case AuthenticationType.Basic:
                ApplyBasicAuthentication(httpRequest, request);
                break;

            case AuthenticationType.BearerToken:
                ApplyBearerTokenAuthentication(httpRequest, request);
                break;

            case AuthenticationType.None:
            default:
                // No authentication required
                break;
        }
    }

    /// <summary>
    /// Applies authentication to WebSocket options based on the request's authentication type
    /// </summary>
    /// <param name="options">The WebSocket client options to apply authentication to</param>
    /// <param name="request">The domain request containing authentication details</param>
    public static void ApplyAuthentication(System.Net.WebSockets.ClientWebSocketOptions options, Request request)
    {
        switch (request.AuthType)
        {
            case AuthenticationType.Basic:
                if (!string.IsNullOrEmpty(request.BasicAuthUsername))
                {
                    var credentials = $"{request.BasicAuthUsername}:{request.BasicAuthPassword ?? string.Empty}";
                    var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
                    options.SetRequestHeader(HttpConstants.Headers.Authorization, 
                        $"{HttpConstants.Authentication.BasicScheme} {encodedCredentials}");
                }
                break;

            case AuthenticationType.BearerToken:
                if (!string.IsNullOrEmpty(request.BearerToken))
                {
                    options.SetRequestHeader(HttpConstants.Headers.Authorization, 
                        $"{HttpConstants.Authentication.BearerScheme} {request.BearerToken}");
                }
                break;

            case AuthenticationType.None:
            default:
                // No authentication required
                break;
        }
    }

    /// <summary>
    /// Checks if a header should be skipped because authentication is configured
    /// </summary>
    /// <param name="headerKey">The header key to check</param>
    /// <param name="request">The request containing authentication configuration</param>
    /// <returns>True if the header should be skipped, false otherwise</returns>
    public static bool ShouldSkipAuthorizationHeader(string headerKey, Request request)
    {
        return request.AuthType != AuthenticationType.None &&
               headerKey.Equals(HttpConstants.Headers.Authorization, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Applies enabled headers to an HTTP request, optionally skipping the Authorization header
    /// </summary>
    /// <param name="httpRequest">The HTTP request message to add headers to</param>
    /// <param name="request">The domain request containing headers</param>
    public static void ApplyHeaders(HttpRequestMessage httpRequest, Request request)
    {
        foreach (var header in request.Headers.Where(h => !request.DisabledHeaders.Contains(h.Key)))
        {
            // Skip Authorization header if authentication is configured to avoid conflicts
            if (ShouldSkipAuthorizationHeader(header.Key, request))
            {
                continue;
            }
            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    private static void ApplyBasicAuthentication(HttpRequestMessage httpRequest, Request request)
    {
        if (!string.IsNullOrEmpty(request.BasicAuthUsername))
        {
            var credentials = $"{request.BasicAuthUsername}:{request.BasicAuthPassword ?? string.Empty}";
            var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
            httpRequest.Headers.TryAddWithoutValidation(HttpConstants.Headers.Authorization, 
                $"{HttpConstants.Authentication.BasicScheme} {encodedCredentials}");
        }
    }

    private static void ApplyBearerTokenAuthentication(HttpRequestMessage httpRequest, Request request)
    {
        if (!string.IsNullOrEmpty(request.BearerToken))
        {
            httpRequest.Headers.TryAddWithoutValidation(HttpConstants.Headers.Authorization, 
                $"{HttpConstants.Authentication.BearerScheme} {request.BearerToken}");
        }
    }
}
