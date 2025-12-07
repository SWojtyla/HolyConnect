# HolyConnect

A powerful API testing tool built with .NET 10 MAUI and MudBlazor, designed as a Postman-like application following clean architecture principles.

## Features

- 🌐 **REST API Support**: Send GET, POST, PUT, DELETE, PATCH, HEAD, and OPTIONS requests
- 📊 **GraphQL Support**: Test GraphQL queries and mutations with ease
- 🔔 **GraphQL Subscriptions**: Support for GraphQL subscriptions via WebSocket (graphql-transport-ws protocol) and Server-Sent Events (SSE)
- 🔌 **WebSocket Support**: Connect to WebSocket servers for real-time bidirectional communication
- 🗂️ **Environment Management**: Organize your API requests by environments
- 📁 **Collection Hierarchy**: Create nested collections to organize requests
- 💾 **Request Storage**: Save and reuse your API requests
- 🔤 **Variables**: Use environment and collection variables with `{{ variableName }}` syntax (like Postman/Bruno)
- 🔄 **Git Integration**: Version control your collections with git support (initialize, commit, branch, push, pull)
- 📋 **Response Extraction**: Extract values from responses using JSONPath/XPath and save to clipboard or variables
- 🔀 **Flows**: Chain multiple requests together in sequence, passing variables between steps for complex workflows
- 🎨 **Clean UI**: Modern interface built with MudBlazor components
- 🏗️ **Extensible Architecture**: Built with clean architecture principles for easy extension

## Architecture

The project follows clean architecture principles with the following layers:

### Domain Layer (`HolyConnect.Domain`)
- Core entities: Environment, Collection, Request (REST, GraphQL, WebSocket), Flow
- Business logic and domain rules

### Application Layer (`HolyConnect.Application`)
- Use cases and business workflows
- Service interfaces (IRepository, IRequestExecutor, IFlowService)
- Application services (EnvironmentService, CollectionService, RequestService, FlowService)

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
4. Use variables with `{{ variableName }}` syntax in URL, headers, query params, or body
5. Click "Create"

### Executing a Request

1. Open a saved request
2. Modify parameters if needed (variables will be automatically resolved)
3. Click "Send"
4. View response (status, headers, body, timing)

### Using Variables

1. Define variables in environments or collections
2. Use `{{ variableName }}` syntax in your requests
3. Collection variables override environment variables
4. See the [Variables Wiki](/wiki/variables) for detailed documentation

### Using Git Version Control

1. Navigate to the Git page from the menu
2. Initialize a git repository for your storage path
3. Create branches for different versions of your collections
4. Commit your changes with descriptive messages
5. Switch between branches to work on different versions
6. Fetch, pull, and push to sync with remote repositories
7. View the current branch in the app bar header

### Extracting Response Values

1. **Ad-hoc extraction** (Extract tab in response):
   - Enter a JSONPath pattern (e.g., `$.data.user.id`) for JSON/GraphQL
   - Or XPath pattern (e.g., `//user/id`) for XML
   - Click "Extract" to see the value
   - Click "Copy to Clipboard" to copy the extracted value

2. **Automated extraction** (Extractions tab in request):
   - Add extraction rules to automatically capture values after each request
   - Specify the pattern, variable name, and save location (environment or collection)
   - Extracted values are automatically saved to variables for reuse in subsequent requests

**Example patterns:**
- JSON: `$.data.user.id`, `$.items[0].name`, `$.response.token`
- XML: `//user/id`, `//items/item[1]/name`, `//response/token`

### Creating and Using Flows

Flows allow you to execute multiple requests in sequence, where variables extracted from one step are automatically available to subsequent steps.

1. **Creating a Flow**:
   - Navigate to an environment
   - Click the "+" button and select "New Flow"
   - Give your flow a name and optional description
   - Add steps by selecting requests to execute in order
   - Configure each step:
     - **Enabled**: Toggle whether the step should execute
     - **Continue on Error**: If enabled, the flow continues even if this step fails
     - **Delay**: Optional delay in milliseconds before executing this step
   - Reorder steps using the up/down arrows
   - Click "Create Flow"

2. **Executing a Flow**:
   - Select a flow from the sidebar
   - Click "Execute Flow" to run all steps in sequence
   - View real-time execution status and results
   - Each step shows:
     - Execution status (Success, Failed, Skipped)
     - Response time and status code
     - Full response details

3. **Using Variables in Flows**:
   - Configure response extractions on individual requests
   - When a request is executed as part of a flow, extracted values are automatically saved
   - Subsequent steps in the flow can use these variables with `{{ variableName }}` syntax
   - Example workflow:
     1. **Login Request**: Extract authentication token from response → save as `authToken`
     2. **Get User Request**: Use `{{ authToken }}` in Authorization header
     3. **Update User Request**: Use `{{ authToken }}` and user data from previous step

**Use Cases for Flows:**
- Authentication flows (login → get token → make authenticated requests)
- Multi-step data creation (create user → create profile → link accounts)
- Testing data dependencies (create order → add items → process payment)
- End-to-end workflow testing with variable passing

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