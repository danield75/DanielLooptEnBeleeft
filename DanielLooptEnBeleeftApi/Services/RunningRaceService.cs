using AngleSharp;
using DanielLooptEnBeleeftApi.Contracts.RunningRaces;
using DanielLooptEnBeleeftApi.Domain.Entities;
using DanielLooptEnBeleeftApi.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DanielLooptEnBeleeftApi.Services;

public class RunningRaceService : IRunningRaceService
{
    private const string SourceUrl = "https://danieldrion.blogspot.com/p/uitslagen.html";

    private readonly IRunningRaceRepository _repo;
    private readonly IHttpClientFactory _httpClientFactory;

    public RunningRaceService(IHttpClientFactory httpClientFactory, IRunningRaceRepository repo)
    {
        _httpClientFactory = httpClientFactory;
        _repo = repo;
    }

    public Task<List<RunningRace>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllWithReportAsync(ct);

    public Task<RunningRace?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repo.GetByIdWithReportAsync(id, ct);

    public async Task<RunningRace> CreateAsync(RunningRace runningRace, CancellationToken ct = default)
    {
        await _repo.AddAsync(runningRace, ct);
        await _repo.SaveChangesAsync(ct);
        return runningRace;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var race = await _repo.GetByIdWithReportAsync(id, ct);
        if (race is null) return false;

        _repo.Remove(race);
        await _repo.SaveChangesAsync(ct);
        return true;
    }

