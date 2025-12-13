# Bruno Import Implementation Summary

## Problem Statement
Double check if the import from Bruno functionality works with the new way environments and collections work. Both for a single file and for a folder. Should be able to import their environments too - both environments and collections.

## Solution Implemented

The Bruno import functionality has been fully updated to support the new architecture where environments and collections are independent entities. The implementation now correctly imports:

1. **Environments** from `environments/` folder
2. **Collection variables** from `collection.bru` files
3. **Collection metadata** from `bruno.json` files
4. **Requests** maintaining the folder hierarchy as nested collections

## What Was Changed

### 1. Domain Layer
**File:** `src/HolyConnect.Domain/Entities/ImportResult.cs`
- Added `ImportedEnvironments` list to track imported environment entities

### 2. Application Layer
**File:** `src/HolyConnect.Application/Interfaces/IImportService.cs`
- Added new method: `ImportFromBrunoEnvironmentAsync(string brunoEnvironmentContent, string environmentName)`
- Updated documentation for `ImportFromBrunoFolderAsync` to mention environment import

### 3. Infrastructure Layer

**File:** `src/HolyConnect.Infrastructure/Services/ImportStrategies/BrunoImportStrategy.cs`
- Added `ParseEnvironment()` method to parse Bruno environment files
- Added `ParseCollectionVariables()` to extract variables from collection.bru
- Added `ParseCollectionSecretVariables()` to extract secret variable names
- Updated `ParseBrunoSections()` to handle both braces `{}` and brackets `[]` for section delimiters
  - Braces used for: vars, meta, headers, body, auth, etc.
  - Brackets used for: vars:secret arrays

**File:** `src/HolyConnect.Infrastructure/Services/ImportService.cs`
- Added `IEnvironmentService` dependency
- Implemented `ImportFromBrunoEnvironmentAsync()` for single environment file import
- Updated `ImportFromBrunoFolderAsync()`:
  - Calls `ImportEnvironmentsFromFolderAsync()` to process `environments/` subfolder
  - Parses `bruno.json` for collection name
  - Parses `collection.bru` for collection variables
  - Skips `environments/` folder during recursive request processing
- Added `ImportEnvironmentsFromFolderAsync()` helper method
- Updated `ProcessFolderAsync()` to handle collection configuration files

### 4. Tests

**File:** `tests/HolyConnect.Infrastructure.Tests/Services/ImportStrategies/BrunoImportStrategyTests.cs`
Added 6 new tests:
- `ParseEnvironment_WithValidEnvironmentFile_ShouldReturnEnvironment`
- `ParseEnvironment_WithEmptyContent_ShouldReturnNull`
- `ParseEnvironment_WithOnlyVariables_ShouldReturnEnvironmentWithoutSecrets`
- `ParseEnvironment_WithMultipleSecretVariables_ShouldReturnAllSecrets`
- `ParseCollectionVariables_WithValidContent_ShouldReturnVariables`
- `ParseCollectionVariables_WithEmptyContent_ShouldReturnEmptyDictionary`
- `ParseCollectionSecretVariables_WithValidContent_ShouldReturnSecrets`

**File:** `tests/HolyConnect.Infrastructure.Tests/Services/ImportServiceFolderTests.cs`
Updated constructor to include `IEnvironmentService` mock
Added 5 new tests:
- `ImportFromBrunoFolderAsync_WithEnvironmentsFolder_ShouldImportEnvironments`
- `ImportFromBrunoFolderAsync_WithCollectionBru_ShouldImportCollectionVariables`
- `ImportFromBrunoFolderAsync_WithBrunoJson_ShouldUseCollectionName`
- `ImportFromBrunoFolderAsync_WithEnvironmentsAndRequests_ShouldImportBoth`
- `ImportFromBrunoFolderAsync_CompleteStructure_ShouldImportEverythingCorrectly` (comprehensive integration test)

**File:** `tests/HolyConnect.Infrastructure.Tests/Services/ImportServiceTests.cs`
Updated constructor to include `IEnvironmentService` mock

### 5. Documentation

**File:** `docs/BRUNO_IMPORT.md` (NEW)
Comprehensive documentation covering:
- What gets imported (requests, environments, collections)
- Bruno file format examples
- Folder structure
- Import methods and API
- Import process flow
- Import result structure
- Variable resolution
- Best practices
- Complete code examples

## Bruno Folder Structure Support

The implementation now fully supports the standard Bruno folder structure:

```
my-api-collection/
├── environments/              # ✅ Imported as Environment entities
│   ├── development.bru
│   ├── staging.bru
│   └── production.bru
├── bruno.json                 # ✅ Used for collection name
├── collection.bru             # ✅ Parsed for collection variables
├── users/                     # ✅ Becomes nested collection
│   ├── get-user.bru
│   └── create-user.bru
└── api/
    └── v1/
        ├── endpoint1.bru
        └── endpoint2.bru
```

## Bruno File Format Support

### Environment File Format
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

