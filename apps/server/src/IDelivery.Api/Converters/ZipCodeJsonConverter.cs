using System.Text.Json;
using System.Text.Json.Serialization;
using IDelivery.Domain.Common.ValueObjects;

namespace IDelivery.Api.Converters;

public sealed class ZipCodeJsonConverter : JsonConverter<ZipCode>
{
    public override ZipCode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return ZipCode.Create(value).Value;
    }

    public override void Write(Utf8JsonWriter writer, ZipCode value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.Value);
    }
}
