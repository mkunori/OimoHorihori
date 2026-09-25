namespace OimoHorihori.Models;

public partial class GameState
{
    public bool IsAchievementUnlocked(string achievementId)
    {
        return AchievementUnlockedAtUtc.ContainsKey(achievementId);
    }

    public DateTimeOffset? GetAchievementUnlockedAtUtc(string achievementId)
    {
        if (AchievementUnlockedAtUtc.TryGetValue(achievementId, out DateTimeOffset unlockedAt))
        {
            return unlockedAt;
        }

        return null;
    }

    public IReadOnlyList<AchievementDefinition> CheckAchievements()
    {
        List<AchievementDefinition> unlockedAchievements = new();

        foreach (
            AchievementDefinition achievement in AchievementCatalog.All)
        {
            if (IsAchievementUnlocked(achievement.Id))
            {
                continue;
            }

            if (!achievement.Condition(this))
            {
                continue;
            }

            AchievementUnlockedAtUtc[achievement.Id] = DateTimeOffset.UtcNow;
            unlockedAchievements.Add(achievement);
        }

        return unlockedAchievements;
    }
}