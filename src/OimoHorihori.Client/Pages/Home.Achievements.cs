using OimoHorihori.Models;

namespace OimoHorihori.Pages;

public partial class Home
{
    private Queue<AchievementDefinition> achievementNotificationQueue = new();

    private AchievementDefinition? currentAchievementNotification;

    private void CheckAchievements()
    {
        IReadOnlyList<AchievementDefinition> unlockedAchievements = game.CheckAchievements();

        foreach (
            AchievementDefinition achievement in unlockedAchievements)
        {
            achievementNotificationQueue.Enqueue(achievement);
        }

        ShowNextAchievementNotification();
    }

    private void ShowNextAchievementNotification()
    {
        if (currentAchievementNotification is not null)
        {
            return;
        }

        if (achievementNotificationQueue.Count == 0)
        {
            return;
        }

        currentAchievementNotification = achievementNotificationQueue.Dequeue();
    }

    private void CloseAchievementNotification()
    {
        currentAchievementNotification = null;

        ShowNextAchievementNotification();
    }
}