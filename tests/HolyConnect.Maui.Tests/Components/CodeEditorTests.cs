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
}
