# GitHub Copilot Instructions for HolyConnect

## Overview
HolyConnect is an API testing tool built with .NET MAUI and Blazor, following clean architecture principles. These instructions ensure consistent code quality, maintainability, and adherence to best practices across the codebase.

## Architecture Principles

### Clean Architecture
- **Follow the Dependency Rule**: Dependencies must point inward. The Domain layer has no dependencies. Application depends only on Domain. Infrastructure depends on Application and Domain. The Presentation (MAUI) layer depends on all other layers.
- **Maintain Layer Separation**: Each layer has distinct responsibilities:
  - **Domain**: Core business entities and logic, no external dependencies
  - **Application**: Business workflows, service interfaces, use cases
  - **Infrastructure**: External concerns (data persistence, HTTP clients, file systems)
  - **Presentation**: UI components, pages, user interaction
- **Abstractions over Implementations**: Define interfaces in Application layer, implement in Infrastructure layer
- **Inversion of Control**: Use dependency injection for all dependencies

### Clean Code Practices
- **Single Responsibility Principle**: Each class should have one reason to change
- **DRY (Don't Repeat Yourself)**: Extract common functionality into reusable methods or classes
- **SOLID Principles**: Apply all SOLID principles consistently
- **Meaningful Names**: Use descriptive, intention-revealing names for classes, methods, variables
- **Small Functions**: Keep methods focused and concise (ideally under 20 lines)
- **Proper Error Handling**: Use specific exception types, handle errors at appropriate levels
- **Async/Await**: Use async patterns for all I/O operations to maintain responsiveness

## .NET and C# Best Practices

### General Guidelines
- **Use Latest C# Features**: Leverage modern C# features (pattern matching, records, nullable reference types)
- **Nullable Reference Types**: Enable and properly use nullable reference types throughout the codebase
- **Immutability**: Prefer immutable objects where appropriate (use init-only setters, records)
- **LINQ**: Use LINQ for collection operations, but be mindful of performance
- **Dispose Pattern**: Properly implement IDisposable for unmanaged resources
- **Naming Conventions**:
  - PascalCase for public members, classes, namespaces
  - camelCase for private fields with underscore prefix (_fieldName)
  - Use descriptive names that explain intent

### Code Organization
- **Namespace Structure**: Match folder structure with namespace organization
- **File Organization**: One class per file, named to match the class name
- **Region Usage**: Avoid #region; organize code logically instead
- **Using Statements**: Place using statements at the top of files
- **Dependency Injection**: Register services in MauiProgram.cs with appropriate lifetimes

## Blazor and MAUI Best Practices

### Blazor Components
- **Component Structure**: Keep components focused and composable
- **Parameter Binding**: Use [Parameter] attribute for component parameters
- **Event Callbacks**: Use EventCallback<T> for parent-child communication
- **State Management**: Manage state at the appropriate component level
- **Lifecycle Methods**: Use OnInitialized/OnInitializedAsync appropriately
- **CSS Isolation**: Use component-scoped CSS when possible
- **Code-Behind**: Use partial classes for complex component logic

### MAUI Specifics
- **Platform Differences**: Handle platform-specific code appropriately
- **Resource Management**: Properly manage platform resources
- **Navigation**: Use NavigationManager for routing
- **Performance**: Be mindful of UI thread operations, use async appropriately

### UI/UX Guidelines
- **MudBlazor Components**: Use MudBlazor components consistently for UI
- **Responsive Design**: Ensure UI works on different screen sizes
- **Accessibility**: Follow accessibility best practices (ARIA labels, keyboard navigation)
- **Error States**: Provide clear feedback for errors and loading states
- **Validation**: Implement proper form validation

## Testing Requirements

### Test Coverage
- **Always Write Tests**: Write unit tests for every new feature or bug fix
- **Test All Layers**: Ensure comprehensive coverage across Domain, Application, Infrastructure, and UI layers
- **Test Structure**: Organize tests to mirror the source code structure

### Unit Testing Standards
- **AAA Pattern**: Follow Arrange-Act-Assert pattern in all tests
- **One Assertion Per Test**: Focus each test on a single behavior (exceptions for related assertions)
- **Descriptive Test Names**: Use descriptive names that explain what is being tested and expected outcome
  - Format: `MethodName_StateUnderTest_ExpectedBehavior`
  - Example: `CreateEnvironmentAsync_WithValidData_ShouldCreateEnvironment`
- **Mock Dependencies**: Use Moq for mocking dependencies in unit tests
- **Test Independence**: Each test should be independent and not rely on other tests
- **Test Data**: Use meaningful test data that represents real-world scenarios

### Testing by Layer
- **Domain Tests**: Test business logic without any dependencies or mocking
- **Application Tests**: Test services with mocked repositories and dependencies
- **Infrastructure Tests**: Test implementations with mocked external dependencies (HttpClient, etc.)
- **UI Tests**: Test component logic and rendering (use bUnit for Blazor components)

### Running Tests
- **Before Committing**: Always run all tests before committing changes
- **CI/CD**: Ensure tests pass in CI/CD pipeline
- **Test Failures**: Investigate and fix failing tests immediately

## Implementation Workflow

### When Implementing New Features
1. **Design First**: Plan the feature following clean architecture layers
2. **Start with Domain**: Add/modify domain entities first
3. **Define Interfaces**: Create interfaces in Application layer
4. **Implement Logic**: Add application services and business logic
5. **Add Infrastructure**: Implement interfaces in Infrastructure layer
6. **Create UI**: Build Blazor components and pages
7. **Write Tests**: Add comprehensive unit tests for all layers
8. **Update Documentation**: Keep architecture and API documentation current
9. **Code Review**: Ensure code follows all guidelines before merging

### When Fixing Bugs
1. **Write Failing Test**: Create a test that reproduces the bug
2. **Fix the Issue**: Implement the fix following clean code principles
3. **Verify Tests Pass**: Ensure all tests pass, including the new one
4. **Refactor if Needed**: Improve code quality while maintaining test coverage
5. **Update Tests**: Add regression tests if appropriate

### When Refactoring
1. **Ensure Test Coverage**: Have comprehensive tests before refactoring
2. **Refactor Incrementally**: Make small, focused changes
3. **Run Tests Frequently**: Verify tests pass after each change
4. **Improve Design**: Apply clean architecture and SOLID principles
5. **Update Documentation**: Reflect changes in documentation

## Documentation Standards

### Code Documentation
- **XML Comments**: Use XML documentation comments for public APIs
- **Complex Logic**: Add comments explaining "why" for complex implementations
- **Avoid Obvious Comments**: Don't state what the code obviously does
- **Keep Updated**: Update comments when code changes

### Architecture Documentation
- **ARCHITECTURE.md**: Keep architecture documentation current with code changes
- **README.md**: Update feature list and usage instructions
- **Diagram Updates**: Update architecture diagrams when structure changes

### API Documentation
- **Service Documentation**: Document service methods, parameters, return types
- **Interface Documentation**: Document interface contracts and expectations
- **Example Usage**: Provide code examples for complex features

## Code Quality Enforcement

### Before Committing
- [ ] All unit tests pass
- [ ] New features have comprehensive test coverage
- [ ] Code follows naming conventions
- [ ] No compiler warnings
- [ ] Clean architecture principles maintained
- [ ] SOLID principles applied
- [ ] Documentation updated if needed

### Code Review Checklist
- [ ] Clean architecture layers respected
- [ ] Proper dependency direction maintained
- [ ] Unit tests added/updated
- [ ] Test coverage is comprehensive
- [ ] Code is readable and maintainable
- [ ] Best practices followed
- [ ] No code smells or anti-patterns
- [ ] Documentation updated

## Security Considerations
- **Input Validation**: Validate all user inputs
- **Secure Storage**: Use platform-specific secure storage for sensitive data
- **HTTPS Only**: Ensure all HTTP requests use HTTPS in production
- **Error Messages**: Don't expose sensitive information in error messages
- **Dependency Updates**: Keep dependencies updated for security patches

## Performance Guidelines
- **Async Operations**: Use async/await for all I/O operations
- **Efficient Queries**: Use appropriate LINQ methods and avoid unnecessary iterations
- **Memory Management**: Dispose resources properly, avoid memory leaks
- **UI Responsiveness**: Keep UI thread free from long-running operations
- **Lazy Loading**: Implement lazy loading for expensive operations

## Continuous Improvement
- **Stay Current**: Keep up with .NET, Blazor, and MAUI best practices
- **Refactor Regularly**: Improve code quality incrementally
- **Learn from Issues**: Document lessons learned from bugs and issues
- **Share Knowledge**: Document patterns and solutions for the team

## Project-Specific Information

### Build and Test Commands

#### Building the Project
```bash
# Build the entire solution
cd /path/to/HolyConnect
dotnet build HolyConnect.sln

# Build fails without MAUI workload installed
dotnet workload restore

# Build specific project
dotnet build src/HolyConnect.Domain/HolyConnect.Domain.csproj
```

#### Running Tests
```bash
# Run all tests (344+ tests across all layers)
dotnet test --no-build

# Run tests for specific layer
dotnet test tests/HolyConnect.Domain.Tests/
dotnet test tests/HolyConnect.Application.Tests/
dotnet test tests/HolyConnect.Infrastructure.Tests/
dotnet test tests/HolyConnect.Maui.Tests/

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"
```

#### Running the Application
```bash
# Run on Android (requires Android SDK)
cd src/HolyConnect.Maui
dotnet build -t:Run -f net10.0-android
```

### Key Project Patterns and Locations

#### Dependency Injection Registration
**Location**: `src/HolyConnect.Maui/MauiProgram.cs`

All services are registered in `MauiProgram.CreateMauiApp()`:
- **Repositories**: Singleton with `MultiFileRepository<T>` for file-based persistence
- **Services**: Scoped lifetime for application services
- **Request Executors**: Scoped, multiple implementations of `IRequestExecutor`
- **HttpClient**: Scoped for making HTTP requests

Example pattern:
```csharp
// Repositories (Singleton)
builder.Services.AddSingleton<IRepository<Environment>>(sp =>
    new MultiFileRepository<Environment>(e => e.Id, GetStoragePathSafe, "environments", e => e.Name));

// Services (Scoped)
builder.Services.AddScoped<IEnvironmentService, EnvironmentService>();

// Request Executors (Scoped, multiple implementations)
builder.Services.AddScoped<IRequestExecutor, RestRequestExecutor>();
builder.Services.AddScoped<IRequestExecutor, GraphQLRequestExecutor>();
```

#### Storage and Persistence
**Pattern**: `MultiFileRepository<T>` for better performance with large collections
**Location**: `src/HolyConnect.Infrastructure/Persistence/MultiFileRepository.cs`

- Each entity stored in separate JSON file
- Default path: `LocalApplicationData/HolyConnect/`
- Configurable via Settings service
- Organized in subdirectories: `environments/`, `collections/`, `requests/`, `history/`

#### Request Execution Pattern
**Interface**: `IRequestExecutor` in `Application/Interfaces/`
**Implementations**: In `Infrastructure/Services/`

To add a new request type:
1. Create entity in Domain (inherit from `Request`)
2. Implement `IRequestExecutor` in Infrastructure
3. Register in `MauiProgram.cs`
4. Update UI components to support new type

Existing executors:
- `RestRequestExecutor`: GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS
- `GraphQLRequestExecutor`: Queries and Mutations
- `GraphQLSubscriptionWebSocketExecutor`: GraphQL subscriptions (graphql-transport-ws)
- `GraphQLSubscriptionSSEExecutor`: GraphQL subscriptions via Server-Sent Events
- `WebSocketRequestExecutor`: Generic WebSocket connections

#### Authentication Handling
**Helper Class**: `HttpAuthenticationHelper` in `Infrastructure/Common/`

Centralized authentication logic for both HTTP and WebSocket:
- `ApplyAuthentication(HttpRequestMessage, Request)`: HTTP requests
- `ApplyAuthentication(ClientWebSocketOptions, Request)`: WebSocket connections
- Supports: None, Basic, Bearer Token
- Automatically skips Authorization header if manually provided

#### Constants and Magic Strings
**Constants Class**: `HttpConstants` in `Infrastructure/Common/`

Use constants instead of magic strings:
- Headers: `HttpConstants.Headers.Authorization`, `HttpConstants.Headers.ContentType`
- Media Types: `HttpConstants.MediaTypes.ApplicationJson`
- Auth Schemes: `HttpConstants.Auth.Basic`, `HttpConstants.Auth.Bearer`

### Testing Patterns

#### Test Framework and Libraries
- **Testing Framework**: xUnit
- **Mocking**: Moq
- **UI Testing**: bUnit (for Blazor components)

#### Test Naming Convention
Format: `MethodName_StateUnderTest_ExpectedBehavior`

Examples:
- `CreateEnvironmentAsync_WithValidData_ShouldCreateEnvironment`
- `ExecuteAsync_WithInvalidUrl_ShouldThrowException`
- `GetAllAsync_WhenRepositoryEmpty_ShouldReturnEmptyList`

#### Test Structure (AAA Pattern)
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var mockRepo = new Mock<IRepository<Entity>>();
    mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Entity());
    var service = new Service(mockRepo.Object);

    // Act
    var result = await service.MethodAsync(id);

    // Assert
    Assert.NotNull(result);
    mockRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
}
```

#### Layer-Specific Testing Approaches

**Domain Tests**: No mocking, pure business logic
```csharp
[Fact]
public void Property_WhenSet_ShouldReturnCorrectValue()
{
    var entity = new Entity { Name = "Test" };
    Assert.Equal("Test", entity.Name);
}
```

**Application Tests**: Mock repositories and external dependencies
```csharp
private readonly Mock<IRepository<Entity>> _mockRepository;
private readonly Service _service;

