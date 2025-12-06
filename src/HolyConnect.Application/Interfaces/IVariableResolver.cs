using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Interface for resolving variables in request values.
/// Variables follow the pattern {{ variableName }} and are resolved from environment and collection.
/// </summary>
public interface IVariableResolver
{
    /// <summary>
    /// Resolves all variables in the input text using environment and collection variables.
    /// Collection variables take precedence over environment variables.
    /// </summary>
    /// <param name="input">The input text containing variable placeholders</param>
    /// <param name="environment">The environment containing variables</param>
    /// <param name="collection">Optional collection containing variables</param>
    /// <returns>The input text with all variables resolved</returns>
    string ResolveVariables(string input, Domain.Entities.Environment environment, Collection? collection = null);

    /// <summary>
    /// Checks if the input contains any variable patterns.
    /// </summary>
    /// <param name="input">The input text to check</param>
    /// <returns>True if the input contains variable patterns, false otherwise</returns>
    bool ContainsVariables(string input);

    /// <summary>
    /// Extracts all variable names from the input text.
    /// </summary>
    /// <param name="input">The input text containing variable placeholders</param>
    /// <returns>A list of variable names found in the input</returns>
    IEnumerable<string> ExtractVariableNames(string input);

    /// <summary>
    /// Gets the value of a specific variable from environment and collection.
    /// Collection variables take precedence over environment variables.
    /// </summary>
    /// <param name="variableName">The name of the variable</param>
    /// <param name="environment">The environment containing variables</param>
    /// <param name="collection">Optional collection containing variables</param>
    /// <returns>The variable value if found, null otherwise</returns>
    string? GetVariableValue(string variableName, Domain.Entities.Environment environment, Collection? collection = null);

    /// <summary>
    /// Sets a variable value in the environment or collection.
    /// </summary>
    /// <param name="variableName">The name of the variable to set</param>
    /// <param name="value">The value to set</param>
    /// <param name="environment">The environment to update</param>
    /// <param name="collection">Optional collection to update</param>
    /// <param name="saveToCollection">If true and collection is provided, save to collection; otherwise save to environment</param>
    void SetVariableValue(string variableName, string value, Domain.Entities.Environment environment, Collection? collection = null, bool saveToCollection = false);
}
