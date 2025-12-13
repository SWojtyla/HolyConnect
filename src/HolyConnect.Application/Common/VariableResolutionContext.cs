using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Common;

/// <summary>
/// Value object that encapsulates the context needed for variable resolution.
/// Simplifies method signatures and improves testability.
/// </summary>
public class VariableResolutionContext
{
    public Domain.Entities.Environment Environment { get; }
    public Collection? Collection { get; }
    public Request? Request { get; }

    public VariableResolutionContext(
        Domain.Entities.Environment environment,
        Collection? collection = null,
        Request? request = null)
    {
        Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        Collection = collection;
        Request = request;
    }

    /// <summary>
    /// Creates a context with only environment.
    /// </summary>
    public static VariableResolutionContext FromEnvironment(Domain.Entities.Environment environment)
    {
        return new VariableResolutionContext(environment);
    }

    /// <summary>
    /// Creates a context with environment and collection.
    /// </summary>
    public static VariableResolutionContext FromEnvironmentAndCollection(
        Domain.Entities.Environment environment,
        Collection? collection)
    {
        return new VariableResolutionContext(environment, collection);
    }

    /// <summary>
    /// Creates a context with environment, collection, and request.
    /// </summary>
    public static VariableResolutionContext FromAll(
        Domain.Entities.Environment environment,
        Collection? collection,
        Request request)
    {
        return new VariableResolutionContext(environment, collection, request);
    }
}
