using DanielLooptEnBeleeftApi.Contracts.RunningRaces;
using DanielLooptEnBeleeftApi.DTOs.Strava;

namespace DanielLooptEnBeleeftApi.Interfaces;

public interface IStravaService
{
    string GetAuthorizationUrl();
    Task<StravaTokenResponse> ExchangeCodeAsync(string code, CancellationToken ct = default);
    Task<string> GetValidAccessTokenAsync(CancellationToken ct = default);
    Task<List<StravaActivityDto>> GetRunActivitiesAsync(string accessToken, CancellationToken ct = default);
    Task<ImportRunningRacesResult> ImportActivitiesAsync(List<StravaActivityDto> activities, CancellationToken ct = default);
}
