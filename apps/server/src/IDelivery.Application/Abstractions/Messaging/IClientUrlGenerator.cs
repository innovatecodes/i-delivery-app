namespace IDelivery.Application.Abstractions.Messaging
{
    public interface IClientUrlGenerator
    {
        /// <summary>
        /// Gera uma URL completa para o cliente baseada em uma rota e parâmetros opcionais.
        /// </summary>
        string Generate(string route, Dictionary<string, string> queryParams);
    }
}
