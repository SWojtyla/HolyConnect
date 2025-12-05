using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Services;

public class SettingsServiceTests
{
    private readonly Mock<ISettingsService> _mockSettingsProvider;
    private readonly SettingsService _service;

    public SettingsServiceTests()
    {
        _mockSettingsProvider = new Mock<ISettingsService>();
        _service = new SettingsService(_mockSettingsProvider.Object);
    }

    [Fact]
    public async Task GetSettingsAsync_ShouldReturnSettings()
    {
        // Arrange
        var expectedSettings = new AppSettings
        {
            StoragePath = "/test/path",
            IsDarkMode = true
        };

        _mockSettingsProvider.Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(expectedSettings);

        // Act
        var result = await _service.GetSettingsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("/test/path", result.StoragePath);
        Assert.True(result.IsDarkMode);
        _mockSettingsProvider.Verify(s => s.GetSettingsAsync(), Times.Once);
    }

    [Fact]
    public async Task SaveSettingsAsync_ShouldCallProvider()
    {
        // Arrange
        var settings = new AppSettings
        {
            StoragePath = "/new/path",
            IsDarkMode = false
        };

        _mockSettingsProvider.Setup(s => s.SaveSettingsAsync(It.IsAny<AppSettings>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.SaveSettingsAsync(settings);

        // Assert
        _mockSettingsProvider.Verify(s => s.SaveSettingsAsync(settings), Times.Once);
    }
}
