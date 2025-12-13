# Bruno Import Functionality

This document describes how HolyConnect imports data from Bruno API client files and folders.

## Overview

HolyConnect supports importing requests, collections, and environments from Bruno, a popular API testing tool. The import functionality works with both individual `.bru` files and entire Bruno folder structures.

## What Gets Imported

### 1. Requests
Individual `.bru` files containing API request definitions are imported as Request entities in HolyConnect.

**Supported Request Types:**
- REST requests (GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS)
- GraphQL queries, mutations, and subscriptions

**Request Features:**
- URL and HTTP method
- Headers
- Authentication (Bearer token, Basic auth)
- Request body (JSON, XML, Text, HTML, JavaScript)
- GraphQL queries and variables

### 2. Environments
Environment files from the `environments/` subfolder are imported as Environment entities.

**Environment Features:**
- Variables (key-value pairs)
- Secret variables (marked with `vars:secret`)

**File Format:**
```bruno
vars {
  baseUrl: https://api.example.com
  apiKey: secret-key-123
  timeout: 5000
}

vars:secret [
  apiKey
]
```

### 3. Collections
Folder structures are imported as nested Collections in HolyConnect.

**Collection Features:**
- Collection hierarchy matching folder structure
- Collection variables from `collection.bru` files
- Collection metadata from `bruno.json` files

**collection.bru Format:**
```bruno
vars {
  sharedEndpoint: /api/v1
  collectionVar: value
}

vars:secret [
  collectionVar
]
```

**bruno.json Format:**
```json
{
  "version": "1",
  "name": "My API Collection",
  "type": "collection"
}
```

## Bruno Folder Structure

A typical Bruno folder structure:

```
my-api-collection/
├── environments/           # Environment definitions
│   ├── development.bru     # Dev environment variables
│   ├── staging.bru         # Staging environment variables
│   └── production.bru      # Production environment variables
├── bruno.json              # Collection metadata (name, version)
├── collection.bru          # Collection-level variables
├── users/                  # Subfolder (becomes nested collection)
│   ├── get-user.bru        # Individual request file
│   └── create-user.bru     # Individual request file
└── api/
    └── v1/
        ├── endpoint1.bru
        └── endpoint2.bru
```

## Import Methods

### Single File Import

Import an individual Bruno request file:

```csharp
var result = await importService.ImportFromBrunoAsync(
    brunoFileContent: fileContent,
    collectionId: targetCollectionId,  // Optional
    customName: "My Custom Request"     // Optional
);

if (result.Success)
{
    var importedRequest = result.ImportedRequest;
}
```

### Single Environment Import

Import an individual Bruno environment file:

```csharp
var result = await importService.ImportFromBrunoEnvironmentAsync(
    brunoEnvironmentContent: fileContent,
    environmentName: "Development"
);

if (result.Success)
{
    var importedEnvironment = result.ImportedEnvironments.First();
}
```

### Folder Import

Import an entire Bruno collection folder:

```csharp
var result = await importService.ImportFromBrunoFolderAsync(
    folderPath: "/path/to/bruno/collection",
    parentCollectionId: null  // Optional parent collection
);

if (result.Success)
{
    Console.WriteLine($"Imported {result.ImportedEnvironments.Count} environments");
    Console.WriteLine($"Imported {result.ImportedCollections.Count} collections");
    Console.WriteLine($"Imported {result.ImportedRequests.Count} requests");
}
```

## Import Process

When importing a Bruno folder, HolyConnect:

1. **Imports Environments First**
   - Scans the `environments/` subfolder
   - Parses each `.bru` file as an environment
   - Extracts variables and secret variable names
   - Creates Environment entities

2. **Creates Collection Hierarchy**
   - Reads `bruno.json` for collection name (if present)
   - Parses `collection.bru` for collection variables (if present)
   - Creates a root collection for the folder
   - Recursively processes subfolders as nested collections

3. **Imports Requests**
   - Scans all `.bru` files (excluding `collection.bru`)
   - Parses each file based on its type (REST or GraphQL)
   - Associates requests with their parent collection
   - Skips the `environments/` folder during request scanning

## Import Result

The `ImportResult` object contains:

```csharp
public class ImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Single file import
    public Request? ImportedRequest { get; set; }
    
    // Folder import
    public List<Request> ImportedRequests { get; set; }
    public List<Collection> ImportedCollections { get; set; }
    public List<Environment> ImportedEnvironments { get; set; }
    public List<string> Warnings { get; set; }
    
    // Statistics
    public int TotalFilesProcessed { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
}
```

## Variable Resolution

After importing, variables work with HolyConnect's variable system:

- **Environment variables**: Available globally when environment is active
- **Collection variables**: Available to all requests in that collection
- **Precedence**: Collection variables override environment variables

Example request using variables:
```bruno
get {
  url: {{baseUrl}}{{sharedEndpoint}}/users/123
}

headers {
  Authorization: Bearer {{apiKey}}
}
```

## Best Practices

1. **Organization**: Keep Bruno collections well-organized with clear folder structures
2. **Environments**: Use the `environments/` folder for all environment-specific variables
3. **Collection Variables**: Use `collection.bru` for variables shared across requests in that collection
4. **Secret Variables**: Always mark sensitive values in `vars:secret` section
5. **Testing**: Import to a test collection first to verify structure

## Example: Complete Import

Here's a complete example of importing a Bruno collection:

```csharp
// Service injection
private readonly IImportService _importService;

// Import Bruno collection
var result = await _importService.ImportFromBrunoFolderAsync(
    "/path/to/my-api-collection"
);

if (result.Success)
{
    // Environments imported
    foreach (var env in result.ImportedEnvironments)
    {
        Console.WriteLine($"Environment: {env.Name}");
        Console.WriteLine($"  Variables: {env.Variables.Count}");
        Console.WriteLine($"  Secrets: {env.SecretVariableNames.Count}");
    }
    
    // Collections imported
    foreach (var collection in result.ImportedCollections)
    {
        Console.WriteLine($"Collection: {collection.Name}");
        Console.WriteLine($"  Variables: {collection.Variables.Count}");
    }
    
    // Requests imported
    foreach (var request in result.ImportedRequests)
    {
        Console.WriteLine($"Request: {request.Name}");
    }
}
else
{
    Console.WriteLine($"Import failed: {result.ErrorMessage}");
    foreach (var warning in result.Warnings)
    {
        Console.WriteLine($"  Warning: {warning}");
    }
}
```

## Limitations

- Only `.bru` files are imported; other file types are ignored
- Collection descriptions are auto-generated based on folder path
- Dynamic variables from Bruno are not currently imported
- Bruno scripts (pre-request, post-request) are not imported

## Related Documentation

- [Architecture](../ARCHITECTURE.md) - Overall application architecture
- [Contributing](../CONTRIBUTING.md) - How to contribute to import functionality
