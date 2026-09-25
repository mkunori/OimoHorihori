using OimoHorihori.Shared.Accounts;
using OimoHorihori.Shared.Auth;
using OimoHorihori.Shared.Saves;
using System.Net.Http.Json;

namespace OimoHorihori.Pages;

public partial class Home
{
    private string registerUserName = string.Empty;
    private string registerPin = string.Empty;
    private string? registerMessage;
    private string? registerError;
    private bool isRegistering;
    private string loginUserName = string.Empty;
    private string loginPin = string.Empty;
    private string? loginMessage;
    private string? loginError;
    private bool isLoggingIn;
    private CurrentUserResponse? currentUser;
    private bool showDeleteAccountConfirmation;
    private bool isDeletingAccount;
    private string? deleteAccountError;

    private async Task RegisterAccountAsync()
    {
        registerMessage = null;
        registerError = null;

        string userName = registerUserName.Trim();

        string pin = registerPin;

        if (userName.Length is < 1 or > 10)
        {
            registerError = "ユーザー名は1〜10文字で入力してください。";

            return;
        }

        bool validPin = pin.Length == 4 && pin.All(c => c is >= '0' and <= '9');

        if (!validPin)
        {
            registerError = "PINは数字4桁で入力してください。";

            return;
        }

        RegisterRequest request = new(userName, pin);

        isRegistering = true;

        try
        {
            HttpResponseMessage response = await Http.PostAsJsonAsync("api/auth/register", request);

            if (response.IsSuccessStatusCode)
            {
                RegisterResponse? result = await response.Content.ReadFromJsonAsync<RegisterResponse>();

                if (result is null)
                {
                    registerError = "サーバーから正しい応答を取得できませんでした。";

                    return;
                }

                registerMessage = $"{result.UserName} を登録しました。";
                registerPin = string.Empty;

                return;
            }

            ApiErrorResponse? error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

            registerError = error?.Message ?? "ユーザー登録に失敗しました。";
        }
        catch (HttpRequestException)
        {
            registerError = "サーバーへ接続できませんでした。";
        }
        catch (Exception)
        {
            registerError = "ユーザー登録中にエラーが発生しました。";
        }
        finally
        {
            isRegistering = false;
        }
    }

    private async Task LoginAsync()
    {
        loginMessage = null;
        loginError = null;

        string userName = loginUserName.Trim();

        string pin = loginPin;

        if (userName.Length is < 1 or > 10)
        {
            loginError = "ユーザー名またはPINが正しくありません。";

            return;
        }

        bool validPin = pin.Length == 4 && pin.All(c => c is >= '0' and <= '9');

        if (!validPin)
        {
            loginError = "ユーザー名またはPINが正しくありません。";

            return;
        }

        LoginRequest request = new(userName, pin);

        isLoggingIn = true;

        try
        {
            HttpResponseMessage response = await Http.PostAsJsonAsync("api/auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                LoginResponse? result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result is null)
                {
                    loginError = "サーバーから正しい応答を取得できませんでした。";

                    return;
                }

                await AuthService.SaveTokenAsync(result.SessionToken);

                currentUser = new CurrentUserResponse(result.UserId, result.UserName);

                isInitializing = true;

                try
                {
                    await InitializeServerSaveAsync();
                }
                finally
                {
                    isInitializing = false;
                }

                loginMessage = $"{result.UserName} でログインしました。";
                loginPin = string.Empty;

                return;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                loginError = "ユーザー名またはPINが正しくありません。";

                return;
            }

            ApiErrorResponse? error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

            loginError = error?.Message ?? "ログインに失敗しました。";
        }
        catch (HttpRequestException)
        {
            loginError = "サーバーへ接続できませんでした。";
        }
        catch (Exception)
        {
            loginError = "ログイン中にエラーが発生しました。";
        }
        finally
        {
            isLoggingIn = false;
        }
    }

    private async Task LogoutAsync()
    {
        await AuthService.LogoutAsync();

        currentUser = null;

        loginUserName = string.Empty;
        loginPin = string.Empty;
        loginMessage = null;
        loginError = null;

        serverRevision = 0;
        serverSaveReady = false;

        pendingLocalSave = null;
        conflictingServerSave = null;

        showLocalSaveMigration = false;
        showSaveConflict = false;
    }

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

            currentView = HomeView.Account;
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