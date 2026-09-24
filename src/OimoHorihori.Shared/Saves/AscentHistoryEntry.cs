namespace OimoHorihori.Shared.Saves;

public class AscentHistoryEntry
{
    public int AscentNumber { get; set; }
    public DateTimeOffset AscendedAtUtc { get; set; }
    public double AscentDurationSeconds { get; set; }
    public int ReplantCount { get; set; }
    public double FinalRunProducedPotato { get; set; }
}