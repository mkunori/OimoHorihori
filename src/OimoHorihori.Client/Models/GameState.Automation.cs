namespace OimoHorihori.Models;

public partial class GameState
{
    public bool UnlockAutoBuy()
    {
        if (AutoBuyUnlocked)
        {
            return false;
        }

        if (!TrySpendRoot(1))
        {
            return false;
        }

        AutoBuyUnlocked = true;
        AutoBuyEnabled = false;

        return true;
    }

    public bool SetAutoBuyEnabled(bool enabled)
    {
        if (!AutoBuyUnlocked)
        {
            AutoBuyEnabled = false;

            return false;
        }

        if (AutoBuyEnabled == enabled)
        {
            return false;
        }

        AutoBuyEnabled = enabled;

        return true;
    }

    public bool UnlockAutoRetill()
    {
        if (AutoRetillUnlocked)
        {
            return false;
        }

        if (!TrySpendRoot(1))
        {
            return false;
        }

        AutoRetillUnlocked = true;
        AutoRetillEnabled = false;

        return true;
    }

    public bool SetAutoRetillEnabled(bool enabled)
    {
        if (!AutoRetillUnlocked)
        {
            AutoRetillEnabled = false;

            return false;
        }

        if (AutoRetillEnabled == enabled)
        {
            return false;
        }

        AutoRetillEnabled = enabled;

        return true;
    }

    public bool ProcessAutoBuy(PurchaseMode purchaseMode)
    {
        if (!AutoBuyUnlocked || !AutoBuyEnabled)
        {
            return false;
        }

        int maxLevels = purchaseMode switch
        {
            PurchaseMode.One => 1,
            PurchaseMode.Ten => 10,
            PurchaseMode.Max => int.MaxValue,
            _ => 1
        };

        for (int i = Farms.Count - 1; i >= 0; i--)
        {
            Farm farm = Farms[i];

            if (farm.IsMaxLevel)
            {
                continue;
            }

            int affordableLevels = farm.GetAffordableLevels(Potato, maxLevels, FieldCostMultiplier);

            if (affordableLevels <= 0)
            {
                continue;
            }

            int purchasedLevels = BuyFarm(farm, maxLevels);

            if (purchasedLevels <= 0)
            {
                continue;
            }

            if (purchaseMode == PurchaseMode.Ten)
            {
                HasUsedTenPurchaseMode = true;
            }

            if (purchaseMode == PurchaseMode.Max)
            {
                HasUsedMaxPurchaseMode = true;
            }

            return true;
        }

        return false;
    }

    public int ProcessAutoRetill()
    {
        if (!AutoRetillUnlocked || !AutoRetillEnabled)
        {
            return 0;
        }

        int retillCount = 0;

        foreach (Farm farm in Farms)
        {
            if (!CanRetillFarm(farm))
            {
                continue;
            }

            if (RetillFarm(farm))
            {
                retillCount++;
            }
        }

        return retillCount;
    }

    public bool ProcessAutomation(PurchaseMode purchaseMode)
    {
        bool changed = false;

        // すでにLv上限なら先にRETILL
        if (ProcessAutoRetill() > 0)
        {
            changed = true;
        }

        // 現在選択中の +1 / +10 / MAX で購入
        if (ProcessAutoBuy(purchaseMode))
        {
            changed = true;
        }

        // 購入によってLv上限に到達した場合は、
        // 同じ処理内でRETILL
        if (ProcessAutoRetill() > 0)
        {
            changed = true;
        }

        return changed;
    }
}