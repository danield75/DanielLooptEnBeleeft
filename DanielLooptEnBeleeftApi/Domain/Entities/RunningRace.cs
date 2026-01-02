namespace DanielLooptEnBeleeftApi.Domain.Entities;

public class RunningRace
{
    public int Id { get; set; }

    public DateOnly Datum { get; set; }

    public decimal Distance { get; set; }
    public DistanceUnit DistanceUnit { get; set; }

    public string RaceName { get; set; } = string.Empty;

    public string? ExternelLinkToRace { get; set; }

    // uu:mm:ss
    public TimeSpan FinishTime { get; set; }

    // km/h (kan berekend worden, maar jij wil het als veld: ok)
    public decimal SpeedKmPerHour { get; set; }

    public string? OverallPlace { get; set; }           // bv "16/16"
    public string? AgeCategoryPlace { get; set; } // bv "3/10"

    public string? LinkToRaceResult { get; set; }
    public string? LinkToRaceReport { get; set; } // handig als je ook extern verslag wil

    public RaceReport? RaceReport { get; set; }
}