public ServiceTests()
{
    _mockRepository = new Mock<IRepository<Entity>>();
    _service = new Service(_mockRepository.Object);
}
```

**Infrastructure Tests**: Mock HttpClient and external services
```csharp
var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
mockHttpMessageHandler.Protected()
    .Setup<Task<HttpResponseMessage>>("SendAsync", ...)
    .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });
```

### GraphQL-Specific Features

#### Monaco Editor Integration
**Component**: `Components/Shared/CodeEditor.razor`
- Supports GraphQL syntax highlighting
- Theme-aware (light/dark mode)
- Auto-completion and IntelliSense
- Integrated with GraphQL schema viewer

#### Schema Viewer
**Component**: `Components/Shared/GraphQLSchemaViewer.razor`
- Displays queries, mutations, subscriptions, and types
- Color-coded type badges
- Field descriptions and type signatures
- Fetched via introspection query

#### GraphQL Subscriptions
Two protocols supported:
1. **WebSocket** (graphql-transport-ws protocol): `GraphQLSubscriptionWebSocketExecutor`
2. **Server-Sent Events**: `GraphQLSubscriptionSSEExecutor`

### Common Patterns Used

#### Variable Resolution
**Service**: `VariableResolver` in Application layer
**Syntax**: `{{ variableName }}`
**Precedence**: Collection variables override environment variables
**Locations**: URL, headers, query parameters, request body

#### Response Value Extraction
**Service**: `ResponseValueExtractor` in Application layer
**Patterns**: 
- JSONPath for JSON/GraphQL: `$.data.user.id`
- XPath for XML: `//user/id`
**Usage**: Ad-hoc extraction or automated via `ResponseExtraction` rules

