namespace DanielLooptEnBeleeftApi.Interfaces;

public interface IReverseGeocodingService
{
    Task<string?> GetCityAsync(double lat, double lng, CancellationToken ct = default);
}