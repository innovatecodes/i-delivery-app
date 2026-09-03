
namespace IDelivery.Application.Settings
{
    public sealed class ClientSettings
    {
        public const string SectionName = "ClientSettings";

        public string BaseUrl { get; init; } = string.Empty;
        public ClientRoutesSettings Routes { get; init; } = new();
    }
}
