using OimoHorihori.Models;

namespace OimoHorihori.Pages;

public partial class Home
{
    private bool showOimoPowerConfirmation;

    private OimoSpeciesDefinition? selectedOimoPowerSpecies;

    private void OpenOimoPowerConfirmation(OimoSpeciesDefinition species)
    {
        if (!game.CanUnlockOimoPower(species))
        {
            return;
        }

        selectedOimoPowerSpecies = species;
        showOimoPowerConfirmation = true;
    }

    private void CancelOimoPowerConfirmation()
    {
        showOimoPowerConfirmation = false;
        selectedOimoPowerSpecies = null;
    }

    private async Task ConfirmOimoPowerAsync()
    {
        if (selectedOimoPowerSpecies is null)
        {
            showOimoPowerConfirmation = false;

            return;
        }

        bool unlocked = game.UnlockOimoPower(selectedOimoPowerSpecies);

        if (!unlocked)
        {
            showOimoPowerConfirmation = false;
            selectedOimoPowerSpecies = null;

            return;
        }

        showOimoPowerConfirmation = false;
        selectedOimoPowerSpecies = null;

        CheckAchievements();

        await SaveGameAsync();
    }
}