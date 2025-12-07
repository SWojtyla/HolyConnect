using HolyConnect.Domain.Entities;
using Newtonsoft.Json;

namespace HolyConnect.Infrastructure.Common;

/// <summary>
/// Helper class for GraphQL operations
/// </summary>
public static class GraphQLHelper
{
    /// <summary>
    /// Creates a GraphQL request payload from a GraphQLRequest
    /// </summary>
    public static object CreatePayload(GraphQLRequest request)
    {
        return new
        {
            query = request.Query,
            variables = string.IsNullOrEmpty(request.Variables)
                ? null
                : JsonConvert.DeserializeObject(request.Variables),
            operationName = request.OperationName
        };
    }

    /// <summary>
    /// Serializes a GraphQL payload to JSON
    /// </summary>
    public static string SerializePayload(GraphQLRequest request)
    {
        var payload = CreatePayload(request);
        return JsonConvert.SerializeObject(payload);
    }
}
