using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using OimoHorihori.Shared.Auth;
using OimoHorihori.Shared.Accounts;

namespace OimoHorihori.Services;

public class AuthService
{
    private const string TokenKey = "oimo.sessionToken";

    private readonly HttpClient http;
    private readonly IJSRuntime js;

    public AuthService(HttpClient http, IJSRuntime js)
    {
        this.http = http;
        this.js = js;
    }

    public async Task SaveTokenAsync(string token)
    {
        await js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
    }

    public async Task ClearTokenAsync()
    {
        await js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
    }

    public async Task<CurrentUserResponse?> GetCurrentUserAsync()
    {
        string? token = await GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        using HttpRequestMessage request = new(HttpMethod.Get, "api/auth/me");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using HttpResponseMessage response = await http.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await ClearTokenAsync();

            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
    }

    public async Task LogoutAsync()
    {
        string? token = await GetTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            using HttpRequestMessage request = new(HttpMethod.Post, "api/auth/logout");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                await http.SendAsync(request);
            }
            catch
            {
                // Serverに到達できなくても
                // ローカル側はログアウトする
            }
        }

        await ClearTokenAsync();
    }

    public async Task<DeleteAccountResponse?> DeleteAccountAsync()
    {
        string? token = await GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        using HttpRequestMessage request = new(HttpMethod.Delete, "api/account");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using HttpResponseMessage response = await http.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await ClearTokenAsync();

            return null;
        }

        response.EnsureSuccessStatusCode();

        DeleteAccountResponse? result = await response.Content.ReadFromJsonAsync<DeleteAccountResponse>();

        await ClearTokenAsync();

        return result;
    }
}