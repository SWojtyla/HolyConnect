using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Common;

public class RequestClonerTests
{
    [Fact]
    public void Clone_RestRequest_ShouldCreateDeepCopy()
    {
        // Arrange
        var original = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Original Request",
            Description = "Test Description",
            Url = "https://api.example.com/test",
            Method = Domain.Entities.HttpMethod.Post,
            Body = "{ \"test\": \"data\" }",
            ContentType = "application/json",
            BodyType = BodyType.Json,
            QueryParameters = new Dictionary<string, string> { { "param1", "value1" } },
            DisabledQueryParameters = new HashSet<string> { "disabled1" },
            Headers = new Dictionary<string, string> { { "X-Custom", "value" } },
            DisabledHeaders = new HashSet<string> { "Disabled-Header" },
            CollectionId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            AuthType = AuthenticationType.BearerToken,
            BearerToken = "test-token",
            ResponseExtractions = new List<ResponseExtraction>
            {
                new ResponseExtraction { VariableName = "test", Pattern = "$.data" }
            }
        };

        // Act
        var clone = RequestCloner.Clone(original) as RestRequest;

        // Assert
        Assert.NotNull(clone);
        Assert.NotSame(original, clone);
        Assert.Equal(original.Id, clone!.Id);
        Assert.Equal(original.Name, clone.Name);
        Assert.Equal(original.Url, clone.Url);
        Assert.Equal(original.Method, clone.Method);
        Assert.Equal(original.Body, clone.Body);
        Assert.Equal(original.ContentType, clone.ContentType);
        
        // Verify collections are copied, not referenced
        Assert.NotSame(original.Headers, clone.Headers);
        Assert.NotSame(original.QueryParameters, clone.QueryParameters);
        Assert.NotSame(original.DisabledQueryParameters, clone.DisabledQueryParameters);
        