    // Update laat ik functioneel gelijk aan jouw code, maar verplaatst naar service:
    public async Task<bool> UpdateAsync(int id, RunningRace runningRace, CancellationToken ct = default)
    {
        if (id != runningRace.Id) throw new InvalidOperationException("Id mismatch.");

        var existingRace = await _repo.GetByIdWithReportAsync(id, ct);
        if (existingRace is null) return false;

        existingRace.Datum = runningRace.Datum;
        existingRace.Distance = runningRace.Distance;
        existingRace.DistanceUnit = runningRace.DistanceUnit;
        existingRace.RaceName = runningRace.RaceName;
        existingRace.RacePlace = runningRace.RacePlace;
        existingRace.RaceStartTime = runningRace.RaceStartTime;
        existingRace.LinkToRaceWebsite = runningRace.LinkToRaceWebsite;
        existingRace.FinishTime = runningRace.FinishTime;
        existingRace.OverallPlace = runningRace.OverallPlace;
        existingRace.NumberParticipantsOverall = runningRace.NumberParticipantsOverall;
        existingRace.AgeCategoryPlace = runningRace.AgeCategoryPlace;
        existingRace.NumberParticipantsAgeCategory = runningRace.NumberParticipantsAgeCategory;
        existingRace.LinkToRaceResult = runningRace.LinkToRaceResult;
        existingRace.LinkToRaceReport = runningRace.LinkToRaceReport;

        if (runningRace.RaceReport is null)
        {
            existingRace.RaceReport = null;
        }
        else if (existingRace.RaceReport is null)
        {
            existingRace.RaceReport = new RaceReport
            {
                RunningRaceId = existingRace.Id,
                ReportText = runningRace.RaceReport.ReportText
            };
        }
        else
        {
            existingRace.RaceReport.ReportText = runningRace.RaceReport.ReportText;
        }

        await _repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ImportRunningRacesResult> ImportFromBlogspotAsync(CancellationToken ct = default)
    {
        var http = _httpClientFactory.CreateClient();
        var html = await http.GetStringAsync(SourceUrl, ct);

        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req =>
        {
            req.Content(html);
            req.Address(SourceUrl);
        }, ct);

        // 1) bestaande keys preloaden (Datum + RaceName)
        //var existingkeys = (await _repo.getexistingkeysasync(ct)).tohashset();
        //var keyset = existingkeys
        //    .select(x => makekey(x.datum, x.racename))
        //    .tohashset(stringcomparer.ordinal);

        var existingEntities = await _repo.GetAllTrackedAsync(ct);
        var existingByKey = existingEntities
            .ToDictionary(r => MakeKey(r.Datum, r.RaceName), StringComparer.Ordinal);

        int parsed = 0, inserted = 0, updated = 0, skipped = 0, failed = 0;

        // 2) enkel jaar-tabellen parsen (met titel "YYYY : N wedstrijden")
        foreach (var table in document.QuerySelectorAll("table"))
        {
            var titleText = table.QuerySelector("b, strong")?.TextContent?.Trim();
            if (!IsYearTableTitle(titleText)) continue;

            var rows = table.QuerySelectorAll("tr").ToList();
            var headerIndex = rows.FindIndex(r =>
            {
                var t = (r.TextContent ?? "");
                return t.Contains("Datum", StringComparison.OrdinalIgnoreCase)
                    && t.Contains("Wedstrijd", StringComparison.OrdinalIgnoreCase)
                    && t.Contains("Afstand", StringComparison.OrdinalIgnoreCase)
                    && t.Contains("Tijd", StringComparison.OrdinalIgnoreCase)
                    && t.Contains("Plaats", StringComparison.OrdinalIgnoreCase);
            });

            if (headerIndex < 0) continue;

            foreach (var row in rows.Skip(headerIndex + 1))
            {
                var cells = row.QuerySelectorAll("td,th")
                               .Select(c => (c.TextContent ?? "").Trim())
                               .ToList();

                // Verwacht minstens: Datum, Wedstrijd, Afstand, Tijd, Snelheid, Plaats
                if (cells.Count < 6) continue;

                if (!TryParseDate(cells[0], out var date)) continue; // niet-data rijen overslaan
                var rawRaceName = (cells[1] ?? "").Trim();
                if (string.IsNullOrWhiteSpace(rawRaceName)) continue;

                parsed++;

                if (!TryParseDistance(cells[2], out var distance, out var distanceUnit))
                {
                    failed++;
                    continue;
                }

                var finishTime = TryParseFinishTime(cells[3]); // mag null zijn als parsing faalt

                ParsePlace(cells[5], out var overallPlace, out var participantsOverall);

                var key = MakeKey(date, rawRaceName);

                // Circuit + opgeschoonde naam bepalen
                var cleanRaceName = ExtractRaceCircuit(rawRaceName, out var raceCircuit);

                if (existingByKey.TryGetValue(key, out var existing))
                {
                    // Update bestaande record
                    var changed = false;

                    if (!string.Equals(existing.RaceName, cleanRaceName, StringComparison.Ordinal))
                    {
                        existing.RaceName = cleanRaceName;
                        changed = true;
                    }

                    if (existing.RaceCircuit != raceCircuit)
                    {
                        existing.RaceCircuit = raceCircuit;
                        changed = true;
                    }

                    if (changed)
                        updated++;
                    else
                        skipped++;

                    continue;
                }

                // Entity vullen (pas types/namen aan indien jouw entity anders is)
                var entity = new RunningRace
                {
                    Datum = date,
                    RaceName = cleanRaceName,
                    RaceCircuit = raceCircuit,
                    Distance = distance,
                    DistanceUnit = distanceUnit,
                    OverallPlace = overallPlace,
                    NumberParticipantsOverall = participantsOverall,
                    FinishTime = finishTime
                };

                await CreateAsync(entity);
                existingByKey[key] = entity;
                inserted++;
            }
        }

        // Alle updates in 1 keer wegschrijven
        await _repo.SaveChangesAsync(ct);

        return new ImportRunningRacesResult(parsed, inserted, skipped, failed);
    }

    private static string MakeKey(DateOnly date, string raceName)
        => $"{date:yyyyMMdd}|{NormalizeRaceName(raceName)}";

    private static string NormalizeRaceName(string raceName)
        => (raceName ?? "").Trim().ToUpperInvariant();

    private static bool IsYearTableTitle(string? title)
        => !string.IsNullOrWhiteSpace(title)
           && Regex.IsMatch(title, @"^\s*\d{4}\s*:\s*\d+\s*wedstrijden\s*$", RegexOptions.IgnoreCase);

