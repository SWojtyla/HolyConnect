using HolyConnect.Application.Interfaces;

namespace HolyConnect.Application.Common;

/// <summary>
/// Aggregates services related to request/flow execution context.
/// Reduces constructor parameter count for services that need execution capabilities.
/// Does not include IRequestService to avoid circular dependencies.
/// </summary>
public class RequestExecutionContext
{
    public IActiveEnvironmentService ActiveEnvironment { get; }
    public IVariableResolver VariableResolver { get; }
    public IResponseValueExtractor? ResponseExtractor { get; }
    public IRequestExecutorFactory ExecutorFactory { get; }

    public RequestExecutionContext(
        IActiveEnvironmentService activeEnvironment,
        IVariableResolver variableResolver,
        IRequestExecutorFactory executorFactory,
        IResponseValueExtractor? responseExtractor = null)
    {
        ActiveEnvironment = activeEnvironment;
        VariableResolver = variableResolver;
        ExecutorFactory = executorFactory;
        ResponseExtractor = responseExtractor;
    }
}
