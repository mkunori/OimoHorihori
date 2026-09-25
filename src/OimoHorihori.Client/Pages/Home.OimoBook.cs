using OimoHorihori.Models;

namespace OimoHorihori.Pages;

public partial class Home
{
    private OimoSpeciesDefinition? lastDiscoveredOimo;
    private bool showOimoDiscovery;

    private void CloseOimoDiscovery()
    {
        showOimoDiscovery = false;
    }

    private int GetLastDiscoveredOimoCount()
    {
        if (lastDiscoveredOimo is null)
        {
            return 0;
        }

        return game.GetOimoDiscoveryCount(lastDiscoveredOimo.Id);
    }
}