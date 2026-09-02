using System.Text.Json;
using System.Text.Json.Serialization;
using IDelivery.Domain.Common.ValueObjects;

namespace IDelivery.Api.Converters;

public sealed class MoneyJsonConverter : JsonConverter<Money>
{
    public override Money? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var amount = reader.GetDecimal();
            return Money.Create(amount).Value;
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            decimal? amount = null;
            string? currency = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();

                    if (string.Equals(propertyName, "amount", StringComparison.OrdinalIgnoreCase))
                        amount = reader.GetDecimal();
                    else if (string.Equals(propertyName, "currency", StringComparison.OrdinalIgnoreCase))
                        currency = reader.GetString();
                }
            }

            if (amount.HasValue)
                return Money.Create(amount.Value, currency ?? "BRL").Value;

            return null;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, Money value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        writer.WriteNumber("amount", value.Amount);
        writer.WriteString("currency", value.Currency);
        writer.WriteEndObject();
    }
}
