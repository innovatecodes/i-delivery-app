using System.Text.Json;
using System.Text.Json.Serialization;
using IDelivery.Domain.Common.ValueObjects;

namespace IDelivery.Api.Converters;

public sealed class EmailJsonConverter : JsonConverter<Email>
{
    public override Email? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Email.Create(value).Value;
    }

    public override void Write(Utf8JsonWriter writer, Email value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.Value);
    }
}
