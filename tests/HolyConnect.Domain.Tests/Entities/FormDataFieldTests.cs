using HolyConnect.Domain.Entities;

namespace HolyConnect.Domain.Tests.Entities;

public class FormDataFieldTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange & Act
        var field = new FormDataField
        {
            Key = "username",
            Value = "testuser",
            Enabled = true
        };

        // Assert
        Assert.Equal("username", field.Key);
        Assert.Equal("testuser", field.Value);
        Assert.True(field.Enabled);
    }

    [Fact]
    public void DefaultState_ShouldBeEnabled()
    {
        // Arrange & Act
        var field = new FormDataField();

        // Assert
        Assert.True(field.Enabled);
        Assert.Equal(string.Empty, field.Key);
        Assert.Equal(string.Empty, field.Value);
    }

    [Fact]
    public void Properties_ShouldBeModifiable()
    {
        // Arrange
        var field = new FormDataField { Key = "old", Value = "old" };

        // Act
        field.Key = "new";
        field.Value = "new";
        field.Enabled = false;

        // Assert
        Assert.Equal("new", field.Key);
        Assert.Equal("new", field.Value);
        Assert.False(field.Enabled);
    }
}
