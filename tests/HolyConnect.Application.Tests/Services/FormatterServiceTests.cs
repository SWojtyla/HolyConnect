using HolyConnect.Application.Services;
using Xunit;

namespace HolyConnect.Application.Tests.Services;

public class FormatterServiceTests
{
    private readonly FormatterService _formatterService;

    public FormatterServiceTests()
    {
        _formatterService = new FormatterService();
    }

    [Fact]
    public void FormatJson_WithValidJson_ShouldFormatCorrectly()
    {
        // Arrange
        var json = "{\"name\":\"test\",\"value\":123}";

        // Act
        var result = _formatterService.FormatJson(json);

        // Assert
        Assert.Contains("\"name\": \"test\"", result);
        Assert.Contains("\"value\": 123", result);
        Assert.Contains("\n", result); // Should have line breaks
    }

    [Fact]
    public void FormatJson_WithInvalidJson_ShouldReturnOriginal()
    {
        // Arrange
        var invalidJson = "{invalid json";

        // Act
        var result = _formatterService.FormatJson(invalidJson);

        // Assert
        Assert.Equal(invalidJson, result);
    }

    [Fact]
    public void FormatJson_WithEmptyString_ShouldReturnEmpty()
    {
        // Arrange
        var empty = "";

        // Act
        var result = _formatterService.FormatJson(empty);

        // Assert
        Assert.Equal(empty, result);
    }

    [Fact]
    public void FormatJson_WithNull_ShouldReturnNull()
    {
        // Act
        var result = _formatterService.FormatJson(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FormatXml_WithValidXml_ShouldFormatCorrectly()
    {
        // Arrange
        var xml = "<root><name>test</name><value>123</value></root>";

        // Act
        var result = _formatterService.FormatXml(xml);

        // Assert
        Assert.Contains("<root>", result);
        Assert.Contains("<name>test</name>", result);
        Assert.Contains("<value>123</value>", result);
        Assert.Contains("\n", result); // Should have line breaks
    }

    [Fact]
    public void FormatXml_WithInvalidXml_ShouldReturnOriginal()
    {
        // Arrange
        var invalidXml = "<root><unclosed>";

        // Act
        var result = _formatterService.FormatXml(invalidXml);

        // Assert
        Assert.Equal(invalidXml, result);
    }

    [Fact]
    public void FormatXml_WithEmptyString_ShouldReturnEmpty()
    {
        // Arrange
        var empty = "";

        // Act
        var result = _formatterService.FormatXml(empty);

        // Assert
        Assert.Equal(empty, result);
    }

    [Fact]
    public void FormatGraphQL_WithValidQuery_ShouldFormatCorrectly()
    {
        // Arrange
        var graphql = "query { user { name email } }";

        // Act
        var result = _formatterService.FormatGraphQL(graphql);

        // Assert
        Assert.Contains("query", result);
        Assert.Contains("user", result);
        Assert.Contains("{", result);
        Assert.Contains("}", result);
    }

    [Fact]
    public void FormatGraphQL_WithEmptyString_ShouldReturnEmpty()
    {
        // Arrange
        var empty = "";

        // Act
        var result = _formatterService.FormatGraphQL(empty);

        // Assert
        Assert.Equal(empty, result);
    }

    [Fact]
    public void FormatGraphQL_WithComplexQuery_ShouldIndentCorrectly()
    {
        // Arrange
        var graphql = @"query GetUser($id: ID!) {
user(id: $id) {
name
email
profile {
bio
avatar
}
}
}";

        // Act
        var result = _formatterService.FormatGraphQL(graphql);

        // Assert
        Assert.Contains("query", result);
        Assert.Contains("\n", result); // Should have line breaks for indentation
        Assert.Contains("  name", result); // Should have indentation
    }
}
