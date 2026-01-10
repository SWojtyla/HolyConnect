# UI Navigation and Routing Guide

## Overview

This guide documents the navigation structure, routing patterns, and page relationships in HolyConnect's Blazor UI. Use this as a reference when implementing new pages, understanding navigation flows, or debugging routing issues.

## Navigation Structure

### Main Navigation Menu
**Location**: `src/HolyConnect.Maui/Components/Layout/NavMenu.razor`

The left sidebar navigation provides access to all major features:

| Menu Item | Route | Icon | Description |
|-----------|-------|------|-------------|
| Home | `/` | Home | Application home/landing page |
| Collections | Dynamic | Folder | Lists all root collections (expandable group) |
| Variable Matrix | `/variables/matrix` | GridOn | View/manage variables across environments |
| Manage Variables | `/environments` | Dns | Manage environments and their variables |
| Import | `/import` | Upload | Import requests from Bruno/cURL |
| Flows | `/flows` | AccountTree | Manage and execute request flows |
| Git | `/git` | Source | Git integration and version control |
| History | `/history` | History | View request execution history |
| Variables Wiki | `/wiki/variables` | Code | Documentation for variable syntax |
| Keyboard Shortcuts | `/shortcuts` | Keyboard | View all keyboard shortcuts |
| Settings | `/settings` | Settings | Application settings |

### Dynamic Navigation
Collections in the navigation menu are loaded dynamically and update when:
- A new collection is created
- A collection is deleted
- Navigation occurs (via `NavigationManager.LocationChanged` event)

## Route Patterns

### Collections

#### View Collection
**Route**: `/collection/{CollectionId:guid}`
**Also**: `/collection/{CollectionId:guid}/request/{RequestId:guid}`
**Component**: `Components/Pages/Collections/CollectionView.razor`

Displays a collection with:
- Left sidebar: Collection tree structure with requests and sub-collections
- Right panel: Selected request editor and response viewer

**Navigation From**:
- NavMenu (click collection)
- Home page collection cards
- After creating/editing a collection

**Navigation To**:
- Edit Collection: `/collection/{CollectionId}/edit`
- Create Sub-collection: `/collection/{CollectionId}/subcollection/create`
- Create Request: `/collection/{CollectionId}/request/create`

#### Create Collection
**Route**: `/collection/create`
**Component**: `Components/Pages/Collections/CollectionCreate.razor`

**Route**: `/collection/{ParentCollectionId:guid}/subcollection/create`
**Component**: `Components/Pages/Collections/CollectionCreate.razor`

**Navigation From**:
- Home page "Create Collection" button
- CollectionView "Add" menu → "Sub-collection"

**Navigation To**:
- After save: `/collection/{newCollectionId}`

#### Edit Collection
**Route**: `/collection/{CollectionId:guid}/edit`
**Component**: `Components/Pages/Collections/CollectionEdit.razor`

**Navigation From**:
- CollectionView "Edit" button

**Navigation To**:
- After save: `/collection/{CollectionId}`

### Environments

#### List Environments
**Route**: `/environments`
**Component**: `Components/Pages/Environments/Environments.razor`

Lists all environments in a card grid layout.

**Navigation From**:
- NavMenu "Manage Variables"

**Navigation To**:
- View Environment: `/environment/{EnvironmentId}`
- Create Environment: `/environment/create`

#### View Environment
**Route**: `/environment/{EnvironmentId:guid}`
**Component**: `Components/Pages/Environments/EnvironmentView.razor`

Displays environment details with variables (static and dynamic).

**Navigation From**:
- Environments list page
- Variable Matrix page

**Navigation To**:
- Edit Environment: `/environment/{EnvironmentId}/edit`
- Create Request: `/environment/{EnvironmentId}/request/create`

#### Create Environment
**Route**: `/environment/create`
**Component**: `Components/Pages/Environments/EnvironmentCreate.razor`

**Navigation From**:
- Environments list page "Create Environment" button

**Navigation To**:
- After save: `/environment/{newEnvironmentId}`

#### Edit Environment
**Route**: `/environment/{EnvironmentId:guid}/edit`
**Component**: `Components/Pages/Environments/EnvironmentEdit.razor`

**Navigation From**:
- EnvironmentView "Edit" button

**Navigation To**:
- After save: `/environment/{EnvironmentId}`

### Requests