#### Git Integration
**Service**: `GitService` in Infrastructure layer
**Library**: LibGit2Sharp
**Operations**: Initialize, commit, branch, fetch, pull, push
**Storage**: Uses same storage path as data files

### Important File Locations

#### Configuration and DI
- `src/HolyConnect.Maui/MauiProgram.cs`: Service registration and DI setup
- `src/HolyConnect.Domain/Entities/AppSettings.cs`: App settings model

#### Core Business Logic
- `src/HolyConnect.Domain/Entities/`: All domain entities
- `src/HolyConnect.Application/Services/`: Business logic services
- `src/HolyConnect.Application/Interfaces/`: Service contracts

#### Infrastructure
- `src/HolyConnect.Infrastructure/Services/`: Request executors
- `src/HolyConnect.Infrastructure/Persistence/`: Data persistence
- `src/HolyConnect.Infrastructure/Common/`: Shared helpers and constants

#### UI Components
- `src/HolyConnect.Maui/Components/Pages/`: Main pages
- `src/HolyConnect.Maui/Components/Shared/`: Reusable components
- `src/HolyConnect.Maui/Components/Layout/`: Layout components

#### Tests
- `tests/HolyConnect.Domain.Tests/`: Domain layer tests (80+ tests)
- `tests/HolyConnect.Application.Tests/`: Application layer tests (99+ tests)
- `tests/HolyConnect.Infrastructure.Tests/`: Infrastructure layer tests (154+ tests)
- `tests/HolyConnect.Maui.Tests/`: UI component tests (11+ tests)