### Collection Variables File Format
```bruno
vars {
  sharedEndpoint: /api/v1
  collectionVar: value
}

vars:secret [
  collectionVar
]
```

### Collection Metadata File Format
```json
{
  "version": "1",
  "name": "My API Collection",
  "type": "collection"
}
```

## Import Workflow

### Single File Import
1. Parse .bru file content
2. Determine request type (REST or GraphQL)
3. Extract all request details (URL, headers, auth, body)
4. Create and save Request entity

### Single Environment Import
1. Parse environment .bru file
2. Extract variables from `vars {}` section
3. Extract secret names from `vars:secret []` section
4. Create and save Environment entity

### Folder Import
1. **Import Environments** (first pass)
   - Scan `environments/` subfolder for .bru files
   - Parse each as environment
   - Create Environment entities
   
2. **Create Collection Hierarchy** (second pass)
   - Read `bruno.json` for collection name (optional)
   - Parse `collection.bru` for variables (optional)
   - Create root collection
   - Recursively process subfolders (skipping `environments/`)
   
3. **Import Requests** (during recursive processing)
   - Scan all .bru files (excluding `collection.bru`)
   - Parse each request
   - Associate with parent collection
   - Create Request entities

## Test Coverage

### Unit Tests
- **BrunoImportStrategy**: 16 tests (all passing)
  - Request parsing (REST and GraphQL)
  - Environment parsing
  - Collection variables parsing
  
- **ImportServiceFolder**: 14 tests (all passing)
  - Empty folder handling
  - Single file import
  - Multiple files import
  - Nested folder structure
  - Environment import
  - Collection variables
  - Integration test

### Integration Tests
- Complete folder structure with environments, collections, and requests
- Verifies end-to-end import process

### Test Statistics
- **71/71** import-related tests passing ✅
- **311/312** infrastructure tests passing (1 unrelated failure in GitServiceTests)

## API Usage Examples

### Import Single Environment
```csharp
var result = await importService.ImportFromBrunoEnvironmentAsync(
    brunoEnvironmentContent: fileContent,
    environmentName: "Development"
);

if (result.Success)
{
    var env = result.ImportedEnvironments.First();
    Console.WriteLine($"Imported {env.Variables.Count} variables");
}
```

### Import Folder with Everything
```csharp
var result = await importService.ImportFromBrunoFolderAsync(
    folderPath: "/path/to/bruno/collection"
);

if (result.Success)
{
    Console.WriteLine($"Environments: {result.ImportedEnvironments.Count}");
    Console.WriteLine($"Collections: {result.ImportedCollections.Count}");
    Console.WriteLine($"Requests: {result.ImportedRequests.Count}");
}
```

## Compatibility with New Architecture

The implementation is fully compatible with the refactored architecture where:
- **Environments** are independent entities that only store variables
- **Collections** are independent hierarchical containers for requests
- **Active Environment** is a global setting (not tied to collections)
- **Collection Variables** can override environment variables

## Key Design Decisions

1. **Parser Enhancement**: Extended `ParseBrunoSections()` to handle both `{}` and `[]` delimiters
   - Maintains backward compatibility with existing request parsing
   - Enables parsing of `vars:secret []` arrays

2. **Variable Name Matching**: Uses `StringComparer.OrdinalIgnoreCase`
   - More user-friendly (like Postman)
   - Prevents issues with variable name casing

3. **Environment Processing**: Separate pass before collection processing
   - Ensures all environments are available before requests are created
   - Cleaner separation of concerns

4. **Collection Variables**: Parsed and updated after collection creation
   - Allows collection to be created with basic info first
   - Variables added via update operation

## Future Enhancements (Not in Scope)

These could be added in future iterations:
- Bruno scripts (pre-request, post-request)
- Bruno tests/assertions
- Dynamic variables (like `{{$timestamp}}`)
- Environment descriptions from metadata
- Collection descriptions from metadata

## Verification

To verify the implementation works correctly:

1. Create a Bruno collection with environments
2. Export/copy the folder
3. Use `ImportFromBrunoFolderAsync()` to import
4. Verify:
   - All environments appear in HolyConnect
   - Collection hierarchy matches folder structure
   - Collection variables are present
   - All requests are imported with correct details
   - Secret variables are marked correctly

## Related Files

- Implementation: `src/HolyConnect.Infrastructure/Services/ImportStrategies/BrunoImportStrategy.cs`
- Service: `src/HolyConnect.Infrastructure/Services/ImportService.cs`
- Interface: `src/HolyConnect.Application/Interfaces/IImportService.cs`
- Tests: `tests/HolyConnect.Infrastructure.Tests/Services/ImportStrategies/BrunoImportStrategyTests.cs`
- Tests: `tests/HolyConnect.Infrastructure.Tests/Services/ImportServiceFolderTests.cs`
- Documentation: `docs/BRUNO_IMPORT.md`

## Conclusion

The Bruno import functionality has been fully updated and tested to work with the new architecture. Both single file and folder imports now correctly handle environments and collection variables, maintaining complete compatibility with Bruno's file format and folder structure.