#### Create Request
**Route**: `/collection/{CollectionId:guid}/request/create`
**Also**: `/environment/{EnvironmentId:guid}/request/create`
**Component**: `Components/Pages/Requests/RequestCreate.razor`

Creates a new request. Can be initiated from either a collection or an environment.

**Navigation From**:
- CollectionView "Add" menu → "Request"
- EnvironmentView "Create Request" button

**Navigation To**:
- After save: `/collection/{CollectionId}/request/{newRequestId}`

**Note**: Request editing happens inline within CollectionView - there is no separate edit page.

### Flows

#### List Flows
**Route**: `/flows`
**Component**: `Components/Pages/Flows/Flows.razor`

Card-based layout showing all flows.

**Navigation From**:
- NavMenu "Flows"

**Navigation To**:
- View Flow: `/flow/{FlowId}/view`
- Create Flow: `/flow/create`
- Edit Flow: `/flow/{FlowId}/edit`
- Execute Flow: `/flow/{FlowId}/execute`

#### View Flow
**Route**: `/flow/{FlowId:guid}/view`
**Component**: `Components/Pages/Flows/FlowView.razor`

View flow details without executing.

**Navigation From**:
- Flows list page "View" button

**Navigation To**:
- Edit Flow: `/flow/{FlowId}/edit`
- Execute Flow: `/flow/{FlowId}/execute`

#### Create Flow
**Route**: `/flow/create`
**Component**: `Components/Pages/Flows/FlowCreate.razor`

**Navigation From**:
- Flows list page "Create Flow" button

**Navigation To**:
- After save: `/flow/{newFlowId}/view`

#### Edit Flow
**Route**: `/flow/{FlowId:guid}/edit`
**Component**: `Components/Pages/Flows/FlowEdit.razor`

**Navigation From**:
- Flows list page "Edit" button
- FlowView page "Edit" button

**Navigation To**:
- After save: `/flow/{FlowId}/view`

#### Execute Flow
**Route**: `/flow/{FlowId:guid}/execute`
**Component**: `Components/Pages/Flows/FlowExecute.razor`

Run a flow with environment selection and view real-time results.

**Navigation From**:
- Flows list page "Execute" button
- FlowView page "Execute" button

**Navigation To**:
- After completion: Stays on execution page to review results

### Utility Pages

#### Home
**Route**: `/`
**Component**: `Components/Pages/Home.razor`

Landing page with quick access to collections and environments.

#### History
**Route**: `/history`
**Component**: `Components/Pages/History.razor`

View past request executions.

#### Import
**Route**: `/import`
**Component**: `Components/Pages/Import.razor`

Import requests from external sources (Bruno, cURL).

#### Git Management
**Route**: `/git`
**Component**: `Components/Pages/Git/GitManagement.razor`

Git integration and version control.

#### Variable Matrix
**Route**: `/variables/matrix`
**Component**: `Components/Pages/Variables/VariablesMatrix.razor`

Grid view of variables across all environments.

#### Variables Wiki
**Route**: `/wiki/variables`
**Component**: `Components/Pages/Docs/VariablesWiki.razor`

Documentation for variable syntax and usage.

#### Keyboard Shortcuts
**Route**: `/shortcuts`
**Component**: `Components/Pages/KeyboardShortcuts.razor`

List of all keyboard shortcuts.

#### Settings
**Route**: `/settings`
**Component**: `Components/Pages/Settings.razor`

Application settings and configuration.

#### Not Found
**Route**: Any unmatched route
**Component**: `Components/Pages/NotFound.razor`

404 error page.

## Navigation Patterns

### Programmatic Navigation

Use `NavigationManager` for navigation:

```csharp
@inject NavigationManager NavigationManager

// Navigate to a route
NavigationManager.NavigateTo("/collection/{collectionId}");

// Navigate with query parameters
NavigationManager.NavigateTo($"/flow/{flowId}/execute?env={envId}");

// Go back (browser back button simulation)
await JSRuntime.InvokeVoidAsync("history.back");
```

### Route Parameters

Extract route parameters in component code:

```csharp
@page "/collection/{CollectionId:guid}"

[Parameter]
public Guid CollectionId { get; set; }

protected override async Task OnInitializedAsync()
{
    // Use CollectionId parameter
    _collection = await CollectionService.GetByIdAsync(CollectionId);
}
```

