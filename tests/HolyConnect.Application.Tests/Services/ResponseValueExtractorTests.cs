using HolyConnect.Application.Services;

namespace HolyConnect.Application.Tests.Services;

public class ResponseValueExtractorTests
{
    private readonly ResponseValueExtractor _extractor;

    public ResponseValueExtractorTests()
    {
        _extractor = new ResponseValueExtractor();
    }

    #region JSON Extraction Tests

    [Fact]
    public void ExtractFromJson_WithSimpleProperty_ShouldExtractValue()
    {
        // Arrange
        var json = @"{""id"": 123, ""name"": ""John Doe""}";
        var jsonPath = "$.id";

        // Act
        var result = _extractor.ExtractFromJson(json, jsonPath);

        // Assert
        Assert.Equal("123", result);
    }

    [Fact]
    public void ExtractFromJson_WithNestedProperty_ShouldExtractValue()
    {
        // Arrange
        var json = @"{""user"": {""id"": 456, ""email"": ""john@example.com""}}";
        var jsonPath = "$.user.email";

        // Act
        var result = _extractor.ExtractFromJson(json, jsonPath);

        // Assert
        Assert.Equal("john@example.com", result);
    }

    [Fact]
    public void ExtractFromJson_WithArrayIndex_ShouldExtractValue()
    {
        // Arrange
        var json = @"{""users"": [{""id"": 1}, {""id"": 2}, {""id"": 3}]}";
        var jsonPath = "$.users[1].id";

        // Act
        var result = _extractor.ExtractFromJson(json, jsonPath);

        // Assert
        Assert.Equal("2", result);
    }

    [Fact]
    public void ExtractFromJson_WithStringValue_ShouldExtractString()
    {
        // Arrange
        var json = @"{""token"": ""abc123xyz""}";
        var jsonPath = "$.token";

        // Act
        var result = _extractor.ExtractFromJson(json, jsonPath);

        // Assert
        Assert.Equal("abc123xyz", result);
    }

    [Fact]
    public void ExtractFromJson_WithBooleanValue_ShouldExtractBoolean()
    {
        // Arrange
        var json = @"{""isActive"": true}";
        var jsonPath = "$.isActive";

        // Act
        var result = _extractor.ExtractFromJson(json, jsonPath);

        // Assert
        Assert.Equal("true", result);
    }

    [Fact]
    public void ExtractFromJson_WithFloatValue_ShouldExtractFloat()
    {
        // Arrange
        var json = @"{""price"": 19.99}";
        var jsonPath = "$.price";

        // Act
        var result = _extractor.ExtractFromJson(json, jsonPath);

        // Assert
        Assert.Equal("19.99", result);
    }

