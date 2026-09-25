using OimoHorihori.Models;
using OimoHorihori.Shared.Saves;

namespace OimoHorihori.Pages;

public partial class Home
{
    private bool showResetConfirmation;
    private bool isResetting;
    private string? resetError;

    private void OpenResetConfirmation()
    {
        resetError = null;

        showResetConfirmation = true;
    }

    private void CancelReset()
    {
        if (isResetting)
        {
            return;
        }

        showResetConfirmation = false;
        resetError = null;
    }

    private async Task ResetGameAsync()
    {
        if (isResetting)
        {
            return;
        }

        isResetting = true;
        resetError = null;

        try
        {
            //
            // ログイン中なら
            // 先にServer Saveを削除する
            //
            if (currentUser is not null)
            {
                bool deleted = await ServerSave.DeleteAsync();

                if (!deleted)
                {
                    resetError = "サーバーのセーブデータを初期化できませんでした。";
                    saveStatus = SaveStatus.Error;

                    return;
                }
            }

            //
            // GameStateを完全初期化
            //
            game = new GameState();
            serverRevision = 0;
            serverSaveReady = false;
            pendingLocalSave = null;
            conflictingServerSave = null;
            showSaveConflict = false;
            showLocalSaveMigration = false;

            //
            // localStorageも初期状態へ
            //
            SaveData initialSave = game.CreateSaveData();

            await SaveService.SaveAsync(initialSave);

            //
            // ログイン中なら
            // 新しい初期状態をServerにも作る
            //
            if (currentUser is not null)
            {
                await CreateInitialServerSaveAsync();
            }
            else
            {
                saveStatus = SaveStatus.LocalSaved;
                lastSaveCompletedAtUtc = DateTimeOffset.UtcNow;
            }

            showResetConfirmation = false;
            showOfflineResult = false;
            showOimoDiscovery = false;

            offlineOimoDiscoveries.Clear();

            currentView = HomeView.Horihori;
            purchaseMode = PurchaseMode.One;

            DateTimeOffset now = DateTimeOffset.UtcNow;

            ResetRuntimeTimestamps(now);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(
                $"Save reset failed: {ex}");

            resetError = "サーバーへ接続できないため初期化できませんでした。";
            saveStatus = SaveStatus.Error;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Save reset failed: {ex}");

            resetError = "セーブデータの初期化に失敗しました。";
            saveStatus = SaveStatus.Error;
        }
        finally
        {
            isResetting = false;
        }
    }
}