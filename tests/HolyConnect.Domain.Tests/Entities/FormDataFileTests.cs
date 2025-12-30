using HolyConnect.Domain.Entities;

namespace HolyConnect.Domain.Tests.Entities;

public class FormDataFileTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange & Act
        var file = new FormDataFile
        {
            Key = "document",
            FilePath = "/path/to/file.pdf",
            ContentType = "application/pdf",
            Enabled = true
        };

        // Assert
        Assert.Equal("document", file.Key);
        Assert.Equal("/path/to/file.pdf", file.FilePath);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.True(file.Enabled);
    }

    [Fact]
    public void DefaultState_ShouldBeEnabled()
    {
        // Arrange & Act
        var file = new FormDataFile();

        // Assert
        Assert.True(file.Enabled);
        Assert.Equal(string.Empty, file.Key);
        Assert.Equal(string.Empty, file.FilePath);
        Assert.Null(file.ContentType);
    }

    [Fact]
    public void Properties_ShouldBeModifiable()
    {
        // Arrange
        var file = new FormDataFile 
        { 
            Key = "old", 
            FilePath = "/old/path",
            ContentType = "text/plain"
        };

        // Act
        file.Key = "new";
        file.FilePath = "/new/path";
        file.ContentType = "image/png";
        file.Enabled = false;

        // Assert
        Assert.Equal("new", file.Key);
        Assert.Equal("/new/path", file.FilePath);
        Assert.Equal("image/png", file.ContentType);
        Assert.False(file.Enabled);
    }
}
