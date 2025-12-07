namespace HolyConnect.Domain.Entities;

public class GraphQLRequest : Request
{
    public string Query { get; set; } = string.Empty;
    public string? Variables { get; set; }
    public string? OperationName { get; set; }
    public override RequestType Type => RequestType.GraphQL;
    public GraphQLOperationType OperationType { get; set; } = GraphQLOperationType.Query;
    public GraphQLSubscriptionProtocol SubscriptionProtocol { get; set; } = GraphQLSubscriptionProtocol.WebSocket;
}

public enum GraphQLOperationType
{
    Query,
    Mutation,
    Subscription
}

public enum GraphQLSubscriptionProtocol
{
    WebSocket,
    ServerSentEvents
}
