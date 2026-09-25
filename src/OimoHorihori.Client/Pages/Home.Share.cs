using Microsoft.JSInterop;
using OimoHorihori.Utilities;

namespace OimoHorihori.Pages;

public partial class Home
{
    private string? shareResultMessage;

    private string BuildShareText()
    {
        return
            "🍠 OIMO HORIHORI\n"
            + $"累計 {NumberFormatter.Format(game.TotalPotato)}芋"
            + $" / REPLANT {game.ReplantCount}回"
            + $" / 種芋 {game.SeedPotato}"
            + $" / 図鑑 {game.DiscoveredOimoSpeciesCount}/20\n"
            + "#OIMOHORIHORI";
    }

    private async Task ShareGameAsync()
    {
        string text = BuildShareText();
        string result = await JS.InvokeAsync<string>("oimoShare", text);

        shareResultMessage = result switch
        {
            "shared" => "共有しました",
            "copied" => "共有テキストをコピーしました",
            "cancelled" => null,
            _ => "共有できませんでした"
        };
    }
}