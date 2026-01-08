using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Common;

public class VariableResolutionHelperTests
{
    private readonly Mock<IVariableResolver> _mockResolver;
    private readonly Domain.Entities.Environment _environment;
    private readonly Collection _collection;

    public VariableResolutionHelperTests()
    {
        _mockResolver = new Mock<IVariableResolver>();
        _environment = new Domain.Entities.Environment
        {
            Id = Guid.NewGuid(),
            Name = "Test Environment",
            Variables = new Dictionary<string, string>
            {
                { "baseUrl", "https://api.example.com" },
                { "apiKey", "test-key-123" }
            }
        };
        _collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test Collection",
            Variables = new Dictionary<string, string>
            {
                { "endpoint", "/users" }
            }
        };

        // Setup mock to return resolved values (simulate variable replacement)
        _mockResolver.Setup(r => r.ResolveVariables(It.IsAny<string>(), It.IsAny<Domain.Entities.Environment>(), It.IsAny<Collection>(), It.IsAny<Request>()))
            .Returns<string, Domain.Entities.Environment, Collection, Request>((input, env, coll, req) =>
            {
                // Simple mock: return the input as-is (in real scenario, it would replace {{ var }} with values)
                return input.Replace("{{ baseUrl }}", "https://api.example.com")
                           .Replace("{{ endpoint }}", "/users")
                           .Replace("{{ apiKey }}", "test-key-123");
            });
    }

    [Fact]
    public void ResolveAllVariables_RestRequest_ShouldResolveAllProperties()
    {
        // Arrange
        var request = new RestRequest
        {
            Url = "{{ baseUrl }}{{ endpoint }}",
            Method = Domain.Entities.HttpMethod.Get,
            Headers = new Dictionary<string, string>
            {
                { "X-API-Key", "{{ apiKey }}" },
                { "Content-Type", "application/json" }
            },
            QueryParameters = new Dictionary<string, string>
            {
                { "filter", "{{ baseUrl }}" }
            },
            Body = "{ \"url\": \"{{ baseUrl }}\" }",
            BasicAuthUsername = "user-{{ apiKey }}",
            BasicAuthPassword = "pass-{{ apiKey }}",
            BearerToken = "Bearer {{ apiKey }}"
        };

        // Act
        VariableResolutionHelper.ResolveAllVariables(request, _mockResolver.Object, _environment, _collection);

        // Assert
        Assert.Equal("https://api.example.com/users", request.Url);
        Assert.Equal("test-key-123", request.Headers["X-API-Key"]);
        Assert.Equal("https://api.example.com", request.QueryParameters["filter"]);
        Assert.Equal("{ \"url\": \"https://api.example.com\" }", request.Body);
        Assert.Equal("user-test-key-123", request.BasicAuthUsername);
        Assert.Equal("pass-test-key-123", request.BasicAuthPassword);
        Assert.Equal("Bearer test-key-123", request.BearerToken);
    }

    [Fact]
    public void ResolveAllVariables_GraphQLRequest_ShouldResolveGraphQLProperties()
    {
        // Arrange
        var request = new GraphQLRequest
        {
            Url = "{{ baseUrl }}/graphql",
            Query = "query { user(id: \"{{ apiKey }}\") { id name } }",
            Variables = "{ \"userId\": \"{{ apiKey }}\" }",
            OperationName = "GetUser-{{ apiKey }}",
            Headers = new Dictionary<string, string>
            {
                { "Authorization", "Bearer {{ apiKey }}" }
            }
        };

        // Act
        VariableResolutionHelper.ResolveAllVariables(request, _mockResolver.Object, _environment, _collection);

        // Assert
        Assert.Equal("https://api.example.com/graphql", request.Url);
        Assert.Contains("test-key-123", request.Query);
        Assert.Contains("test-key-123", request.Variables!);
        Assert.Contains("test-key-123", request.OperationName!);
        Assert.Equal("Bearer test-key-123", request.Headers["Authorization"]);
    }

    [Fact]
    public void ResolveAllVariables_WebSocketRequest_ShouldResolveWebSocketProperties()
    {
        // Arrange
        var request = new WebSocketRequest
        {
            Url = "wss:{{ baseUrl }}{{ endpoint }}",
            Message = "{ \"action\": \"subscribe\", \"key\": \"{{ apiKey }}\" }",
            Protocols = new List<string> { "protocol-{{ apiKey }}", "chat" },
            Headers = new Dictionary<string, string>
            {
                { "Sec-WebSocket-Protocol", "{{ apiKey }}" }
            }
        };

        // Act
        VariableResolutionHelper.ResolveAllVariables(request, _mockResolver.Object, _environment, _collection);

        // Assert
        Assert.Equal("wss:https://api.example.com/users", request.Url);
        Assert.Contains("test-key-123", request.Message);
        Assert.Equal("protocol-test-key-123", request.Protocols[0]);
        Assert.Equal("test-key-123", request.Headers["Sec-WebSocket-Protocol"]);
    }

    [Fact]
    public void ResolveAllVariables_WithNullCollection_ShouldStillResolve()
    {
        // Arrange
        var request = new RestRequest
        {
            Url = "{{ baseUrl }}/test",
            Headers = new Dictionary<string, string>
            {
                { "X-API-Key", "{{ apiKey }}" }
            }
        };

        // Act
        VariableResolutionHelper.ResolveAllVariables(request, _mockResolver.Object, _environment, null);

        // Assert
        Assert.Equal("https://api.example.com/test", request.Url);
        Assert.Equal("test-key-123", request.Headers["X-API-Key"]);
        
        // Verify resolver was called with null collection
        _mockResolver.Verify(r => r.ResolveVariables(
            It.IsAny<string>(),
            _environment,
            null,
            request), Times.AtLeastOnce());
    }

    [Fact]
    public void ResolveAllVariables_WithEmptyStrings_ShouldHandleGracefully()
    {
        // Arrange
        var request = new RestRequest
        {
            Url = "{{ baseUrl }}",
            BasicAuthUsername = "",
            BasicAuthPassword = null,
            BearerToken = "",
            Body = ""
        };

        // Act
        VariableResolutionHelper.ResolveAllVariables(request, _mockResolver.Object, _environment, _collection);

        // Assert - should not throw and should resolve URL
        Assert.Equal("https://api.example.com", request.Url);
    }

    [Fact]
    public void ResolveAllVariables_Headers_ShouldResolveKeysAndValues()
    {
        // Arrange
        var request = new RestRequest
        {
            Url = "https://example.com",
            Headers = new Dictionary<string, string>
            {
                { "X-Header-{{ apiKey }}", "value-{{ apiKey }}" },
                { "Static-Header", "static-value" }
            }
        };

        // Act
        VariableResolutionHelper.ResolveAllVariables(request, _mockResolver.Object, _environment, _collection);

        // Assert
        Assert.True(request.Headers.ContainsKey("X-Header-test-key-123"));
        Assert.Equal("value-test-key-123", request.Headers["X-Header-test-key-123"]);
        Assert.Equal("static-value", request.Headers["Static-Header"]);
    }

    [Fact]
    public void ResolveAllVariables_QueryParameters_ShouldResolveKeysAndValues()
    {
        // Arrange
        var request = new RestRequest
        {
            Url = "https://example.com",
            QueryParameters = new Dictionary<string, string>
            {
                { "key-{{ apiKey }}", "value-{{ apiKey }}" },
                { "page", "1" }
            }
        };

        // Act
        VariableResolutionHelper.ResolveAllVariables(request, _mockResolver.Object, _environment, _collection);

        // Assert
        Assert.True(request.QueryParameters.ContainsKey("key-test-key-123"));
        Assert.Equal("value-test-key-123", request.QueryParameters["key-test-key-123"]);
        Assert.Equal("1", request.QueryParameters["page"]);
    }

    [Fact]
    public void ResolveAllVariables_FormDataFields_ShouldResolveKeysAndValues()
    {
        // Arrange
        var request = new RestRequest
        {
            Url = "{{ baseUrl }}",
            BodyType = BodyType.FormData,
            FormDataFields = new List<FormDataField>
            {
                new FormDataField { Key = "username", Value = "{{ apiKey }}", Enabled = true },
                new FormDataField { Key = "field-{{ apiKey }}", Value = "value-{{ baseUrl }}", Enabled = true },
                new FormDataField { Key = "static", Value = "no-variables", Enabled = false }
            }
        };

        // Act
        VariableResolutionHelper.ResolveAllVariables(request, _mockResolver.Object, _environment, _collection);

        // Assert
        Assert.Equal(3, request.FormDataFields.Count);
        
        // First field - value should be resolved
        Assert.Equal("username", request.FormDataFields[0].Key);
        Assert.Equal("test-key-123", request.FormDataFields[0].Value);
        
        // Second field - both key and value should be resolved
        Assert.Equal("field-test-key-123", request.FormDataFields[1].Key);
        Assert.Equal("value-https://api.example.com", request.FormDataFields[1].Value);
        
        // Third field - still resolved even if disabled
        Assert.Equal("static", request.FormDataFields[2].Key);
        Assert.Equal("no-variables", request.FormDataFields[2].Value);
    }

    [Fact]
    public void ResolveAllVariables_FormDataFiles_ShouldResolveKeys()
    {
        // Arrange
        var request = new RestRequest
        {
            Url = "{{ baseUrl }}",
            BodyType = BodyType.FormData,
            FormDataFiles = new List<FormDataFile>
            {
                new FormDataFile { Key = "file-{{ apiKey }}", FilePath = "/path/to/file.txt", Enabled = true },
                new FormDataFile { Key = "avatar", FilePath = "/path/to/avatar.png", ContentType = "image/png", Enabled = true }
            }
        };

        // Act
        VariableResolutionHelper.ResolveAllVariables(request, _mockResolver.Object, _environment, _collection);

        // Assert
        Assert.Equal(2, request.FormDataFiles.Count);
        
        // First file - key should be resolved, but file path should remain unchanged
        Assert.Equal("file-test-key-123", request.FormDataFiles[0].Key);
        Assert.Equal("/path/to/file.txt", request.FormDataFiles[0].FilePath);
        
        // Second file - key should remain as-is
        Assert.Equal("avatar", request.FormDataFiles[1].Key);
        Assert.Equal("/path/to/avatar.png", request.FormDataFiles[1].FilePath);
    }

    [Fact]
    public void ResolveAllVariables_FormDataFields_WithEmptyOrNullValues_ShouldHandleGracefully()
    {
        // Arrange
        var request = new RestRequest
        {
            Url = "{{ baseUrl }}",
            BodyType = BodyType.FormData,
            FormDataFields = new List<FormDataField>
            {
                new FormDataField { Key = "key1", Value = null!, Enabled = true },
                new FormDataField { Key = "key2", Value = "", Enabled = true },
                new FormDataField { Key = "{{ apiKey }}", Value = "test", Enabled = true }
            }
        };

        // Act
        VariableResolutionHelper.ResolveAllVariables(request, _mockResolver.Object, _environment, _collection);

        // Assert - should not throw
        Assert.Equal(3, request.FormDataFields.Count);
        Assert.Equal("key1", request.FormDataFields[0].Key);
        Assert.Equal("", request.FormDataFields[0].Value); // null becomes empty string
        Assert.Equal("key2", request.FormDataFields[1].Key);
        Assert.Equal("", request.FormDataFields[1].Value);
        Assert.Equal("test-key-123", request.FormDataFields[2].Key);
        Assert.Equal("test", request.FormDataFields[2].Value);
    }

    [Fact]
    public void ResolveAllVariables_FormDataFieldsAndRegularBody_ShouldResolveFormDataFieldsOnly()
    {
        // Arrange - simulate edge case where both body and form data are set (form data takes precedence)
        var request = new RestRequest
        {
            Url = "{{ baseUrl }}",
            BodyType = BodyType.FormData,
            Body = "{{ apiKey }}", // This should still be resolved
            FormDataFields = new List<FormDataField>
            {
                new FormDataField { Key = "username", Value = "{{ apiKey }}", Enabled = true }
            }
        };

        // Act
        VariableResolutionHelper.ResolveAllVariables(request, _mockResolver.Object, _environment, _collection);

        // Assert - both body and form data should be resolved
        Assert.Equal("test-key-123", request.Body);
        Assert.Equal("test-key-123", request.FormDataFields[0].Value);
    }
}
