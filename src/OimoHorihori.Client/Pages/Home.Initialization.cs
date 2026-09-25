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

            try
            {
                currentUser = await AuthService.GetCurrentUserAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auth restore failed: {ex}");

                currentUser = null;
            }

            SaveData? save = await SaveService.LoadAsync();

            DateTimeOffset now = DateTimeOffset.UtcNow;

            if (save is not null)
            {
                int loadedVersion = save.Version;

                //
                // 現在のアプリより新しい
                // セーブは絶対に触らない
                //
                if (loadedVersion
                    > GameConstants.SaveVersion)
                {
                    newerSaveVersion = loadedVersion;

                    return;
                }

                bool loaded = game.TryLoadSaveData(save);

                if (!loaded)
                {
                    Console.WriteLine("Save load failed. " + $"Version={save.Version}");
                }
                else
                {
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

                    if (elapsedSeconds > 0)
                    {
                        if (game.HasStarted)
                        {
                            double cappedSeconds = Math.Min(elapsedSeconds, game.OfflineLimitSeconds);

                            offlineReward = game.ProduceOffline(elapsedSeconds);
                            offlineDuration = TimeSpan.FromSeconds(cappedSeconds);
                        }

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
                    }
                }
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
            lastUpdateTime = now;
            lastAutoSaveTime = now;
            lastAutoActionTime = now;

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

            //
            // 読み込んだSaveで
            // 新たに条件を満たした実績を確認
            //
            IReadOnlyList<AchievementDefinition> unlockedAchievements = game.CheckAchievements();

            foreach (AchievementDefinition achievement in unlockedAchievements)
            {
                achievementNotificationQueue.Enqueue(achievement);
            }

            if (unlockedAchievements.Count > 0)
            {
                ShowNextAchievementNotification();

                await SaveGameAsync();
            }
        }
        finally
        {
            isInitializing = false;
        }
    }
}