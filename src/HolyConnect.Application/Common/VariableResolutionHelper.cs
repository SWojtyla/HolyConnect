using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Common;

/// <summary>
/// Helper class for resolving variables in Request objects
/// </summary>
public static class VariableResolutionHelper
{
    /// <summary>
    /// Resolves all variables in a request using environment and collection variables
    /// </summary>
    public static void ResolveAllVariables(Request request, IVariableResolver variableResolver, Domain.Entities.Environment environment, Collection? collection)
    {
        // Resolve common properties
        ResolveCommonProperties(request, variableResolver, environment, collection);

        // Resolve request-specific properties
        switch (request)
        {
            case RestRequest restRequest:
                ResolveRestRequestProperties(restRequest, variableResolver, environment, collection);
                break;
            case GraphQLRequest graphQLRequest:
                ResolveGraphQLRequestProperties(graphQLRequest, variableResolver, environment, collection);
                break;
            case WebSocketRequest webSocketRequest:
                ResolveWebSocketRequestProperties(webSocketRequest, variableResolver, environment, collection);
                break;
        }
    }

    private static void ResolveCommonProperties(Request request, IVariableResolver variableResolver, Domain.Entities.Environment environment, Collection? collection)
    {
        // Resolve URL
        request.Url = variableResolver.ResolveVariables(request.Url, environment, collection, request);

        // Resolve headers
        var resolvedHeaders = new Dictionary<string, string>();
        foreach (var header in request.Headers)
        {
            var resolvedKey = variableResolver.ResolveVariables(header.Key, environment, collection, request);
            var resolvedValue = variableResolver.ResolveVariables(header.Value, environment, collection, request);
            resolvedHeaders[resolvedKey] = resolvedValue;
        }
        request.Headers = resolvedHeaders;

        // Resolve authentication fields
        if (!string.IsNullOrEmpty(request.BasicAuthUsername))
        {
            request.BasicAuthUsername = variableResolver.ResolveVariables(request.BasicAuthUsername, environment, collection, request);
        }
        if (!string.IsNullOrEmpty(request.BasicAuthPassword))
        {
            request.BasicAuthPassword = variableResolver.ResolveVariables(request.BasicAuthPassword, environment, collection, request);
        }
        if (!string.IsNullOrEmpty(request.BearerToken))
        {
            request.BearerToken = variableResolver.ResolveVariables(request.BearerToken, environment, collection, request);
        }
    }

    private static void ResolveRestRequestProperties(RestRequest request, IVariableResolver variableResolver, Domain.Entities.Environment environment, Collection? collection)
    {
        // Resolve body
        if (!string.IsNullOrEmpty(request.Body))
        {
            request.Body = variableResolver.ResolveVariables(request.Body, environment, collection, request);
        }

        // Resolve query parameters
        var resolvedQueryParams = new Dictionary<string, string>();
        foreach (var param in request.QueryParameters)
        {
            var resolvedKey = variableResolver.ResolveVariables(param.Key, environment, collection, request);
            var resolvedValue = variableResolver.ResolveVariables(param.Value, environment, collection, request);
            resolvedQueryParams[resolvedKey] = resolvedValue;
        }
        request.QueryParameters = resolvedQueryParams;
    }

    private static void ResolveGraphQLRequestProperties(GraphQLRequest request, IVariableResolver variableResolver, Domain.Entities.Environment environment, Collection? collection)
    {
        // Resolve query
        if (!string.IsNullOrEmpty(request.Query))
        {
            request.Query = variableResolver.ResolveVariables(request.Query, environment, collection, request);
        }

        // Resolve variables
        if (!string.IsNullOrEmpty(request.Variables))
        {
            request.Variables = variableResolver.ResolveVariables(request.Variables, environment, collection, request);
        }

        // Resolve operation name
        if (!string.IsNullOrEmpty(request.OperationName))
        {
            request.OperationName = variableResolver.ResolveVariables(request.OperationName, environment, collection, request);
        }
    }

    private static void ResolveWebSocketRequestProperties(WebSocketRequest request, IVariableResolver variableResolver, Domain.Entities.Environment environment, Collection? collection)
    {
        // Resolve message
        if (!string.IsNullOrEmpty(request.Message))
        {
            request.Message = variableResolver.ResolveVariables(request.Message, environment, collection, request);
        }

        // Resolve protocols
        var resolvedProtocols = new List<string>();
        foreach (var protocol in request.Protocols)
        {
            resolvedProtocols.Add(variableResolver.ResolveVariables(protocol, environment, collection, request));
        }
        request.Protocols = resolvedProtocols;
    }
}
