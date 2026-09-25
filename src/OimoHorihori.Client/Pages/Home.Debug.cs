using OimoHorihori.Constants;

namespace OimoHorihori.Pages;

public partial class Home
{
    private async Task DebugAddPotatoAsync(double amount)
    {
        game.ProducePotato(amount);

        await SaveGameAsync();
    }

    private async Task DebugOfflineAsync(double seconds)
    {
        double cappedSeconds = Math.Min(seconds, game.OfflineLimitSeconds);

        offlineReward = game.ProduceOffline(seconds);
        offlineDuration = TimeSpan.FromSeconds(cappedSeconds);
        showOfflineResult = true;

        await SaveGameAsync();
    }

    private async Task DebugAddSeedPotatoAsync()
    {
        game.SeedPotato += 100;
        game.TotalSeedPotatoEarned += 100;

        await SaveGameAsync();
    }

    private async Task DebugBuyProductionUpgradeAsync()
    {
        if (game.BuyProductionUpgrade())
        {
            CheckAchievements();

            await SaveGameAsync();
        }
    }

    private async Task DebugBuyDigUpgradeAsync()
    {
        if (game.BuyDigUpgrade())
        {
            CheckAchievements();

            await SaveGameAsync();
        }
    }

    private async Task DebugBuyOfflineUpgradeAsync()
    {
        if (game.BuyOfflineUpgrade())
        {
            CheckAchievements();

            await SaveGameAsync();
        }
    }

    private async Task DebugBuyRetillEfficiencyUpgradeAsync()
    {
        if (game.BuyRetillEfficiencyUpgrade())
        {
            CheckAchievements();

            await SaveGameAsync();
        }
    }

    private async Task DebugBuyFieldCostReductionUpgradeAsync()
    {
        if (game.BuyFieldCostReductionUpgrade())
        {
            CheckAchievements();

            await SaveGameAsync();
        }
    }

    private async Task DebugAscentReadyAsync()
    {
        if (game.CurrentAscentReplantCount == 0)
        {
            double replantRemaining = Math.Max(0, GameConstants.ReplantBaseProduction - game.RunProducedPotato);

            if (replantRemaining > 0)
            {
                game.ProducePotato(replantRemaining);
            }

            game.Replant();

            game.HasStarted = true;

            game.RunStartedAtUtc = DateTimeOffset.UtcNow;
        }

        double ascentRemaining = Math.Max(0, GameConstants.AscentTargetProduction - game.RunProducedPotato);

        if (ascentRemaining > 0)
        {
            game.ProducePotato(ascentRemaining);
        }

        CheckAchievements();

        DateTimeOffset now = DateTimeOffset.UtcNow;

        lastUpdateTime = now;
        lastAutoActionTime = now;

        await SaveGameAsync();
    }
}