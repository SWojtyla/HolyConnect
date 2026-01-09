using HolyConnect.Application.Services;

namespace HolyConnect.Application.Tests.Services;

public class KeyboardShortcutServiceTests
{
    private readonly KeyboardShortcutService _service;

    public KeyboardShortcutServiceTests()
    {
        _service = new KeyboardShortcutService();
    }

    [Fact]
    public void RegisterShortcut_WithValidParameters_ShouldReturnId()
    {
        // Arrange
        var handlerCalled = false;
        Func<Task> handler = () =>
        {
            handlerCalled = true;
            return Task.CompletedTask;
        };

        // Act
        var shortcutId = _service.RegisterShortcut("k", true, false, false, handler, "Test shortcut");

        // Assert
        Assert.NotNull(shortcutId);
        Assert.NotEmpty(shortcutId);
    }

    [Fact]
    public async Task HandleKeyPress_WithRegisteredShortcut_ShouldExecuteHandler()
    {
        // Arrange
        var handlerCalled = false;
        Func<Task> handler = () =>
        {
            handlerCalled = true;
            return Task.CompletedTask;
        };
        _service.RegisterShortcut("k", true, false, false, handler, "Test shortcut");

        // Act
        var handled = await _service.HandleKeyPress("k", true, false, false);

        // Assert
        Assert.True(handled);
        Assert.True(handlerCalled);
    }

    [Fact]
    public async Task HandleKeyPress_WithUnregisteredShortcut_ShouldReturnFalse()
    {
        // Act
        var handled = await _service.HandleKeyPress("x", true, false, false);

        // Assert
        Assert.False(handled);
    }

    [Fact]
    public async Task HandleKeyPress_WithDifferentModifiers_ShouldNotMatch()
    {
        // Arrange
        var handlerCalled = false;
        Func<Task> handler = () =>
        {
            handlerCalled = true;
            return Task.CompletedTask;
        };
        _service.RegisterShortcut("k", true, false, false, handler, "Test shortcut");

        // Act - Try with shift key added
        var handled = await _service.HandleKeyPress("k", true, true, false);

        // Assert
        Assert.False(handled);
        Assert.False(handlerCalled);
    }

    [Fact]
    public async Task HandleKeyPress_WithCaseInsensitiveKey_ShouldMatch()
    {
        // Arrange
        var handlerCalled = false;
        Func<Task> handler = () =>
        {
            handlerCalled = true;
            return Task.CompletedTask;
        };
        _service.RegisterShortcut("k", true, false, false, handler, "Test shortcut");

        // Act - Use uppercase K
        var handled = await _service.HandleKeyPress("K", true, false, false);

        // Assert
        Assert.True(handled);
        Assert.True(handlerCalled);
    }

    [Fact]
    public async Task UnregisterShortcut_WithValidId_ShouldRemoveShortcut()
    {
        // Arrange
        var handlerCalled = false;
        Func<Task> handler = () =>
        {
            handlerCalled = true;
            return Task.CompletedTask;
        };
        var shortcutId = _service.RegisterShortcut("k", true, false, false, handler, "Test shortcut");

        // Act
        _service.UnregisterShortcut(shortcutId);
        var handled = await _service.HandleKeyPress("k", true, false, false);

        // Assert
        Assert.False(handled);
        Assert.False(handlerCalled);
    }

    [Fact]
    public void GetAllShortcuts_WithRegisteredShortcuts_ShouldReturnDictionary()
    {
        // Arrange
        _service.RegisterShortcut("k", true, false, false, () => Task.CompletedTask, "Open search");
        _service.RegisterShortcut("h", true, false, false, () => Task.CompletedTask, "Go home");
        _service.RegisterShortcut("n", true, true, false, () => Task.CompletedTask, "New item");

        // Act
        var shortcuts = _service.GetAllShortcuts();

        // Assert
        Assert.Equal(3, shortcuts.Count);
        Assert.Contains("Ctrl+K", shortcuts.Keys);
        Assert.Contains("Ctrl+H", shortcuts.Keys);
        Assert.Contains("Ctrl+Shift+N", shortcuts.Keys);
        Assert.Equal("Open search", shortcuts["Ctrl+K"]);
        Assert.Equal("Go home", shortcuts["Ctrl+H"]);
        Assert.Equal("New item", shortcuts["Ctrl+Shift+N"]);
    }

    [Fact]
    public async Task RegisterShortcut_WithMultipleHandlers_ShouldExecuteCorrectOne()
    {
        // Arrange
        var handler1Called = false;
        var handler2Called = false;
        
        Func<Task> handler1 = () =>
        {
            handler1Called = true;
            return Task.CompletedTask;
        };
        
        Func<Task> handler2 = () =>
        {
            handler2Called = true;
            return Task.CompletedTask;
        };
        
        _service.RegisterShortcut("k", true, false, false, handler1, "First shortcut");
        _service.RegisterShortcut("h", true, false, false, handler2, "Second shortcut");

        // Act
        await _service.HandleKeyPress("h", true, false, false);

        // Assert
        Assert.False(handler1Called);
        Assert.True(handler2Called);
    }
}
