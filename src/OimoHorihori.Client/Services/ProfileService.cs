using System.Net.Http.Headers;
using System.Net.Http.Json;
using OimoHorihori.Shared.Profiles;

namespace OimoHorihori.Services;

public class ProfileService
{
    private readonly HttpClient http;
    private readonly AuthService authService;

    public ProfileService(HttpClient http, AuthService authService)
    {
        this.http = http;
        this.authService = authService;
    }

    public async Task<TitleSettingsResponse?> GetTitlesAsync()
    {
        string? token = await authService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        using HttpRequestMessage request = new(HttpMethod.Get, "api/profile/titles");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using HttpResponseMessage response = await http.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TitleSettingsResponse>();
    }

    public async Task EquipTitleAsync(string? titleId)
    {
        string? token = await authService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }

        using HttpRequestMessage request = new(HttpMethod.Put, "api/profile/title");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        request.Content = JsonContent.Create(new EquipTitleRequest(titleId));

        using HttpResponseMessage response = await http.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }

    public async Task<PublicProfileResponse?> GetPublicProfileAsync(Guid userId)
    {
        using HttpResponseMessage response = await http.GetAsync($"api/profiles/{userId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PublicProfileResponse>();
    }
}