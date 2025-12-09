using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Service for generating dynamic/fake test data based on variable definitions.
/// </summary>
public interface IDataGeneratorService
{
    /// <summary>
    /// Generates a value for a dynamic variable based on its type and constraints.
    /// </summary>
    /// <param name="dynamicVariable">The dynamic variable definition</param>
    /// <returns>Generated string value</returns>
    string GenerateValue(DynamicVariable dynamicVariable);

    /// <summary>
    /// Validates that a dynamic variable's configuration is valid.
    /// </summary>
    /// <param name="dynamicVariable">The dynamic variable to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool ValidateConfiguration(DynamicVariable dynamicVariable);
}
