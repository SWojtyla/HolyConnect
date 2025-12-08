using System.Text.Json;
using System.Text.Json.Serialization;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Infrastructure.Persistence;

public class CollectionJsonConverter : JsonConverter<Collection>
{
    private const string SecretPlaceholder = "***SECRET***";

    public override Collection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Use default deserialization
        var jsonDocument = JsonDocument.ParseValue(ref reader);
        var json = jsonDocument.RootElement.GetRawText();
        
        // Create a new options without this converter to avoid recursion
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Clear();
        foreach (var converter in options.Converters)
        {
            if (converter.GetType() != typeof(CollectionJsonConverter))
            {
                newOptions.Converters.Add(converter);
            }
        }
        
        return JsonSerializer.Deserialize<Collection>(json, newOptions);
    }

    public override void Write(Utf8JsonWriter writer, Collection value, JsonSerializerOptions options)
    {
        // Store original secret variable values
        var originalSecretValues = new Dictionary<string, string>();
        foreach (var secretVar in value.SecretVariables)
        {
            if (value.Variables.TryGetValue(secretVar, out var originalValue))
            {
                originalSecretValues[secretVar] = originalValue;
                value.Variables[secretVar] = SecretPlaceholder;
            }
        }

        try
        {
            // Create a new options without this converter to avoid recursion
            var newOptions = new JsonSerializerOptions(options);
            newOptions.Converters.Clear();
            foreach (var converter in options.Converters)
            {
                if (converter.GetType() != typeof(CollectionJsonConverter))
                {
                    newOptions.Converters.Add(converter);
                }
            }
            
            JsonSerializer.Serialize(writer, value, newOptions);
        }
        finally
        {
            // Restore only the modified secret variable values
            foreach (var kvp in originalSecretValues)
            {
                value.Variables[kvp.Key] = kvp.Value;
            }
        }
    }
}