        // Modifying clone should not affect original
        clone.Headers["New-Header"] = "new-value";
        Assert.False(original.Headers.ContainsKey("New-Header"));
    }

    [Fact]
    public void Clone_GraphQLRequest_ShouldCreateDeepCopy()
    {
        // Arrange
        var original = new GraphQLRequest
        {
            Id = Guid.NewGuid(),
            Name = "GraphQL Query",
            Url = "https://api.example.com/graphql",
            Query = "query { user { id name } }",
            Variables = "{ \"userId\": 123 }",
            OperationName = "GetUser",
            OperationType = GraphQLOperationType.Query,
            SubscriptionProtocol = GraphQLSubscriptionProtocol.WebSocket,
            Headers = new Dictionary<string, string> { { "Authorization", "Bearer token" } },
            DisabledHeaders = new HashSet<string>(),
            CollectionId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            AuthType = AuthenticationType.None
        };

        // Act
        var clone = RequestCloner.Clone(original) as GraphQLRequest;

        // Assert
        Assert.NotNull(clone);
        Assert.NotSame(original, clone);
        Assert.Equal(original.Query, clone!.Query);
        Assert.Equal(original.Variables, clone.Variables);
        Assert.Equal(original.OperationName, clone.OperationName);
        Assert.Equal(original.OperationType, clone.OperationType);
        Assert.Equal(original.SubscriptionProtocol, clone.SubscriptionProtocol);
        
        // Verify collections are copied
        Assert.NotSame(original.Headers, clone.Headers);
    }

    [Fact]
    public void Clone_WebSocketRequest_ShouldCreateDeepCopy()
    {
        // Arrange
        var original = new WebSocketRequest
        {
            Id = Guid.NewGuid(),
            Name = "WebSocket Test",
            Url = "wss://api.example.com/ws",
            Message = "Test message",
            Protocols = new List<string> { "protocol1", "protocol2" },
            ConnectionType = WebSocketConnectionType.Standard,
            Headers = new Dictionary<string, string> { { "Sec-WebSocket-Protocol", "chat" } },
            DisabledHeaders = new HashSet<string>(),
            CollectionId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var clone = RequestCloner.Clone(original) as WebSocketRequest;

        // Assert
        Assert.NotNull(clone);
        Assert.NotSame(original, clone);
        Assert.Equal(original.Message, clone!.Message);
        Assert.Equal(original.ConnectionType, clone.ConnectionType);
        
        // Verify protocols list is copied
        Assert.NotSame(original.Protocols, clone.Protocols);
        Assert.Equal(original.Protocols.Count, clone.Protocols.Count);
        
        // Modifying clone should not affect original
        clone.Protocols.Add("protocol3");
        Assert.Equal(2, original.Protocols.Count);
        Assert.Equal(3, clone.Protocols.Count);
    }

    [Fact]
    public void Clone_UnsupportedRequestType_ShouldThrowException()
    {
        // Arrange
        var mockRequest = new Mock<Request>();
        mockRequest.Setup(r => r.Type).Returns(RequestType.GraphQL); // Type doesn't match actual class

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => RequestCloner.Clone(mockRequest.Object));
    }

    [Fact]
    public void Clone_PreservesAuthenticationFields()
    {
        // Arrange
        var original = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Auth Test",
            Url = "https://api.example.com",
            AuthType = AuthenticationType.Basic,
            BasicAuthUsername = "user@example.com",
            BasicAuthPassword = "secret123"
        };

        // Act
        var clone = RequestCloner.Clone(original) as RestRequest;

        // Assert
        Assert.NotNull(clone);
        Assert.Equal(AuthenticationType.Basic, clone!.AuthType);
        Assert.Equal("user@example.com", clone.BasicAuthUsername);
        Assert.Equal("secret123", clone.BasicAuthPassword);
    }

    [Fact]
    public void Clone_RestRequest_ShouldCopyFormDataFields()
    {
        // Arrange
        var original = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Form Data Test",
            Url = "https://api.example.com/upload",
            Method = Domain.Entities.HttpMethod.Post,
            BodyType = BodyType.FormData,
            FormDataFields = new List<FormDataField>
            {
                new FormDataField { Key = "username", Value = "john_doe", Enabled = true },
                new FormDataField { Key = "email", Value = "john@example.com", Enabled = true },
                new FormDataField { Key = "disabled", Value = "should copy", Enabled = false }
            }
        };

        // Act
        var clone = RequestCloner.Clone(original) as RestRequest;

        // Assert
        Assert.NotNull(clone);
        Assert.NotSame(original, clone);
        Assert.Equal(3, clone!.FormDataFields.Count);
        
        // Verify fields are copied, not referenced
        Assert.NotSame(original.FormDataFields, clone.FormDataFields);
        
        // Verify field values
        Assert.Equal("username", clone.FormDataFields[0].Key);
        Assert.Equal("john_doe", clone.FormDataFields[0].Value);
        Assert.True(clone.FormDataFields[0].Enabled);
        
        Assert.Equal("email", clone.FormDataFields[1].Key);
        Assert.Equal("john@example.com", clone.FormDataFields[1].Value);
        Assert.True(clone.FormDataFields[1].Enabled);
        
        Assert.Equal("disabled", clone.FormDataFields[2].Key);
        Assert.False(clone.FormDataFields[2].Enabled);
        
        // Modifying clone should not affect original
        clone.FormDataFields[0].Value = "modified";
        Assert.Equal("john_doe", original.FormDataFields[0].Value);
    }

    [Fact]
    public void Clone_RestRequest_ShouldCopyFormDataFiles()
    {
        // Arrange
        var original = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "File Upload Test",
            Url = "https://api.example.com/upload",
            Method = Domain.Entities.HttpMethod.Post,
            BodyType = BodyType.FormData,
            FormDataFiles = new List<FormDataFile>
            {
                new FormDataFile 
                { 
                    Key = "document", 
                    FilePath = "/path/to/file.pdf", 
                    ContentType = "application/pdf",
                    Enabled = true 
                },
                new FormDataFile 
                { 
                    Key = "image", 
                    FilePath = "/path/to/image.png", 
                    ContentType = "image/png",
                    Enabled = false 
                }
            }
        };

        // Act
        var clone = RequestCloner.Clone(original) as RestRequest;

        // Assert
        Assert.NotNull(clone);
        Assert.NotSame(original, clone);
        Assert.Equal(2, clone!.FormDataFiles.Count);
        
        // Verify files are copied, not referenced
        Assert.NotSame(original.FormDataFiles, clone.FormDataFiles);
        
        // Verify file values
        Assert.Equal("document", clone.FormDataFiles[0].Key);
        Assert.Equal("/path/to/file.pdf", clone.FormDataFiles[0].FilePath);
        Assert.Equal("application/pdf", clone.FormDataFiles[0].ContentType);
        Assert.True(clone.FormDataFiles[0].Enabled);
        
        Assert.Equal("image", clone.FormDataFiles[1].Key);
        Assert.False(clone.FormDataFiles[1].Enabled);
        
        // Modifying clone should not affect original
        clone.FormDataFiles[0].FilePath = "/different/path.pdf";
        Assert.Equal("/path/to/file.pdf", original.FormDataFiles[0].FilePath);
    }

    [Fact]
    public void Clone_RestRequest_WithEmptyFormData_ShouldNotThrow()
    {
        // Arrange
        var original = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Empty Form Data",
            Url = "https://api.example.com",
            Method = Domain.Entities.HttpMethod.Post,
            BodyType = BodyType.FormData,
            FormDataFields = new List<FormDataField>(),
            FormDataFiles = new List<FormDataFile>()
        };

        // Act
        var clone = RequestCloner.Clone(original) as RestRequest;

        // Assert
        Assert.NotNull(clone);
        Assert.Empty(clone!.FormDataFields);
        Assert.Empty(clone.FormDataFiles);
    }
}
