using Microsoft.EntityFrameworkCore;
using OimoHorihori.Server.Data;
using OimoHorihori.Server.Models;
using OimoHorihori.Shared.Rankings;
using OimoHorihori.Shared.Saves;

namespace OimoHorihori.Server.Services;

public class RankingService
{
    private readonly AppDbContext db;

    public RankingService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task UpdateFromSaveAsync(UserAccount user, SaveData save)
    {
        RankingRecord? record = await db.RankingRecords.SingleOrDefaultAsync(record => record.UserId == user.Id);

        if (record is null)
        {
            record = new RankingRecord
            {
                UserId = user.Id
            };

            db.RankingRecords.Add(record);
        }

        record.UserName = user.UserName;
        record.TotalPotato = save.TotalPotato;
        record.BestProductionPerSecond = save.BestProductionPerSecond;
        record.ReplantCount = save.ReplantCount;
        record.UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public async Task<RankingResponse> GetRankingAsync(RankingCategory category, Guid? currentUserId)
    {
        List<RankingRecord> records = await db.RankingRecords.AsNoTracking().ToListAsync();

        IEnumerable<RankingRecord> ordered = category switch
        {
            RankingCategory.TotalPotato => records.OrderByDescending(record => record.TotalPotato).ThenBy(record => record.UserName),
            RankingCategory.BestProductionPerSecond => records.OrderByDescending(record => record.BestProductionPerSecond).ThenBy(record => record.UserName),
            RankingCategory.Replant => records.OrderByDescending(record => record.ReplantCount).ThenBy(record => record.UserName),
            _ => records
        };

        List<RankingRecord> sorted = ordered.ToList();
        List<RankingEntryResponse> entries = new();
        double? previousValue = null;
        int currentRank = 0;

        for (int i = 0; i < sorted.Count; i++)
        {
            RankingRecord record = sorted[i];

            double value = GetValue(record, category);

            if (previousValue is null || value != previousValue.Value)
            {
                currentRank = i + 1;
            }

            entries.Add(new RankingEntryResponse(record.UserId, currentRank, record.UserName, value, currentUserId.HasValue && record.UserId == currentUserId.Value));

            previousValue = value;
        }

        List<RankingEntryResponse> topEntries = entries.Take(30).ToList();

        RankingEntryResponse? myEntry = currentUserId.HasValue ? entries.FirstOrDefault(entry => entry.IsCurrentUser) : null;

        return new RankingResponse(category, topEntries, myEntry);
    }

    private static double GetValue(RankingRecord record, RankingCategory category)
    {
        return category switch
        {
            RankingCategory.TotalPotato => record.TotalPotato,
            RankingCategory.BestProductionPerSecond => record.BestProductionPerSecond,
            RankingCategory.Replant => record.ReplantCount,
            _ => 0
        };
    }
}