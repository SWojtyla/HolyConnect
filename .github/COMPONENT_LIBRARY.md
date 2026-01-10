# Component Library Reference

## Overview

This document provides a comprehensive reference for reusable Blazor components in HolyConnect. Use this guide when building new features or understanding existing component usage patterns.

## Component Organization

Components are organized by function in the following directories:

```
src/HolyConnect.Maui/Components/
├── Layout/           # Application layout components
├── Pages/            # Full page components (routable)
└── Shared/           # Reusable shared components
    ├── Common/       # General-purpose components
    ├── Dialogs/      # Modal dialog components
    ├── Editors/      # Input/editing components
    ├── Utilities/    # Helper/utility components
    └── Viewers/      # Display/viewing components
```

## Layout Components

### MainLayout
**Path**: `Components/Layout/MainLayout.razor`

Main application layout providing:
- Top app bar with title and actions
- Left navigation drawer
- Main content area

**Usage**: Automatically applied to all pages via `Routes.razor`

### NavMenu
**Path**: `Components/Layout/NavMenu.razor`

Left sidebar navigation menu.

**Features**:
- Dynamic collection list
- Icon-based navigation items
- Expandable groups
- Auto-refresh on navigation

**Services Used**:
- `ICollectionService`: Load collections
- `NavigationManager`: Track location changes

## Common Components

### CollectionTreeItem
**Path**: `Components/Shared/Common/CollectionTreeItem.razor`

Recursive tree view item for displaying collection hierarchies.

**Parameters**:
- `Collection`: Collection to display
- `SelectedCollectionId`: Currently selected collection ID
- `OnCollectionSelected`: Callback when collection is clicked
- `OnRequestSelected`: Callback when request is clicked
- `OnRenameRequested`: Callback for rename action
- `OnDeleteRequested`: Callback for delete action
- `Level`: Current nesting level (for indentation)

**Usage**:
```razor
<CollectionTreeItem 
    Collection="@collection"
    SelectedCollectionId="@_selectedCollectionId"
    OnCollectionSelected="@HandleCollectionSelected"
    OnRequestSelected="@HandleRequestSelected"
    Level="0" />
```

**Features**:
- Recursive rendering for nested collections
- Visual indicators for request types (REST, GraphQL, WebSocket)
- Context menu for rename/delete
- Indentation based on nesting level

### VariableTextField
**Path**: `Components/Shared/Common/VariableTextField.razor`

Text field with variable suggestion support.

**Parameters**:
- `Value`: Current text value
- `ValueChanged`: Callback when value changes
- `Label`: Field label
- `Placeholder`: Placeholder text
- `Lines`: Number of text lines (default: 1)
- `EnvironmentVariables`: Available environment variables
- `CollectionVariables`: Available collection variables

**Features**:
- Autocomplete for `{{` variable syntax
- Dropdown showing available variables
- Support for single-line and multi-line input
- Variable precedence (collection > environment)

**Usage**:
```razor
<VariableTextField 
    @bind-Value="@_request.Url"
    Label="URL"
    Placeholder="Enter request URL"
    EnvironmentVariables="@_environmentVars"
    CollectionVariables="@_collectionVars" />
```

## Dialog Components

### ConfirmDialog
**Path**: `Components/Shared/Dialogs/ConfirmDialog.razor`

Confirmation dialog for destructive actions.

**Parameters**:
- `ContentText`: Message to display
- `ButtonText`: Confirm button text (default: "OK")
- `Color`: Button color (default: Primary)

**Usage**:
```csharp
var parameters = new DialogParameters
{
    ["ContentText"] = "Are you sure you want to delete this item?",
    ["ButtonText"] = "Delete",
    ["Color"] = Color.Error
};

var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirm Delete", parameters);
var result = await dialog.Result;

if (!result.Canceled)
{
    // User confirmed
}
```

**Important**: See `.github/copilot-mistakes.md` for correct dialog usage patterns.

### RenameDialog
**Path**: `Components/Shared/Dialogs/RenameDialog.razor`

Dialog for renaming entities (requests, collections, environments).

**Parameters**:
- `CurrentName`: Current entity name
- `EntityType`: Type of entity being renamed (e.g., "Request", "Collection")

