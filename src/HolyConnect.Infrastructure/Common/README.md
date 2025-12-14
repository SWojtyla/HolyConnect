# Infrastructure Common Helpers

This directory contains helper classes for common infrastructure operations. The helpers are organized by concern to promote code reuse and maintainability.

## Helper Categories

### HTTP/REST Helpers
- **HttpAuthenticationHelper.cs** - Applies authentication (Basic, Bearer) to HTTP requests and WebSocket connections
- **HttpRequestHelper.cs** - Builds HTTP request messages, sets content, adds headers
- **HttpConstants.cs** - Constants for HTTP headers, media types, and authentication schemes

### Response Handling
- **RequestResponseBuilder.cs** - Fluent builder for constructing RequestResponse objects with consistent API
- **ResponseHelper.cs** - Utility methods for capturing headers, finalizing streaming responses, handling exceptions

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
// Using RequestResponseBuilder (recommended pattern)
var builder = RequestResponseBuilder.Create(); // or CreateStreaming()

var response = await builder
    .WithSentRequest(httpRequest, url, method, body)
    .WithStatus(httpResponse)
    .WithHeaders(httpResponse.Headers)
    .WithBodyFromContentAsync(httpResponse.Content)
    .StopTiming()
    .Build();

// Error handling
var response = builder
    .WithException(ex)
    .Build();

// Streaming responses
var response = RequestResponseBuilder.CreateStreaming()
    .AddStreamEvent("Connection established", "connect")
    .AddStreamEvent(data, "message")
    .FinalizeStreaming()
    .Build();

// Using ResponseHelper (utility methods)
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
2. **Builder Pattern** - Use RequestResponseBuilder for consistent response construction
3. **Static Methods** - Utility helpers use static methods as they have no state
4. **Reusability** - Shared across multiple request executors
5. **Constants** - Use HttpConstants instead of magic strings
6. **Testability** - Small, focused methods that are easy to test
7. **Fluent API** - Builder provides chainable methods for readable code

## Adding New Helpers

When adding new helper functionality:
1. Determine the category (HTTP, Response, GraphQL, WebSocket, etc.)
2. Add to existing helper if it fits, or create new one if it's a new concern
3. Use static methods for stateless operations
4. Document the helper's purpose and usage patterns
5. Update this README with the new helper
