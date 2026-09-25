using Microsoft.JSInterop;
using OimoHorihori.Constants;
using OimoHorihori.Models;

namespace OimoHorihori.Pages;

public partial class Home
{
    private PeriodicTimer? timer;
    private CancellationTokenSource cancellationTokenSource = new();
    private DateTimeOffset lastUpdateTime;
    private DateTimeOffset lastAutoSaveTime;
    private DateTimeOffset lastAutoActionTime;

    private void ResetRuntimeTimestamps(DateTimeOffset now)
    {
        lastUpdateTime = now;
        lastAutoSaveTime = now;
        lastAutoActionTime = now;
    }

    private async Task StartGame()
    {
        if (isInitializing || isResetting)
        {
            return;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        game.HasStarted = true;

        if (game.GameStartedAtUtc == default)
        {
            game.GameStartedAtUtc = now;
        }

        if (game.CurrentAscentStartedAtUtc == default)
        {
            game.CurrentAscentStartedAtUtc = now;
        }

        game.RunStartedAtUtc = now;
        game.DigButtonCount++;
        game.ProducePotato(0.1);

        CheckAchievements();

        lastUpdateTime = now;
        lastAutoActionTime = now;

        await SaveGameAsync();
    }

    private async Task RunGameLoopAsync()
    {
        if (timer is null)
        {
            return;
        }

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationTokenSource.Token))
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;

                double elapsedSeconds = (now - lastUpdateTime).TotalSeconds;

                lastUpdateTime = now;

                if (isPageHidden)
                {
                    continue;
                }

                if (isInitializing || isResetting)
                {
                    continue;
                }

                if (game.GameStartedAtUtc != default && double.IsFinite(elapsedSeconds) && elapsedSeconds > 0)
                {
                    game.TotalPlayTimeSeconds += elapsedSeconds;
                }

                //
                // いも図鑑オンライン発見
                //
                if (game.GameStartedAtUtc != default)
                {
                    IReadOnlyList<OimoSpeciesDefinition> discoveries = game.AdvanceOimoDiscovery(elapsedSeconds);

                    if (discoveries.Count > 0)
                    {
                        lastDiscoveredOimo = discoveries[^1];
                        showOimoDiscovery = true;

                        CheckAchievements();

                        await SaveGameAsync();
                    }
                }

                //
                // 通常生産
                //
                if (game.HasStarted)
                {
                    game.ProduceForSeconds(elapsedSeconds);
                    game.BestProductionPerSecond = Math.Max(game.BestProductionPerSecond, game.ProductionPerSecond);

                    CheckAchievements();
                }

                //
                // AUTO BUY / AUTO RETILL
                //
                if (game.HasStarted && (now - lastAutoActionTime).TotalSeconds >= GameConstants.AutoActionIntervalSeconds)
                {
                    bool automationChanged = game.ProcessAutomation();

                    lastAutoActionTime = now;

                    if (automationChanged)
                    {
                        CheckAchievements();
                    }
                }

                //
                // Auto Save
                //
                if (game.GameStartedAtUtc != default && (now - lastAutoSaveTime).TotalSeconds >= GameConstants.AutoSaveIntervalSeconds)
                {
                    await SaveGameAsync();

                    lastAutoSaveTime = now;
                }

                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        cancellationTokenSource.Cancel();

        timer?.Dispose();

        if (visibilityReference is not null)
        {
            try
            {
                await JS.InvokeVoidAsync("oimoVisibility.unregister");
            }
            catch
            {
                // ページ終了中などは無視
            }

            visibilityReference.Dispose();
        }

        cancellationTokenSource.Dispose();
    }
}