**Usage**:
```csharp
var parameters = new DialogParameters
{
    ["CurrentName"] = request.Name,
    ["EntityType"] = "Request"
};

var dialog = await DialogService.ShowAsync<RenameDialog>("Rename Request", parameters);
var result = await dialog.Result;

if (!result.Canceled && result.Data is string newName)
{
    request.Name = newName;
    await RequestService.UpdateAsync(request);
}
```

### GlobalSearchDialog
**Path**: `Components/Shared/Dialogs/GlobalSearchDialog.razor`

Global search across requests, collections, and environments.

**Keyboard Shortcut**: `Ctrl+K` (Cmd+K on Mac)

**Features**:
- Fuzzy search across all entities
- Type filtering (requests, collections, environments)
- Keyboard navigation
- Navigate to selected item

**Usage**:
```csharp
// Triggered via keyboard shortcut or search button
await DialogService.ShowAsync<GlobalSearchDialog>("Search");
```

**Services Used**:
- `IGlobalSearchService`: Perform search operations

### SelectOptionDialog
**Path**: `Components/Shared/Dialogs/SelectOptionDialog.razor`

Generic dialog for selecting from a list of options.

**Parameters**:
- `Options`: List of selectable options
- `Title`: Dialog title
- `SelectedValue`: Currently selected value

**Usage**:
```csharp
var parameters = new DialogParameters
{
    ["Options"] = new List<string> { "Option 1", "Option 2", "Option 3" },
    ["Title"] = "Select an option"
};

var dialog = await DialogService.ShowAsync<SelectOptionDialog>("Select", parameters);
var result = await dialog.Result;

if (!result.Canceled && result.Data is string selected)
{
    // Use selected option
}
```

### DiffViewerDialog
**Path**: `Components/Shared/Dialogs/DiffViewerDialog.razor`

Side-by-side diff viewer for comparing text/JSON content.

**Parameters**:
- `OriginalContent`: Original/left side content
- `ModifiedContent`: Modified/right side content
- `OriginalTitle`: Title for left side (default: "Original")
- `ModifiedTitle`: Title for right side (default: "Modified")

**Usage**:
```csharp
var parameters = new DialogParameters
{
    ["OriginalContent"] = previousVersion,
    ["ModifiedContent"] = currentVersion,
    ["OriginalTitle"] = "Previous Version",
    ["ModifiedTitle"] = "Current Version"
};

await DialogService.ShowAsync<DiffViewerDialog>("Compare Versions", parameters);
```

### KeyboardShortcutsDialog
**Path**: `Components/Shared/Dialogs/KeyboardShortcutsDialog.razor`

Display all available keyboard shortcuts.

**Keyboard Shortcut**: `Ctrl+/`

**Usage**:
```csharp
await DialogService.ShowAsync<KeyboardShortcutsDialog>("Keyboard Shortcuts");
```

## Editor Components

### RequestEditor
**Path**: `Components/Shared/Editors/RequestEditor.razor`

Main request editor component that delegates to specialized editors based on request type.

**Parameters**:
- `Request`: Request to edit
- `Environment`: Current environment (for variable resolution)
- `Collection`: Parent collection (for collection variables)

**Features**:
- Automatic editor selection based on request type
- Delegates to RestRequestEditor, GraphQLRequestEditor, or WebSocketRequestEditor

**Usage**:
```razor
<RequestEditor 
    Request="@_currentRequest"
    Environment="@_environment"
    Collection="@_collection" />
```

### RestRequestEditor
**Path**: `Components/Shared/Editors/RestRequestEditor.razor`

Editor for REST API requests.

**Parameters**:
- `Request`: RestRequest to edit
- `Environment`: Current environment
- `Collection`: Parent collection

**Features**:
- HTTP method selection (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS)
- URL input with variable support
- Headers editor
- Query parameters editor
- Request body editor with syntax highlighting
- Authentication configuration (None, Basic, Bearer)
- Response extraction rules

**Tabs**:
1. **Request**: URL, method, body
2. **Headers**: Key-value header pairs
3. **Query Params**: URL query parameters
4. **Auth**: Authentication settings
5. **Extractions**: Response extraction rules

### GraphQLRequestEditor
**Path**: `Components/Shared/Editors/GraphQLRequestEditor.razor`

Editor for GraphQL queries, mutations, and subscriptions.

