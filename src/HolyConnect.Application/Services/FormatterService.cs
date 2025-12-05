using System.Text;
using System.Xml;
using System.Xml.Linq;
using HolyConnect.Application.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HolyConnect.Application.Services;

public class FormatterService : IFormatterService
{
    public string FormatJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        try
        {
            var parsedJson = JToken.Parse(json);
            return parsedJson.ToString(Newtonsoft.Json.Formatting.Indented);
        }
        catch
        {
            return json;
        }
    }

    public string FormatXml(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return string.Empty;
        }

        try
        {
            var doc = XDocument.Parse(xml);
            var stringBuilder = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Replace,
                OmitXmlDeclaration = false
            };

            using (var writer = XmlWriter.Create(stringBuilder, settings))
            {
                doc.Save(writer);
            }

            return stringBuilder.ToString();
        }
        catch
        {
            return xml;
        }
    }

    public string FormatGraphQL(string? graphql)
    {
        if (string.IsNullOrWhiteSpace(graphql))
        {
            return string.Empty;
        }

        // Basic GraphQL formatting
        try
        {
            var lines = graphql.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var formatted = new StringBuilder();
            var indentLevel = 0;
            var indent = "  ";

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    continue;
                }

                // Decrease indent for closing braces
                if (trimmedLine.StartsWith("}"))
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                }

                // Add indented line
                formatted.AppendLine(new string(' ', indentLevel * indent.Length) + trimmedLine);

                // Increase indent for opening braces
                if (trimmedLine.EndsWith("{"))
                {
                    indentLevel++;
                }
            }

            return formatted.ToString().TrimEnd();
        }
        catch
        {
            return graphql;
        }
    }
}
