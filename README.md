# HolyConnect

A powerful API testing tool built with .NET 10 MAUI and MudBlazor, designed as a Postman-like application following clean architecture principles.

## Features

- 🌐 **REST API Support**: Send GET, POST, PUT, DELETE, PATCH, HEAD, and OPTIONS requests
- 📊 **GraphQL Support**: Test GraphQL queries and mutations with ease
- 🔔 **GraphQL Subscriptions**: Support for GraphQL subscriptions via WebSocket (graphql-transport-ws protocol) and Server-Sent Events (SSE)
- 🔌 **WebSocket Support**: Connect to WebSocket servers for real-time bidirectional communication with streaming message log
- ⌨️ **Keyboard Shortcuts**: Navigate quickly with keyboard shortcuts (Ctrl+K for global search, Ctrl+H for home, and more)
- 🔍 **Global Search**: Fast fuzzy search across all environments, collections, requests, and flows (Ctrl+K)
- 🗂️ **Environment Management**: Organize your API requests by environments
- 📁 **Collection Hierarchy**: Create nested collections to organize requests
- 💾 **Request Storage**: Save and reuse your API requests
- 🔤 **Variables**: Use environment and collection variables with `{{ variableName }}` syntax (like Postman/Bruno)
- ✨ **Dynamic Test Data Generation**: Generate realistic fake data for testing with constraints (names, emails, dates, numbers, etc.)
- 🔒 **Secret Variables**: Mark sensitive variables as secret to exclude them from git commits
- 🔄 **Git Integration**: Full version control with git support (initialize, commit, branch, push, pull, commit history, selective staging)
- 📋 **Response Extraction**: Extract values from responses using JSONPath/XPath and save to clipboard or variables
- 🔀 **Flows**: Chain multiple requests together in sequence, passing variables between steps for complex workflows
- 📥 **Import Support**: Import requests from curl commands and Bruno files (.bru)
  - **Single File Import**: Import individual Bruno or curl requests
  - **Folder Import**: Import complete Bruno collections with folder hierarchy preserved
  - Automatically creates collections and subcollections matching folder structure
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

For detailed architecture documentation, see **[ARCHITECTURE.md](ARCHITECTURE.md)**.

## Documentation

📚 **[Complete Documentation Index](DOCUMENTATION_INDEX.md)** - Find all documentation organized by task, topic, and role

### Quick Links for Developers

**Getting Started**:
- 🎯 **[Quick Reference](.github/QUICK_REFERENCE.md)** - Common tasks and code snippets
- 🧭 **[UI Navigation Guide](.github/UI_NAVIGATION_GUIDE.md)** - Navigation patterns and routing
- 🧩 **[Component Library](.github/COMPONENT_LIBRARY.md)** - Reusable UI components
- ⚠️ **[Common Mistakes](.github/copilot-mistakes.md)** - Pitfalls to avoid

**Comprehensive Guides**:
- 📖 **[Copilot Instructions](.github/copilot-instructions.md)** - Complete development guidelines
- 🏛️ **[Architecture](ARCHITECTURE.md)** - System design and principles
- 🤝 **[Contributing](CONTRIBUTING.md)** - Development workflow

**Feature Documentation**:
- ⚡ **[Flows Feature](docs/FLOWS_FEATURE.md)** - Sequential request execution
- 📥 **[Bruno Import](docs/BRUNO_IMPORT.md)** - Import from Bruno API client

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

HolyConnect supports two types of variables: **static variables** with fixed values and **dynamic variables** that generate realistic fake test data.

#### Static Variables

1. Define variables in environments or collections
2. Use `{{ variableName }}` syntax in your requests
3. Collection variables override environment variables
4. See the [Variables Wiki](/wiki/variables) for detailed documentation

#### Dynamic Test Data Variables

Dynamic variables automatically generate realistic fake data for testing:

1. **Creating Dynamic Variables**:
   - In the Environment or Collection edit page, scroll to the "Dynamic Test Data Variables" section
   - Click "Add Dynamic Variable"
   - Set a variable name (e.g., `firstName`, `email`, `birthdate`)
   - Select a data type from 25+ options including:
     - **Person data**: FirstName, LastName, FullName, Email, PhoneNumber, Username
     - **Numbers**: Integer, Decimal (with min/max constraints)
     - **Dates**: Date, DatePast, DateFuture, DateTime (with age constraints)
     - **Internet**: URL, IPAddress, MacAddress
     - **Identifiers**: GUID/UUID
     - **Address**: StreetAddress, City, Country, ZipCode
     - **Text**: Word, Sentence, Paragraph
     - And more...

2. **Configuring Constraints**:
   - Click the settings icon next to a dynamic variable
   - For **numbers**: Set minimum and maximum values (e.g., number between 1-100)
   - For **dates**: Set minimum/maximum age (e.g., birthdate of someone 18-65 years old)
   - Constraints ensure generated data meets your testing requirements

3. **Using Dynamic Variables**:
   - Use dynamic variables just like static variables: `{{ variableName }}`
   - Each request execution generates fresh data
   - Perfect for load testing, data variety, and automated scenarios
   - Example: `{ "name": "{{ firstName }} {{ lastName }}", "email": "{{ email }}", "age": {{ randomAge }} }`

4. **Variable Precedence**:
   - Static variables always take precedence over dynamic variables
   - Priority: Request static > Collection static > Environment static > Request dynamic > Collection dynamic > Environment dynamic

