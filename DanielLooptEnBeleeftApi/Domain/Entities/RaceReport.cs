namespace DanielLooptEnBeleeftApi.Domain.Entities;

public class RaceReport
{
    public int Id { get; set; }

    public int RunningRaceId { get; set; }
    public RunningRace RunningRace { get; set; } = null!;

    // HTML toegestaan
    public string ReportText { get; set; } = string.Empty;
}
