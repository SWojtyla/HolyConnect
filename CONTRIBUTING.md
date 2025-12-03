# Contributing to HolyConnect

Thank you for your interest in contributing to HolyConnect! This document provides guidelines and instructions for contributing to the project.

## Code of Conduct

- Be respectful and inclusive
- Welcome newcomers and help them learn
- Focus on constructive feedback
- Maintain professional communication

## Getting Started

### Prerequisites

- .NET 10 SDK
- MAUI workload (`dotnet workload install maui-android`)
- Git
- IDE: Visual Studio 2022, VS Code, or Rider

### Setting Up Development Environment

1. Fork the repository
2. Clone your fork:
   ```bash
   git clone https://github.com/YOUR_USERNAME/HolyConnect.git
   cd HolyConnect
   ```

3. Build the solution:
   ```bash
   dotnet build HolyConnect.sln
   ```

4. Run the application:
   ```bash
   cd src/HolyConnect.Maui
   dotnet build -t:Run -f net10.0-android
   ```

## Development Workflow

### Branch Naming

- Feature: `feature/feature-name`
- Bug fix: `fix/bug-description`
- Documentation: `docs/description`
- Refactoring: `refactor/description`

### Commit Messages

Follow conventional commits format:
```
<type>(<scope>): <subject>

<body>

<footer>
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

Example:
```
feat(requests): Add WebSocket support

Implement WebSocket request type with connection management
and message streaming capabilities.

Closes #123
```

## Architecture Guidelines

### Clean Architecture Principles

1. **Domain Layer** (HolyConnect.Domain)
   - No external dependencies
   - Pure business logic
   - No framework-specific code

2. **Application Layer** (HolyConnect.Application)
   - Depends only on Domain
   - Defines interfaces
   - Implements use cases

3. **Infrastructure Layer** (HolyConnect.Infrastructure)
   - Implements Application interfaces
   - External service integrations
   - Data access implementations

4. **Presentation Layer** (HolyConnect.Maui)
   - MAUI Blazor UI
   - Consumes all layers
   - User interaction

### Code Style

- Use C# naming conventions
- Enable nullable reference types
- Follow async/await patterns
- Use dependency injection
- Write self-documenting code
- Add XML documentation for public APIs

Example:
```csharp
/// <summary>
/// Executes a request and returns the response.
/// </summary>
/// <param name="request">The request to execute.</param>
/// <returns>The response from the request execution.</returns>
public async Task<RequestResponse> ExecuteAsync(Request request)
{
    // Implementation
}
```

## Adding Features

### Adding a New Request Type

1. **Create Domain Entity** (Domain Layer)
```csharp
// src/HolyConnect.Domain/Entities/WebSocketRequest.cs
public class WebSocketRequest : Request
{
    public override RequestType Type => RequestType.WebSocket;
    public string? SubProtocol { get; set; }
    public List<string> Messages { get; set; } = new();
}
```

2. **Implement Executor** (Infrastructure Layer)
```csharp
// src/HolyConnect.Infrastructure/Services/WebSocketRequestExecutor.cs
public class WebSocketRequestExecutor : IRequestExecutor
{
    public bool CanExecute(Request request) 
        => request is WebSocketRequest;
    
    public async Task<RequestResponse> ExecuteAsync(Request request)
    {
        // Implementation
    }
}
```

3. **Register Service** (Presentation Layer)
```csharp
// src/HolyConnect.Maui/MauiProgram.cs
builder.Services.AddScoped<IRequestExecutor, WebSocketRequestExecutor>();
```

4. **Update UI**
- Add request type option in forms
- Create specific UI for WebSocket features
- Update request editor component

### Adding Tests

Create corresponding test projects:
```
tests/
├── HolyConnect.Domain.Tests/
│   └── Entities/
│       └── WebSocketRequestTests.cs
├── HolyConnect.Infrastructure.Tests/
│   └── Services/
│       └── WebSocketRequestExecutorTests.cs
```

Example test:
```csharp
[Fact]
public async Task ExecuteAsync_ValidWebSocketRequest_ReturnsResponse()
{
    // Arrange
    var executor = new WebSocketRequestExecutor();
    var request = new WebSocketRequest
    {
        Url = "wss://echo.websocket.org",
        Name = "Test WebSocket"
    };

    // Act
    var response = await executor.ExecuteAsync(request);

    // Assert
    Assert.NotNull(response);
    Assert.True(response.StatusCode >= 0);
}
```

## Pull Request Process

### Before Submitting

1. **Update from main**:
   ```bash
   git checkout main
   git pull upstream main
   git checkout your-branch
   git rebase main
   ```

2. **Run tests**:
   ```bash
   dotnet test
   ```

3. **Build solution**:
   ```bash
   dotnet build HolyConnect.sln
   ```

4. **Check code style**:
   ```bash
   dotnet format
   ```

### PR Checklist

- [ ] Code follows project style guidelines
- [ ] All tests pass
- [ ] New tests added for new features
- [ ] Documentation updated
- [ ] Commit messages follow conventions
- [ ] No merge conflicts
- [ ] PR description is clear and complete

### PR Description Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
Describe testing performed

## Screenshots (if applicable)
Add screenshots for UI changes

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Tests added/updated
- [ ] Documentation updated
```

## Testing Guidelines

### Unit Tests
- Test business logic in isolation
- Mock external dependencies
- One assertion per test (when possible)
- Use descriptive test names

### Integration Tests
- Test component interactions
- Use real implementations when practical
- Test error scenarios

### UI Tests
- Test user workflows
- Verify component rendering
- Test navigation and state management

## Documentation

### Code Documentation
- XML comments for public APIs
- Inline comments for complex logic
- README files for components

### Architecture Documentation
- Update ARCHITECTURE.md for significant changes
- Document design decisions
- Explain patterns and practices

## Issues and Discussions

### Reporting Bugs

Include:
- Description of the issue
- Steps to reproduce
- Expected behavior
- Actual behavior
- Environment (OS, .NET version)
- Screenshots/logs

### Feature Requests

Include:
- Use case description
- Proposed solution
- Alternative solutions considered
- Benefits to users

### Questions

- Check existing issues and docs
- Provide context
- Be specific
- Show what you've tried

## Review Process

### For Contributors
- Respond to review comments
- Make requested changes
- Ask for clarification when needed
- Be patient and respectful

### For Reviewers
- Be constructive and kind
- Explain reasoning
- Suggest improvements
- Approve when ready

## Recognition

Contributors will be:
- Listed in CONTRIBUTORS.md
- Mentioned in release notes
- Credited in commit messages

## Getting Help

- GitHub Discussions for questions
- GitHub Issues for bugs/features
- Check documentation first
- Be specific and provide context

## Resources

- [.NET MAUI Documentation](https://docs.microsoft.com/dotnet/maui/)
- [MudBlazor Documentation](https://mudblazor.com/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [C# Coding Conventions](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)

Thank you for contributing to HolyConnect! 🙏
