namespace DanielLooptEnBeleeftApi.Domain.Entities;

public class RunningRace
{
    public int Id { get; set; }

    public required DateOnly Datum { get; set; }

    public required int Distance { get; set; }
    public required DistanceUnit DistanceUnit { get; set; }

    public required string RaceName { get; set; }

    public required string RacePlace { get; set; }

    public TimeSpan? RaceStartTime { get; set; }

    public string? LinkToRaceWebsite { get; set; }

    public TimeSpan? FinishTime { get; set; }

    public int? OverallPlace { get; set; }
    public int? NumberParticipantsOverall { get; set; }
    public int? AgeCategoryPlace { get; set; }
    public int? NumberParticipantsAgeCategory { get; set; }

    public string? LinkToRaceResult { get; set; }
    public string? LinkToRaceReport { get; set; }

    public RaceReport? RaceReport { get; set; }
}
