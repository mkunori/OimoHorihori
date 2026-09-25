using Microsoft.JSInterop;
using OimoHorihori.Models;

namespace OimoHorihori.Pages;

public partial class Home
{
    private bool showOfflineResult;
    private TimeSpan offlineDuration;
    private double offlineReward;
    private List<OimoSpeciesDefinition> offlineOimoDiscoveries = new();
    private bool isPageHidden;
    private DateTimeOffset? pageHiddenAtUtc;
    private DotNetObjectReference<Home>? visibilityReference;

    private void CloseOfflineResult()
    {
        showOfflineResult = false;

        offlineOimoDiscoveries.Clear();
    }

    [JSInvokable]
    public async Task OnVisibilityChanged(bool hidden)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        if (hidden)
        {
            if (isPageHidden)
            {
                return;
            }

            isPageHidden = true;
            pageHiddenAtUtc = now;

            ResetRuntimeTimestamps(now);

            await SaveGameAsync();

            return;
        }

        //
        // ここからvisible復帰
        //
        if (!isPageHidden)
        {
            lastUpdateTime = now;

            return;
        }

        isPageHidden = false;

        if (pageHiddenAtUtc is null)
        {
            lastUpdateTime = now;

            return;
        }

        double elapsedSeconds = (now - pageHiddenAtUtc.Value).TotalSeconds;

        pageHiddenAtUtc = null;

        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds <= 0)
        {
            lastUpdateTime = now;

            return;
        }

        //
        // 芋のオフライン生産
        //
        if (game.HasStarted)
        {
            double cappedSeconds = Math.Min(elapsedSeconds, game.OfflineLimitSeconds);

            offlineReward = game.ProduceOffline(elapsedSeconds);
            offlineDuration = TimeSpan.FromSeconds(cappedSeconds);
        }
        else
        {
            offlineReward = 0;
            offlineDuration = TimeSpan.Zero;
        }

        //
        // 図鑑は別途最大24時間
        //
        if (game.GameStartedAtUtc != default)
        {
            IReadOnlyList<OimoSpeciesDefinition> discoveries = game.ProduceOfflineOimoDiscoveries(elapsedSeconds);

            offlineOimoDiscoveries = discoveries.ToList();
        }

        //
        // 数秒アプリを切り替えただけでは
        // おかえりモーダルを出さない
        //
        if ((game.HasStarted && elapsedSeconds >= 30) || offlineOimoDiscoveries.Count > 0)
        {
            showOfflineResult = true;
        }

        CheckAchievements();

        //
        // hidden時間を通常ゲームループで
        // 二重計上させない
        //
        ResetRuntimeTimestamps(now);

        await SaveGameAsync();

        await InvokeAsync(StateHasChanged);
    }
}