using System.Net;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services;
using Moq;
using Moq.Protected;

namespace HolyConnect.Infrastructure.Tests.Services;

public class RestRequestExecutorTests
{
    [Fact]
    public void CanExecute_ShouldReturnTrue_ForRestRequest()
    {
        // Arrange
        var httpClient = new HttpClient();
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest();

        // Act
        var result = executor.CanExecute(request);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExecute_ShouldReturnFalse_ForNonRestRequest()
    {
        // Arrange
        var httpClient = new HttpClient();
        var executor = new RestRequestExecutor(httpClient);
        var request = new GraphQLRequest();

        // Act
        var result = executor.CanExecute(request);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowException_ForNonRestRequest()
    {
        // Arrange
        var httpClient = new HttpClient();
        var executor = new RestRequestExecutor(httpClient);
        var request = new GraphQLRequest();

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
                Content = new StringContent("{\"message\": \"success\"}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.Equal(200, response.StatusCode);
        Assert.NotNull(response.Body);
        Assert.Contains("success", response.Body);
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
            Content = new StringContent("test")
        };
        responseMessage.Headers.Add("X-Custom-Header", "CustomValue");

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.True(response.Headers.ContainsKey("X-Custom-Header"));
        Assert.Equal("CustomValue", response.Headers["X-Custom-Header"]);
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
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.Equal(0, response.StatusCode);
        Assert.Contains("Error", response.StatusMessage);
        Assert.Contains("Network error", response.Body);
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
                Content = new StringContent("{\"message\": \"success\"}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Post,
            Body = "{\"test\": \"data\"}",
            Headers = { { "Authorization", "Bearer token123" } }
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response.SentRequest);
        Assert.Equal("Post", response.SentRequest.Method);
        Assert.Contains("api.example.com/test", response.SentRequest.Url);
        Assert.Equal("{\"test\": \"data\"}", response.SentRequest.Body);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCaptureSentRequestWithQueryParameters()
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
                Content = new StringContent("test")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get,
            QueryParameters = { { "page", "1" }, { "limit", "10" } }
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response.SentRequest);
        Assert.Equal(2, response.SentRequest.QueryParameters.Count);
        Assert.Equal("1", response.SentRequest.QueryParameters["page"]);
        Assert.Equal("10", response.SentRequest.QueryParameters["limit"]);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldExcludeDisabledHeaders()
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
                Content = new StringContent("test")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get,
            Headers = { { "X-Custom-Header", "value1" }, { "X-Disabled-Header", "value2" } },
            DisabledHeaders = { "X-Disabled-Header" }
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response.SentRequest);
        Assert.Contains("X-Custom-Header", response.SentRequest.Headers.Keys);
        Assert.DoesNotContain("X-Disabled-Header", response.SentRequest.Headers.Keys);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldExcludeDisabledQueryParameters()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri != null && 
                    req.RequestUri.Query.Contains("page=1") && 
                    !req.RequestUri.Query.Contains("limit")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("test")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get,
            QueryParameters = { { "page", "1" }, { "limit", "10" } },
            DisabledQueryParameters = { "limit" }
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response.SentRequest);
        Assert.Single(response.SentRequest.QueryParameters);
        Assert.Equal("1", response.SentRequest.QueryParameters["page"]);
        Assert.DoesNotContain("limit", response.SentRequest.QueryParameters.Keys);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSendAllParametersWhenNoneDisabled()
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
                Content = new StringContent("test")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get,
            Headers = { { "X-Header-1", "value1" }, { "X-Header-2", "value2" } },
            QueryParameters = { { "page", "1" }, { "limit", "10" } }
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(response.SentRequest);
        Assert.Contains("X-Header-1", response.SentRequest.Headers.Keys);
        Assert.Contains("X-Header-2", response.SentRequest.Headers.Keys);
        Assert.Equal(2, response.SentRequest.QueryParameters.Count);
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
                Content = new StringContent("test")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get,
            AuthType = AuthenticationType.Basic,
            BasicAuthUsername = "testuser",
            BasicAuthPassword = "testpass"
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
        Assert.Equal("testuser:testpass", decodedCredentials);
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
                Content = new StringContent("test")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get,
            AuthType = AuthenticationType.BearerToken,
            BearerToken = "mytoken123"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("Authorization"));
        var authHeader = capturedRequest.Headers.GetValues("Authorization").First();
        Assert.Equal("Bearer mytoken123", authHeader);
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
                Content = new StringContent("test")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get,
            AuthType = AuthenticationType.None,
            BasicAuthUsername = "testuser",
            BasicAuthPassword = "testpass",
            BearerToken = "mytoken123"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.False(capturedRequest.Headers.Contains("Authorization"));
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleBasicAuthWithEmptyPassword()
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
                Content = new StringContent("test")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get,
            AuthType = AuthenticationType.Basic,
            BasicAuthUsername = "testuser",
            BasicAuthPassword = null
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("Authorization"));
        var authHeader = capturedRequest.Headers.GetValues("Authorization").First();
        var encodedCredentials = authHeader.Substring(6);
        var decodedBytes = Convert.FromBase64String(encodedCredentials);
        var decodedCredentials = System.Text.Encoding.UTF8.GetString(decodedBytes);
        Assert.Equal("testuser:", decodedCredentials);
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
                Content = new StringContent("test")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get,
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
                Content = new StringContent("test")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get
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

    [Fact]
    public async Task ExecuteAsync_ShouldAllowUserAgentOverride()
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
                Content = new StringContent("test")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Get,
            Headers = { { "User-Agent", "CustomAgent/2.0" } }
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("User-Agent"));
        var userAgent = capturedRequest.Headers.GetValues("User-Agent").First();
        Assert.Equal("CustomAgent/2.0", userAgent);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetContentTypeBasedOnBodyType_Json()
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
                Content = new StringContent("test")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Post,
            Body = "{\"test\": \"data\"}",
            BodyType = BodyType.Json
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Content);
        Assert.Equal("application/json", capturedRequest.Content.Headers.ContentType?.MediaType);
        
        // Verify it's in the sent request
        Assert.NotNull(response.SentRequest);
        Assert.True(response.SentRequest.Headers.ContainsKey("Content-Type"));
        Assert.Contains("application/json", response.SentRequest.Headers["Content-Type"]);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSetContentTypeBasedOnBodyType_Xml()
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
                Content = new StringContent("test")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Post,
            Body = "<root><test>data</test></root>",
            BodyType = BodyType.Xml
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Content);
        Assert.Equal("application/xml", capturedRequest.Content.Headers.ContentType?.MediaType);
        
        // Verify it's in the sent request
        Assert.NotNull(response.SentRequest);
        Assert.True(response.SentRequest.Headers.ContainsKey("Content-Type"));
        Assert.Contains("application/xml", response.SentRequest.Headers["Content-Type"]);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAllowContentTypeOverride()
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
                Content = new StringContent("test")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Post,
            Body = "{\"test\": \"data\"}",
            BodyType = BodyType.Json,
            ContentType = "application/custom+json"
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Content);
        Assert.Equal("application/custom+json", capturedRequest.Content.Headers.ContentType?.MediaType);
    }
}
