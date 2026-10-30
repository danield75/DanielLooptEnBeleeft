using System.Text.Json.Serialization;

namespace DanielLooptEnBeleeftApi.DTOs.Strava;

public class StravaActivityDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("sport_type")]
    public string SportType { get; set; } = string.Empty;  // "Run", "VirtualRun", ...

    [JsonPropertyName("workout_type")]
    public int? WorkoutType { get; set; }

    [JsonPropertyName("distance")]
    public float Distance { get; set; }       // in meters

    [JsonPropertyName("elapsed_time")]
    public int ElapsedTime { get; set; }      // in seconden

    [JsonPropertyName("moving_time")]
    public int MovingTime { get; set; }       // in seconden

    [JsonPropertyName("average_speed")]
    public float AverageSpeed { get; set; }   // in m/s

    [JsonPropertyName("start_date_local")]
    public DateTime StartDateLocal { get; set; }

    [JsonPropertyName("start_latlng")]
    public List<double>? StartLatlng { get; set; }  // [lat, lng]
}
