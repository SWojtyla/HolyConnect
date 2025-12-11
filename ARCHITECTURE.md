# HolyConnect Architecture

## Overview

HolyConnect follows Clean Architecture principles, ensuring separation of concerns, testability, and maintainability. The application is divided into four distinct layers, each with specific responsibilities.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                       │
│                   (HolyConnect.Maui)                        │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Blazor Components (Pages, Shared Components)       │    │
│  │  MudBlazor UI Framework                             │    │
│  │  Navigation, Routing, User Interaction             │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                       │
│                (HolyConnect.Infrastructure)                  │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Persistence (InMemoryRepository)                   │    │
│  │  HTTP Clients (RestRequestExecutor)                 │    │
│  │  External Services (GraphQLRequestExecutor)         │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                         │
│                 (HolyConnect.Application)                    │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Services (EnvironmentService, RequestService)      │    │
│  │  Interfaces (IRepository, IRequestExecutor)         │    │
│  │  Use Cases and Business Workflows                   │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                            │
│                   (HolyConnect.Domain)                       │
│  ┌────────────────────────────────────────────────────┐    │
│  │  Entities (Environment, Collection, Request)        │    │
│  │  Domain Logic and Business Rules                    │    │
│  │  No Dependencies                                     │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

## Layer Responsibilities

### 1. Domain Layer (HolyConnect.Domain)
**Dependencies**: None

The innermost layer containing:
- **Entities**: Core business objects
  - `Environment`: Represents an API environment with variables
  - `Collection`: Hierarchical container for organizing requests
  - `Request`: Abstract base for all request types
  - `RestRequest`: HTTP REST API requests
  - `GraphQLRequest`: GraphQL queries, mutations, and subscriptions
  - `WebSocketRequest`: WebSocket connection requests
  - `Flow`: Sequence of requests executed in order with variable passing
  - `FlowStep`: Individual step in a flow execution
  - `FlowExecutionResult`: Result of executing a flow
  - `RequestResponse`: Response data from executed requests, including streaming support
  - `ResponseExtraction`: Rules for extracting values from response bodies
  - `StreamEvent`: Individual events in streaming responses (WebSocket, SSE)
  - `DynamicVariable`: Defines dynamic test data generation with constraints
  - `DataGeneratorType`: Enumeration of available data types (FirstName, Email, Date, etc.)
  - `ConstraintRule`: Rules for constraining generated data (min/max, age ranges, etc.)
  - `ImportResult`: Result of an import operation with success status, errors, and warnings
  - `ImportSource`: Enumeration of supported import formats (Curl, Bruno, etc.)

- **Business Rules**: Domain logic independent of external concerns
- **No Framework Dependencies**: Pure C# classes

### 2. Application Layer (HolyConnect.Application)
**Dependencies**: Domain Layer

Contains business logic and orchestration:
- **Service Interfaces**:
  - `IRepository<T>`: Generic repository pattern for data access
  - `IRequestExecutor`: Interface for executing different request types
  - `IResponseValueExtractor`: Interface for extracting values from response bodies
  - `IClipboardService`: Interface for clipboard operations
  - `IDataGeneratorService`: Interface for generating dynamic test data
  - `IImportService`: Interface for importing requests from various formats (curl, Bruno, etc.)

- **Application Services**:
  - `EnvironmentService`: Manages environment CRUD operations
  - `CollectionService`: Handles collection management
  - `RequestService`: Coordinates request storage and execution
  - `FlowService`: Manages flows and executes sequential request chains
  - `VariableResolver`: Resolves variables in requests using environment and collection values
  - `ResponseValueExtractor`: Extracts values from JSON/XML responses using JSONPath/XPath patterns
  - `IGitService`: Interface for git version control operations

- **Use Cases**: Business workflows that orchestrate domain entities

- **Variable Resolution**:
  - Variables use the `{{ variableName }}` syntax (like Postman and Bruno)
  - Supports both static and dynamic variables
  - Static variables have fixed values set manually
  - Dynamic variables generate fake test data on each request execution
  - Resolved before request execution in `RequestService`
  - Precedence: Request static > Collection static > Environment static > Request dynamic > Collection dynamic > Environment dynamic
  - Supports URL, headers, query parameters, and request body

