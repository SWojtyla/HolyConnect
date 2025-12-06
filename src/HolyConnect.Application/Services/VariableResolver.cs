using System.Text.RegularExpressions;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

/// <summary>
/// Service for resolving variables in request values.
/// Variables follow the pattern {{ variableName }} and are resolved from environment and collection.
/// Collection variables take precedence over environment variables.
/// </summary>
public class VariableResolver : IVariableResolver
{
    private static readonly Regex VariablePattern = new(@"\{\{\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*\}\}", RegexOptions.Compiled);

    public string ResolveVariables(string input, Domain.Entities.Environment environment, Collection? collection = null)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return VariablePattern.Replace(input, match =>
        {
            var variableName = match.Groups[1].Value.Trim();
            var value = GetVariableValue(variableName, environment, collection);
            return value ?? match.Value; // Keep original if not found
        });
    }

    public bool ContainsVariables(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        return VariablePattern.IsMatch(input);
    }

    public IEnumerable<string> ExtractVariableNames(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return Enumerable.Empty<string>();
        }

        var matches = VariablePattern.Matches(input);
        return matches.Select(m => m.Groups[1].Value.Trim()).Distinct();
    }

    public string? GetVariableValue(string variableName, Domain.Entities.Environment environment, Collection? collection = null)
    {
        // Collection variables take precedence
        if (collection?.Variables.TryGetValue(variableName, out var collectionValue) == true)
        {
            return collectionValue;
        }

        // Fall back to environment variables
        if (environment.Variables.TryGetValue(variableName, out var environmentValue))
        {
            return environmentValue;
        }

        return null;
    }

    public void SetVariableValue(string variableName, string value, Domain.Entities.Environment environment, Collection? collection = null, bool saveToCollection = false)
    {
        if (saveToCollection && collection != null)
        {
            collection.Variables[variableName] = value;
        }
        else
        {
            environment.Variables[variableName] = value;
        }
    }
}
