# Infrastructure Common Helpers

This directory contains helper classes for common infrastructure operations. The helpers are organized by concern to promote code reuse and maintainability.

## Helper Categories

### HTTP/REST Helpers
- **HttpAuthenticationHelper.cs** - Applies authentication (Basic, Bearer) to HTTP requests and WebSocket connections
- **HttpRequestHelper.cs** - Builds HTTP request messages, sets content, adds headers
- **HttpConstants.cs** - Constants for HTTP headers, media types, and authentication schemes

### Response Handling
- **ResponseHelper.cs** - Builds and populates RequestResponse objects, captures headers, handles exceptions

### GraphQL Helpers
- **GraphQLHelper.cs** - Creates and serializes GraphQL payloads

### WebSocket Helpers
- **WebSocketHelper.cs** - WebSocket operations including URL conversion, header application, safe closing

## Usage Patterns

### Authentication
```csharp
// HTTP Request
HttpAuthenticationHelper.ApplyAuthentication(httpRequest, request);
HttpAuthenticationHelper.ApplyHeaders(httpRequest, request);

// WebSocket
HttpAuthenticationHelper.ApplyAuthentication(webSocket.Options, request);
WebSocketHelper.ApplyHeaders(webSocket.Options, request);
```

### Response Building
```csharp
// Capture headers
ResponseHelper.CaptureHeaders(response.Headers, httpResponse.Headers);

// Handle exceptions
ResponseHelper.HandleException(response, ex, stopwatch.ElapsedMilliseconds);

// Finalize streaming response
ResponseHelper.FinalizeStreamingResponse(response);
```

### GraphQL Operations
```csharp
// Create payload
var payload = GraphQLHelper.CreatePayload(graphQLRequest);
var json = GraphQLHelper.SerializePayload(graphQLRequest);
```

### WebSocket Operations
```csharp
// Convert HTTP URL to WebSocket URL
var wsUrl = WebSocketHelper.ConvertToWebSocketUrl(httpUrl);

// Send JSON message
await WebSocketHelper.SendJsonMessageAsync(webSocket, message);

// Safe close
await WebSocketHelper.SafeCloseAsync(webSocket, response);
```

## Design Principles

1. **Single Responsibility** - Each helper focuses on one specific concern
2. **Static Methods** - All helpers use static methods as they have no state
3. **Reusability** - Shared across multiple request executors
4. **Constants** - Use HttpConstants instead of magic strings
5. **Testability** - Small, focused methods that are easy to test

## Adding New Helpers

When adding new helper functionality:
1. Determine the category (HTTP, Response, GraphQL, WebSocket, etc.)
2. Add to existing helper if it fits, or create new one if it's a new concern
3. Use static methods for stateless operations
4. Document the helper's purpose and usage patterns
5. Update this README with the new helper
