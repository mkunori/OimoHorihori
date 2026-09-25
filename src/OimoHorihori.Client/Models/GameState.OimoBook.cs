using OimoHorihori.Constants;

namespace OimoHorihori.Models;

public partial class GameState
{
    public int GetOimoDiscoveryCount(string speciesId)
    {
        return OimoDiscoveryCounts.TryGetValue(speciesId, out int count) ? count : 0;
    }

    public bool IsOimoDiscovered(string speciesId)
    {
        return GetOimoDiscoveryCount(speciesId) > 0;
    }

    public int DiscoveredOimoSpeciesCount
    {
        get
        {
            int count = 0;

            foreach (OimoSpeciesDefinition species in OimoSpeciesCatalog.All)
            {
                if (IsOimoDiscovered(species.Id))
                {
                    count++;
                }
            }

            return count;
        }
    }

    public int TotalOimoDiscoveries
    {
        get
        {
            int total = 0;

            foreach (OimoSpeciesDefinition species in OimoSpeciesCatalog.All)
            {
                total += GetOimoDiscoveryCount(species.Id);
            }

            return total;
        }
    }

    public OimoSpeciesDefinition? TryDiscoverOimo()
    {
        double roll = Random.Shared.NextDouble();

        if (roll >= GameConstants.OimoDiscoveryChance)
        {
            return null;
        }

        return DiscoverRandomOimo();
    }

    private void AddOimoDiscovery(OimoSpeciesDefinition species)
    {
        if (!OimoDiscoveryCounts.TryAdd(species.Id, 1))
        {
            OimoDiscoveryCounts[species.Id]++;
        }
    }

    private OimoSpeciesDefinition DiscoverRandomOimo()
    {
        int index = Random.Shared.Next(OimoSpeciesCatalog.All.Count);

        OimoSpeciesDefinition species = OimoSpeciesCatalog.All[index];

        AddOimoDiscovery(species);

        return species;
    }

    public IReadOnlyList<OimoSpeciesDefinition> AdvanceOimoDiscovery(double seconds)
    {
        if (!double.IsFinite(seconds) || seconds <= 0)
        {
            return Array.Empty<OimoSpeciesDefinition>();
        }

        OimoDiscoveryElapsedSeconds += seconds;

        int trialCount = (int)Math.Floor(OimoDiscoveryElapsedSeconds / GameConstants.OimoDiscoveryIntervalSeconds);

        if (trialCount <= 0)
        {
            return Array.Empty<OimoSpeciesDefinition>();
        }

        OimoDiscoveryElapsedSeconds -= trialCount * GameConstants.OimoDiscoveryIntervalSeconds;

        List<OimoSpeciesDefinition> discoveries = new();

        for (int i = 0; i < trialCount; i++)
        {
            OimoSpeciesDefinition? discovered = TryDiscoverOimo();

            if (discovered is not null)
            {
                discoveries.Add(discovered);
            }
        }

        return discoveries;
    }

    public IReadOnlyList<OimoSpeciesDefinition> ProduceOfflineOimoDiscoveries(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds <= 0)
        {
            return Array.Empty<OimoSpeciesDefinition>();
        }

        double cappedSeconds = Math.Min(elapsedSeconds, GameConstants.OimoOfflineDiscoveryLimitSeconds);

        double totalElapsedSeconds = OimoDiscoveryElapsedSeconds + cappedSeconds;

        int trialCount = (int)Math.Floor(totalElapsedSeconds / GameConstants.OimoDiscoveryIntervalSeconds);

        OimoDiscoveryElapsedSeconds = totalElapsedSeconds - trialCount * GameConstants.OimoDiscoveryIntervalSeconds;

        if (trialCount <= 0)
        {
            return Array.Empty<OimoSpeciesDefinition>();
        }

        double cumulativeChance =
            1.0
            - Math.Pow(1.0 - GameConstants.OimoDiscoveryChance, trialCount);

        cumulativeChance = Math.Clamp(cumulativeChance, 0.0, 1.0);

        if (Random.Shared.NextDouble() >= cumulativeChance)
        {
            return Array.Empty<OimoSpeciesDefinition>();
        }

        OimoSpeciesDefinition discovered =
            DiscoverRandomOimo();

        return new[]
        {
            discovered
        };
    }
}