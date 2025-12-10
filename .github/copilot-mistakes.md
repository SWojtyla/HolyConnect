# Copilot Instructions - Common Mistakes to Avoid

## MudBlazor Component Usage

### ✅ Correct: Using IDialogService for Confirmations

When implementing confirmation dialogs in HolyConnect (which uses MudBlazor), **ALWAYS** use `IDialogService` with the `ConfirmDialog` component.

**Correct Pattern:**

```csharp
@inject IDialogService DialogService
@using HolyConnect.Maui.Components.Shared.Dialogs

private async Task DeleteItem(Guid id)
{
    var parameters = new DialogParameters
    {
        ["ContentText"] = "Are you sure you want to delete this item?",
        ["ButtonText"] = "Delete",
        ["Color"] = MudBlazor.Color.Error
    };

    var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirm Delete", parameters);
    var result = await dialog.Result;

    if (!result.Canceled)
    {
        // Perform delete action
        Snackbar.Add("Item deleted", Severity.Success);
    }
}
```

### ❌ Incorrect: Awaiting Snackbar.Add

**DO NOT** try to await `Snackbar.Add()` as it does not return an awaitable type. The following patterns are **WRONG**:

```csharp
// WRONG - Snackbar.Add is not awaitable
var result = await Snackbar.Add("Message", Severity.Warning, config => { ... });

// WRONG - SnackbarResult does not exist in MudBlazor
if (result == SnackbarResult.Clicked) { ... }
```

**Why this is wrong:**
1. `ISnackbar.Add()` returns `Snackbar` (not `Task<Snackbar>`)
2. There is no `SnackbarResult` type in MudBlazor
3. Snackbars are for notifications, not confirmations

### Key Points

1. **Snackbar** = Notifications (fire and forget)
   - Use for success/error/info messages
   - Cannot await user interaction
   - No return value needed

2. **DialogService** = User confirmations (awaitable)
   - Use when you need user confirmation
   - Can await the result
   - Returns `DialogResult` with `Canceled` property

### Reference Examples in Codebase

See these files for correct dialog usage:
- `src/HolyConnect.Maui/Components/Pages/Environments/EnvironmentView.razor` (lines 350-365)
- `src/HolyConnect.Maui/Components/Shared/Dialogs/ConfirmDialog.razor` (dialog component)

## Build Verification

Always build the MAUI project after making UI changes:

```bash
cd /home/runner/work/HolyConnect/HolyConnect
dotnet workload restore  # First time only
dotnet build src/HolyConnect.Maui/HolyConnect.Maui.csproj
```

Look for errors (not warnings - many warnings are pre-existing in the project).

## Common Issues Fixed

1. **Issue:** "The name 'SnackbarResult' does not exist in the current context"
   - **Cause:** Tried to use non-existent `SnackbarResult` type
   - **Fix:** Use `DialogService` with `DialogResult` instead

2. **Issue:** "'Snackbar' does not contain a definition for 'GetAwaiter'"
   - **Cause:** Tried to `await Snackbar.Add()`
   - **Fix:** Use `DialogService.ShowAsync<ConfirmDialog>()` for confirmations

## When to Use Each

| Scenario | Component | Example |
|----------|-----------|---------|
| Show success message | `Snackbar.Add()` | `Snackbar.Add("Saved!", Severity.Success)` |
| Show error message | `Snackbar.Add()` | `Snackbar.Add(ex.Message, Severity.Error)` |
| Confirm deletion | `DialogService` | See correct pattern above |
| Ask yes/no question | `DialogService` | See correct pattern above |
| Show info toast | `Snackbar.Add()` | `Snackbar.Add("Info", Severity.Info)` |

---

**Last Updated:** 2025-12-10
**Created by:** GitHub Copilot (fixing Settings.razor confirmation dialog)
