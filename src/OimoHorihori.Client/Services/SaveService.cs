using System.Text.Json;
using Microsoft.JSInterop;
using OimoHorihori.Models;
using OimoHorihori.Shared.Saves;

namespace OimoHorihori.Services;

public class SaveService
{
    private const string SaveKey = "oimo_horihori_save";
    private readonly IJSRuntime jsRuntime;

    public SaveService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public async Task SaveAsync(SaveData saveData)
    {
        string json = JsonSerializer.Serialize(saveData);

        await jsRuntime.InvokeVoidAsync("localStorage.setItem", SaveKey, json);
    }

    public async Task<SaveData?> LoadAsync()
    {
        string? json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", SaveKey);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<SaveData>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task DeleteAsync()
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", SaveKey);
    }
}