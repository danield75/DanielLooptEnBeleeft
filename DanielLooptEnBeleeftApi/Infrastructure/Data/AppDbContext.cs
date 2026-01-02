using DanielLooptEnBeleeftApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DanielLooptEnBeleeftApi.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<RunningRace> RunningRaces => Set<RunningRace>();
    public DbSet<RaceReport> RaceReports => Set<RaceReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RunningRace>()
            .HasOne(x => x.RaceReport)
            .WithOne(v => v.RunningRace)
            .HasForeignKey<RaceReport>(v => v.RunningRaceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RunningRace>()
            .Property(x => x.RaceName)
            .HasMaxLength(200);

        modelBuilder.Entity<RunningRace>()
            .Property(x => x.ExternelLinkToRace)
            .HasMaxLength(1000);

        modelBuilder.Entity<RunningRace>()
            .Property(x => x.LinkToRaceResult)
            .HasMaxLength(1000);

        modelBuilder.Entity<RunningRace>()
            .Property(x => x.LinkToRaceReport)
            .HasMaxLength(1000);
    }
}
