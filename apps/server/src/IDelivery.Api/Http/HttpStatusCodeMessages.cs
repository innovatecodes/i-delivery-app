using System.Net;

namespace IDelivery.Api.Http;

/// <summary>
/// Mensagens padronizadas para códigos de status HTTP.
/// Fornece uma descrição amigável para cada código de status.
/// </summary>
public static class HttpStatusCodeMessages
{
    private const string DefaultMessage = "Erro inesperado";

    private static readonly IReadOnlyDictionary<HttpStatusCode, string> Messages =
        new Dictionary<HttpStatusCode, string>
        {
            [HttpStatusCode.OK] =
                "A solicitação foi bem-sucedida e a resposta contém os dados solicitados",

            [HttpStatusCode.Created] =
                "A solicitação foi bem-sucedida e resultou na criação de um novo recurso",

            [HttpStatusCode.Accepted] =
                "A solicitação foi aceita, mas ainda não processada",

            [HttpStatusCode.NoContent] =
                "A solicitação foi bem-sucedida, mas não há conteúdo para retornar",

            [HttpStatusCode.BadRequest] =
                "A requisição é inválida ou malformada",

            [HttpStatusCode.Unauthorized] =
                "Falta de autenticação ou credenciais inválidas",

            [HttpStatusCode.Forbidden] =
                "Acesso ao recurso negado",

            [HttpStatusCode.NotFound] =
                "O recurso solicitado não foi encontrado",

            [HttpStatusCode.MethodNotAllowed] =
                "Método HTTP não permitido para este recurso",

            [HttpStatusCode.Conflict] =
                "Conflito ao processar a solicitação. Verifique os dados e tente novamente",

            [HttpStatusCode.PaymentRequired] =
                "Pagamento necessário",

            [HttpStatusCode.RequestEntityTooLarge] =
                "O arquivo enviado é muito grande. O limite de tamanho foi excedido",

            [HttpStatusCode.UnsupportedMediaType] =
                "O tipo de mídia enviado não é suportado. Verifique o formato do arquivo",

            [HttpStatusCode.TooManyRequests] =
                "Número de requisições excedeu o limite",

            [HttpStatusCode.InternalServerError] =
                "Erro interno do servidor. Por favor, tente novamente mais tarde",

            [HttpStatusCode.NotImplemented] =
                "Funcionalidade solicitada não implementada no servidor",

            [HttpStatusCode.BadGateway] =
                "O servidor recebeu uma resposta inválida ao acessar outro servidor",

            [HttpStatusCode.ServiceUnavailable] =
                "O servidor está temporariamente indisponível devido a manutenção ou sobrecarga",

            [HttpStatusCode.Gone] =
                "O recurso não está mais disponível",

            [HttpStatusCode.GatewayTimeout] =
                "O servidor não recebeu uma resposta a tempo ao acessar outro servidor"
        };

    /// <summary>
    /// Obtém a mensagem padronizada para o código de status HTTP.
    /// </summary>
    public static string Get(HttpStatusCode statusCode)
    {
        return Messages.TryGetValue(statusCode, out var message)
            ? message
            : DefaultMessage;
    }
}