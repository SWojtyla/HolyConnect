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

## Summary
These instructions ensure HolyConnect maintains high code quality, follows clean architecture principles, and implements .NET and Blazor best practices. Always write comprehensive tests, keep documentation current, and follow the established patterns when adding new features or fixing issues.
