using System.Text.Json.Serialization;

namespace DanielLooptEnBeleeftApi.DTOs.Strava;

public class NominatimResponse
{
    [JsonPropertyName("address")]
    public NominatimAddress? Address { get; set; }
}

public class NominatimAddress
{
    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("town")]
    public string? Town { get; set; }

    [JsonPropertyName("village")]
    public string? Village { get; set; }

    [JsonPropertyName("municipality")]
    public string? Municipality { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    // Geeft de beste beschikbare plaatsnaam terug
    public string? BestCity =>
        City ?? Town ?? Village ?? Municipality;
}