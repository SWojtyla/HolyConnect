# Flows Feature

## Overview
Flows in HolyConnect allow you to create and execute sequences of API requests. Each step in a flow can extract values from responses and pass them as variables to subsequent steps, enabling complex testing workflows and API orchestration.

## UI Components

### 1. Flows Page (`/flows`)
The main page for managing all flows.

**Features:**
- Card-based layout displaying all flows
- Each card shows:
  - Flow name and description
  - Associated collection (if any)
  - Number of steps
  - Created timestamp
- Actions available:
  - **View**: Navigate to a dedicated flow view page to see flow details
  - **Edit**: Modify the flow's configuration
  - **Execute**: Run the flow with environment selection
  - **Delete**: Remove the flow (with confirmation)
- Empty state with call-to-action when no flows exist

**Navigation:**
Access via the "Flows" menu item in the left sidebar (AccountTree icon)

### 2. Flow View Page (`/flow/{id}/view`)
View detailed information about a specific flow.

**Features:**
- Flow name, description, and metadata
- Collection association (if any)
- Created timestamp
- Total steps and enabled steps count
- Step-by-step details:
  - Order and request information
  - URL and request type
  - Enabled/disabled status
  - Continue on error setting
  - Delay configuration
- Actions:
  - Edit flow
  - Execute flow

**Usage:**
1. From Flows page, click "View" button on any flow card
2. Or navigate directly via URL `/flow/{id}/view`

### 3. Flow Create Page (`/flow/create`)
Create new flows with multiple request steps.

**Features:**
- Flow name and description
- Collection selection (optional)
- Step management:
  - Select requests from ANY collection (not restricted to one environment)
  - Add multiple steps
  - Reorder steps (up/down arrows)
  - Configure each step:
    - Select request from any available request
    - Enable/disable step
    - Continue on error toggle
    - Delay before execution (milliseconds)
  - Remove steps

**Key Changes:**
- **No environment required during creation** - flows are now environment-independent
- **Select requests from all collections** - not limited to a single environment's requests
- **Environment chosen at execution time** - more flexible workflow

**Usage:**
1. Navigate from Flows page "Create Flow" button
2. Add flow details and configure steps
3. Save the flow

### 4. Flow Edit Page (`/flow/{id}/edit`)
Modify existing flows.

**Features:**
- All features from Create page
- Preserves existing step configurations
- No environment restriction - can modify requests from any collection

**Usage:**
1. From Flows page, click "Edit" in the flow card menu
2. Or from Flow View page, click "Edit" button
3. Modify flow configuration
4. Save changes or cancel

### 5. Flow Execute Page (`/flow/{id}/execute`)
Execute a flow with environment selection and view real-time results.

**Features:**
- **Environment selection** - choose which environment to execute the flow in
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

**Key Changes:**
- **Environment selected at execution time** - not tied to a specific environment
- More flexible execution across different environments

**Usage:**
1. From Flows page, click "Execute" button
2. Or from Flow View page, click "Execute" button
3. Select the environment to use for execution
4. Click "Execute Flow" to start

### 6. Environment View Integration
Flows can be created and managed independently of environments.

**Features:**
- Flows are no longer tied to a specific environment
- Execute any flow with any environment
- More flexible workflow management

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
- `GetFlowsByCollectionIdAsync(Guid collectionId)` - Filter by collection
- `UpdateFlowAsync(Flow flow)` - Update existing flow
- `DeleteFlowAsync(Guid id)` - Delete flow
- `ExecuteFlowAsync(Guid flowId, Guid environmentId, CancellationToken)` - Execute flow steps with specified environment

### Flow Execution
When a flow is executed:
1. Loads the specified environment (passed as parameter)
2. Loads collection (if specified in flow)
3. Creates temporary variables dictionary
4. Executes steps in order
5. Applies delays if configured
6. Handles errors based on `ContinueOnError` setting
7. Extracts and propagates variables between steps
8. Returns comprehensive execution results

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
- `FlowsPageTests.cs` - 7 tests for UI component logic
  - Flow listing
  - Empty state handling
  - Deletion confirmation
  - Flow retrieval
  - Update functionality
  - Flow view page details
  - Environment-based execution

**Test Coverage:** All tests pass (113 Domain + 182 Application + 126 UI + 299 Infrastructure)

## Implementation Details

### Key Features
1. **Environment-Independent Creation**: Flows are no longer tied to a specific environment during creation
2. **Cross-Collection Request Selection**: Select requests from any collection when building flows
3. **Runtime Environment Selection**: Choose which environment to execute the flow in at execution time
4. **Variable Propagation**: Variables extracted in one step are available to subsequent steps
5. **Error Handling**: Steps can be configured to continue on error or stop execution
6. **Delay Support**: Add delays between steps for rate limiting or timing requirements
7. **Step Management**: Reorder, enable/disable, and configure individual steps
8. **Environment Isolation**: Temporary variables don't permanently modify environment/collection
9. **Dedicated View Page**: View flow details without executing

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
