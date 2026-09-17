using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using OimoHorihori.Shared.Saves;

namespace OimoHorihori.Services;

public class ServerSaveService
{
    private readonly HttpClient http;
    private readonly AuthService authService;

    public ServerSaveService(HttpClient http, AuthService authService)
    {
        this.http = http;
        this.authService = authService;
    }

    public async Task<ServerSaveResponse?> LoadAsync()
    {
        string? token = await authService.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        using HttpRequestMessage request = new(HttpMethod.Get, "api/save");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using HttpResponseMessage response = await http.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ServerSaveResponse>();
    }

    public async Task<ServerSaveResult> SaveAsync(long revision, SaveData save)
    {
        string? token = await authService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return new ServerSaveResult(false, false, null);
        }

        ServerSaveRequest body = new(revision, save);

        using HttpRequestMessage request = new(HttpMethod.Put, "api/save");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        request.Content = JsonContent.Create(body);

        using HttpResponseMessage response = await http.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            ServerSaveResponse? latest = await response.Content.ReadFromJsonAsync<ServerSaveResponse>();

            return new ServerSaveResult(false, true, latest);
        }

        if (!response.IsSuccessStatusCode)
        {
            return new ServerSaveResult(false, false, null);
        }

        ServerSaveResponse? saved = await response.Content.ReadFromJsonAsync<ServerSaveResponse>();

        return new ServerSaveResult(true, false, saved);
    }
}