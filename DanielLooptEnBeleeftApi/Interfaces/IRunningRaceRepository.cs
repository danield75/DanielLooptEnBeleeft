using DanielLooptEnBeleeftApi.Domain.Entities;

namespace DanielLooptEnBeleeftApi.Application.Interfaces;

public interface IRunningRaceRepository
{
    Task<List<RunningRace>> GetAllWithReportAsync(CancellationToken ct = default);
    Task<RunningRace?> GetByIdWithReportAsync(int id, CancellationToken ct = default);

    Task AddAsync(RunningRace race, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);

    void Remove(RunningRace race);
}
