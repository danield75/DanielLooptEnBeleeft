using DanielLooptEnBeleeftApi.Contracts.RunningRaces;
using DanielLooptEnBeleeftApi.Domain.Entities;

namespace DanielLooptEnBeleeftApi.Interfaces;

public interface IRunningRaceService
{
    Task<List<RunningRace>> GetAllAsync(CancellationToken ct = default);
    Task<RunningRace?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<RunningRace> CreateAsync(RunningRace runningRace, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, RunningRace runningRace, CancellationToken ct = default);
    Task<ImportRunningRacesResult> ImportFromBlogspotAsync(CancellationToken ct = default);
}
