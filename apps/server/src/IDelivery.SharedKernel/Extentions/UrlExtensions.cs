namespace IDelivery.SharedKernel.Extentions
{
    public static class UrlExtensions
    {
        /// <summary>
        /// Combina uma URL base com uma rota de forma segura, evitando barras duplas ou ausentes.
        /// </summary>
        public static string CombineRoute(this string? baseUrl, string route)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return route.TrimStart('/');
            }

            return $"{baseUrl.TrimEnd('/')}/{route.TrimStart('/')}";
        }
    }
}
