using System.Text.Json;
using System.Text.Json.Serialization;

namespace IDelivery.Api.Extensions
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Configura as opções de serialização JSON da API
        /// </summary>
        public static IMvcBuilder AddCustomJsonOptions(this IMvcBuilder builder)
        {
            return builder.AddJsonOptions(options =>
            {
                // Define camelCase como padrão para os nomes das propriedades JSON.
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

                // Serializa enums como texto (ex: "SuperAdmin" em vez de "1").
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                // Evita loops em referências circulares.
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

                // Ignora propriedades nulas no JSON.
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

                // JSON minificado (sem indentação).
                options.JsonSerializerOptions.WriteIndented = false;
            });
        }
    }
}


