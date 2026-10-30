using System.Text.Json;
using DanielLooptEnBeleeftApi.DTOs.Strava;
using DanielLooptEnBeleeftApi.Interfaces;

namespace DanielLooptEnBeleeftApi.Services;

public class NominatimGeocodingService : IReverseGeocodingService
{
    private readonly HttpClient _http;

    public NominatimGeocodingService(HttpClient http)
    {
        _http = http;
        // Nominatim vereist een User-Agent header
        _http.DefaultRequestHeaders.UserAgent
            .ParseAdd("DanielLooptEnBeleeftApi/1.0");
    }

    public async Task<string?> GetCityAsync(
        double lat, double lng, CancellationToken ct = default)
    {
        var url = $"https://nominatim.openstreetmap.org/reverse" +
                  $"?lat={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                  $"&lon={lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                  $"&format=jsonv2&addressdetails=1";

        try
        {
            var json = await _http.GetStringAsync(url, ct);
            var result = JsonSerializer.Deserialize<NominatimResponse>(json);
            return result?.Address?.BestCity;
        }
        catch
        {
            return null; // Geocoding is niet-kritisch, geen crash
        }
    }
}
