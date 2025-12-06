using System.Xml;
using System.Xml.XPath;
using HolyConnect.Application.Interfaces;
using Newtonsoft.Json.Linq;

namespace HolyConnect.Application.Services;

/// <summary>
/// Service for extracting values from response bodies using pattern matching.
/// Supports JSONPath for JSON/GraphQL responses and XPath for XML responses.
/// </summary>
public class ResponseValueExtractor : IResponseValueExtractor
{
    public string? ExtractValue(string responseBody, string pattern, string contentType)
    {
        if (string.IsNullOrWhiteSpace(responseBody) || string.IsNullOrWhiteSpace(pattern))
        {
            return null;
        }

        // Determine format based on content type
        var contentTypeLower = contentType.ToLowerInvariant();
        
        if (contentTypeLower.Contains("json") || contentTypeLower.Contains("graphql"))
        {
            return ExtractFromJson(responseBody, pattern);
        }
        else if (contentTypeLower.Contains("xml"))
        {
            return ExtractFromXml(responseBody, pattern);
        }
        else
        {
            // Try to detect format from content
            var trimmed = responseBody.TrimStart();
            if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
            {
                return ExtractFromJson(responseBody, pattern);
            }
            else if (trimmed.StartsWith("<"))
            {
                return ExtractFromXml(responseBody, pattern);
            }
        }

        return null;
    }

    public string? ExtractFromJson(string jsonContent, string jsonPath)
    {
        if (string.IsNullOrWhiteSpace(jsonContent) || string.IsNullOrWhiteSpace(jsonPath))
        {
            return null;
        }

        try
        {
            var jToken = JToken.Parse(jsonContent);
            var selectedToken = jToken.SelectToken(jsonPath);
            
            if (selectedToken == null)
            {
                return null;
            }

            // Return the value as string
            // For primitive types, return the value directly
            // For objects/arrays, return the JSON representation
            return selectedToken.Type switch
            {
                JTokenType.String => selectedToken.Value<string>(),
                JTokenType.Integer => selectedToken.Value<long>().ToString(),
                JTokenType.Float => selectedToken.Value<double>().ToString(),
                JTokenType.Boolean => selectedToken.Value<bool>().ToString().ToLowerInvariant(),
                JTokenType.Null => null,
                _ => selectedToken.ToString() // Objects and arrays as JSON string
            };
        }
        catch
        {
            return null;
        }
    }

    public string? ExtractFromXml(string xmlContent, string xpath)
    {
        if (string.IsNullOrWhiteSpace(xmlContent) || string.IsNullOrWhiteSpace(xpath))
        {
            return null;
        }

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlContent);
            
            var navigator = doc.CreateNavigator();
            var result = navigator?.Evaluate(xpath);

            if (result == null)
            {
                return null;
            }

            // Handle different XPath result types
            if (result is XPathNodeIterator iterator)
            {
                if (iterator.MoveNext())
                {
                    return iterator.Current?.Value;
                }
            }
            else
            {
                return result.ToString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
