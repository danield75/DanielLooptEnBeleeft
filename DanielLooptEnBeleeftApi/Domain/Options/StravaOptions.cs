namespace DanielLooptEnBeleeftApi.Domain.Options;

public class StravaOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string AfterDate { get; set; } = "2023-12-17";
}
