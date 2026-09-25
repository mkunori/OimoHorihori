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

    public bool ProcessAutoBuy()
    {
        if (!AutoBuyUnlocked || !AutoBuyEnabled)
        {
            return false;
        }

        for (int i = Farms.Count - 1; i >= 0; i--)
        {
            Farm farm = Farms[i];

            if (farm.IsMaxLevel)
            {
                continue;
            }

            int affordableLevels = farm.GetAffordableLevels(Potato, 1, FieldCostMultiplier);

            if (affordableLevels <= 0)
            {
                continue;
            }

            int purchasedLevels = BuyFarm(farm, 1);

            return purchasedLevels > 0;
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

    public bool ProcessAutomation()
    {
        bool changed = false;

        // すでにLv上限なら
        // 先にRETILLする
        if (ProcessAutoRetill() > 0)
        {
            changed = true;
        }

        // 1回の処理につき
        // 購入は1Lvだけ
        if (ProcessAutoBuy())
        {
            changed = true;
        }

        // 今回の+1でLv上限に届いた場合、
        // 同じ処理内でRETILLする
        if (ProcessAutoRetill() > 0)
        {
            changed = true;
        }

        return changed;
    }
}