- **Dynamic Test Data Generation**:
  - Uses Bogus library for realistic fake data generation
  - Supports 25+ data types including names, emails, dates, numbers, addresses, etc.
  - Configurable constraints (min/max values, age ranges, date ranges)
  - Variables defined at environment, collection, or request level
  - Generated fresh on each request execution for realistic testing
  - Useful for load testing, data variety, and automated test scenarios

- **Response Value Extraction**:
  - Supports JSONPath for JSON/GraphQL responses (e.g., `$.data.user.id`)
  - Supports XPath for XML responses (e.g., `//user/id`)
  - Automatic extraction after request execution using configured rules
  - Extracted values can be saved to environment or collection variables
  - Ad-hoc extraction available via UI for manual value extraction

### 3. Infrastructure Layer (HolyConnect.Infrastructure)
**Dependencies**: Application Layer, Domain Layer

Implements external concerns:
- **Data Persistence**:
  - `InMemoryRepository<T>`: In-memory implementation of IRepository
  - Future: Can be replaced with SQLite, Entity Framework, etc.

- **Request Executors**:
  - `RestRequestExecutor`: Executes HTTP REST requests using HttpClient
  - `GraphQLRequestExecutor`: Executes GraphQL queries and mutations
  - `GraphQLSubscriptionWebSocketExecutor`: Executes GraphQL subscriptions via WebSocket (graphql-transport-ws protocol)
  - `GraphQLSubscriptionSSEExecutor`: Executes GraphQL subscriptions via Server-Sent Events (SSE)
  - `WebSocketRequestExecutor`: Executes general WebSocket connections for bidirectional communication

- **Data Generation**:
  - `DataGeneratorService`: Generates realistic fake test data using Bogus library
  - Supports 25+ data types with configurable constraints
  - Generates fresh data on each invocation for realistic testing

- **Version Control**:
  - `GitService`: Provides git operations using LibGit2Sharp
  - Supports initialize, commit, branch, fetch, pull, push operations

- **Import Services**:
  - `ImportService`: Coordinates importing requests from various formats using Strategy pattern
  - `IImportStrategy`: Interface defining contract for import format parsers
  - `CurlImportStrategy`: Parses curl commands with full feature detection (method, headers, body, auth)
  - `BrunoImportStrategy`: Parses Bruno (.bru) files for REST and GraphQL requests
  - Extensible architecture: add new formats by implementing `IImportStrategy` and registering in DI
  - Supports automatic detection of HTTP method, headers, body, authentication
  - Follows Strategy pattern for clean separation of parsing logic per format

- **External Services**: 
  - HTTP clients, file systems, databases, git repositories
  - `ClipboardService`: Platform-specific clipboard operations using MAUI Essentials

### 4. Presentation Layer (HolyConnect.Maui)
**Dependencies**: All layers

The UI layer built with .NET MAUI and Blazor:
- **Components**:
  - **Pages**: Home, EnvironmentView, EnvironmentEdit, CollectionEdit, RequestCreate, etc.
  - **Shared**: CollectionTreeItem, RequestEditor, DynamicVariableEditor
  - **Layout**: MainLayout, NavMenu

- **Dynamic Variable Management**:
  - `DynamicVariableEditor`: Reusable component for configuring dynamic test data variables
  - Integrated into EnvironmentEdit and CollectionEdit pages
  - Supports data type selection, constraint configuration, and secret masking
  - Visual distinction between static and dynamic variables

- **Dependency Injection**: Configured in `MauiProgram.cs`
- **Navigation**: Blazor routing and NavigationManager
- **UI Framework**: MudBlazor for Material Design components

## Dependency Flow

```
Domain ← Application ← Infrastructure ← Maui
   ↑         ↑              ↑             ↑
   │         │              │             │
   │         │              │             └── User Interface
   │         │              └────────────── External Services
   │         └───────────────────────────── Business Logic
   └─────────────────────────────────────── Core Entities
```

## Key Design Patterns

### 1. Repository Pattern
- Abstracts data access through `IRepository<T>`
- Allows easy switching of persistence mechanisms
- Currently using in-memory storage, easily replaceable

### 2. Strategy Pattern
- **Request Execution**: `IRequestExecutor` interface for different request types
  - Each executor handles specific request types (REST, GraphQL, WebSocket, etc.)
  - New request types can be added without modifying existing code
