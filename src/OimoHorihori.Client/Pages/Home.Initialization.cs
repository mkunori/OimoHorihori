using Microsoft.JSInterop;
using OimoHorihori.Constants;
using OimoHorihori.Models;
using OimoHorihori.Shared.Saves;

namespace OimoHorihori.Pages;

public partial class Home
{
    private bool isInitializing = true;

    private string initializationMessage = "セーブデータを読み込み中...";

    protected override async Task OnInitializedAsync()
    {
        try
        {
            initializationMessage = "セーブデータを読み込み中...";

            await RestoreAuthenticationAsync();

            DateTimeOffset now = DateTimeOffset.UtcNow;

            bool localSaveLoaded = await LoadLocalSaveAsync(now);

            if (!localSaveLoaded)
            {
                return;
            }

            //
            // ログイン済みならServer Saveと同期
            //
            if (currentUser is not null && newerSaveVersion is null)
            {
                initializationMessage = "サーバーと同期中...";

                await InvokeAsync(StateHasChanged);
                await InitializeServerSaveAsync();
            }

            //
            // 各種時間基準を初期化
            //
            ResetRuntimeTimestamps(now);
            await StartRuntimeAsync();

            //
            // 読み込んだSaveで
            // 新たに条件を満たした実績を確認
            //
            await CheckInitialAchievementsAsync();
        }
        finally
        {
            isInitializing = false;
        }
    }

    private async Task<bool> LoadLocalSaveAsync(DateTimeOffset now)
    {
        SaveData? save = await SaveService.LoadAsync();

        if (save is null)
        {
            return true;
        }

        int loadedVersion = save.Version;

        //
        // 現在のアプリより新しい
        // セーブは絶対に触らない
        //
        if (loadedVersion > GameConstants.SaveVersion)
        {
            newerSaveVersion = loadedVersion;

            return false;
        }

        bool loaded = game.TryLoadSaveData(save);

        if (!loaded)
        {
            Console.WriteLine("Save load failed. " + $"Version={save.Version}");

            return true;
        }

        //
        // 古いVersionを読み込んだ場合、
        // GameState内で移行済みのデータを
        // 現行形式で保存し直す
        //
        if (loadedVersion < GameConstants.SaveVersion)
        {
            await SaveGameAsync();
        }

        double elapsedSeconds = (now - save.LastSaveTimeUtc).TotalSeconds;

        if (elapsedSeconds <= 0)
        {
            return true;
        }

        //
        // オフライン生産
        //
        if (game.HasStarted)
        {
            double cappedSeconds = Math.Min(elapsedSeconds, game.OfflineLimitSeconds);
            offlineReward = game.ProduceOffline(elapsedSeconds);
            offlineDuration = TimeSpan.FromSeconds(cappedSeconds);
        }

        //
        // オフライン中の図鑑発見
        //
        if (game.GameStartedAtUtc != default)
        {
            IReadOnlyList<OimoSpeciesDefinition> discoveries = game.ProduceOfflineOimoDiscoveries(elapsedSeconds);

            offlineOimoDiscoveries = discoveries.ToList();
        }

        if (game.HasStarted || offlineOimoDiscoveries.Count > 0)
        {
            showOfflineResult = true;
        }

        await SaveGameAsync();

        return true;
    }

    private async Task RestoreAuthenticationAsync()
    {
        try
        {
            currentUser = await AuthService.GetCurrentUserAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Auth restore failed: {ex}");

            currentUser = null;
        }
    }

    private async Task StartRuntimeAsync()
    {
        //
        // visibility監視を開始
        //
        visibilityReference = DotNetObjectReference.Create(this);

        initializationMessage = "ゲームを準備中...";

        await JS.InvokeVoidAsync("oimoVisibility.register", visibilityReference);

        //
        // ゲームループ開始
        //
        timer = new PeriodicTimer(TimeSpan.FromMilliseconds(GameConstants.GameLoopIntervalMilliseconds));

        _ = RunGameLoopAsync();
    }

    private async Task CheckInitialAchievementsAsync()
    {
        IReadOnlyList<AchievementDefinition> unlockedAchievements = game.CheckAchievements();

        foreach (AchievementDefinition achievement in unlockedAchievements)
        {
            achievementNotificationQueue.Enqueue(achievement);
        }

        if (unlockedAchievements.Count <= 0)
        {
            return;
        }

        ShowNextAchievementNotification();

        await SaveGameAsync();
    }
}