using OimoHorihori.Models;
using OimoHorihori.Services;
using OimoHorihori.Shared.Saves;

namespace OimoHorihori.Pages;

public partial class Home
{
    private int? newerSaveVersion;
    private SaveStatus saveStatus = SaveStatus.None;
    private DateTimeOffset? lastSaveCompletedAtUtc;

    private async Task SaveGameAsync()
    {
        if (showSaveConflict)
        {
            saveStatus = SaveStatus.Conflict;

            return;
        }

        saveStatus = SaveStatus.Saving;

        await InvokeAsync(StateHasChanged);

        SaveData save = game.CreateSaveData();

        //
        // 未登録・未ログイン
        //
        if (currentUser is null)
        {
            await SaveService.SaveAsync(save);

            lastSaveCompletedAtUtc = DateTimeOffset.UtcNow;
            saveStatus = SaveStatus.LocalSaved;

            return;
        }

        //
        // Server Save準備前
        //
        if (!serverSaveReady)
        {
            await SaveService.SaveAsync(save);

            lastSaveCompletedAtUtc = DateTimeOffset.UtcNow;
            saveStatus = SaveStatus.LocalSaved;

            return;
        }

        try
        {
            ServerSaveResult result = await ServerSave.SaveAsync(serverRevision, save);

            //
            // Server保存成功
            //
            if (result.Success && result.Save is not null)
            {
                serverRevision = result.Save.Revision;
                serverSaveReady = true;

                //
                // localStorageは
                // キャッシュとして保持
                //
                await SaveService.SaveAsync(result.Save.Save);
                lastSaveCompletedAtUtc = DateTimeOffset.UtcNow;
                saveStatus = SaveStatus.ServerSaved;

                return;
            }

            //
            // Revision競合
            //
            if (result.Conflict && result.Save is not null)
            {
                conflictingServerSave = result.Save;
                showSaveConflict = true;
                saveStatus = SaveStatus.Conflict;

                return;
            }

            //
            // Server保存に失敗したが
            // Conflictではない場合はLocalへ退避
            //
            await SaveService.SaveAsync(save);

            lastSaveCompletedAtUtc = DateTimeOffset.UtcNow;
            saveStatus = SaveStatus.LocalSaved;
        }
        catch (HttpRequestException)
        {
            //
            // Serverへ到達できなければ
            // localStorageへ退避
            //
            try
            {
                await SaveService.SaveAsync(save);

                lastSaveCompletedAtUtc = DateTimeOffset.UtcNow;
                saveStatus = SaveStatus.ServerUnavailable;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Local save failed: {ex}");

                saveStatus = SaveStatus.Error;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Save failed: {ex}");

            saveStatus = SaveStatus.Error;
        }
    }
}