### Multiple Route Templates

Pages can have multiple route templates:

```csharp
@page "/collection/create"
@page "/collection/{ParentCollectionId:guid}/subcollection/create"

[Parameter]
public Guid? ParentCollectionId { get; set; }
```

### Query Parameters

Access query parameters via `NavigationManager`:

```csharp
var uri = new Uri(NavigationManager.Uri);
var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
var envId = query["env"];
```

## Layout Structure

### MainLayout
**Location**: `src/HolyConnect.Maui/Components/Layout/MainLayout.razor`

Standard layout with:
- Top app bar with title and global actions
- Left navigation drawer (NavMenu)
- Main content area

All pages use MainLayout by default (configured in `Routes.razor`).

## Keyboard Shortcuts

HolyConnect implements global keyboard shortcuts via `KeyboardShortcutService`.

**Common Shortcuts**:
- `Ctrl+K` (Cmd+K on Mac): Global search
- `Ctrl+/`: Show keyboard shortcuts dialog
- `Ctrl+N`: New request (when in collection view)
- `F5`: Execute selected request

See `Components/Pages/KeyboardShortcuts.razor` for full list.

## Dialog Navigation

Some navigation happens via dialogs instead of full page navigation:

### Global Search Dialog
**Trigger**: `Ctrl+K` or Search button in app bar
**Component**: `Components/Shared/Dialogs/GlobalSearchDialog.razor`

Search across requests, collections, environments. Selecting a result navigates to the appropriate page.

### Rename Dialog
**Trigger**: Rename button on requests/collections
**Component**: `Components/Shared/Dialogs/RenameDialog.razor`

Modal dialog for renaming entities.

### Confirm Dialog
**Trigger**: Delete actions
**Component**: `Components/Shared/Dialogs/ConfirmDialog.razor`

Confirmation dialog for destructive actions.

## Best Practices

### 1. Navigation After CRUD Operations

Always navigate after save/create/delete operations:

```csharp
private async Task SaveCollection()
{
    await CollectionService.CreateAsync(_collection);
    NavigationManager.NavigateTo($"/collection/{_collection.Id}");
}
```

### 2. Handle Navigation Events

Subscribe to location changes when needed:

```csharp
protected override void OnInitialized()
{
    NavigationManager.LocationChanged += OnLocationChanged;
}

private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
{
    StateHasChanged();
}

public void Dispose()
{
    NavigationManager.LocationChanged -= OnLocationChanged;
}
```

### 3. Use Route Constraints

Enforce type safety with route constraints:

```csharp
@page "/collection/{CollectionId:guid}"  // Only accepts valid GUIDs
@page "/flow/{FlowId:guid}/execute"
```

### 4. Preserve State During Navigation

Use services or state management for data that should persist:

```csharp
// Use ActiveEnvironmentService to maintain selected environment
@inject IActiveEnvironmentService ActiveEnvironmentService

// Environment selection persists across navigation
var currentEnv = await ActiveEnvironmentService.GetActiveEnvironmentAsync();
```

### 5. Handle Invalid Routes

Provide meaningful error states:

```csharp
protected override async Task OnInitializedAsync()
{
    _collection = await CollectionService.GetByIdAsync(CollectionId);
    
    if (_collection == null)
    {
        NavigationManager.NavigateTo("/not-found");
    }
}
```

## Troubleshooting

### Route Not Matching
- Verify route template syntax (especially constraints)
- Check for conflicting route patterns
- Ensure component has `@page` directive

### Parameter Not Populating
- Check parameter name matches route placeholder exactly (case-sensitive)
- Verify `[Parameter]` attribute is present
- Check route constraints match parameter type

### Navigation Not Updating UI
- Call `StateHasChanged()` after data loads
- Subscribe to `LocationChanged` event if needed
- Check component lifecycle methods

### Back Button Not Working
- Browser back button works automatically with Blazor routing
- For custom back navigation, use `NavigationManager.NavigateTo()` with previous URL

## Related Documentation

- [Component Library Reference](.github/COMPONENT_LIBRARY.md) - Reusable UI components
- [Copilot Instructions](.github/copilot-instructions.md) - Development guidelines
- [Architecture](ARCHITECTURE.md) - Application architecture overview
- [Flows Feature](docs/FLOWS_FEATURE.md) - Flows-specific navigation patterns
