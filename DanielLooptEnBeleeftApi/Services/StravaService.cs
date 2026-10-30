using DanielLooptEnBeleeftApi.Contracts.RunningRaces;
using DanielLooptEnBeleeftApi.Domain.Entities;
using DanielLooptEnBeleeftApi.Domain.Options;
using DanielLooptEnBeleeftApi.DTOs.Strava;
using DanielLooptEnBeleeftApi.Infrastructure.Data;
using DanielLooptEnBeleeftApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DanielLooptEnBeleeftApi.Services;

public class StravaService : IStravaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly StravaOptions _options;
    private readonly AppDbContext _db;
    private readonly IReverseGeocodingService _geocoding;

    public StravaService(
        IHttpClientFactory httpClientFactory,
        IOptions<StravaOptions> options,
        AppDbContext db,
        IReverseGeocodingService geocoding)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _db = db;
        _geocoding = geocoding;
    }

    // ── 1) Redirect URL naar Strava ──────────────────────────────────────────

    public string GetAuthorizationUrl()
    {
        var redirectEncoded = Uri.EscapeDataString(_options.RedirectUri);
        return $"https://www.strava.com/oauth/authorize" +
               $"?client_id={_options.ClientId}" +
               $"&response_type=code" +
               $"&redirect_uri={redirectEncoded}" +
               $"&approval_prompt=force" +
               $"&scope=activity:read_all";
    }

    // ── 2) Code → Access token ───────────────────────────────────────────────

    public async Task<StravaTokenResponse> ExchangeCodeAsync(string code, CancellationToken ct = default)
    {
        var http = _httpClientFactory.CreateClient();

        var response = await http.PostAsync(
            "https://www.strava.com/oauth/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["code"] = code,
                ["grant_type"] = "authorization_code"
            }), ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<StravaTokenResponse>(json)
               ?? throw new Exception("Kon Strava token response niet deserialiseren.");
    }

    // ── 3) Geldig access token ophalen (refresh indien verlopen) ─────────────

    public async Task<string> GetValidAccessTokenAsync(CancellationToken ct = default)
    {
        var token = await _db.StravaTokens.FirstOrDefaultAsync(ct)
                    ?? throw new InvalidOperationException(
                           "Geen Strava token gevonden. Ga eerst naar /api/strava/login.");

        // Verlopen? → refresh (met 5 min buffer)
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= token.ExpiresAt - 300)
        {
            var refreshed = await RefreshTokenAsync(token.RefreshToken, ct);
            token.AccessToken = refreshed.AccessToken;
            token.RefreshToken = refreshed.RefreshToken;
            token.ExpiresAt = refreshed.ExpiresAt;
            token.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return token.AccessToken;
    }

    private async Task<StravaTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        var http = _httpClientFactory.CreateClient();

        var response = await http.PostAsync(
            "https://www.strava.com/oauth/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token"
            }), ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<StravaTokenResponse>(json)
               ?? throw new Exception("Kon Strava refresh response niet deserialiseren.");
    }

    // ── 4) Activiteiten ophalen van Strava ───────────────────────────────────

    public async Task<List<StravaActivityDto>> GetRunActivitiesAsync(string accessToken, CancellationToken ct = default)
    {
        // AfterDate → Unix timestamp
        DateOnly afterDate = DateOnly.ParseExact(_options.AfterDate, "yyyy-MM-dd");
        long afterUnix = new DateTimeOffset(
            afterDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            .ToUnixTimeSeconds();

        HttpClient http = _httpClientFactory.CreateClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        List<StravaActivityDto> allActivities = new List<StravaActivityDto>();
        int page = 1;
        int perPage = 200; // Strava maximum per pagina

        while (true)
        {
            string url = $"https://www.strava.com/api/v3/athlete/activities" +
                      $"?after={afterUnix}&per_page={perPage}&page={page}";

            string json = await http.GetStringAsync(url, ct);
            List<StravaActivityDto> batch = JsonSerializer.Deserialize<List<StravaActivityDto>>(json) ?? [];

            if (batch.Count == 0) break;

            // Enkel loopactiviteiten bijhouden
            List<StravaActivityDto> runs = batch.Where(a =>
                a.SportType is "Run" or "VirtualRun" or "TrailRun"
                && a.WorkoutType == 1)
                .ToList();

            allActivities.AddRange(runs);

            if (batch.Count < perPage) break; // laatste pagina
            page++;
        }

        return allActivities;
    }

    // ── 5) Mappen naar RunningRace en opslaan ────────────────────────────────

    public async Task<ImportRunningRacesResult> ImportActivitiesAsync(
        List<StravaActivityDto> stravaActivities,
        CancellationToken ct = default)
    {
        // Bestaande sleutels preloaden (datum + naam)
        HashSet<string> existingKeys = (await _db.RunningRaces
            .AsNoTracking()
            .Select(r => new { r.Datum, r.RaceName, r.RacePlace })
            .ToListAsync(ct))
            .Select(x => MakeKey(x.Datum, x.RaceName + " " + x.RacePlace))
            .ToHashSet(StringComparer.Ordinal);

        // Bestaande Strava ID's preloaden (om dubbels via ID te vermijden)
        HashSet<long> existingStravaIds = await _db.RunningRaces
            .AsNoTracking()
            .Where(r => r.StravaActivityId != null)
            .Select(r => r.StravaActivityId!.Value)
            .ToHashSetAsync(ct);

        int inserted = 0, skipped = 0;

        foreach (StravaActivityDto stravaActivity in stravaActivities)
        {
            // Skip als Strava ID al bestaat
            if (existingStravaIds.Contains(stravaActivity.Id)) { skipped++; continue; }

            var date = DateOnly.FromDateTime(stravaActivity.StartDateLocal);
            var name = stravaActivity.Name.Trim();
            var key = MakeKey(date, name);

            // Skip als datum+naam al bestaat
            if (existingKeys.Contains(key)) { skipped++; continue; }

            // ── Plaatsnaam ophalen via GPS ──────────────────────────────────
            string? location = null;
            if (stravaActivity.StartLatlng is { Count: >= 2 })
            {
                location = await _geocoding.GetCityAsync(
                    stravaActivity.StartLatlng[0],
                    stravaActivity.StartLatlng[1],
                    ct);

                // Nominatim rate limit: max 1 request/seconde
                await Task.Delay(1100, ct);
            }
            // ───────────────────────────────────────────────────────────────

            // Afstand: meters → km (of meters indien < 1 km)
            decimal distanceM = (decimal) stravaActivity.Distance;
            decimal distanceKm = Math.Round(distanceM / 1000, 2);

            (decimal distance, DistanceUnit unit) = distanceM < 4000
                ? (Math.Round(distanceM, 0), DistanceUnit.M)
                : (distanceKm, DistanceUnit.Km);

            // Tijd: seconden → TimeSpan
            var finishTime = TimeSpan.FromSeconds(stravaActivity.ElapsedTime);

            // Snelheid: m/s → km/h
            // decimal speedKmh = Math.Round((decimal) stravaActivity.AverageSpeed * 3.6m, 2);

            var race = new RunningRace
            {
                Datum = date,
                RaceName = name,
                RacePlace = location,
                Distance = distance,
                DistanceUnit = unit,
                FinishTime = finishTime,
                //AverageSpeedKmh = speedKmh,
                StravaActivityId = stravaActivity.Id
            };

            _db.RunningRaces.Add(race);
            existingKeys.Add(key);
            existingStravaIds.Add(stravaActivity.Id);
            inserted++;
        }

        await _db.SaveChangesAsync(ct);

        return new ImportRunningRacesResult(stravaActivities.Count, inserted, skipped, 0);
    }

    private static string MakeKey(DateOnly date, string name)
        => $"{date:yyyy-MM-dd}|{name.Trim().ToUpperInvariant()}";
}