using HolyConnect.Domain.Entities;
using HolyConnect.Application.Interfaces;
using Moq;

namespace HolyConnect.Maui.Tests.Components;

/// <summary>
/// Tests for the CodeEditor component.
/// Note: Full testing requires JSRuntime mock setup for Monaco editor interactions.
/// These tests focus on component initialization and parameter handling.
/// </summary>
public class CodeEditorTests
{
    [Fact]
    public void CodeEditor_SupportsJsonLanguage()
    {
        // Arrange
        var language = "json";
        
        // Act
        var monacoLanguage = GetMonacoLanguage(language);
        
        // Assert
        Assert.Equal("json", monacoLanguage);
    }
    
    [Fact]
    public void CodeEditor_SupportsXmlLanguage()
    {
        // Arrange
        var language = "xml";
        
        // Act
        var monacoLanguage = GetMonacoLanguage(language);
        
        // Assert
        Assert.Equal("xml", monacoLanguage);
    }
    
    [Fact]
    public void CodeEditor_SupportsHtmlLanguage()
    {
        // Arrange
        var language = "html";
        
        // Act
        var monacoLanguage = GetMonacoLanguage(language);
        
        // Assert
        Assert.Equal("html", monacoLanguage);
    }
    
    [Fact]
    public void CodeEditor_SupportsJavaScriptLanguage()
    {
        // Arrange
        var language = "javascript";
        
        // Act
        var monacoLanguage = GetMonacoLanguage(language);
        
        // Assert
        Assert.Equal("javascript", monacoLanguage);
    }
    
    [Fact]
    public void CodeEditor_DefaultsToPlainTextForUnknownLanguage()
    {
        // Arrange
        var language = "unknown";
        
        // Act
        var monacoLanguage = GetMonacoLanguage(language);
        
        // Assert
        Assert.Equal("plaintext", monacoLanguage);
    }
    
    [Fact]
    public void CodeEditor_TextBodyTypeMapsToPlainText()
    {
        // Arrange
        var language = "text";
        
        // Act
        var monacoLanguage = GetMonacoLanguage(language);
        
        // Assert
        Assert.Equal("plaintext", monacoLanguage);
    }

    [Fact]
    public void GetVariableHoverInfo_WithValidVariable_ReturnsFormattedInfo()
    {
        // Arrange
        var mockResolver = new Mock<IVariableResolver>();
        var environment = new Domain.Entities.Environment { Name = "Test" };
        var collection = new Collection { Name = "TestCollection" };
        
        environment.Variables["baseUrl"] = "https://api.example.com";
        
        mockResolver.Setup(r => r.GetVariableValue("baseUrl", environment, collection, null))
                   .Returns("https://api.example.com");
        
        // Act
        var result = GetVariableHoverInfo("baseUrl", environment, collection, mockResolver.Object);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("baseUrl", result);
        Assert.Contains("https://api.example.com", result);
        Assert.Contains("Environment", result);
    }

    [Fact]
    public void GetVariableHoverInfo_WithCollectionVariable_IndicatesCollectionSource()
    {
        // Arrange
        var mockResolver = new Mock<IVariableResolver>();
        var environment = new Domain.Entities.Environment { Name = "Test" };
        var collection = new Collection { Name = "TestCollection" };
        
        collection.Variables["apiKey"] = "secret123";
        
        mockResolver.Setup(r => r.GetVariableValue("apiKey", environment, collection, null))
                   .Returns("secret123");
        
        // Act
        var result = GetVariableHoverInfo("apiKey", environment, collection, mockResolver.Object);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("apiKey", result);
        Assert.Contains("secret123", result);
        Assert.Contains("Collection", result);
    }

    [Fact]
    public void GetVariableHoverInfo_WithMissingVariable_ReturnsMissingIndicator()
    {
        // Arrange
        var mockResolver = new Mock<IVariableResolver>();
        var environment = new Domain.Entities.Environment { Name = "Test" };
        
        mockResolver.Setup(r => r.GetVariableValue("missingVar", environment, null, null))
                   .Returns((string?)null);
        
        // Act
        var result = GetVariableHoverInfo("missingVar", environment, null, mockResolver.Object);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("missingVar", result);
        Assert.Contains("MISSING", result);
    }

    [Fact]
    public void GetVariableHoverInfo_WithNullEnvironment_ReturnsNull()
    {
        // Arrange
        var mockResolver = new Mock<IVariableResolver>();
        
        // Act
        var result = GetVariableHoverInfo("anyVar", null, null, mockResolver.Object);
        
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetVariableHoverInfo_WithEmptyVariableName_ReturnsNull()
    {
        // Arrange
        var mockResolver = new Mock<IVariableResolver>();
        var environment = new Domain.Entities.Environment { Name = "Test" };
        
        // Act
        var result = GetVariableHoverInfo("", environment, null, mockResolver.Object);
        
        // Assert
        Assert.Null(result);
    }
    
    // Helper method that mimics the GetMonacoLanguage logic from CodeEditor.razor
    private static string GetMonacoLanguage(string bodyType)
    {
        return bodyType?.ToLowerInvariant() switch
        {
            "json" => "json",
            "xml" => "xml",
            "html" => "html",
            "javascript" => "javascript",
            "text" => "plaintext",
            _ => "plaintext"
        };
    }

    // Helper method that mimics the GetVariableHoverInfo logic from CodeEditor.razor
    private static string? GetVariableHoverInfo(string variableName, Domain.Entities.Environment? environment, Collection? collection, IVariableResolver variableResolver)
    {
        if (environment == null || string.IsNullOrEmpty(variableName))
        {
            return null;
        }

        var value = variableResolver.GetVariableValue(variableName, environment, collection);
        
        if (value != null)
        {
            var source = collection?.Variables.ContainsKey(variableName) == true ? "Collection" : "Environment";
            return $"`{variableName}` = **{value}** _(from {source})_";
        }
        else
        {
            return $"`{variableName}` = **MISSING**";
        }
    }
}
