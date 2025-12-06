namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Service for extracting values from response bodies using pattern matching.
/// Supports JSONPath for JSON/GraphQL responses and XPath for XML responses.
/// </summary>
public interface IResponseValueExtractor
{
    /// <summary>
    /// Extracts a value from a response body using the specified pattern.
    /// </summary>
    /// <param name="responseBody">The response body content</param>
    /// <param name="pattern">The extraction pattern (JSONPath or XPath)</param>
    /// <param name="contentType">The content type of the response (to determine format)</param>
    /// <returns>The extracted value as a string, or null if extraction fails</returns>
    string? ExtractValue(string responseBody, string pattern, string contentType);

    /// <summary>
    /// Extracts a value from JSON content using JSONPath.
    /// </summary>
    /// <param name="jsonContent">The JSON content</param>
    /// <param name="jsonPath">The JSONPath expression (e.g., "$.data.user.id")</param>
    /// <returns>The extracted value as a string, or null if extraction fails</returns>
    string? ExtractFromJson(string jsonContent, string jsonPath);

    /// <summary>
    /// Extracts a value from XML content using XPath.
    /// </summary>
    /// <param name="xmlContent">The XML content</param>
    /// <param name="xpath">The XPath expression (e.g., "//user/id")</param>
    /// <returns>The extracted value as a string, or null if extraction fails</returns>
    string? ExtractFromXml(string xmlContent, string xpath);
}
