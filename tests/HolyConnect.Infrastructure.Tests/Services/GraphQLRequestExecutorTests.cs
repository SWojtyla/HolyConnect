using System.Net;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services;
using Moq;
using Moq.Protected;

namespace HolyConnect.Infrastructure.Tests.Services;

public class GraphQLRequestExecutorTests
{
    [Fact]
    public void CanExecute_ShouldReturnTrue_ForGraphQLRequest()
    {
        // Arrange
        var httpClient = new HttpClient();
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest();

        // Act
        var result = executor.CanExecute(request);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExecute_ShouldReturnFalse_ForNonGraphQLRequest()
    {
        // Arrange
        var httpClient = new HttpClient();
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new RestRequest();

        // Act
        var result = executor.CanExecute(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_ForNonGraphQLRequest()
    {
        // Arrange
        var httpClient = new HttpClient();
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new RestRequest();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => executor.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnResponse_WithStatusCode()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\": {\"user\": {\"name\": \"John\"}}}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            Query = "query { user { name } }"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response.Body);
        Assert.Contains("user", response.Body);
        Assert.Contains("John", response.Body);
        Assert.True(response.ResponseTime >= 0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldIncludeHeaders()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"data\": {}}")
        };
        responseMessage.Headers.Add("X-GraphQL-Version", "1.0");

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            Query = "query { test }"
        };
        request.Headers["Authorization"] = "Bearer token123";

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.True(response.Headers.ContainsKey("X-GraphQL-Version"));
        Assert.Equal("1.0", response.Headers["X-GraphQL-Version"]);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleVariables()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        HttpRequestMessage? capturedRequest = null;

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\": {}}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            Query = "query($id: ID!) { user(id: $id) { name } }",
            Variables = "{\"id\": \"123\"}"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        var content = await capturedRequest.Content!.ReadAsStringAsync();
        Assert.Contains("variables", content);
        Assert.Contains("123", content);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleOperationName()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        HttpRequestMessage? capturedRequest = null;

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\": {}}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            Query = "query GetUser { user { name } }",
            OperationName = "GetUser"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        var content = await capturedRequest.Content!.ReadAsStringAsync();
        Assert.Contains("operationName", content);
        Assert.Contains("GetUser", content);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleException()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            Query = "query { test }"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.Equal(0, response.StatusCode);
        Assert.Contains("Error", response.StatusMessage);
        Assert.Contains("Network error", response.Body);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUsePostMethod()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        HttpRequestMessage? capturedRequest = null;

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\": {}}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            Query = "query { test }"
        };

        // Act
        await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(System.Net.Http.HttpMethod.Post, capturedRequest.Method);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetContentType_ToApplicationJson()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        HttpRequestMessage? capturedRequest = null;

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\": {}}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            Query = "query { test }"
        };

        // Act
        await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Content);
        Assert.Equal("application/json", capturedRequest.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleNullVariables()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        HttpRequestMessage? capturedRequest = null;

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\": {}}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            Query = "query { test }",
            Variables = null
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        var content = await capturedRequest.Content!.ReadAsStringAsync();
        Assert.Contains("query", content);
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleEmptyVariables()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\": {}}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            Query = "query { test }",
            Variables = ""
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCaptureSentRequest()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\": {}}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            Query = "query { user { name } }",
            Variables = "{\"id\": \"123\"}"
        };
        request.Headers["Authorization"] = "Bearer token123";

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response.SentRequest);
        Assert.Equal("POST", response.SentRequest.Method);
        Assert.Equal("https://api.example.com/graphql", response.SentRequest.Url);
        Assert.NotNull(response.SentRequest.Body);
        Assert.Contains("query", response.SentRequest.Body);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldApplyBasicAuthentication_WhenBasicAuthTypeIsSet()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        HttpRequestMessage? capturedRequest = null;

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\": {}}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            Query = "query { test }",
            AuthType = AuthenticationType.Basic,
            BasicAuthUsername = "graphqluser",
            BasicAuthPassword = "graphqlpass"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("Authorization"));
        var authHeader = capturedRequest.Headers.GetValues("Authorization").First();
        Assert.StartsWith("Basic ", authHeader);
        
        // Verify the encoded credentials
        var encodedCredentials = authHeader.Substring(6);
        var decodedBytes = Convert.FromBase64String(encodedCredentials);
        var decodedCredentials = System.Text.Encoding.UTF8.GetString(decodedBytes);
        Assert.Equal("graphqluser:graphqlpass", decodedCredentials);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldApplyBearerTokenAuthentication_WhenBearerTokenTypeIsSet()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        HttpRequestMessage? capturedRequest = null;

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\": {}}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            Query = "query { test }",
            AuthType = AuthenticationType.BearerToken,
            BearerToken = "graphqltoken456"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("Authorization"));
        var authHeader = capturedRequest.Headers.GetValues("Authorization").First();
        Assert.Equal("Bearer graphqltoken456", authHeader);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotApplyAuthentication_WhenAuthTypeIsNone()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        HttpRequestMessage? capturedRequest = null;

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\": {}}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            Query = "query { test }",
            AuthType = AuthenticationType.None,
            BasicAuthUsername = "user",
            BearerToken = "token"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.False(capturedRequest.Headers.Contains("Authorization"));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPreferAuthenticationOverCustomAuthorizationHeader()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        HttpRequestMessage? capturedRequest = null;

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\": {}}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            Query = "query { test }",
            AuthType = AuthenticationType.BearerToken,
            BearerToken = "correct-token",
            Headers = { { "Authorization", "Bearer old-token" } }
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("Authorization"));
        var authHeaders = capturedRequest.Headers.GetValues("Authorization").ToList();
        
        // Should only have one Authorization header with the Bearer token from auth config
        Assert.Single(authHeaders);
        Assert.Equal("Bearer correct-token", authHeaders[0]);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAddUserAgentHeader_ByDefault()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        HttpRequestMessage? capturedRequest = null;

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"data\": {}}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new GraphQLRequestExecutor(httpClient);
        var request = new GraphQLRequest
        {
            Url = "https://api.example.com/graphql",
            Query = "query { test }"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("User-Agent"));
        var userAgent = capturedRequest.Headers.GetValues("User-Agent").First();
        Assert.Equal("HolyConnect/1.0", userAgent);
        
        // Verify it's in the sent request
        Assert.NotNull(response.SentRequest);
        Assert.True(response.SentRequest.Headers.ContainsKey("User-Agent"));
        Assert.Equal("HolyConnect/1.0", response.SentRequest.Headers["User-Agent"]);
    }
}
