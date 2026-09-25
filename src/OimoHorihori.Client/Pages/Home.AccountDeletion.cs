using OimoHorihori.Shared.Accounts;
using OimoHorihori.Shared.Saves;

namespace OimoHorihori.Pages;

public partial class Home
{
    private bool showDeleteAccountConfirmation;
    private bool isDeletingAccount;
    private string? deleteAccountError;

    private void OpenDeleteAccountConfirmation()
    {
        deleteAccountError = null;

        showDeleteAccountConfirmation = true;
    }

    private void CloseDeleteAccountConfirmation()
    {
        if (isDeletingAccount)
        {
            return;
        }

        showDeleteAccountConfirmation = false;
        deleteAccountError = null;
    }

    private async Task DeleteAccountAsync()
    {
        if (currentUser is null || isDeletingAccount)
        {
            return;
        }

        isDeletingAccount = true;
        deleteAccountError = null;

        try
        {
            DeleteAccountResponse? result = await AuthService.DeleteAccountAsync();

            if (result is null)
            {
                deleteAccountError = "アカウントを削除できませんでした。";

                return;
            }

            //
            // Serverの最終セーブを
            // localStorageへ引き継ぐ
            //
            if (result.LatestSave is not null)
            {
                bool loaded = game.TryLoadSaveData(result.LatestSave);

                if (!loaded)
                {
                    deleteAccountError = "セーブデータを読み込めませんでした。";

                    return;
                }

                await SaveService.SaveAsync(result.LatestSave);
            }
            else
            {
                //
                // 通常は発生しないが、
                // Server Saveが無い場合の保険
                //
                SaveData localSave = game.CreateSaveData();

                await SaveService.SaveAsync(localSave);
            }

            currentUser = null;

            serverRevision = 0;
            serverSaveReady = false;

            pendingLocalSave = null;
            conflictingServerSave = null;

            showLocalSaveMigration = false;
            showSaveConflict = false;

            titleSettings = null;
            selectedPublicProfile = null;

            showDeleteAccountConfirmation = false;

            loginUserName = string.Empty;
            loginPin = string.Empty;
            loginError = null;

            loginMessage = "アカウントを削除しました。" + "ゲームはローカルデータで続けられます。";

            currentView =
                HomeView.Account;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Account delete failed: {ex}");

            deleteAccountError = "アカウントを削除できませんでした。";
        }
        finally
        {
            isDeletingAccount = false;
        }
    }
}