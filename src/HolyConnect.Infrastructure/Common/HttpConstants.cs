namespace HolyConnect.Infrastructure.Common;

/// <summary>
/// HTTP-related constants used throughout the infrastructure layer
/// </summary>
public static class HttpConstants
{
    /// <summary>
    /// HTTP header names
    /// </summary>
    public static class Headers
    {
        public const string Authorization = "Authorization";
        public const string ContentType = "Content-Type";
        public const string Accept = "Accept";
        public const string UserAgent = "User-Agent";
    }

    /// <summary>
    /// Common media types
    /// </summary>
    public static class MediaTypes
    {
        public const string ApplicationJson = "application/json";
        public const string ApplicationXml = "application/xml";
        public const string TextPlain = "text/plain";
        public const string TextHtml = "text/html";
        public const string ApplicationJavaScript = "application/javascript";
        public const string ApplicationOctetStream = "application/octet-stream";
    }

    /// <summary>
    /// Authentication-related constants
    /// </summary>
    public static class Authentication
    {
        public const string BasicScheme = "Basic";
        public const string BearerScheme = "Bearer";
    }

    /// <summary>
    /// Default values for HTTP headers
    /// </summary>
    public static class Defaults
    {
        public const string UserAgent = "HolyConnect/1.0";
    }

    /// <summary>
    /// WebSocket-related constants
    /// </summary>
    public static class WebSocket
    {
        public const string GraphQLTransportWsProtocol = "graphql-transport-ws";
        public const string DefaultCloseStatus = "Connection closed";
        public const int DefaultReceiveTimeoutSeconds = 30;
    }

    /// <summary>
    /// GraphQL-related constants
    /// </summary>
    public static class GraphQL
    {
        public const string ConnectionInitMessage = "connection_init";
        public const string ConnectionAckMessage = "connection_ack";
        public const string SubscribeMessage = "subscribe";
        public const string NextMessage = "next";
        public const string ErrorMessage = "error";
        public const string CompleteMessage = "complete";
    }
}
