using System.Net;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services;
using Moq;
using Moq.Protected;

namespace HolyConnect.Infrastructure.Tests.Services;

public class GraphQLSchemaServiceTests
{
    [Fact]
    public async Task FetchSchemaAsync_WithValidUrl_ShouldReturnSchema()
    {
        // Arrange
        var mockSchema = @"{""data"":{""__schema"":{""queryType"":{""name"":""Query""}}}}";
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockSchema)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new GraphQLSchemaService(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql"
        };

        // Act
        var result = await service.FetchSchemaAsync(request.Url, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(mockSchema, result);
    }

    [Fact]
    public async Task FetchSchemaAsync_WithEmptyUrl_ShouldReturnNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new GraphQLSchemaService(httpClient);
        var request = new GraphQLRequest();

        // Act
        var result = await service.FetchSchemaAsync(string.Empty, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchSchemaAsync_WithFailedRequest_ShouldReturnNull()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new GraphQLSchemaService(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql"
        };

        // Act
        var result = await service.FetchSchemaAsync(request.Url, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchSchemaAsync_ShouldCacheSchema()
    {
        // Arrange
        var mockSchema = @"{""data"":{""__schema"":{""queryType"":{""name"":""Query""}}}}";
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockSchema)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new GraphQLSchemaService(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql"
        };

        // Act
        var firstResult = await service.FetchSchemaAsync(request.Url, request);
        var cachedResult = service.GetCachedSchema(request.Url);

        // Assert
        Assert.NotNull(firstResult);
        Assert.NotNull(cachedResult);
        Assert.Equal(firstResult, cachedResult);
    }

    [Fact]
    public void GetCachedSchema_WithNonExistentUrl_ShouldReturnNull()
    {
        // Arrange
        var httpClient = new HttpClient();
        var service = new GraphQLSchemaService(httpClient);

        // Act
        var result = service.GetCachedSchema("https://nonexistent.com/graphql");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ClearCache_ShouldRemoveCachedSchema()
    {
        // Arrange
        var mockSchema = @"{""data"":{""__schema"":{""queryType"":{""name"":""Query""}}}}";
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockSchema)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new GraphQLSchemaService(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql"
        };

        // Act
        await service.FetchSchemaAsync(request.Url, request);
        service.ClearCache(request.Url);
        var cachedResult = service.GetCachedSchema(request.Url);

        // Assert
        Assert.Null(cachedResult);
    }

    [Fact]
    public async Task ClearAllCaches_ShouldRemoveAllCachedSchemas()
    {
        // Arrange
        var mockSchema = @"{""data"":{""__schema"":{""queryType"":{""name"":""Query""}}}}";
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockSchema)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new GraphQLSchemaService(httpClient);
        var request1 = new GraphQLRequest
        {
            Url = "https://api1.example.com/graphql"
        };
        var request2 = new GraphQLRequest
        {
            Url = "https://api2.example.com/graphql"
        };

        // Act
        await service.FetchSchemaAsync(request1.Url, request1);
        await service.FetchSchemaAsync(request2.Url, request2);
        service.ClearAllCaches();
        var cachedResult1 = service.GetCachedSchema(request1.Url);
        var cachedResult2 = service.GetCachedSchema(request2.Url);

        // Assert
        Assert.Null(cachedResult1);
        Assert.Null(cachedResult2);
    }

    [Fact]
    public async Task FetchSchemaAsync_WithBasicAuth_ShouldIncludeAuthorizationHeader()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var mockSchema = @"{""data"":{""__schema"":{""queryType"":{""name"":""Query""}}}}";
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockSchema)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new GraphQLSchemaService(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            AuthType = AuthenticationType.Basic,
            BasicAuthUsername = "user",
            BasicAuthPassword = "pass"
        };

        // Act
        await service.FetchSchemaAsync(request.Url, request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("Authorization"));
        var authHeader = capturedRequest.Headers.GetValues("Authorization").First();
        Assert.StartsWith("Basic ", authHeader);
    }

    [Fact]
    public async Task FetchSchemaAsync_WithBearerToken_ShouldIncludeAuthorizationHeader()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var mockSchema = @"{""data"":{""__schema"":{""queryType"":{""name"":""Query""}}}}";
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockSchema)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new GraphQLSchemaService(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            AuthType = AuthenticationType.BearerToken,
            BearerToken = "test-token"
        };

        // Act
        await service.FetchSchemaAsync(request.Url, request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("Authorization"));
        var authHeader = capturedRequest.Headers.GetValues("Authorization").First();
        Assert.Equal("Bearer test-token", authHeader);
    }
}
