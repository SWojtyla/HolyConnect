# HolyConnect

A powerful API testing tool built with .NET 10 MAUI and MudBlazor, designed as a Postman-like application following clean architecture principles.

## Features

- 🌐 **REST API Support**: Send GET, POST, PUT, DELETE, PATCH, HEAD, and OPTIONS requests
- 📊 **GraphQL Support**: Test GraphQL queries and mutations with ease
- 🗂️ **Environment Management**: Organize your API requests by environments
- 📁 **Collection Hierarchy**: Create nested collections to organize requests
- 💾 **Request Storage**: Save and reuse your API requests
- 🎨 **Clean UI**: Modern interface built with MudBlazor components
- 🏗️ **Extensible Architecture**: Built with clean architecture principles for easy extension

## Architecture

The project follows clean architecture principles with the following layers:

### Domain Layer (`HolyConnect.Domain`)
- Core entities: Environment, Collection, Request (REST, GraphQL)
- Business logic and domain rules

### Application Layer (`HolyConnect.Application`)
- Use cases and business workflows
- Service interfaces (IRepository, IRequestExecutor)
- Application services (EnvironmentService, CollectionService, RequestService)

### Infrastructure Layer (`HolyConnect.Infrastructure`)
- Data persistence (InMemoryRepository)
- HTTP client implementations (RestRequestExecutor, GraphQLRequestExecutor)
- External service integrations

### Presentation Layer (`HolyConnect.Maui`)
- MAUI Blazor Hybrid UI
- MudBlazor components
- Razor pages and components

## Getting Started

### Prerequisites

- .NET 10 SDK
- MAUI workload installed (`dotnet workload install maui-android` for Android)

### Building the Project

```bash
# Clone the repository
git clone https://github.com/SWojtyla/HolyConnect.git
cd HolyConnect

# Build the solution
dotnet build HolyConnect.sln
```

### Running the Application

```bash
# Run on Android
cd src/HolyConnect.Maui
dotnet build -t:Run -f net10.0-android
```

## Project Structure

```
HolyConnect/
├── src/
│   ├── HolyConnect.Domain/          # Domain entities and business logic
│   ├── HolyConnect.Application/     # Application services and interfaces
│   ├── HolyConnect.Infrastructure/  # Data access and external services
│   └── HolyConnect.Maui/           # MAUI Blazor UI
├── tests/                           # Unit and integration tests
└── HolyConnect.sln                 # Solution file
```

## Usage

### Creating an Environment

1. Click "Create New Environment" on the home page
2. Enter environment name and optional description
3. Add environment variables (key-value pairs)
4. Click "Create"

### Creating a Collection

1. Navigate to an environment
2. Click the "+" icon next to the environment name
3. Enter collection name and description
4. Click "Create"

### Creating a Request

1. Select a collection
2. Click "New Request"
3. Enter request details (name, URL, method, headers, body)
4. Click "Create"

### Executing a Request

1. Open a saved request
2. Modify parameters if needed
3. Click "Send"
4. View response (status, headers, body, timing)

## Extending the Application

The application is designed to be easily extensible:

### Adding a New Request Type

1. Create a new entity in `HolyConnect.Domain/Entities` inheriting from `Request`
2. Implement `IRequestExecutor` in `HolyConnect.Infrastructure/Services`
3. Register the executor in `MauiProgram.cs`
4. Update the UI to support the new request type

Example:
```csharp
public class WebSocketRequest : Request
{
    public override RequestType Type => RequestType.WebSocket;
    // Add WebSocket-specific properties
}
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the terms found in the LICENSE file.

## Acknowledgments

- Built with [.NET MAUI](https://dotnet.microsoft.com/apps/maui)
- UI components from [MudBlazor](https://mudblazor.com/)
- Inspired by [Postman](https://www.postman.com/)