**Parameters**:
- `Request`: GraphQLRequest to edit
- `Environment`: Current environment
- `Collection`: Parent collection

**Features**:
- GraphQL operation type selection (Query, Mutation, Subscription)
- Monaco editor with GraphQL syntax highlighting
- Variables editor (JSON)
- Headers editor
- Schema viewer integration
- Subscription protocol selection (WebSocket, SSE)
- Response extraction rules

**Tabs**:
1. **Query**: GraphQL operation with Monaco editor
2. **Variables**: JSON variables
3. **Headers**: HTTP headers
4. **Schema**: GraphQL schema viewer
5. **Extractions**: Response extraction rules

### WebSocketRequestEditor
**Path**: `Components/Shared/Editors/WebSocketRequestEditor.razor`

Editor for WebSocket connections.

**Parameters**:
- `Request`: WebSocketRequest to edit
- `Environment`: Current environment
- `Collection`: Parent collection

**Features**:
- WebSocket URL input
- Headers editor
- Connection message configuration
- Protocol selection

**Tabs**:
1. **Connection**: URL and settings
2. **Headers**: WebSocket headers
3. **Messages**: Pre-configured messages to send

### HeadersEditor
**Path**: `Components/Shared/Editors/HeadersEditor.razor`

Key-value editor for HTTP headers.

**Parameters**:
- `Headers`: Dictionary of header key-value pairs
- `HeadersChanged`: Callback when headers change
- `AvailableVariables`: Variables for autocomplete

**Features**:
- Add/remove header rows
- Variable autocomplete in values
- Common header suggestions
- Enable/disable individual headers

**Usage**:
```razor
<HeadersEditor 
    Headers="@_request.Headers"
    HeadersChanged="@HandleHeadersChanged"
    AvailableVariables="@_allVariables" />
```

### CodeEditor
**Path**: `Components/Shared/Editors/CodeEditor.razor`

Monaco-based code editor with syntax highlighting.

**Parameters**:
- `Value`: Editor content
- `ValueChanged`: Callback when content changes
- `Language`: Syntax language (json, graphql, xml, javascript, etc.)
- `ReadOnly`: Whether editor is read-only
- `MinHeight`: Minimum editor height

**Features**:
- Syntax highlighting for multiple languages
- Theme-aware (light/dark mode)
- Auto-completion
- Error highlighting
- Line numbers
- Minimap

**Usage**:
```razor
<CodeEditor 
    @bind-Value="@_request.Body"
    Language="json"
    MinHeight="300px" />
```

### GraphQLCodeEditor
**Path**: `Components/Shared/Editors/GraphQLCodeEditor.razor`

Specialized code editor for GraphQL with schema support.

**Parameters**:
- `Value`: GraphQL query/mutation/subscription
- `ValueChanged`: Callback when content changes
- `SchemaUrl`: GraphQL endpoint URL for introspection
- `Headers`: Headers for schema introspection request

**Features**:
- GraphQL-specific syntax highlighting
- Schema introspection
- Auto-completion based on schema
- Query validation
- Type information on hover

### StaticVariableEditor
**Path**: `Components/Shared/Editors/StaticVariableEditor.razor`

Editor for static environment/collection variables.

**Parameters**:
- `Variables`: Dictionary of variable key-value pairs
- `VariablesChanged`: Callback when variables change
- `SecretVariableNames`: List of secret variable names
- `SecretVariableNamesChanged`: Callback when secret variables change

**Features**:
- Add/remove variable rows
- Mark variables as secret (masked in UI)
- Inline editing
- Variable name validation

**Usage**:
```razor
<StaticVariableEditor 
    Variables="@_environment.Variables"
    VariablesChanged="@HandleVariablesChanged"
    SecretVariableNames="@_environment.SecretVariableNames"
    SecretVariableNamesChanged="@HandleSecretNamesChanged" />
```

### DynamicVariableEditor
**Path**: `Components/Shared/Editors/DynamicVariableEditor.razor`

Editor for dynamic variables with test data generation.

**Parameters**:
- `DynamicVariables`: List of dynamic variable definitions
- `DynamicVariablesChanged`: Callback when variables change

**Features**:
- Data type selection (FirstName, Email, Date, Number, etc.)
- Constraint rules (min/max, age ranges, formats)
- Multiple constraints per variable
- Preview generated values

