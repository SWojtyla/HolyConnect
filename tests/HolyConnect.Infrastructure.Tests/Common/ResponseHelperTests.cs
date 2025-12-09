using HolyConnect.Infrastructure.Common;

namespace HolyConnect.Infrastructure.Tests.Common;

public class ResponseHelperTests
{
    [Theory]
    [InlineData(200, true)]
    [InlineData(201, true)]
    [InlineData(204, true)]
    [InlineData(299, true)]
    [InlineData(199, false)]
    [InlineData(300, false)]
    [InlineData(400, false)]
    [InlineData(404, false)]
    [InlineData(500, false)]
    [InlineData(0, false)]
    public void IsSuccessStatusCode_ShouldReturnCorrectValue(int statusCode, bool expectedResult)
    {
        // Act
        var result = ResponseHelper.IsSuccessStatusCode(statusCode);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void IsSuccessStatusCode_WithMinSuccessCode_ShouldReturnTrue()
    {
        // Arrange
        var statusCode = 200;

        // Act
        var result = ResponseHelper.IsSuccessStatusCode(statusCode);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSuccessStatusCode_WithMaxSuccessCode_ShouldReturnTrue()
    {
        // Arrange
        var statusCode = 299;

        // Act
        var result = ResponseHelper.IsSuccessStatusCode(statusCode);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsSuccessStatusCode_WithJustBelowSuccessRange_ShouldReturnFalse()
    {
        // Arrange
        var statusCode = 199;

        // Act
        var result = ResponseHelper.IsSuccessStatusCode(statusCode);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsSuccessStatusCode_WithJustAboveSuccessRange_ShouldReturnFalse()
    {
        // Arrange
        var statusCode = 300;

        // Act
        var result = ResponseHelper.IsSuccessStatusCode(statusCode);

        // Assert
        Assert.False(result);
    }
}
