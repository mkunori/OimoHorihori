namespace OimoHorihori.Shared.Saves;

public class ReplantHistoryEntry
{
    public DateTimeOffset ReplantedAtUtc { get; set; }
    public double RunDurationSeconds { get; set; }
    public double RunProducedPotato { get; set; }
    public int EarnedSeedPotato { get; set; }
}