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
}
