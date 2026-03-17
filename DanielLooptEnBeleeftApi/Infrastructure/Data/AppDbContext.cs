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

        modelBuilder.Entity<RunningRace>(entity =>
        {
            entity.Property(e => e.RaceName)
                .HasMaxLength(200);

            entity.Property(e => e.RacePlace)
                .HasMaxLength(200);

            entity.Property(e => e.LinkToRaceWebsite)
                .HasMaxLength(1000);

            entity.Property(e => e.LinkToRaceResult)
                .HasMaxLength(1000);

            entity.Property(e => e.LinkToRaceReport)
                .HasMaxLength(1000);
        });
    }
}