- **Import Parsing**: `IImportStrategy` interface for different import formats
  - Each strategy handles parsing for one format (cURL, Bruno, etc.)
  - ImportService acts as coordinator, delegates to appropriate strategy
  - New formats can be added by implementing `IImportStrategy` and registering in DI
  - Example: Adding Postman support requires only creating `PostmanImportStrategy`

### 3. Dependency Injection
- All dependencies injected through constructor
- Services registered in `MauiProgram.cs`
- Promotes testability and loose coupling

### 4. Single Responsibility Principle
- Each class has one reason to change
- Services focused on specific domain areas
- Clear separation between layers

## Extensibility Points

### Adding New Request Types

1. **Domain Layer**: Create new request entity
```csharp
public class SoapRequest : Request
{
    public override RequestType Type => RequestType.Soap;
    public string SoapAction { get; set; }
    public string Envelope { get; set; }
}
```

2. **Infrastructure Layer**: Implement executor
```csharp
public class SoapRequestExecutor : IRequestExecutor
{
    public bool CanExecute(Request request) => request is SoapRequest;
    public Task<RequestResponse> ExecuteAsync(Request request) { /* ... */ }
}
```

3. **Presentation Layer**: Register in DI container
```csharp
builder.Services.AddScoped<IRequestExecutor, SoapRequestExecutor>();
```

4. **UI Layer**: Add request type to forms
```razor
<MudSelectItem Value="@RequestType.Soap">SOAP</MudSelectItem>
```

### Adding New Import Formats

1. **Application Layer**: Create strategy implementing `IImportStrategy`
```csharp
public class PostmanImportStrategy : IImportStrategy
{
    public ImportSource Source => ImportSource.Postman;
    
    public Request? Parse(string content, Guid environmentId, 
        Guid? collectionId, string? customName)
    {
        // Parse Postman collection/request JSON
        // Return RestRequest, GraphQLRequest, or null if parsing fails
    }
}
```

2. **Domain Layer**: Add new import source (if needed)
```csharp
public enum ImportSource
{
    Curl,
    Bruno,
    Postman  // New format
}
```

3. **Presentation Layer**: Register strategy in DI container
```csharp
builder.Services.AddScoped<IImportStrategy, PostmanImportStrategy>();
```

4. **UI Layer**: Strategy automatically available via `IImportService`
- No UI changes needed, service routes to correct strategy based on source

### Adding Persistent Storage

Replace `InMemoryRepository<T>` with:
- SQLite for local storage
- Entity Framework Core for database
- File-based storage for exports

### Adding Authentication

1. Create authentication service in Infrastructure
2. Add authentication state in Application layer
3. Implement in request executors
4. Add UI for credential management

## Testing Strategy

### Unit Tests
- **Domain Layer**: Test business logic without dependencies
- **Application Layer**: Test services with mocked repositories
- **Infrastructure Layer**: Test executors with mocked HttpClient

### Integration Tests
- Test full request execution flow
- Test persistence mechanisms
- Test UI components with services

### Example Test Structure
```
tests/
├── HolyConnect.Domain.Tests/
├── HolyConnect.Application.Tests/
├── HolyConnect.Infrastructure.Tests/
└── HolyConnect.Maui.Tests/
```

## Streaming and Real-Time Communication

### WebSocket Support

The application supports bidirectional WebSocket communication for real-time APIs:

