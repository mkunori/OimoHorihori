namespace OimoHorihori.Models;

public partial class GameState
{
    public bool IsOimoPowerFeatureUnlocked => AscentCount >= 1;
    public int UnlockedOimoPowerCount => UnlockedOimoPowerIds.Count;
    public double OimoAllProductionMultiplier => GetOimoPowerMultiplier(OimoPowerEffectType.AllProductionMultiplier);
    public double OimoFarmProductionMultiplier => GetOimoPowerMultiplier(OimoPowerEffectType.FarmProductionMultiplier);
    public double OimoSeedMultiplier => GetOimoPowerMultiplier(OimoPowerEffectType.SeedGainMultiplier);
    public double OimoFieldCostMultiplier => GetOimoPowerMultiplier(OimoPowerEffectType.FieldCostMultiplier);
    public double OimoRetillFinalMultiplier => GetOimoPowerMultiplier(OimoPowerEffectType.RetillFinalMultiplier);

    public double OimoRetillBaseBonus
    {
        get
        {
            double bonus = 0;

            foreach (OimoSpeciesDefinition species in OimoSpeciesCatalog.All)
            {
                if (!IsOimoPowerUnlocked(species.Id))
                {
                    continue;
                }

                if (species.OimoPowerEffectType != OimoPowerEffectType.RetillBaseBonus)
                {
                    continue;
                }

                bonus += species.OimoPowerValue;
            }

            return bonus;
        }
    }

    private double GetOimoPowerMultiplier(OimoPowerEffectType effectType)
    {
        double multiplier = 1.0;

        foreach (OimoSpeciesDefinition species in OimoSpeciesCatalog.All)
        {
            if (!IsOimoPowerUnlocked(species.Id))
            {
                continue;
            }

            if (species.OimoPowerEffectType != effectType)
            {
                continue;
            }

            multiplier *= species.OimoPowerValue;
        }

        return multiplier;
    }

    public bool IsOimoPowerUnlocked(string speciesId)
    {
        return UnlockedOimoPowerIds.Contains(speciesId);
    }

    public bool CanUnlockOimoPower(OimoSpeciesDefinition species)
    {
        if (!IsOimoPowerFeatureUnlocked)
        {
            return false;
        }

        if (!IsOimoDiscovered(species.Id))
        {
            return false;
        }

        if (IsOimoPowerUnlocked(species.Id))
        {
            return false;
        }

        if (!double.IsFinite(species.OimoPowerCost) || species.OimoPowerCost <= 0)
        {
            return false;
        }

        return Potato >= species.OimoPowerCost;
    }

    public bool UnlockOimoPower(OimoSpeciesDefinition species)
    {
        if (!CanUnlockOimoPower(species))
        {
            return false;
        }

        Potato -= species.OimoPowerCost;

        TotalConsumedPotato += species.OimoPowerCost;
        TotalOimoPowerSpentPotato += species.OimoPowerCost;

        UnlockedOimoPowerIds.Add(species.Id);

        BestProductionPerSecond = Math.Max(BestProductionPerSecond, ProductionPerSecond);

        return true;
    }

    public double GetOimoRetillFinalMultiplier(Farm farm)
    {
        if (farm.RetillCount <= 0)
        {
            return 1.0;
        }

        return OimoRetillFinalMultiplier;
    }
}