**Usage**:
```razor
<DynamicVariableEditor 
    DynamicVariables="@_environment.DynamicVariables"
    DynamicVariablesChanged="@HandleDynamicVariablesChanged" />
```

### ResponseExtractionManager
**Path**: `Components/Shared/Editors/ResponseExtractionManager.razor`

Manage response extraction rules for capturing values from API responses.

**Parameters**:
- `Extractions`: List of ResponseExtraction rules
- `ExtractionsChanged`: Callback when rules change

**Features**:
- Add/remove extraction rules
- JSONPath/XPath pattern input
- Target variable name
- Variable scope (environment/collection)
- Pattern validation

**Usage**:
```razor
<ResponseExtractionManager 
    Extractions="@_request.ResponseExtractions"
    ExtractionsChanged="@HandleExtractionsChanged" />
```

## Viewer Components

### ResponseViewer
**Path**: `Components/Shared/Viewers/ResponseViewer.razor`

Display request execution results.

**Parameters**:
- `Response`: RequestResponse object
- `IsExecuting`: Whether request is currently executing
- `IsCancellable`: Whether execution can be cancelled
- `OnCancel`: Callback for cancel action

**Features**:
- Status code display with color coding
- Response time and size
- Tabbed view: Body, Headers, Extracted Values
- Syntax highlighting for response body (JSON, XML, HTML)
- Streaming support for WebSocket/SSE responses
- Copy response to clipboard
- Format JSON/XML responses
- Search in response

**Tabs**:
1. **Body**: Response body with syntax highlighting
2. **Headers**: Response headers
3. **Extracted**: Values extracted via extraction rules
4. **Stream**: Real-time streaming events (for WebSocket/SSE)

**Usage**:
```razor
<ResponseViewer 
    Response="@_response"
    IsExecuting="@_isExecuting"
    IsCancellable="true"
    OnCancel="@HandleCancel" />
```

### GraphQLSchemaViewer
**Path**: `Components/Shared/Viewers/GraphQLSchemaViewer.razor`

Display GraphQL schema with queries, mutations, subscriptions, and types.

**Parameters**:
- `SchemaUrl`: GraphQL endpoint URL
- `Headers`: Headers for introspection request

**Features**:
- Automatic schema introspection
- Categorized view (Queries, Mutations, Subscriptions, Types)
- Type badges with color coding
- Field descriptions
- Type signatures
- Searchable schema

**Usage**:
```razor
<GraphQLSchemaViewer 
    SchemaUrl="@_request.Url"
    Headers="@_request.Headers" />
```

**Services Used**:
- `IGraphQLSchemaService`: Fetch and parse GraphQL schema

### DiffViewer
**Path**: `Components/Shared/Viewers/DiffViewer.razor`

Side-by-side text comparison viewer.

**Parameters**:
- `OriginalContent`: Left side content
- `ModifiedContent`: Right side content
- `OriginalTitle`: Title for left side
- `ModifiedTitle`: Title for right side
- `Language`: Syntax highlighting language

**Features**:
- Side-by-side comparison
- Line-by-line diff highlighting
- Added/removed line indicators
- Syntax highlighting
- Synchronized scrolling

**Usage**:
```razor
<DiffViewer 
    OriginalContent="@_previousVersion"
    ModifiedContent="@_currentVersion"
    OriginalTitle="Previous"
    ModifiedTitle="Current"
    Language="json" />
```

## Utility Components

### StatusBadge
**Path**: `Components/Shared/Utilities/StatusBadge.razor`

Display HTTP status codes with appropriate colors.

**Parameters**:
- `StatusCode`: HTTP status code (e.g., 200, 404, 500)

**Features**:
- Color-coded by status class (success, redirect, client error, server error)
- Standard HTTP status text
- Compact chip display

**Usage**:
```razor
<StatusBadge StatusCode="@_response.StatusCode" />
```

### EmptyState
**Path**: `Components/Shared/Utilities/EmptyState.razor`

Display empty state with icon, message, and call-to-action.

**Parameters**:
- `Icon`: Material icon to display
- `Title`: Empty state title
- `Message`: Description message
- `ActionText`: Button text (optional)
- `OnAction`: Callback when button clicked

**Features**:
- Centered layout with icon
- Optional action button
- Customizable colors

