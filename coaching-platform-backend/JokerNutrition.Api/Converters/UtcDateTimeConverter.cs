using System.Text.Json;
using System.Text.Json.Serialization;

namespace JokerNutrition.Api.Converters;

/// <summary>
/// Ensures all DateTime values serialized by System.Text.Json are treated as UTC
/// and include the 'Z' suffix. This fixes the issue where SQL Server's datetime2
/// strips DateTimeKind, causing the frontend to misinterpret UTC timestamps as local time.
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTime();
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Force UTC kind so System.Text.Json appends 'Z'
        var utcValue = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        writer.WriteStringValue(utcValue);
    }
}

/// <summary>
/// Nullable DateTime variant of UtcDateTimeConverter.
/// </summary>
public class UtcNullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var value = reader.GetDateTime();
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        var utcValue = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
        writer.WriteStringValue(utcValue);
    }
}
