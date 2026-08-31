using IDelivery.Domain.Common.Exceptions;

namespace IDelivery.Domain.Common.ValueObjects;

/// <summary>
/// Representa coordenadas geográficas (latitude, longitude).
/// Valida ranges válidos e fornece cálculo de distância (Haversine).
/// </summary>
public sealed class Coordinates : ValueObject
{
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }

    // Construtor para EF Core
    private Coordinates() { }

    private Coordinates(decimal latitude, decimal longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new DomainException("A latitude deve estar entre -90 e 90");
        if (longitude < -180 || longitude > 180)
            throw new DomainException("A longitude deve estar entre -180 e 180");

        Latitude = Math.Round(latitude, 6);
        Longitude = Math.Round(longitude, 6);
    }

    public static Coordinates Create(decimal latitude, decimal longitude) => new(latitude, longitude);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }

    /// <summary>
    /// Calcula distância em km até outra coordenada (fórmula Haversine).
    /// </summary>
    public double DistanceTo(Coordinates other)
    {
        const double earthRadiusKm = 6371;

        var lat1 = (double)Latitude * Math.PI / 180;
        var lat2 = (double)other.Latitude * Math.PI / 180;
        var deltaLat = ((double)other.Latitude - (double)Latitude) * Math.PI / 180;
        var deltaLon = ((double)other.Longitude - (double)Longitude) * Math.PI / 180;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    public static implicit operator (decimal Lat, decimal Lng)(Coordinates coords) => (coords.Latitude, coords.Longitude);
    public static implicit operator Coordinates((decimal Lat, decimal Lng) tuple) => Create(tuple.Lat, tuple.Lng);

    public override string ToString() => $"{Latitude}, {Longitude}";
}