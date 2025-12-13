namespace HolyConnect.Application.Common;

/// <summary>
/// Helper class for HTTP response status code operations.
/// </summary>
public static class HttpStatusCodeHelper
{
    /// <summary>
    /// Checks if an HTTP status code indicates success (2xx range).
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check</param>
    /// <returns>True if the status code is in the success range (200-299), false otherwise</returns>
    public static bool IsSuccessStatusCode(int statusCode)
    {
        return statusCode >= 200 && statusCode <= 299;
    }

    /// <summary>
    /// Checks if an HTTP status code indicates a client error (4xx range).
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check</param>
    /// <returns>True if the status code is in the client error range (400-499), false otherwise</returns>
    public static bool IsClientErrorStatusCode(int statusCode)
    {
        return statusCode >= 400 && statusCode <= 499;
    }

    /// <summary>
    /// Checks if an HTTP status code indicates a server error (5xx range).
    /// </summary>
    /// <param name="statusCode">The HTTP status code to check</param>
    /// <returns>True if the status code is in the server error range (500-599), false otherwise</returns>
    public static bool IsServerErrorStatusCode(int statusCode)
    {
        return statusCode >= 500 && statusCode <= 599;
    }
}
