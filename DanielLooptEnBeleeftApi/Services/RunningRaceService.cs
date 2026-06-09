using DanielLooptEnBeleeftApi.Application.Interfaces;
using DanielLooptEnBeleeftApi.Domain.Entities;

namespace DanielLooptEnBeleeftApi.Application.Services;

public class RunningRaceService
{
    private readonly IRunningRaceRepository _repo;

    public RunningRaceService(IRunningRaceRepository repo)
    {
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
}