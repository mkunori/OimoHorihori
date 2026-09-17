namespace OimoHorihori.Server.Models;

public class RankingRecord
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public double TotalPotato { get; set; }
    public double BestProductionPerSecond { get; set; }
    public int ReplantCount { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}