#### Secret Variables

HolyConnect supports secret variables for sensitive data like API keys, tokens, and passwords:

1. **Marking Variables as Secret**:
   - When editing an environment or collection, add or edit a variable
   - Check the "Secret" checkbox (with a lock icon) to mark it as secret
   - Secret variables are displayed as password fields with a visibility toggle
   
2. **How Secret Variables Work**:
   - Secret variables are stored in a separate `secrets/` directory
   - Each environment/collection has its own secrets file: `secrets/environment-{id}-secrets.json`
   - The `secrets/` directory and `*secrets*.json` files are automatically excluded from git
   - Secret variable names are tracked in the main entity file, but values are stored separately
   - When loading an environment/collection, secrets are automatically merged with regular variables
   
3. **Best Practices**:
   - Always mark sensitive data (API keys, tokens, passwords) as secret
   - Regular variables are committed to git, secret variables are not
   - Secret variables work the same way as regular variables in requests using `{{ variableName }}` syntax
   - Collection secret variables override environment secret variables, just like regular variables
   
4. **Git Behavior**:
   - When you commit changes, regular variables will be included in the commit
   - Secret variables will NOT be committed to git (they're in `.gitignore`)
   - This allows you to share your request collections without exposing sensitive credentials

### Using Git Version Control

1. Navigate to the Git page from the menu
2. Initialize a git repository for your storage path
3. Create branches for different versions of your collections
4. **View file changes** with color-coded status indicators
5. **Stage/unstage specific files** or commit all changes at once
6. Commit your changes with descriptive messages
7. **View commit history** to see past changes
8. Switch between branches to work on different versions
9. Fetch, pull, and push to sync with remote repositories
10. View the current branch in the app bar header

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

### Importing Requests

HolyConnect supports importing requests from various formats to help you migrate from other tools or quickly set up requests from documentation.

1. **Importing from curl**:
   - Navigate to the Import page from the main menu
   - Select the target environment where you want to import the request
   - (Optional) Select a collection to organize the imported request
   - Paste your curl command in the text area
   - Click "Import" to create the request

2. **Supported curl features**:
   - HTTP methods (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS)
   - URL with query parameters
   - Request headers (-H or --header)
   - Request body (-d, --data, --data-raw, --data-binary)
   - Basic authentication (-u or --user)
   - Bearer token authentication (Authorization header)
   - Multi-line curl commands with backslash continuation

3. **Importing from Bruno files**:
   - **Single File**: Click "Browse File" to select a single .bru file
   - **Folder Import**: Enter a folder path to import an entire Bruno collection
   - The folder structure is preserved as collections and subcollections
   
4. **Bruno Folder Import Features**:
   - **Preserves hierarchy**: Each folder becomes a collection, subfolders become subcollections
   - **Automatic organization**: Requests are automatically placed in their parent folder's collection
   - **Nested support**: Unlimited folder depth is supported
   - **Mixed types**: Handles both REST and GraphQL requests in the same import
   - **Detailed reporting**: Shows success/failure statistics and warnings
   
5. **Example Bruno folder structure**:
   ```
   my-api-collection/
   ├── api/
   │   ├── users/
   │   │   ├── get-users.bru
   │   │   └── create-user.bru
   │   └── posts/
   │       └── get-posts.bru
   └── auth/
       └── login.bru
   ```
   
   After import, this creates:
   - Collection: "my-api-collection"
     - Subcollection: "api"
       - Subcollection: "users" (with "Get Users" and "Create User" requests)
       - Subcollection: "posts" (with "Get Posts" request)
     - Subcollection: "auth" (with "Login" request)

6. **Example curl commands**:
   ```bash
   # Simple GET request
   curl 'https://api.github.com/users/octocat'
   
   # POST with JSON body
   curl -X POST 'https://api.example.com/users' \
     -H 'Content-Type: application/json' \
     -d '{"name":"John","email":"john@example.com"}'
   
   # Request with authentication
   curl -H 'Authorization: Bearer token123' 'https://api.example.com/protected'
   ```

7. **Coming soon**:
   - Postman collection import
   - OpenAPI/Swagger import

### Keyboard Shortcuts

HolyConnect provides comprehensive keyboard shortcuts to speed up your workflow. Press `?` from anywhere in the app to see all available shortcuts.

#### Navigation Shortcuts
- **Ctrl+K** (⌘K on Mac): Open global search - quickly find and navigate to environments, collections, requests, or flows
- **Ctrl+H**: Go to home page
- **Ctrl+E**: Go to environments/manage variables
- **Ctrl+G**: Go to Git management
- **Ctrl+Shift+H**: Go to request history
- **Ctrl+Shift+I**: Go to import page
- **Ctrl+Shift+F**: Go to flows
- **Ctrl+,**: Go to settings

#### Quick Actions
- **Ctrl+N**: Create new request
- **Ctrl+Shift+N**: Create new collection
- **Ctrl+Shift+E**: Create new environment

#### Help
- **?**: Show keyboard shortcuts reference

**Note**: On Mac, use **⌘** (Command) instead of **Ctrl** for all shortcuts.

**Global Search** (Ctrl+K):
- Search across all environments, collections, requests, and flows
- Fuzzy matching automatically finds relevant items
- Navigate results with arrow keys, select with Enter
- Shows contextual information like parent collections and request types
- Results are sorted by relevance

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