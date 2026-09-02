using System.Text.Json;
using System.Text.Json.Serialization;
using IDelivery.Domain.Common.ValueObjects;

namespace IDelivery.Api.Converters;

public sealed class AddressJsonConverter : JsonConverter<Address>
{
    public override Address? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            return null;

        string? street = null;
        string? number = null;
        string? complement = null;
        string? neighborhood = null;
        string? city = null;
        string? state = null;
        string? zipCode = null;
        string? reference = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName?.ToLowerInvariant())
                {
                    case "street":
                        street = reader.GetString();
                        break;
                    case "number":
                        number = reader.GetString();
                        break;
                    case "complement":
                        complement = reader.GetString();
                        break;
                    case "neighborhood":
                        neighborhood = reader.GetString();
                        break;
                    case "city":
                        city = reader.GetString();
                        break;
                    case "state":
                        state = reader.GetString();
                        break;
                    case "zipcode":
                    case "zip_code":
                    case "cep":
                        zipCode = reader.GetString();
                        break;
                    case "reference":
                        reference = reader.GetString();
                        break;
                }
            }
        }

        if (street is null || number is null || neighborhood is null || city is null || state is null || zipCode is null)
            return null;

        return Address.Create(street, number, complement, neighborhood, city, state, zipCode, reference).Value;
    }

    public override void Write(Utf8JsonWriter writer, Address value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        writer.WriteString("street", value.Street);
        writer.WriteString("number", value.Number);

        if (value.Complement is not null)
            writer.WriteString("complement", value.Complement);

        writer.WriteString("neighborhood", value.Neighborhood);
        writer.WriteString("city", value.City);
        writer.WriteString("state", value.State);
        writer.WriteString("zipCode", value.ZipCode.Value);

        if (value.Reference is not null)
            writer.WriteString("reference", value.Reference);

        writer.WriteEndObject();
    }
}
