using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HolyConnect.Application.Interfaces;

namespace HolyConnect.Application.Services;

/// <summary>
/// Service for managing keyboard shortcuts
/// </summary>
public class KeyboardShortcutService : IKeyboardShortcutService
{
    private readonly Dictionary<string, ShortcutHandler> _shortcuts = new();
    
    public string RegisterShortcut(string key, bool ctrlKey, bool shiftKey, bool altKey, Func<Task> handler, string description)
    {
        var shortcutKey = CreateShortcutKey(key, ctrlKey, shiftKey, altKey);
        var shortcutId = Guid.NewGuid().ToString();
        
        _shortcuts[shortcutKey] = new ShortcutHandler
        {
            Id = shortcutId,
            Key = key,
            CtrlKey = ctrlKey,
            ShiftKey = shiftKey,
            AltKey = altKey,
            Handler = handler,
            Description = description
        };
        
        return shortcutId;
    }
    
    public void UnregisterShortcut(string shortcutId)
    {
        var shortcut = _shortcuts.FirstOrDefault(s => s.Value.Id == shortcutId);
        if (shortcut.Key != null)
        {
            _shortcuts.Remove(shortcut.Key);
        }
    }
    
    public async Task<bool> HandleKeyPress(string key, bool ctrlKey, bool shiftKey, bool altKey)
    {
        var shortcutKey = CreateShortcutKey(key, ctrlKey, shiftKey, altKey);
        
        if (_shortcuts.TryGetValue(shortcutKey, out var handler))
        {
            await handler.Handler();
            return true;
        }
        
        return false;
    }
    
    public IDictionary<string, string> GetAllShortcuts()
    {
        return _shortcuts.ToDictionary(
            kvp => FormatShortcutKey(kvp.Value),
            kvp => kvp.Value.Description
        );
    }
    
    private static string CreateShortcutKey(string key, bool ctrlKey, bool shiftKey, bool altKey)
    {
        return $"{(ctrlKey ? "Ctrl+" : "")}{(shiftKey ? "Shift+" : "")}{(altKey ? "Alt+" : "")}{key.ToLower()}";
    }
    
    private static string FormatShortcutKey(ShortcutHandler handler)
    {
        var parts = new List<string>();
        
        if (handler.CtrlKey) parts.Add("Ctrl");
        if (handler.ShiftKey) parts.Add("Shift");
        if (handler.AltKey) parts.Add("Alt");
        parts.Add(handler.Key.ToUpper());
        
        return string.Join("+", parts);
    }
    
    private class ShortcutHandler
    {
        public string Id { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public bool CtrlKey { get; set; }
        public bool ShiftKey { get; set; }
        public bool AltKey { get; set; }
        public Func<Task> Handler { get; set; } = () => Task.CompletedTask;
        public string Description { get; set; } = string.Empty;
    }
}
