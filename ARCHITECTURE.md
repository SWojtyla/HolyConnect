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
  - `GraphQLRequest`: GraphQL queries and mutations
  - `RequestResponse`: Response data from executed requests

- **Business Rules**: Domain logic independent of external concerns
- **No Framework Dependencies**: Pure C# classes

### 2. Application Layer (HolyConnect.Application)
**Dependencies**: Domain Layer

Contains business logic and orchestration:
- **Service Interfaces**:
  - `IRepository<T>`: Generic repository pattern for data access
  - `IRequestExecutor`: Interface for executing different request types

- **Application Services**:
  - `EnvironmentService`: Manages environment CRUD operations
  - `CollectionService`: Handles collection management
  - `RequestService`: Coordinates request storage and execution
  - `VariableResolver`: Resolves variables in requests using environment and collection values
  - `IGitService`: Interface for git version control operations

- **Use Cases**: Business workflows that orchestrate domain entities

- **Variable Resolution**:
  - Variables use the `{{ variableName }}` syntax (like Postman and Bruno)
  - Resolved before request execution in `RequestService`
  - Collection variables take precedence over environment variables
  - Supports URL, headers, query parameters, and request body

### 3. Infrastructure Layer (HolyConnect.Infrastructure)
**Dependencies**: Application Layer, Domain Layer

Implements external concerns:
- **Data Persistence**:
  - `InMemoryRepository<T>`: In-memory implementation of IRepository
  - Future: Can be replaced with SQLite, Entity Framework, etc.

- **Request Executors**:
  - `RestRequestExecutor`: Executes HTTP REST requests using HttpClient
  - `GraphQLRequestExecutor`: Executes GraphQL queries

- **Version Control**:
  - `GitService`: Provides git operations using LibGit2Sharp
  - Supports initialize, commit, branch, fetch, pull, push operations

- **External Services**: HTTP clients, file systems, databases, git repositories

### 4. Presentation Layer (HolyConnect.Maui)
**Dependencies**: All layers

The UI layer built with .NET MAUI and Blazor:
- **Components**:
  - **Pages**: Home, EnvironmentView, RequestCreate, etc.
  - **Shared**: CollectionTreeItem, RequestEditor
  - **Layout**: MainLayout, NavMenu

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
- `IRequestExecutor` interface for different request types
- Each executor handles specific request types
- New request types can be added without modifying existing code

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

## Security Considerations

1. **Environment Variables**: Consider encryption for sensitive data
2. **Request Storage**: Sanitize and validate all inputs
3. **HTTP Requests**: Validate URLs and headers
4. **Authentication**: Store credentials securely (keychain/credential manager)

## Performance Considerations

1. **Async/Await**: All I/O operations are asynchronous
2. **Memory Management**: In-memory storage is transient (consider persistence)
3. **UI Responsiveness**: Long-running requests don't block UI
4. **Request Caching**: Consider caching responses for repeated requests

## Future Enhancements

1. **Request History**: Track and replay past requests
2. **Response Comparison**: Compare responses across requests
3. **Mock Server**: Built-in mock server for testing
4. **Code Generation**: Generate client code from requests
5. **Team Collaboration**: Share collections and environments via git remotes
6. **Import/Export**: Support Postman, Insomnia formats
7. **WebSocket Support**: Real-time API testing
8. **Authentication Flows**: OAuth, JWT, API Key management
9. **Scripting**: Pre-request and post-request scripts
10. **Dynamic Variables**: Computed values and system variables (e.g., timestamps, random values)
11. **Git Enhancements**: Merge conflict resolution, diff viewer, commit history
