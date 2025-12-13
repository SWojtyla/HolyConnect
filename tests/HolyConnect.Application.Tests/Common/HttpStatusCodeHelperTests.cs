using HolyConnect.Application.Common;

namespace HolyConnect.Application.Tests.Common;

public class HttpStatusCodeHelperTests
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
    public void IsSuccessStatusCode_ShouldReturnCorrectResult(int statusCode, bool expected)
    {
        // Act
        var result = HttpStatusCodeHelper.IsSuccessStatusCode(statusCode);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(400, true)]
    [InlineData(404, true)]
    [InlineData(499, true)]
    [InlineData(399, false)]
    [InlineData(500, false)]
    [InlineData(200, false)]
    public void IsClientErrorStatusCode_ShouldReturnCorrectResult(int statusCode, bool expected)
    {
        // Act
        var result = HttpStatusCodeHelper.IsClientErrorStatusCode(statusCode);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(500, true)]
    [InlineData(502, true)]
    [InlineData(503, true)]
    [InlineData(599, true)]
    [InlineData(499, false)]
    [InlineData(600, false)]
    [InlineData(200, false)]
    [InlineData(400, false)]
    public void IsServerErrorStatusCode_ShouldReturnCorrectResult(int statusCode, bool expected)
    {
        // Act
        var result = HttpStatusCodeHelper.IsServerErrorStatusCode(statusCode);

        // Assert
        Assert.Equal(expected, result);
    }
}