    [Fact]
    public void ExtractFromJson_WithNullValue_ShouldReturnNull()
    {
        // Arrange
        var json = @"{""value"": null}";
        var jsonPath = "$.value";

        // Act
        var result = _extractor.ExtractFromJson(json, jsonPath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractFromJson_WithInvalidPath_ShouldReturnNull()
    {
        // Arrange
        var json = @"{""id"": 123}";
        var jsonPath = "$.nonexistent.path";

        // Act
        var result = _extractor.ExtractFromJson(json, jsonPath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractFromJson_WithInvalidJson_ShouldReturnNull()
    {
        // Arrange
        var json = "invalid json";
        var jsonPath = "$.id";

        // Act
        var result = _extractor.ExtractFromJson(json, jsonPath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractFromJson_WithEmptyJson_ShouldReturnNull()
    {
        // Arrange
        var json = "";
        var jsonPath = "$.id";

        // Act
        var result = _extractor.ExtractFromJson(json, jsonPath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractFromJson_WithEmptyPath_ShouldReturnNull()
    {
        // Arrange
        var json = @"{""id"": 123}";
        var jsonPath = "";

        // Act
        var result = _extractor.ExtractFromJson(json, jsonPath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractFromJson_WithObjectValue_ShouldReturnJsonString()
    {
        // Arrange
        var json = @"{""user"": {""id"": 1, ""name"": ""John""}}";
        var jsonPath = "$.user";

        // Act
        var result = _extractor.ExtractFromJson(json, jsonPath);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("\"id\"", result);
        Assert.Contains("\"name\"", result);
    }

    [Fact]
    public void ExtractFromJson_WithArrayValue_ShouldReturnJsonString()
    {
        // Arrange
        var json = @"{""numbers"": [1, 2, 3]}";
        var jsonPath = "$.numbers";

        // Act
        var result = _extractor.ExtractFromJson(json, jsonPath);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("[", result);
        Assert.Contains("]", result);
    }

    [Fact]
    public void ExtractFromJson_GraphQLResponse_ShouldExtractValue()
    {
        // Arrange
        var json = @"{""data"": {""user"": {""id"": ""user123"", ""username"": ""johndoe""}}}";
        var jsonPath = "$.data.user.id";

        // Act
        var result = _extractor.ExtractFromJson(json, jsonPath);

        // Assert
        Assert.Equal("user123", result);
    }

    #endregion

    #region XML Extraction Tests

    [Fact]
    public void ExtractFromXml_WithSimpleElement_ShouldExtractValue()
    {
        // Arrange
        var xml = @"<root><id>123</id><name>John Doe</name></root>";
        var xpath = "//id";

        // Act
        var result = _extractor.ExtractFromXml(xml, xpath);

        // Assert
        Assert.Equal("123", result);
    }

    [Fact]
    public void ExtractFromXml_WithNestedElement_ShouldExtractValue()
    {
        // Arrange
        var xml = @"<root><user><id>456</id><email>john@example.com</email></user></root>";
        var xpath = "//user/email";

        // Act
        var result = _extractor.ExtractFromXml(xml, xpath);

        // Assert
        Assert.Equal("john@example.com", result);
    }

    [Fact]
    public void ExtractFromXml_WithAttribute_ShouldExtractValue()
    {
        // Arrange
        var xml = @"<root><user id=""789"" name=""Jane""/></root>";
        var xpath = "//user/@id";

        // Act
        var result = _extractor.ExtractFromXml(xml, xpath);

        // Assert
        Assert.Equal("789", result);
    }

    [Fact]
    public void ExtractFromXml_WithNamespace_ShouldExtractValue()
    {
        // Arrange
        var xml = @"<?xml version=""1.0""?><root><item>Value</item></root>";
        var xpath = "//item";

        // Act
        var result = _extractor.ExtractFromXml(xml, xpath);

        // Assert
        Assert.Equal("Value", result);
    }

    [Fact]
    public void ExtractFromXml_WithInvalidPath_ShouldReturnNull()
    {
        // Arrange
        var xml = @"<root><id>123</id></root>";
        var xpath = "//nonexistent";

        // Act
        var result = _extractor.ExtractFromXml(xml, xpath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractFromXml_WithInvalidXml_ShouldReturnNull()
    {
        // Arrange
        var xml = "invalid xml";
        var xpath = "//id";

        // Act
        var result = _extractor.ExtractFromXml(xml, xpath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractFromXml_WithEmptyXml_ShouldReturnNull()
    {
        // Arrange
        var xml = "";
        var xpath = "//id";

        // Act
        var result = _extractor.ExtractFromXml(xml, xpath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractFromXml_WithEmptyPath_ShouldReturnNull()
    {
        // Arrange
        var xml = @"<root><id>123</id></root>";
        var xpath = "";

        // Act
        var result = _extractor.ExtractFromXml(xml, xpath);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region ExtractValue Tests (Auto-detection)

    [Fact]
    public void ExtractValue_WithJsonContentType_ShouldUseJsonExtraction()
    {
        // Arrange
        var body = @"{""id"": 123}";
        var pattern = "$.id";
        var contentType = "application/json";

        // Act
        var result = _extractor.ExtractValue(body, pattern, contentType);

        // Assert
        Assert.Equal("123", result);
    }

    [Fact]
    public void ExtractValue_WithXmlContentType_ShouldUseXmlExtraction()
    {
        // Arrange
        var body = @"<root><id>456</id></root>";
        var pattern = "//id";
        var contentType = "application/xml";

        // Act
        var result = _extractor.ExtractValue(body, pattern, contentType);

        // Assert
        Assert.Equal("456", result);
    }

    [Fact]
    public void ExtractValue_WithGraphQLContentType_ShouldUseJsonExtraction()
    {
        // Arrange
        var body = @"{""data"": {""user"": {""id"": ""user789""}}}";
        var pattern = "$.data.user.id";
        var contentType = "application/graphql";

        // Act
        var result = _extractor.ExtractValue(body, pattern, contentType);

        // Assert
        Assert.Equal("user789", result);
    }

    [Fact]
    public void ExtractValue_WithJsonContent_ShouldAutoDetectJson()
    {
        // Arrange
        var body = @"{""id"": 999}";
        var pattern = "$.id";
        var contentType = "text/plain";

        // Act
        var result = _extractor.ExtractValue(body, pattern, contentType);

        // Assert
        Assert.Equal("999", result);
    }

    [Fact]
    public void ExtractValue_WithXmlContent_ShouldAutoDetectXml()
    {
        // Arrange
        var body = @"<root><id>888</id></root>";
        var pattern = "//id";
        var contentType = "text/plain";

        // Act
        var result = _extractor.ExtractValue(body, pattern, contentType);

        // Assert
        Assert.Equal("888", result);
    }

    [Fact]
    public void ExtractValue_WithEmptyBody_ShouldReturnNull()
    {
        // Arrange
        var body = "";
        var pattern = "$.id";
        var contentType = "application/json";

        // Act
        var result = _extractor.ExtractValue(body, pattern, contentType);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractValue_WithEmptyPattern_ShouldReturnNull()
    {
        // Arrange
        var body = @"{""id"": 123}";
        var pattern = "";
        var contentType = "application/json";

        // Act
        var result = _extractor.ExtractValue(body, pattern, contentType);

        // Assert
        Assert.Null(result);
    }

    #endregion
}