### Documentation Files to Keep Updated

#### Architecture Documentation
- `ARCHITECTURE.md`: Layer responsibilities, design patterns, extensibility
- Keep diagram and layer descriptions synchronized with code changes
- Update when adding new request types or major features

#### Readme
- `README.md`: Feature list, getting started, usage examples
- Update feature list when adding new capabilities
- Keep build/run instructions current

#### Contributing Guide
- `CONTRIBUTING.md`: Development workflow, testing, PR process
- Update when changing development processes
- Keep code style examples current

#### This File
- `.github/copilot-instructions.md`: **ALWAYS UPDATE** when:
  - New patterns or practices are established
  - Build/test commands change
  - New layers or major components are added
  - Service registration patterns change
  - New testing approaches are adopted

### Tips for Working with This Codebase

#### When Adding New Features
1. Check if similar features exist (request executors, services)
2. Follow established patterns (especially in request execution)
3. Use existing helpers (`HttpAuthenticationHelper`, `HttpConstants`)
4. Register new services in `MauiProgram.cs`
5. Add comprehensive tests in appropriate test project
6. Update ARCHITECTURE.md if adding new patterns

#### When Fixing Bugs
1. Start with a failing test that reproduces the bug
2. Check if bug affects multiple executors (use shared helpers)
3. Verify fix doesn't break existing tests
4. Consider adding regression test

