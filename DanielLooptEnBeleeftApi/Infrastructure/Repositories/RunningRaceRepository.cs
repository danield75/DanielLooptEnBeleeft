using DanielLooptEnBeleeftApi.Interfaces;
using DanielLooptEnBeleeftApi.Domain.Entities;
using DanielLooptEnBeleeftApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DanielLooptEnBeleeftApi.Infrastructure.Repositories;

public class RunningRaceRepository : IRunningRaceRepository
{
    private readonly AppDbContext _context;

    public RunningRaceRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<RunningRace>> GetAllWithReportAsync(CancellationToken ct = default)
        => _context.RunningRaces
            .AsNoTracking()
            .Include(r => r.RaceReport)
            .OrderByDescending(r => r.Datum)
            .ToListAsync(ct);

    public Task<RunningRace?> GetByIdWithReportAsync(int id, CancellationToken ct = default)
        => _context.RunningRaces
            .Include(r => r.RaceReport)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(RunningRace race, CancellationToken ct = default)
        => await _context.RunningRaces.AddAsync(race, ct);

    public void Remove(RunningRace race)
        => _context.RunningRaces.Remove(race);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public Task<List<(DateOnly Datum, string RaceName)>> GetExistingKeysAsync(CancellationToken ct = default)
        => _context.RunningRaces
            .AsNoTracking()
            .Select(r => new ValueTuple<DateOnly, string>(r.Datum, r.RaceName))
            .ToListAsync(ct);

    public Task<List<RunningRace>> GetAllTrackedAsync(CancellationToken ct = default)
        => _context.RunningRaces.ToListAsync(ct);
}
