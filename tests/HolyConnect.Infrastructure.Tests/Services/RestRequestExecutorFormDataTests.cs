using System.Net;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services;
using Moq;
using Moq.Protected;

namespace HolyConnect.Infrastructure.Tests.Services;

/// <summary>
/// Dedicated tests for FormData functionality in RestRequestExecutor
/// These tests verify the complete data flow from Request entity to HTTP request
/// </summary>
public class RestRequestExecutorFormDataTests
{
    [Fact]
    public async Task ExecuteAsync_WithEmptyFormDataFieldsList_ShouldNotCreateMultipartContent()
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
                Content = new StringContent("success")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/upload",
            Method = Domain.Entities.HttpMethod.Post,
            BodyType = BodyType.FormData,
            FormDataFields = new List<FormDataField>() // Empty list
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Null(capturedRequest.Content); // No content should be created
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithOnlyDisabledFormDataFields_ShouldNotCreateMultipartContent()
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
                Content = new StringContent("success")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/upload",
            Method = Domain.Entities.HttpMethod.Post,
            BodyType = BodyType.FormData,
            FormDataFields = new List<FormDataField>
            {
                new FormDataField { Key = "field1", Value = "value1", Enabled = false },
                new FormDataField { Key = "field2", Value = "value2", Enabled = false }
            }
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Null(capturedRequest.Content); // No content should be created
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithFormDataFieldsHavingEmptyKeys_ShouldNotCreateMultipartContent()
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
                Content = new StringContent("success")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/upload",
            Method = Domain.Entities.HttpMethod.Post,
            BodyType = BodyType.FormData,
            FormDataFields = new List<FormDataField>
            {
                new FormDataField { Key = "", Value = "value1", Enabled = true },
                new FormDataField { Key = "   ", Value = "value2", Enabled = true }
            }
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Null(capturedRequest.Content); // No content - empty keys are filtered out
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithMixOfEnabledAndDisabledFields_ShouldOnlyIncludeEnabled()
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
                Content = new StringContent("success")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        var request = new RestRequest
        {
            Url = "https://api.example.com/upload",
            Method = Domain.Entities.HttpMethod.Post,
            BodyType = BodyType.FormData,
            FormDataFields = new List<FormDataField>
            {
                new FormDataField { Key = "enabled1", Value = "value1", Enabled = true },
                new FormDataField { Key = "disabled", Value = "should not appear", Enabled = false },
                new FormDataField { Key = "enabled2", Value = "value2", Enabled = true }
            }
        };

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Content);
        Assert.IsType<MultipartFormDataContent>(capturedRequest.Content);
        
        // Read the multipart content to verify
        var multipartContent = (MultipartFormDataContent)capturedRequest.Content;
        Assert.Equal(2, multipartContent.Count()); // Only 2 enabled fields
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithFormDataFieldsInitializedFromDomain_ShouldCreateMultipartContent()
    {
        // This simulates the exact scenario from the UI:
        // 1. RestRequest.FormDataFields is populated
        // 2. RestRequestEditor initializes _formDataFields from it
        // 3. User makes edits
        // 4. SyncFormDataToRequest copies back to RestRequest.FormDataFields
        // 5. ExecuteAsync reads from RestRequest.FormDataFields

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
                Content = new StringContent("success")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        
        // Step 1: Create request with FormDataFields (as loaded from repository)
        var request = new RestRequest
        {
            Url = "https://api.example.com/upload",
            Method = Domain.Entities.HttpMethod.Post,
            BodyType = BodyType.FormData,
            FormDataFields = new List<FormDataField>
            {
                new FormDataField { Key = "username", Value = "john_doe", Enabled = true },
                new FormDataField { Key = "email", Value = "john@example.com", Enabled = true }
            }
        };

        // Step 2-4 would happen in the UI (InitializeFormData, user edits, SyncFormDataToRequest)
        // We simulate the result here by ensuring FormDataFields is populated

        // Step 5: Execute the request
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Content);
        Assert.IsType<MultipartFormDataContent>(capturedRequest.Content);
        
        var multipartContent = (MultipartFormDataContent)capturedRequest.Content;
        Assert.Equal(2, multipartContent.Count());
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_AfterUIEdit_WithNewFieldAdded_ShouldIncludeNewField()
    {
        // Simulates: User adds a new field in UI, syncs to request, then executes

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
                Content = new StringContent("success")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        
        // Initial state: 1 field
        var request = new RestRequest
        {
            Url = "https://api.example.com/upload",
            Method = Domain.Entities.HttpMethod.Post,
            BodyType = BodyType.FormData,
            FormDataFields = new List<FormDataField>
            {
                new FormDataField { Key = "field1", Value = "value1", Enabled = true }
            }
        };

        // Simulate user adding a new field (this is what SyncFormDataToRequest does)
        request.FormDataFields.Add(new FormDataField 
        { 
            Key = "field2", 
            Value = "value2", 
            Enabled = true 
        });

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Content);
        Assert.IsType<MultipartFormDataContent>(capturedRequest.Content);
        
        var multipartContent = (MultipartFormDataContent)capturedRequest.Content;
        Assert.Equal(2, multipartContent.Count()); // Should include both fields
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_AfterUIEdit_WithFieldDisabled_ShouldExcludeDisabledField()
    {
        // Simulates: User disables a field in UI, syncs to request, then executes

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
                Content = new StringContent("success")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        
        // Initial state: 2 enabled fields
        var request = new RestRequest
        {
            Url = "https://api.example.com/upload",
            Method = Domain.Entities.HttpMethod.Post,
            BodyType = BodyType.FormData,
            FormDataFields = new List<FormDataField>
            {
                new FormDataField { Key = "field1", Value = "value1", Enabled = true },
                new FormDataField { Key = "field2", Value = "value2", Enabled = true }
            }
        };

        // Simulate user disabling field1
        request.FormDataFields[0].Enabled = false;

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Content);
        Assert.IsType<MultipartFormDataContent>(capturedRequest.Content);
        
        var multipartContent = (MultipartFormDataContent)capturedRequest.Content;
        Assert.Single(multipartContent); // Should only include field2
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithFormDataFieldsCleared_ShouldNotCreateMultipartContent()
    {
        // Simulates: User removes all fields, syncs (clears FormDataFields), then executes

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
                Content = new StringContent("success")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        
        var request = new RestRequest
        {
            Url = "https://api.example.com/upload",
            Method = Domain.Entities.HttpMethod.Post,
            BodyType = BodyType.FormData,
            FormDataFields = new List<FormDataField>
            {
                new FormDataField { Key = "field1", Value = "value1", Enabled = true }
            }
        };

        // Simulate user removing all fields (this is what SyncFormDataToRequest does when list is empty)
        request.FormDataFields.Clear();

        // Act
        var response = await executor.ExecuteAsync(request);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Null(capturedRequest.Content); // No multipart content
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task Debug_PrintFormDataFieldsState_BeforeExecution()
    {
        // This is a diagnostic test to help debug the issue
        // It prints the state of FormDataFields to understand what's happening

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
                Content = new StringContent("success")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var executor = new RestRequestExecutor(httpClient);
        
        var request = new RestRequest
        {
            Id = Guid.NewGuid(),
            Name = "Test Request",
            Url = "https://api.example.com/upload",
            Method = Domain.Entities.HttpMethod.Post,
            BodyType = BodyType.FormData,
            FormDataFields = new List<FormDataField>
            {
                new FormDataField { Key = "username", Value = "test_user", Enabled = true },
                new FormDataField { Key = "password", Value = "test_pass", Enabled = true }
            }
        };

        // Act - Check the state before execution
        var formDataFieldsCount = request.FormDataFields.Count;
        var enabledFieldsCount = request.FormDataFields.Count(f => f.Enabled);
        var fieldsWithValidKeys = request.FormDataFields.Count(f => !string.IsNullOrEmpty(f.Key));
        var enabledFieldsWithValidKeys = request.FormDataFields.Count(f => f.Enabled && !string.IsNullOrEmpty(f.Key));

        var response = await executor.ExecuteAsync(request);

        // Assert - This helps us understand the state
        Assert.Equal(2, formDataFieldsCount);
        Assert.Equal(2, enabledFieldsCount);
        Assert.Equal(2, fieldsWithValidKeys);
        Assert.Equal(2, enabledFieldsWithValidKeys);
        Assert.Equal(200, response.StatusCode);

        // Output for debugging (will appear in test output)
        Console.WriteLine($"FormDataFields.Count: {formDataFieldsCount}");
        Console.WriteLine($"Enabled fields: {enabledFieldsCount}");
        Console.WriteLine($"Fields with valid keys: {fieldsWithValidKeys}");
        Console.WriteLine($"Enabled fields with valid keys: {enabledFieldsWithValidKeys}");
        
        foreach (var field in request.FormDataFields)
        {
            Console.WriteLine($"  Field: Key='{field.Key}', Value='{field.Value}', Enabled={field.Enabled}");
        }
    }
}
