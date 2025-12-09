using System.Text.RegularExpressions;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Services;

/// <summary>
/// Service for resolving variables in request values.
/// Variables follow the pattern {{ variableName }} and are resolved from environment and collection.
/// Collection variables take precedence over environment variables.
/// Dynamic variables are generated on-the-fly if defined.
/// </summary>
public class VariableResolver : IVariableResolver
{
    private static readonly Regex VariablePattern = new(@"\{\{\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*\}\}", RegexOptions.Compiled);
    private readonly IDataGeneratorService? _dataGeneratorService;

    public VariableResolver(IDataGeneratorService? dataGeneratorService = null)
    {
        _dataGeneratorService = dataGeneratorService;
    }

    public string ResolveVariables(string input, Domain.Entities.Environment environment, Collection? collection = null, Request? request = null)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return VariablePattern.Replace(input, match =>
        {
            var variableName = match.Groups[1].Value.Trim();
            var value = GetVariableValue(variableName, environment, collection, request);
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

    public string? GetVariableValue(string variableName, Domain.Entities.Environment environment, Collection? collection = null, Request? request = null)
    {
        // Priority order: Request static variables > Collection static variables > Environment static variables
        // Then: Request dynamic variables > Collection dynamic variables > Environment dynamic variables
        
        // Check static variables first (highest priority for actual values)
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

        // Check dynamic variables (request > collection > environment)
        if (_dataGeneratorService != null)
        {
            // Check request-level dynamic variables
            var dynamicVariable = request?.DynamicVariables.FirstOrDefault(dv => dv.Name == variableName);
            
            // Check collection-level dynamic variables
            if (dynamicVariable == null)
            {
                dynamicVariable = collection?.DynamicVariables.FirstOrDefault(dv => dv.Name == variableName);
            }
            
            // Check environment-level dynamic variables
            if (dynamicVariable == null)
            {
                dynamicVariable = environment.DynamicVariables.FirstOrDefault(dv => dv.Name == variableName);
            }

            if (dynamicVariable != null)
            {
                return _dataGeneratorService.GenerateValue(dynamicVariable);
            }
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