- **Standard WebSocket**: Connect to any WebSocket server (ws:// or wss://)
- **Message Exchange**: Send messages and receive responses in real-time
- **Protocol Support**: Custom subprotocols can be specified
- **Authentication**: Supports Basic and Bearer token authentication
- **Timeout Handling**: Configurable timeout for receiving messages

### GraphQL Subscriptions

GraphQL subscriptions enable real-time data streaming from GraphQL servers:

#### WebSocket Protocol (graphql-transport-ws)
- Implements the graphql-transport-ws protocol
- Automatic connection initialization and acknowledgment
- Handles subscription lifecycle (subscribe, next, complete, error)
- URL conversion from HTTP/HTTPS to WS/WSS

#### Server-Sent Events (SSE)
- HTTP-based streaming using Server-Sent Events
- Parses SSE format (event types and data fields)
- Handles multiple events in a single connection
- Standard HTTP headers and authentication

### Streaming Response Model

All streaming responses use the `StreamEvent` model:
- **Timestamp**: When the event was received
- **Data**: The event payload
- **EventType**: The type of event (message, data, error, complete, etc.)

The `RequestResponse` model includes:
- **IsStreaming**: Flag indicating if the response is streaming
- **StreamEvents**: List of all events received during the connection
- **Body**: Formatted view of all events with timestamps

## Security Considerations

1. **Environment Variables**: Consider encryption for sensitive data
2. **Request Storage**: Sanitize and validate all inputs
3. **HTTP Requests**: Validate URLs and headers
4. **Authentication**: Store credentials securely (keychain/credential manager)
5. **WebSocket Connections**: Validate WebSocket URLs and enforce secure connections (wss://) in production

## Performance Considerations

1. **Async/Await**: All I/O operations are asynchronous
2. **Memory Management**: In-memory storage is transient (consider persistence)
3. **UI Responsiveness**: Long-running requests don't block UI
4. **Request Caching**: Consider caching responses for repeated requests

## Flows Feature

### Overview
Flows enable sequential execution of multiple requests where variables extracted from one step are automatically available to subsequent steps. This is essential for testing complex API workflows like authentication chains or multi-step operations.

### Architecture
1. **Domain Entities**:
   - `Flow`: Contains flow metadata and list of steps
   - `FlowStep`: Represents a single step with configuration (order, continue on error, delay)
   - `FlowExecutionResult`: Captures execution status, timing, and results for each step
   - `FlowStepResult`: Individual step result with status, timing, and response data

2. **Service Layer**:
   - `FlowService`: Orchestrates flow execution
   - Manages temporary variable state during execution
   - Coordinates with `RequestService` for individual step execution
   - Handles error propagation and continuation logic

3. **Variable Passing**:
   - Flow execution maintains a temporary variables dictionary
   - Variables from environment and collection are merged at start
   - Each step's response extractions update the variable dictionary
   - Variables are available to subsequent steps via standard `{{ variableName }}` syntax
   - Original environment/collection variables remain unchanged unless explicitly saved

4. **Execution Flow**:
   ```
   1. Load flow configuration and requests
   2. Initialize variable dictionary from environment/collection
   3. For each enabled step (in order):
      a. Apply optional delay
      b. Merge flow variables into request context
      c. Execute request via RequestService
      d. Capture response and update variables from extractions
      e. Check error status and continue/stop based on configuration
   4. Return comprehensive execution result
   ```

5. **UI Integration**:
   - `FlowCreate.razor`: Create and configure flows with drag-drop step ordering
   - `FlowExecute.razor`: Execute flows and view detailed results
   - Integrated into `EnvironmentView` sidebar alongside collections and requests

### Use Cases
- **Authentication flows**: Login → extract token → make authenticated requests
- **Data dependencies**: Create parent → extract ID → create children
- **Multi-step workflows**: Complex business processes requiring sequential operations
- **End-to-end testing**: Complete user journeys with variable passing

## Recent Enhancements

1. **WebSocket Support**: ✅ Real-time API testing with WebSocket connections
2. **GraphQL Subscriptions**: ✅ Support for GraphQL subscriptions via WebSocket (graphql-transport-ws) and Server-Sent Events (SSE)
3. **Streaming Responses**: ✅ Capture and display streaming events from WebSocket and SSE connections
4. **Flows**: ✅ Sequential request execution with variable passing between steps

## Future Enhancements

1. **Request History**: Track and replay past requests
2. **Response Comparison**: Compare responses across requests
3. **Mock Server**: Built-in mock server for testing
4. **Code Generation**: Generate client code from requests
5. **Team Collaboration**: Share collections and environments via git remotes
6. **Import/Export**: Support Postman, Insomnia formats
7. **Authentication Flows**: OAuth, JWT, API Key management (enhanced with flows)
8. **Scripting**: Pre-request and post-request scripts
9. **Dynamic Variables**: Computed values and system variables (e.g., timestamps, random values)
10. **Git Enhancements**: Merge conflict resolution, diff viewer, commit history
11. **WebSocket Message History**: Interactive send/receive interface for WebSocket connections
