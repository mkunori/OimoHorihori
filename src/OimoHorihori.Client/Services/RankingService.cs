using System.Net.Http.Headers;
using System.Net.Http.Json;
using OimoHorihori.Shared.Rankings;

namespace OimoHorihori.Services;

public class RankingService
{
    private readonly HttpClient http;
    private readonly AuthService authService;

    public RankingService(HttpClient http, AuthService authService)
    {
        this.http = http;
        this.authService = authService;
    }

    public async Task<RankingResponse?> GetAsync(RankingCategory category)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, $"api/rankings/{category}");

        string? token = await authService.GetTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using HttpResponseMessage response = await http.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<RankingResponse>();
    }
}