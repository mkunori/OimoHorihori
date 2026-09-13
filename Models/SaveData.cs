namespace OimoHorihori.Models;

public class SaveData
{
    public int Version { get; set; } = 1;

    public bool HasStarted { get; set; }

    public double Potato { get; set; }

    public double TotalPotato { get; set; }

    public int Farm1Level { get; set; }

    public int Farm2Level { get; set; }

    public int Farm3Level { get; set; }

    public DateTimeOffset LastSaveTimeUtc { get; set; }
}