namespace DanielLooptEnBeleeftApi.Contracts.RunningRaces;

public sealed record ImportRunningRacesResult(
    int Parsed,
    int Inserted,
    int SkippedAlreadyExists,
    int Failed
);