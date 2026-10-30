using DanielLooptEnBeleeftApi.Contracts.RunningRaces;
using DanielLooptEnBeleeftApi.Domain.Entities;
using DanielLooptEnBeleeftApi.Infrastructure.Data;
using DanielLooptEnBeleeftApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DanielLooptEnBeleeftApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StravaController : ControllerBase
{
    private readonly IStravaService _stravaService;
    private readonly AppDbContext _db;

    public StravaController(IStravaService stravaService, AppDbContext db)
    {
        _stravaService = stravaService;
        _db = db;
    }

    /// <summary>
    /// Stap 1: Redirect naar Strava login pagina
    /// </summary>
    [HttpGet("login")]
    public IActionResult Login()
        => Redirect(_stravaService.GetAuthorizationUrl());

    /// <summary>
    /// Stap 2: Strava stuurt je hier naartoe met een code
    /// </summary>
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Geen code ontvangen van Strava.");

        var tokenResponse = await _stravaService.ExchangeCodeAsync(code, ct);

        // Token opslaan (of updaten indien al bestaat)
        var existing = await _db.StravaTokens.FirstOrDefaultAsync(ct);
        if (existing is null)
        {
            _db.StravaTokens.Add(new StravaToken
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                ExpiresAt = tokenResponse.ExpiresAt,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.AccessToken = tokenResponse.AccessToken;
            existing.RefreshToken = tokenResponse.RefreshToken;
            existing.ExpiresAt = tokenResponse.ExpiresAt;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            Message = "Strava succesvol gekoppeld! Gebruik POST /api/strava/import om je races te importeren."
        });
    }

    /// <summary>
    /// Stap 3: Activiteiten ophalen en importeren in RunningRaces
    /// </summary>
    [HttpPost("import")]
    public async Task<ActionResult<ImportRunningRacesResult>> Import(CancellationToken ct)
    {
        var accessToken = await _stravaService.GetValidAccessTokenAsync(ct);
        var activities = await _stravaService.GetRunActivitiesAsync(accessToken, ct);
        var result = await _stravaService.ImportActivitiesAsync(activities, ct);
        return Ok(result);
    }

    /// <summary>
    /// Huidige Strava koppeling status bekijken
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
    {
        var token = await _db.StravaTokens.AsNoTracking().FirstOrDefaultAsync(ct);
        if (token is null) return Ok(new { Gekoppeld = false });

        var verlooptOp = DateTimeOffset.FromUnixTimeSeconds(token.ExpiresAt).LocalDateTime;
        return Ok(new
        {
            Gekoppeld = true,
            VerlooptOp = verlooptOp,
            UpdatedAt = token.UpdatedAt
        });
    }
}