**Usage**:
```razor
<EmptyState 
    Icon="@Icons.Material.Filled.Folder"
    Title="No collections yet"
    Message="Create your first collection to organize your requests"
    ActionText="Create Collection"
    OnAction="@NavigateToCreate" />
```

### LoadingOverlay
**Path**: `Components/Shared/Utilities/LoadingOverlay.razor`

Full-screen loading overlay with spinner.

**Parameters**:
- `Visible`: Whether overlay is visible
- `Message`: Loading message (optional)

**Features**:
- Blocks user interaction while loading
- Centered spinner
- Optional loading message
- Semi-transparent backdrop

**Usage**:
```razor
<LoadingOverlay 
    Visible="@_isLoading"
    Message="Executing request..." />
```

## MudBlazor Components

HolyConnect uses MudBlazor extensively for UI components. Common components include:

### Form Components
- `MudTextField`: Text input
- `MudSelect`: Dropdown selection
- `MudCheckBox`: Checkbox
- `MudRadioGroup`: Radio buttons
- `MudAutocomplete`: Autocomplete input

### Data Display
- `MudTable`: Data table
- `MudCard`: Card container
- `MudChip`: Chip/badge
- `MudAvatar`: Avatar/icon

### Feedback
- `ISnackbar`: Toast notifications (NOT awaitable - see copilot-mistakes.md)
- `IDialogService`: Modal dialogs (awaitable)
- `MudProgressLinear`: Progress bar
- `MudProgressCircular`: Spinner

### Navigation
- `MudNavMenu`: Navigation menu
- `MudNavLink`: Navigation link
- `MudTabs`: Tab control

### Layout
- `MudContainer`: Container with max-width
- `MudGrid`: Grid layout
- `MudPaper`: Surface elevation
- `MudAppBar`: Application bar
- `MudDrawer`: Side drawer

## Component Patterns

### Parent-Child Communication

**Pass data down**:
```razor
<ChildComponent 
    Data="@_parentData"
    OnDataChanged="@HandleDataChanged" />
```

**Notify parent of changes**:
```csharp
[Parameter]
public EventCallback<DataType> OnDataChanged { get; set; }

private async Task NotifyParent()
{
    await OnDataChanged.InvokeAsync(_data);
}
```

### Two-Way Binding

```razor
<ChildComponent @bind-Value="@_parentValue" />
```

In child component:
```csharp
[Parameter]
public DataType Value { get; set; }

[Parameter]
public EventCallback<DataType> ValueChanged { get; set; }

private async Task UpdateValue(DataType newValue)
{
    Value = newValue;
    await ValueChanged.InvokeAsync(Value);
}
```

### Lifecycle Methods

```csharp
protected override async Task OnInitializedAsync()
{
    // Component initialized - run once
    await LoadData();
}

protected override async Task OnParametersSetAsync()
{
    // Called when parameters change
    await RefreshData();
}

protected override void OnAfterRender(bool firstRender)
{
    if (firstRender)
    {
        // First render complete - safe for JS interop
    }
}
```

### State Management

```csharp
private bool _isLoading;
private List<Item> _items = new();

private async Task LoadData()
{
    _isLoading = true;
    StateHasChanged(); // Force re-render
    
    _items = await Service.GetItems();
    
    _isLoading = false;
    StateHasChanged(); // Force re-render
}
```

## Best Practices

1. **Component Reusability**: Extract common UI patterns into shared components
2. **Parameter Validation**: Validate parameters in `OnParametersSetAsync`
3. **Event Callbacks**: Use `EventCallback<T>` for parent-child communication
4. **State Management**: Keep component state minimal and focused
5. **Dispose Resources**: Implement `IDisposable` for event subscriptions
6. **Avoid Blocking**: Use async/await for all I/O operations
7. **Accessibility**: Include ARIA labels and keyboard navigation
8. **Naming Conventions**: Use descriptive names for components and parameters

## Related Documentation

- [UI Navigation Guide](.github/UI_NAVIGATION_GUIDE.md) - Navigation and routing patterns
- [Copilot Instructions](.github/copilot-instructions.md) - Development guidelines
- [Copilot Mistakes](.github/copilot-mistakes.md) - Common mistakes to avoid
- [MudBlazor Documentation](https://mudblazor.com/) - MudBlazor component library
