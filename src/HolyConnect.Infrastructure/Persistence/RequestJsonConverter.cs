using System.Text.Json;
using System.Text.Json.Serialization;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Infrastructure.Persistence;

public class RequestJsonConverter : JsonConverter<Request>
{
    private const string SecretPlaceholder = "***SECRET***";

    public override Request? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        if (!doc.RootElement.TryGetProperty("Type", out var typeProp))
        {
            throw new JsonException("Missing Type discriminator for Request");
        }

        RequestType type;
        switch (typeProp.ValueKind)
        {
            case JsonValueKind.String:
                var typeString = typeProp.GetString();
                if (!Enum.TryParse<RequestType>(typeString, out type))
                {
                    throw new JsonException($"Unknown Request type: {typeString}");
                }
                break;
            case JsonValueKind.Number:
                if (!typeProp.TryGetInt32(out var typeInt) || !Enum.IsDefined(typeof(RequestType), typeInt))
                {
                    throw new JsonException("Invalid numeric Request type value");
                }
                type = (RequestType)typeInt;
                break;
            default:
                throw new JsonException($"Unsupported Type value kind: {typeProp.ValueKind}");
        }

        var json = doc.RootElement.GetRawText();
        return type switch
        {
            RequestType.Rest => JsonSerializer.Deserialize<RestRequest>(json, options),
            RequestType.GraphQL => JsonSerializer.Deserialize<GraphQLRequest>(json, options),
            RequestType.WebSocket => JsonSerializer.Deserialize<WebSocketRequest>(json, options),
            _ => throw new JsonException($"Unsupported Request type: {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, Request value, JsonSerializerOptions options)
    {
        // Temporarily replace secret header values with placeholder
        var originalHeaders = new Dictionary<string, string>(value.Headers);
        foreach (var secretHeader in value.SecretHeaders)
        {
            if (value.Headers.ContainsKey(secretHeader))
            {
                value.Headers[secretHeader] = SecretPlaceholder;
            }
        }

        try
        {
            switch (value)
            {
                case RestRequest rest:
                    JsonSerializer.Serialize(writer, rest, options);
                    break;
                case GraphQLRequest gql:
                    JsonSerializer.Serialize(writer, gql, options);
                    break;
                case WebSocketRequest ws:
                    JsonSerializer.Serialize(writer, ws, options);
                    break;
                default:
                    throw new JsonException($"Unsupported Request subclass: {value.GetType().Name}");
            }
        }
        finally
        {
            // Restore original header values
            value.Headers = originalHeaders;
        }
    }
}
