namespace DanielLooptEnBeleeftApi.Domain.Entities;

public class StravaToken
{
    public int Id { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public long ExpiresAt { get; set; }   // Unix timestamp
    public DateTime UpdatedAt { get; set; }
}
