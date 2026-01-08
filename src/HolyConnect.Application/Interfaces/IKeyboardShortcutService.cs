using System;
using System.Threading.Tasks;

namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Service for managing keyboard shortcuts
/// </summary>
public interface IKeyboardShortcutService
{
    /// <summary>
    /// Registers a keyboard shortcut handler
    /// </summary>
    /// <param name="key">The key to trigger the shortcut (e.g., "k", "h", "/")</param>
    /// <param name="ctrlKey">Whether Ctrl key is required</param>
    /// <param name="shiftKey">Whether Shift key is required</param>
    /// <param name="altKey">Whether Alt key is required</param>
    /// <param name="handler">The action to execute</param>
    /// <param name="description">Description of the shortcut for documentation</param>
    /// <returns>A unique identifier for the registered shortcut</returns>
    string RegisterShortcut(string key, bool ctrlKey, bool shiftKey, bool altKey, Func<Task> handler, string description);
    
    /// <summary>
    /// Unregisters a keyboard shortcut
    /// </summary>
    /// <param name="shortcutId">The unique identifier returned by RegisterShortcut</param>
    void UnregisterShortcut(string shortcutId);
    
    /// <summary>
    /// Handles a keyboard event
    /// </summary>
    /// <param name="key">The key pressed</param>
    /// <param name="ctrlKey">Whether Ctrl key was pressed</param>
    /// <param name="shiftKey">Whether Shift key was pressed</param>
    /// <param name="altKey">Whether Alt key was pressed</param>
    /// <returns>True if a shortcut was handled, false otherwise</returns>
    Task<bool> HandleKeyPress(string key, bool ctrlKey, bool shiftKey, bool altKey);
    
    /// <summary>
    /// Gets all registered shortcuts for documentation
    /// </summary>
    /// <returns>Dictionary of shortcut combinations to descriptions</returns>
    IDictionary<string, string> GetAllShortcuts();
}
