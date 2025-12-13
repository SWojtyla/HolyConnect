using HolyConnect.Application.Interfaces;

namespace HolyConnect.Application.Common;

/// <summary>
/// Helper class for handling secret variable operations in entities that support secret variables.
/// </summary>
public static class SecretVariableHelper
{
    /// <summary>
    /// Represents the result of separating secret and non-secret variables.
    /// </summary>
    public class SeparatedVariables
    {
        public Dictionary<string, string> SecretVariables { get; set; } = new();
        public Dictionary<string, string> NonSecretVariables { get; set; } = new();
    }

    /// <summary>
    /// Separates variables into secret and non-secret dictionaries based on the provided secret variable names.
    /// </summary>
    /// <param name="variables">All variables to separate</param>
    /// <param name="secretVariableNames">Set of variable names that should be treated as secret</param>
    /// <returns>A SeparatedVariables object containing both dictionaries</returns>
    public static SeparatedVariables SeparateVariables(
        Dictionary<string, string> variables,
        HashSet<string> secretVariableNames)
    {
        var result = new SeparatedVariables();

        foreach (var variable in variables)
        {
            if (secretVariableNames.Contains(variable.Key))
            {
                result.SecretVariables[variable.Key] = variable.Value;
            }
            else
            {
                result.NonSecretVariables[variable.Key] = variable.Value;
            }
        }

        return result;
    }

    /// <summary>
    /// Merges secret variables back into the entity's variables dictionary.
    /// </summary>
    /// <param name="targetVariables">The dictionary to merge secrets into</param>
    /// <param name="secretVariables">The secret variables to merge</param>
    public static void MergeSecretVariables(
        Dictionary<string, string> targetVariables,
        Dictionary<string, string> secretVariables)
    {
        foreach (var secret in secretVariables)
        {
            targetVariables[secret.Key] = secret.Value;
        }
    }

    /// <summary>
    /// Loads secret variables from the service and merges them into the entity's variables.
    /// </summary>
    /// <param name="entityId">ID of the entity (environment or collection)</param>
    /// <param name="targetVariables">The dictionary to merge secrets into</param>
    /// <param name="loadSecretsFunc">Function to load secrets from the service</param>
    public static async Task LoadAndMergeSecretsAsync(
        Guid entityId,
        Dictionary<string, string> targetVariables,
        Func<Guid, Task<Dictionary<string, string>>> loadSecretsFunc)
    {
        var secrets = await loadSecretsFunc(entityId);
        MergeSecretVariables(targetVariables, secrets);
    }
}
