# Flows Feature

## Overview
Flows in HolyConnect allow you to create and execute sequences of API requests. Each step in a flow can extract values from responses and pass them as variables to subsequent steps, enabling complex testing workflows and API orchestration.

## UI Components

### 1. Flows Page (`/flows`)
The main page for managing all flows across all environments.

**Features:**
- Card-based layout displaying all flows
- Each card shows:
  - Flow name and description
  - Associated environment
  - Number of steps
  - Last updated timestamp
- Actions available:
  - **View**: Navigate to the flow's environment view
  - **Edit**: Modify the flow's configuration
  - **Execute**: Run the flow immediately
  - **Delete**: Remove the flow (with confirmation)
- Empty state with call-to-action when no flows exist

**Navigation:**
Access via the "Flows" menu item in the left sidebar (AccountTree icon)

### 2. Flow Create Page (`/flow/create`)
Create new flows with multiple request steps.

**Features:**
- Flow name and description
- Environment selection (required)
- Collection selection (optional)
- Step management:
  - Add multiple steps
  - Reorder steps (up/down arrows)
  - Configure each step:
    - Select request from environment/collection
    - Enable/disable step
    - Continue on error toggle
    - Delay before execution (milliseconds)
  - Remove steps

**Usage:**
1. Navigate from Home page "Create Flow" button
2. Or from Flows page "Create Flow" button
3. Or from Environment view "+" menu

### 3. Flow Edit Page (`/flow/{id}/edit`)
Modify existing flows.

**Features:**
- All features from Create page
- Environment is read-only (cannot be changed)
- Preserves existing step configurations
- Updates flow metadata (UpdatedAt timestamp)

**Usage:**
1. From Flows page, click "Edit" in the flow card menu
2. Modify flow configuration
3. Save changes or cancel

### 4. Flow Execute Page (`/flow/{id}/execute`)
Execute a flow and view real-time results.

**Features:**
- Flow execution with cancellation support
- Step-by-step progress tracking
- Result details for each step:
  - Status (Success, Failed, Skipped, etc.)
  - Duration
  - HTTP status code
  - Response body and headers
- Overall execution summary:
  - Status (Running, Completed, Failed, Cancelled)
  - Total duration
  - Steps completed count
  - Error messages if any

### 5. Environment View Integration
Flows are integrated into the Environment view.

**Features:**
- Flows section in the left sidebar
- Click a flow to view its details
- Execute flow directly from environment view
- Navigate to flow edit page

## Navigation Menu
The "Flows" item has been added to the left sidebar navigation menu:
- Position: Between "Environments" section and "Git"
- Icon: AccountTree (Material Design icon)
- Route: `/flows`

## Data Persistence

### Storage Location
Flows are stored in the file system using the `MultiFileRepository` pattern:
- Directory: `{StoragePath}/flows/`
- Format: One JSON file per flow
- Filename: `{FlowId}.json`

### Repository Configuration
```csharp
builder.Services.AddSingleton<IRepository<Flow>>(sp =>
{
    return new MultiFileRepository<Flow>(
        f => f.Id,
        GetStoragePathSafe,
        "flows",
        f => f.Name);
});
```

## Service Layer

### FlowService
Located at: `src/HolyConnect.Application/Services/FlowService.cs`

**Key Methods:**
- `CreateFlowAsync(Flow flow)` - Create new flow with generated IDs
- `GetAllFlowsAsync()` - Retrieve all flows
- `GetFlowByIdAsync(Guid id)` - Get specific flow
- `GetFlowsByEnvironmentIdAsync(Guid environmentId)` - Filter by environment
- `GetFlowsByCollectionIdAsync(Guid collectionId)` - Filter by collection
- `UpdateFlowAsync(Flow flow)` - Update existing flow
- `DeleteFlowAsync(Guid id)` - Delete flow
- `ExecuteFlowAsync(Guid flowId, CancellationToken)` - Execute flow steps

### Flow Execution
When a flow is executed:
1. Loads environment and collection (if specified)
2. Creates temporary variables dictionary
3. Executes steps in order
4. Applies delays if configured
5. Handles errors based on `ContinueOnError` setting
6. Extracts and propagates variables between steps
7. Returns comprehensive execution results

## Testing

### Domain Tests
- `FlowTests.cs` - Flow entity tests
- `FlowStepTests.cs` - FlowStep entity tests  
- `FlowExecutionResultTests.cs` - Result entity tests

### Application Tests
- `FlowServiceTests.cs` - 15 tests covering all service methods
  - Flow creation with step initialization
  - Retrieval (all, by ID, by environment, by collection)
  - Updates and deletions
  - Flow execution with variable propagation

### UI Tests
- `FlowsPageTests.cs` - 5 tests for UI component logic
  - Flow listing
  - Empty state handling
  - Deletion confirmation
  - Flow retrieval
  - Update functionality

**Test Coverage:** All 232 tests pass (102 Domain + 114 Application + 16 UI)

## Implementation Details

### Key Features
1. **Variable Propagation**: Variables extracted in one step are available to subsequent steps
2. **Error Handling**: Steps can be configured to continue on error or stop execution
3. **Delay Support**: Add delays between steps for rate limiting or timing requirements
4. **Step Management**: Reorder, enable/disable, and configure individual steps
5. **Environment Isolation**: Temporary variables don't permanently modify environment/collection

### Clean Architecture
The flows feature follows the established clean architecture pattern:
- **Domain**: Core entities (Flow, FlowStep, FlowExecutionResult)
- **Application**: Business logic (FlowService, IFlowService)
- **Infrastructure**: Persistence (MultiFileRepository)
- **Presentation**: UI components (Blazor pages)

## Future Enhancements
Potential improvements for the flows feature:
- Import/export flows
- Duplicate flow functionality
- Flow templates
- Scheduled execution
- Webhook triggers
- Flow versioning
- Visual flow designer
- Conditional step execution
- Parallel step execution
- Flow variables scope management
