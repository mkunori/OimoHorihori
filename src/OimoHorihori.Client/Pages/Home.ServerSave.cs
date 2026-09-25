using OimoHorihori.Constants;
using OimoHorihori.Models;
using OimoHorihori.Services;
using OimoHorihori.Shared.Saves;

namespace OimoHorihori.Pages;

public partial class Home
{
    private long serverRevision;
    private bool serverSaveReady;
    private bool showLocalSaveMigration;
    private SaveData? pendingLocalSave;
    private ServerSaveResponse? conflictingServerSave;
    private bool showSaveConflict;

    private async Task InitializeServerSaveAsync()
    {
        serverSaveReady = false;

        if (currentUser is null)
        {
            serverSaveReady = false;
            serverRevision = 0;

            return;
        }

        try
        {
            ServerSaveResponse? serverSave = await ServerSave.LoadAsync();

            //
            // すでにサーバーセーブがある
            //
            if (serverSave is not null)
            {
                int loadedVersion = serverSave.Save.Version;

                bool loaded = game.TryLoadSaveData(serverSave.Save);

                if (!loaded)
                {
                    Console.WriteLine("Server save load failed. " + $"Version={loadedVersion}");

                    return;
                }

                serverRevision = serverSave.Revision;

                //
                // 古いSaveVersionなら、
                // GameStateで移行したものを
                // Serverへ保存し直す
                //
                if (loadedVersion
                    < GameConstants.SaveVersion)
                {
                    SaveData migratedSave = game.CreateSaveData();

                    ServerSaveResult result = await ServerSave.SaveAsync(serverRevision, migratedSave);

                    if (result.Success && result.Save is not null)
                    {
                        serverRevision = result.Save.Revision;

                        serverSaveReady = true;

                        await SaveService.SaveAsync(result.Save.Save);

                        return;
                    }

                    if (result.Conflict
                        && result.Save is not null)
                    {
                        conflictingServerSave = result.Save;

                        showSaveConflict = true;

                        return;
                    }

                    return;
                }

                //
                // 現行Versionなら
                // そのままLocalへキャッシュ
                //
                await SaveService.SaveAsync(serverSave.Save);

                serverSaveReady = true;

                return;
            }

            //
            // Server Saveがまだ無い
            //
            SaveData? localSave = await SaveService.LoadAsync();

            if (localSave is not null)
            {
                pendingLocalSave = localSave;
                showLocalSaveMigration = true;

                return;
            }

            //
            // Localにも何も無ければ
            // 初期状態をServerへ作成
            //
            await CreateInitialServerSaveAsync();
        }
        catch (HttpRequestException)
        {
            //
            // Serverに接続できなくても
            // ローカルゲーム自体は続ける
            //
            serverSaveReady = false;
        }
    }

    private async Task MigrateLocalSaveAsync()
    {
        if (pendingLocalSave is null)
        {
            return;
        }

        ServerSaveResult result = await ServerSave.SaveAsync(0, pendingLocalSave);

        if (result.Success && result.Save is not null)
        {
            serverRevision = result.Save.Revision;

            serverSaveReady = true;

            game.TryLoadSaveData(result.Save.Save);

            await SaveService.SaveAsync(result.Save.Save);

            pendingLocalSave = null;
            showLocalSaveMigration = false;

            return;
        }

        //
        // 確認している間に別端末が
        // Server Saveを作った場合
        //
        if (result.Conflict
            && result.Save is not null)
        {
            conflictingServerSave = result.Save;

            showLocalSaveMigration = false;
            showSaveConflict = true;
        }
    }

    private async Task CreateInitialServerSaveAsync()
    {
        SaveData save = game.CreateSaveData();

        ServerSaveResult result = await ServerSave.SaveAsync(0, save);

        if (result.Success && result.Save is not null)
        {
            serverRevision = result.Save.Revision;
            serverSaveReady = true;

            await SaveService.SaveAsync(result.Save.Save);

            return;
        }

        if (result.Conflict && result.Save is not null)
        {
            conflictingServerSave = result.Save;

            showSaveConflict = true;
        }
    }

    private async Task LoadConflictingSaveAsync()
    {
        if (conflictingServerSave is null)
        {
            return;
        }

        bool loaded = game.TryLoadSaveData(conflictingServerSave.Save);

        if (!loaded)
        {
            return;
        }

        serverRevision = conflictingServerSave.Revision;
        serverSaveReady = true;

        await SaveService.SaveAsync(conflictingServerSave.Save);

        conflictingServerSave = null;
        showSaveConflict = false;

        purchaseMode = PurchaseMode.One;

        DateTimeOffset now = DateTimeOffset.UtcNow;

        ResetRuntimeTimestamps(now);
    }
}