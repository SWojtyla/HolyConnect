using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Common;

/// <summary>
/// Helper class for ordering environments based on saved preferences.
/// </summary>
public static class EnvironmentOrderHelper
{
    /// <summary>
    /// Orders environments based on saved preferences.
    /// Handles potential duplicate IDs (which should not occur in normal operation) by taking the first occurrence.
    /// </summary>
    /// <param name="environments">The environments to order</param>
    /// <param name="order">The saved order (list of environment IDs)</param>
    /// <returns>Ordered list of environments</returns>
    public static List<Domain.Entities.Environment> OrderEnvironments(
        IEnumerable<Domain.Entities.Environment> environments, 
        List<Guid> order)
    {
        if (order == null || !order.Any())
        {
            // Default to alphabetical order by name
            return environments.OrderBy(e => e.Name).ToList();
        }
        
        // Use GroupBy to handle potential duplicate IDs (defensive programming)
        var envDict = environments.GroupBy(e => e.Id).ToDictionary(g => g.Key, g => g.First());
        var orderedEnvs = new List<Domain.Entities.Environment>();
        
        // Add environments in the saved order
        foreach (var id in order)
        {
            if (envDict.TryGetValue(id, out var env))
            {
                orderedEnvs.Add(env);
                envDict.Remove(id);
            }
        }
        
        // Add any new environments that weren't in the saved order (alphabetically)
        orderedEnvs.AddRange(envDict.Values.OrderBy(e => e.Name));
        
        return orderedEnvs;
    }
}
