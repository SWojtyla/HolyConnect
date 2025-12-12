namespace HolyConnect.Domain.Entities;

/// <summary>
/// Represents an environment (e.g., Development, Staging, Production) with its variables.
/// Environments only store variables and are no longer parent containers for collections or requests.
/// </summary>
public class Environment
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Dictionary<string, string> Variables { get; set; } = new();
    public HashSet<string> SecretVariableNames { get; set; } = new();
    public List<DynamicVariable> DynamicVariables { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
