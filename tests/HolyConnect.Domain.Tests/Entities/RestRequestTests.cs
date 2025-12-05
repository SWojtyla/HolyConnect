using HolyConnect.Domain.Entities;
using HttpMethod = HolyConnect.Domain.Entities.HttpMethod;

namespace HolyConnect.Domain.Tests.Entities;

public class RestRequestTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange & Act
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Get Users",
            Url = "https://api.example.com/users",
            Method = HttpMethod.Get
        };

        // Assert
        Assert.NotEqual(Guid.Empty, request.Id);
        Assert.Equal("Get Users", request.Name);
        Assert.Equal("https://api.example.com/users", request.Url);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.NotNull(request.Headers);
        Assert.Empty(request.Headers);
    }

    [Fact]
    public void Type_ShouldBeRest()
    {
        // Arrange
        var request = new RestRequest();

        // Assert
        Assert.Equal(RequestType.Rest, request.Type);
    }

    [Fact]
    public void Headers_ShouldBeModifiable()
    {
        // Arrange
        var request = new RestRequest();

        // Act
        request.Headers["Content-Type"] = "application/json";
        request.Headers["Authorization"] = "Bearer token";

        // Assert
        Assert.Equal(2, request.Headers.Count);
        Assert.Equal("application/json", request.Headers["Content-Type"]);
        Assert.Equal("Bearer token", request.Headers["Authorization"]);
    }

    [Theory]
    [InlineData(HttpMethod.Get)]
    [InlineData(HttpMethod.Post)]
    [InlineData(HttpMethod.Put)]
    [InlineData(HttpMethod.Delete)]
    [InlineData(HttpMethod.Patch)]
    public void Method_ShouldSupportCommonHttpMethods(HttpMethod method)
    {
        // Arrange
        var request = new RestRequest { Method = method };

        // Assert
        Assert.Equal(method, request.Method);
    }

    [Fact]
    public void Body_ShouldBeSettable()
    {
        // Arrange
        var request = new RestRequest();
        var body = "{\"name\": \"John\"}";

        // Act
        request.Body = body;

        // Assert
        Assert.Equal(body, request.Body);
    }

    [Fact]
    public void DisabledHeaders_ShouldInitializeEmpty()
    {
        // Arrange & Act
        var request = new RestRequest();

        // Assert
        Assert.NotNull(request.DisabledHeaders);
        Assert.Empty(request.DisabledHeaders);
    }

    [Fact]
    public void DisabledHeaders_ShouldBeModifiable()
    {
        // Arrange
        var request = new RestRequest();
        request.Headers["Content-Type"] = "application/json";
        request.Headers["Authorization"] = "Bearer token";

        // Act
        request.DisabledHeaders.Add("Authorization");

        // Assert
        Assert.Single(request.DisabledHeaders);
        Assert.Contains("Authorization", request.DisabledHeaders);
        Assert.DoesNotContain("Content-Type", request.DisabledHeaders);
    }

    [Fact]
    public void DisabledQueryParameters_ShouldInitializeEmpty()
    {
        // Arrange & Act
        var request = new RestRequest();

        // Assert
        Assert.NotNull(request.DisabledQueryParameters);
        Assert.Empty(request.DisabledQueryParameters);
    }

    [Fact]
    public void DisabledQueryParameters_ShouldBeModifiable()
    {
        // Arrange
        var request = new RestRequest();
        request.QueryParameters["page"] = "1";
        request.QueryParameters["limit"] = "10";
        request.QueryParameters["filter"] = "active";

        // Act
        request.DisabledQueryParameters.Add("filter");

        // Assert
        Assert.Single(request.DisabledQueryParameters);
        Assert.Contains("filter", request.DisabledQueryParameters);
        Assert.DoesNotContain("page", request.DisabledQueryParameters);
        Assert.DoesNotContain("limit", request.DisabledQueryParameters);
    }

    [Fact]
    public void Request_CanTrackMultipleDisabledHeadersAndQueryParams()
    {
        // Arrange
        var request = new RestRequest();
        request.Headers["Content-Type"] = "application/json";
        request.Headers["Authorization"] = "Bearer token";
        request.Headers["Accept"] = "*/*";
        request.QueryParameters["page"] = "1";
        request.QueryParameters["limit"] = "10";

        // Act
        request.DisabledHeaders.Add("Authorization");
        request.DisabledHeaders.Add("Accept");
        request.DisabledQueryParameters.Add("limit");

        // Assert
        Assert.Equal(2, request.DisabledHeaders.Count);
        Assert.Single(request.DisabledQueryParameters);
        Assert.Contains("Authorization", request.DisabledHeaders);
        Assert.Contains("Accept", request.DisabledHeaders);
        Assert.Contains("limit", request.DisabledQueryParameters);
    }
}
