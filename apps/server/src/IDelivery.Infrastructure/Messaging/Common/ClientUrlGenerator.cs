using IDelivery.Application.Abstractions.Messaging;
using IDelivery.Application.Settings;
using IDelivery.SharedKernel.Extentions;
using Microsoft.Extensions.Options;

namespace IDelivery.Infrastructure.Messaging.Common
{
    internal class ClientUrlGenerator(IOptions<ClientSettings> clientSettings) : IClientUrlGenerator
    {
        private readonly ClientSettings _clientSettings = clientSettings.Value;

        public string Generate(string route, Dictionary<string, string> queryParams)
        {
            var baseUrl = _clientSettings.BaseUrl.CombineRoute(route);

            if (queryParams is null || queryParams.Count == 0)
            {
                return baseUrl;
            }

            // Constrói a QueryString de forma segura codificando os valores
            var queryString = string.Join("&", queryParams.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

            return $"{baseUrl}?{queryString}";
        }
    }
}
