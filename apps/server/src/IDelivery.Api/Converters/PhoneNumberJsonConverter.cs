using System.Text.Json;
using System.Text.Json.Serialization;
using IDelivery.Domain.Common.ValueObjects;

namespace IDelivery.Api.Converters;

public sealed class PhoneNumberJsonConverter : JsonConverter<PhoneNumber>
{
    public override PhoneNumber? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return PhoneNumber.Create(value).Value;
    }

    public override void Write(Utf8JsonWriter writer, PhoneNumber value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.Value);
    }
}