    private static bool TryParseDate(string input, out DateOnly date)
    {
        input = (input ?? "").Trim();
        // Blog gebruikt dd/MM/yyyy
        return DateOnly.TryParseExact(
            input,
            "d/M/yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date
        )
        || DateOnly.TryParseExact(
            input,
            "dd/MM/yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date
        );
    }

    private static bool TryParseDistance(
        string? input,
        out decimal distance,
        out DistanceUnit distanceUnit)
    {
        distance = default;
        distanceUnit = DistanceUnit.Km;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var s = input.Trim().ToLowerInvariant();

        // verwijder spaties (bv. "10 km")
        s = s.Replace(" ", "");

        // unit bepalen op suffix
        if (s.EndsWith("km", StringComparison.Ordinal))
        {
            distanceUnit = DistanceUnit.Km;
            s = s[..^2];
        }
        else if (s.EndsWith("m", StringComparison.Ordinal))
        {
            distanceUnit = DistanceUnit.M; // waarde 0 in jouw enum
            s = s[..^1];
        }
        else
        {
            // geen suffix: default (meestal km)
            distanceUnit = DistanceUnit.Km;
        }

        if (string.IsNullOrWhiteSpace(s))
            return false;

        // Normaliseer getal:
        // - als er een ',' in zit -> ',' is decimaal, '.' zijn duizendtallen -> weg ermee
        // - anders -> '.' is decimaal, ',' zijn duizendtallen -> weg ermee
        if (s.Contains(','))
        {
            s = s.Replace(".", "");
            s = s.Replace(',', '.');
        }
        else
        {
            s = s.Replace(",", "");
        }

        if (!decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            return false;

        if (value <= 0)
            return false;

        distance = value;

        return true;
    }

    private static TimeSpan? TryParseFinishTime(string input)
    {
        var s = (input ?? "").Trim();
        if (string.IsNullOrWhiteSpace(s) || s == "-") return null;

        // voorbeelden: "00:17:26", "0:40:44", "0:02:24,44"
        // normaliseer fracties: ",44" -> ".44"
        s = s.Replace(",", ".", StringComparison.Ordinal);

        // Probeer meest voorkomende patterns
        var formats = new[]
        {
            @"h\:mm\:ss",
            @"hh\:mm\:ss",
            @"h\:mm\:ss\.f",
            @"h\:mm\:ss\.ff",
            @"h\:mm\:ss\.fff",
            @"hh\:mm\:ss\.f",
            @"hh\:mm\:ss\.ff",
            @"hh\:mm\:ss\.fff",
        };

        if (TimeSpan.TryParseExact(s, formats, CultureInfo.InvariantCulture, out var ts))
            return ts;

        // Fallback
        if (TimeSpan.TryParse(s, CultureInfo.InvariantCulture, out ts))
            return ts;

        return null;
    }

    private static void ParsePlace(string input, out int? overallPlace, out int? participantsOverall)
    {
        overallPlace = null;
        participantsOverall = null;

        var s = (input ?? "").Trim();
        // verwacht "33/277"
        var parts = s.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return;

        if (int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var p1))
            overallPlace = p1;

        if (int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var p2))
            participantsOverall = p2;
    }

    private static readonly Dictionary<string, RaceCircuit> CircuitAbbreviations =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["LCC"] = RaceCircuit.LCC,
            ["HC"] = RaceCircuit.HC,
            ["CH"] = RaceCircuit.CH,
            ["HSLC"] = RaceCircuit.HSLC,
            ["VC"] = RaceCircuit.VC
        };

    private static string ExtractRaceCircuit(string raceName, out RaceCircuit? circuit)
    {
        circuit = null;

        var match = Regex.Match(raceName, @"\(([A-Za-z]+)\)");
        if (match.Success && CircuitAbbreviations.TryGetValue(match.Groups[1].Value, out var found))
        {
            circuit = found;
            raceName = raceName.Remove(match.Index, match.Length);
            raceName = Regex.Replace(raceName, @"\s{2,}", " ").Trim();
        }

        return raceName;
    }
}