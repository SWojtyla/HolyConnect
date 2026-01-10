# Quick Reference Guide

## Overview

This is a quick reference guide for common development tasks in HolyConnect. Use this when you need to quickly find how to implement common patterns without reading full documentation.

## Quick Links

- [Adding a New Request Type](#adding-a-new-request-type)
- [Creating a New Page](#creating-a-new-page)
- [Adding a Service](#adding-a-service)
- [Implementing a Dialog](#implementing-a-dialog)
- [Working with Variables](#working-with-variables)
- [Executing Requests](#executing-requests)
- [Common Code Snippets](#common-code-snippets)

---

## Adding a New Request Type

**Scenario**: You need to add support for a new API protocol (e.g., gRPC, SOAP)

### Step 1: Create Domain Entity
**Location**: `src/HolyConnect.Domain/Entities/`

```csharp
public class GrpcRequest : Request
{
    public string ServiceName { get; set; } = string.Empty;
    public string MethodName { get; set; } = string.Empty;
    public string ProtoDefinition { get; set; } = string.Empty;
    
    public override string Type => "gRPC";
}
```

### Step 2: Create Request Executor Interface (if new pattern needed)
**Location**: `src/HolyConnect.Application/Interfaces/`

Use existing `IRequestExecutor` interface:

```csharp
public interface IRequestExecutor
{
    Task<RequestResponse> ExecuteAsync(Request request, 
        Dictionary<string, string> variables, 
        CancellationToken cancellationToken = default);
        
    bool CanExecute(Request request);
}
```

### Step 3: Implement Request Executor
**Location**: `src/HolyConnect.Infrastructure/Services/`

```csharp
public class GrpcRequestExecutor : IRequestExecutor
{
    private readonly HttpClient _httpClient;
    
    public GrpcRequestExecutor(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public bool CanExecute(Request request)
    {
        return request is GrpcRequest;
    }
    
    public async Task<RequestResponse> ExecuteAsync(
        Request request, 
        Dictionary<string, string> variables,
        CancellationToken cancellationToken)
    {
        var grpcRequest = (GrpcRequest)request;
        
        // Implement gRPC execution logic
        // Use HttpAuthenticationHelper for auth
        // Use VariableResolver for variable replacement
        
        return new RequestResponse
        {
            StatusCode = 200,
            Body = "Response body",
            Headers = new Dictionary<string, string>(),
            DurationMs = 123
        };
    }
}
```

### Step 4: Register in DI Container
**Location**: `src/HolyConnect.Maui/MauiProgram.cs`

```csharp
builder.Services.AddScoped<IRequestExecutor, GrpcRequestExecutor>();
```

### Step 5: Create UI Editor Component
**Location**: `src/HolyConnect.Maui/Components/Shared/Editors/`

```razor
@* GrpcRequestEditor.razor *@
@using HolyConnect.Domain.Entities

<MudTextField @bind-Value="@Request.ServiceName" Label="Service Name" />
<MudTextField @bind-Value="@Request.MethodName" Label="Method Name" />
<CodeEditor @bind-Value="@Request.ProtoDefinition" Language="protobuf" />

@code {
    [Parameter] public GrpcRequest Request { get; set; } = null!;
    [Parameter] public Environment? Environment { get; set; }
    [Parameter] public Collection? Collection { get; set; }
}
```

### Step 6: Update RequestEditor
**Location**: `src/HolyConnect.Maui/Components/Shared/Editors/RequestEditor.razor`

```razor
@if (Request is GrpcRequest grpcRequest)
{
    <GrpcRequestEditor Request="@grpcRequest" 
                       Environment="@Environment" 
                       Collection="@Collection" />
}
```

### Step 7: Write Tests

**Domain Tests**: `tests/HolyConnect.Domain.Tests/Entities/GrpcRequestTests.cs`
**Application Tests**: N/A (unless adding service logic)
**Infrastructure Tests**: `tests/HolyConnect.Infrastructure.Tests/Services/GrpcRequestExecutorTests.cs`

---

## Creating a New Page

**Scenario**: You need to add a new page to the application

### Step 1: Create Page Component
**Location**: `src/HolyConnect.Maui/Components/Pages/YourFeature/`

```razor
@page "/your-feature"
@page "/your-feature/{Id:guid}"
@inject IYourService YourService
@inject NavigationManager NavigationManager
@inject ISnackbar Snackbar

<MudContainer MaxWidth="MaxWidth.Large" Class="pa-4">
    <MudText Typo="Typo.h4" Class="mb-4">Your Feature</MudText>
    
    @if (_isLoading)
    {
        <MudProgressCircular Indeterminate="true" />
    }
    else if (_data != null)
    {
        <!-- Your UI here -->
    }
    else
    {
        <EmptyState Icon="@Icons.Material.Filled.YourIcon"
                    Title="No data"
                    Message="Get started by creating something" />
    }
</MudContainer>

@code {
    [Parameter] public Guid? Id { get; set; }
    
    private bool _isLoading;
    private YourDataType? _data;
    
    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }
    
    protected override async Task OnParametersSetAsync()
    {
        if (Id.HasValue)
        {
            await LoadData();
        }
    }
    
    private async Task LoadData()
    {
        _isLoading = true;
        try
        {
            _data = Id.HasValue 
                ? await YourService.GetByIdAsync(Id.Value)
                : await YourService.GetAllAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }
}
```

### Step 2: Add Navigation Link
**Location**: `src/HolyConnect.Maui/Components/Layout/NavMenu.razor`

```razor
<MudNavLink Href="/your-feature" 
            Icon="@Icons.Material.Filled.YourIcon">
    Your Feature
</MudNavLink>
```

### Step 3: Document Navigation
**Location**: `.github/UI_NAVIGATION_GUIDE.md`

Add your page to the routing documentation.

---

## Adding a Service

**Scenario**: You need to add a new service for business logic

### Step 1: Create Interface
**Location**: `src/HolyConnect.Application/Interfaces/`

```csharp
public interface IYourService
{
    Task<YourEntity> GetByIdAsync(Guid id);
    Task<IEnumerable<YourEntity>> GetAllAsync();
    Task<YourEntity> CreateAsync(YourEntity entity);
    Task<YourEntity> UpdateAsync(YourEntity entity);
    Task DeleteAsync(Guid id);
}
```

### Step 2: Implement Service
**Location**: `src/HolyConnect.Application/Services/`

```csharp
public class YourService : IYourService
{
    private readonly IRepository<YourEntity> _repository;
    private readonly ILogger<YourService> _logger;
    
    public YourService(
        IRepository<YourEntity> repository,
        ILogger<YourService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<YourEntity> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }
    
    public async Task<IEnumerable<YourEntity>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }
    
    // Implement other methods...
}
```

### Step 3: Register in DI
**Location**: `src/HolyConnect.Maui/MauiProgram.cs`

```csharp
// Register repository (if needed)
builder.Services.AddSingleton<IRepository<YourEntity>>(sp =>
    new MultiFileRepository<YourEntity>(
        e => e.Id,
        GetStoragePathSafe,
        "your-entities",
        e => e.Name));

// Register service
builder.Services.AddScoped<IYourService, YourService>();
```

### Step 4: Write Tests
**Location**: `tests/HolyConnect.Application.Tests/Services/YourServiceTests.cs`

```csharp
public class YourServiceTests
{
    private readonly Mock<IRepository<YourEntity>> _mockRepository;
    private readonly YourService _service;
    
    public YourServiceTests()
    {
        _mockRepository = new Mock<IRepository<YourEntity>>();
        _service = new YourService(_mockRepository.Object, Mock.Of<ILogger<YourService>>());
    }
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new YourEntity { Id = id, Name = "Test" };
        _mockRepository.Setup(r => r.GetByIdAsync(id))
                       .ReturnsAsync(entity);
        
        // Act
        var result = await _service.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }
}
```

---

## Implementing a Dialog

**Scenario**: You need to create a reusable dialog component

### Step 1: Create Dialog Component
**Location**: `src/HolyConnect.Maui/Components/Shared/Dialogs/`

```razor
@inject ISnackbar Snackbar

<MudDialog>
    <DialogContent>
        <MudTextField @bind-Value="@_inputValue" 
                      Label="Enter value"
                      Required="true"
                      RequiredError="Value is required" />
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" 
                   OnClick="Submit"
                   Disabled="@string.IsNullOrWhiteSpace(_inputValue)">
            OK
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    [CascadingParameter] MudDialogInstance MudDialog { get; set; } = null!;
    
    [Parameter] public string InitialValue { get; set; } = string.Empty;
    
    private string _inputValue = string.Empty;
    
    protected override void OnInitialized()
    {
        _inputValue = InitialValue;
    }
    
    private void Cancel()
    {
        MudDialog.Cancel();
    }
    
    private void Submit()
    {
        if (string.IsNullOrWhiteSpace(_inputValue))
        {
            Snackbar.Add("Please enter a value", Severity.Warning);
            return;
        }
        
        MudDialog.Close(DialogResult.Ok(_inputValue));
    }
}
```

### Step 2: Use Dialog
**In any component**:

```csharp
@inject IDialogService DialogService

private async Task ShowYourDialog()
{
    var parameters = new DialogParameters
    {
        ["InitialValue"] = "Default value"
    };
    
    var dialog = await DialogService.ShowAsync<YourDialog>("Dialog Title", parameters);
    var result = await dialog.Result;
    
    if (!result.Canceled && result.Data is string value)
    {
        // Use the value
        Snackbar.Add($"You entered: {value}", Severity.Success);
    }
}
```

---

## Working with Variables

### Resolve Variables in Request URL/Body

```csharp
@inject IVariableResolver VariableResolver

private async Task<string> ResolveUrl(string urlTemplate)
{
    var environment = await ActiveEnvironmentService.GetActiveEnvironmentAsync();
    var collection = await CollectionService.GetByIdAsync(_collectionId);
    
    // Merge variables (collection overrides environment)
    var variables = new Dictionary<string, string>();
    
    if (environment?.Variables != null)
    {
        foreach (var kvp in environment.Variables)
            variables[kvp.Key] = kvp.Value;
    }
    
    if (collection?.Variables != null)
    {
        foreach (var kvp in collection.Variables)
            variables[kvp.Key] = kvp.Value;
    }
    
    // Resolve {{variableName}} syntax
    return await VariableResolver.ResolveVariablesAsync(urlTemplate, variables);
}
```

### Extract Values from Response

```csharp
@inject IResponseValueExtractor ValueExtractor

private async Task ExtractValues(RequestResponse response)
{
    if (response.Body == null) return;
    
    // Extract using JSONPath
    var userId = await ValueExtractor.ExtractValueAsync(
        response.Body,
        "$.data.user.id",
        "application/json"
    );
    
    // Extract using XPath
    var xmlValue = await ValueExtractor.ExtractValueAsync(
        xmlResponse,
        "//user/id",
        "application/xml"
    );
    
    // Save to environment
    if (!string.IsNullOrEmpty(userId))
    {
        environment.Variables["userId"] = userId;
        await EnvironmentService.UpdateAsync(environment);
    }
}
```

---

## Executing Requests

### Execute a Request with Variable Resolution

```csharp
@inject IRequestExecutorFactory ExecutorFactory
@inject IVariableResolver VariableResolver

private async Task ExecuteRequest(Request request)
{
    try
    {
        _isExecuting = true;
        var cancellationTokenSource = new CancellationTokenSource();
        
        // Get executor for this request type
        var executor = ExecutorFactory.GetExecutor(request);
        
        // Resolve variables
        var variables = await GetMergedVariables();
        
        // Execute
        var response = await executor.ExecuteAsync(
            request, 
            variables,
            cancellationTokenSource.Token
        );
        
        // Handle response
        _response = response;
        Snackbar.Add($"Request completed: {response.StatusCode}", Severity.Success);
        
        // Extract values if configured
        await ExtractResponseValues(response);
    }
    catch (Exception ex)
    {
        Snackbar.Add($"Request failed: {ex.Message}", Severity.Error);
    }
    finally
    {
        _isExecuting = false;
    }
}
```

---

## Common Code Snippets

### Show Confirmation Dialog

```csharp
@inject IDialogService DialogService

private async Task<bool> ConfirmDelete(string itemName)
{
    var parameters = new DialogParameters
    {
        ["ContentText"] = $"Are you sure you want to delete '{itemName}'?",
        ["ButtonText"] = "Delete",
        ["Color"] = Color.Error
    };
    
    var dialog = await DialogService.ShowAsync<ConfirmDialog>(
        "Confirm Delete", 
        parameters
    );
    var result = await dialog.Result;
    
    return !result.Canceled;
}
```

### Show Success/Error Message

```csharp
@inject ISnackbar Snackbar

// Success
Snackbar.Add("Operation completed successfully", Severity.Success);

// Error
Snackbar.Add($"Error: {ex.Message}", Severity.Error);

// Warning
Snackbar.Add("Please review your input", Severity.Warning);

// Info
Snackbar.Add("Processing...", Severity.Info);
```

**Important**: Never await `Snackbar.Add()` - it's not awaitable. See `.github/copilot-mistakes.md`.

### Navigate After Save

```csharp
@inject NavigationManager NavigationManager

private async Task SaveAndNavigate()
{
    await YourService.SaveAsync(_entity);
    NavigationManager.NavigateTo($"/entity/{_entity.Id}");
}
```

### Load Data with Error Handling

```csharp
private async Task LoadData()
{
    _isLoading = true;
    try
    {
        _data = await YourService.GetDataAsync();
        StateHasChanged();
    }
    catch (Exception ex)
    {
        Snackbar.Add($"Failed to load data: {ex.Message}", Severity.Error);
        _logger.LogError(ex, "Failed to load data");
    }
    finally
    {
        _isLoading = false;
        StateHasChanged();
    }
}
```

### Subscribe to Navigation Changes

```csharp
@inject NavigationManager NavigationManager
@implements IDisposable

protected override void OnInitialized()
{
    NavigationManager.LocationChanged += OnLocationChanged;
}

private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
{
    await LoadData();
    StateHasChanged();
}

public void Dispose()
{
    NavigationManager.LocationChanged -= OnLocationChanged;
}
```

### Copy to Clipboard

```csharp
@inject IClipboardService ClipboardService

private async Task CopyToClipboard(string text)
{
    await ClipboardService.CopyToClipboardAsync(text);
    Snackbar.Add("Copied to clipboard", Severity.Success);
}
```

### Format JSON Response

```csharp
@inject IFormatterService FormatterService

private async Task FormatResponse()
{
    if (_response?.Body == null) return;
    
    try
    {
        var formatted = await FormatterService.FormatJsonAsync(_response.Body);
        _response.Body = formatted;
    }
    catch
    {
        Snackbar.Add("Invalid JSON", Severity.Error);
    }
}
```

---

## File Locations Quick Reference

| What | Where |
|------|-------|
| Domain Entities | `src/HolyConnect.Domain/Entities/` |
| Service Interfaces | `src/HolyConnect.Application/Interfaces/` |
| Service Implementations | `src/HolyConnect.Application/Services/` |
| Request Executors | `src/HolyConnect.Infrastructure/Services/` |
| Repositories | `src/HolyConnect.Infrastructure/Persistence/` |
| Pages | `src/HolyConnect.Maui/Components/Pages/` |
| Shared Components | `src/HolyConnect.Maui/Components/Shared/` |
| Dialogs | `src/HolyConnect.Maui/Components/Shared/Dialogs/` |
| Editors | `src/HolyConnect.Maui/Components/Shared/Editors/` |
| Viewers | `src/HolyConnect.Maui/Components/Shared/Viewers/` |
| Layout | `src/HolyConnect.Maui/Components/Layout/` |
| DI Registration | `src/HolyConnect.Maui/MauiProgram.cs` |
| Domain Tests | `tests/HolyConnect.Domain.Tests/` |
| Application Tests | `tests/HolyConnect.Application.Tests/` |
| Infrastructure Tests | `tests/HolyConnect.Infrastructure.Tests/` |
| UI Tests | `tests/HolyConnect.Maui.Tests/` |

---

## Testing Quick Reference

### Run All Tests
```bash
cd /home/runner/work/HolyConnect/HolyConnect
dotnet test
```

### Run Specific Test Project
```bash
dotnet test tests/HolyConnect.Application.Tests/
```

### Run Single Test
```bash
dotnet test --filter "FullyQualifiedName=YourNamespace.YourTest"
```

---

## Build Quick Reference

### Build Entire Solution
```bash
dotnet build HolyConnect.sln
```

### Build Specific Project
```bash
dotnet build src/HolyConnect.Maui/HolyConnect.Maui.csproj
```

### Restore Workloads (first time)
```bash
dotnet workload restore
```

---

## Related Documentation

- [Copilot Instructions](.github/copilot-instructions.md) - Complete development guidelines
- [UI Navigation Guide](.github/UI_NAVIGATION_GUIDE.md) - Navigation and routing
- [Component Library](.github/COMPONENT_LIBRARY.md) - Component reference
- [Copilot Mistakes](.github/copilot-mistakes.md) - Common mistakes to avoid
- [Architecture](ARCHITECTURE.md) - Application architecture
- [Flows Feature](docs/FLOWS_FEATURE.md) - Flows documentation
- [Bruno Import](docs/BRUNO_IMPORT.md) - Import functionality
