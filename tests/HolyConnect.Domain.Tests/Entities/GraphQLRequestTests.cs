using HolyConnect.Domain.Entities;

namespace HolyConnect.Domain.Tests.Entities;

public class GraphQLRequestTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange & Act
        var request = new GraphQLRequest
        {
            Id = Guid.NewGuid(),
            Name = "Get User Query",
            Url = "https://api.example.com/graphql",
            Query = "query { user { id name } }"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, request.Id);
        Assert.Equal("Get User Query", request.Name);
        Assert.Equal("https://api.example.com/graphql", request.Url);
        Assert.Equal("query { user { id name } }", request.Query);
    }

    [Fact]
    public void Type_ShouldBeGraphQL()
    {
        // Arrange
        var request = new GraphQLRequest();

        // Assert
        Assert.Equal(RequestType.GraphQL, request.Type);
    }

    [Fact]
    public void Variables_ShouldBeSettable()
    {
        // Arrange
        var request = new GraphQLRequest();
        var variables = "{\"userId\": \"123\"}";

        // Act
        request.Variables = variables;

        // Assert
        Assert.Equal(variables, request.Variables);
    }

    [Fact]
    public void Query_ShouldSupportMutations()
    {
        // Arrange
        var mutation = "mutation { createUser(name: \"John\") { id } }";
        var request = new GraphQLRequest { Query = mutation };

        // Assert
        Assert.Equal(mutation, request.Query);
    }

    [Fact]
    public void OperationType_ShouldDefaultToQuery()
    {
        // Arrange & Act
        var request = new GraphQLRequest();

        // Assert
        Assert.Equal(GraphQLOperationType.Query, request.OperationType);
    }

    [Fact]
    public void OperationType_ShouldBeSettable()
    {
        // Arrange
        var request = new GraphQLRequest();

        // Act
        request.OperationType = GraphQLOperationType.Subscription;

        // Assert
        Assert.Equal(GraphQLOperationType.Subscription, request.OperationType);
    }

    [Fact]
    public void SubscriptionProtocol_ShouldDefaultToWebSocket()
    {
        // Arrange & Act
        var request = new GraphQLRequest();

        // Assert
        Assert.Equal(GraphQLSubscriptionProtocol.WebSocket, request.SubscriptionProtocol);
    }

    [Fact]
    public void SubscriptionProtocol_ShouldBeSettable()
    {
        // Arrange
        var request = new GraphQLRequest();

        // Act
        request.SubscriptionProtocol = GraphQLSubscriptionProtocol.ServerSentEvents;

        // Assert
        Assert.Equal(GraphQLSubscriptionProtocol.ServerSentEvents, request.SubscriptionProtocol);
    }

    [Fact]
    public void Query_ShouldSupportSubscriptions()
    {
        // Arrange
        var subscription = "subscription { messageAdded { id content } }";
        var request = new GraphQLRequest 
        { 
            Query = subscription,
            OperationType = GraphQLOperationType.Subscription
        };

        // Assert
        Assert.Equal(subscription, request.Query);
        Assert.Equal(GraphQLOperationType.Subscription, request.OperationType);
    }
}