#### When Refactoring
1. All 344+ tests must pass before and after
2. Consider extracting to helper classes for duplicated logic
3. Use constants from `HttpConstants` instead of magic strings
4. Maintain clean architecture layer separation
5. Update documentation to reflect new structure

### Common Pitfalls to Avoid

1. **Don't duplicate authentication logic**: Use `HttpAuthenticationHelper`
2. **Don't use magic strings**: Use constants from `HttpConstants`
3. **Don't skip tests**: Every change needs test coverage
4. **Don't violate layer dependencies**: Domain has no dependencies, Application depends only on Domain
5. **Don't forget to register services**: New services must be registered in `MauiProgram.cs`
6. **Don't forget nullable reference types**: They're enabled project-wide
7. **Don't block UI thread**: Always use async/await for I/O operations

## Maintenance Instructions

### Keeping This File Updated

This copilot instructions file is a living document and **MUST be updated** whenever:

1. **New architectural patterns are introduced**: Document the pattern, why it was chosen, and how to use it
2. **Build or test processes change**: Update commands and examples
3. **New layers or major components are added**: Add to file locations and patterns sections
4. **Service registration patterns change**: Update DI section with new examples
5. **New testing approaches are adopted**: Document in testing patterns section
6. **Common issues are discovered**: Add to pitfalls section
7. **New tools or libraries are added**: Document their purpose and usage patterns

### Update Checklist

When making significant changes to the codebase, verify and update:
- [ ] Build and test command examples still work
- [ ] Service registration patterns are current
- [ ] File locations reflect actual structure
- [ ] Testing patterns match current practices
- [ ] Common pitfalls section includes newly discovered issues
- [ ] Architecture documentation references are accurate

### How to Update

1. Make changes incrementally as patterns evolve
2. Use concrete examples from the actual codebase
3. Keep instructions actionable and specific
4. Include file paths and command examples
5. Remove outdated information promptly
6. Version important changes in commit messages

## Summary
These instructions ensure HolyConnect maintains high code quality, follows clean architecture principles, and implements .NET and Blazor best practices. Always write comprehensive tests, keep documentation current, and follow the established patterns when adding new features or fixing issues.

**Remember**: This file is your guide to understanding the codebase quickly. Keep it updated so future you (and others) can work efficiently without re-discovering patterns and practices.
