using HolyConnect.Domain.Entities;

namespace HolyConnect.Domain.Tests.Entities;

public class ImportResultTests
{
    [Fact]
    public void ImportResult_CanBeCreated()
    {
        // Act
        var result = new ImportResult();

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.ImportedRequest);
        Assert.NotNull(result.Warnings);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void ImportResult_SuccessPropertyCanBeSet()
    {
        // Arrange
        var result = new ImportResult();

        // Act
        result.Success = true;

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public void ImportResult_ErrorMessageCanBeSet()
    {
        // Arrange
        var result = new ImportResult();
        var errorMessage = "Test error message";

        // Act
        result.ErrorMessage = errorMessage;

        // Assert
        Assert.Equal(errorMessage, result.ErrorMessage);
    }

    [Fact]
    public void ImportResult_ImportedRequestCanBeSet()
    {
        // Arrange
        var result = new ImportResult();
        var request = new RestRequest { Name = "Test Request" };

        // Act
        result.ImportedRequest = request;

        // Assert
        Assert.Equal(request, result.ImportedRequest);
    }

    [Fact]
    public void ImportResult_WarningsCanBeAdded()
    {
        // Arrange
        var result = new ImportResult();

        // Act
        result.Warnings.Add("Warning 1");
        result.Warnings.Add("Warning 2");

        // Assert
        Assert.Equal(2, result.Warnings.Count);
        Assert.Contains("Warning 1", result.Warnings);
        Assert.Contains("Warning 2", result.Warnings);
    }

    [Fact]
    public void ImportSource_HasCurlValue()
    {
        // Act
        var source = ImportSource.Curl;

        // Assert
        Assert.Equal(ImportSource.Curl, source);
    }

    [Fact]
    public void ImportSource_HasBrunoValue()
    {
        // Act
        var source = ImportSource.Bruno;

        // Assert
        Assert.Equal(ImportSource.Bruno, source);
    }
}
