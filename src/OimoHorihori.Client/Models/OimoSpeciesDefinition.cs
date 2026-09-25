namespace OimoHorihori.Models;

public class OimoSpeciesDefinition
{
    public string Id { get; }
    public string Name { get; }

    public double OimoPowerCost { get; }

    public OimoPowerEffectType OimoPowerEffectType { get; }

    public double OimoPowerValue { get; }

    public string OimoPowerDescription { get; }

    public OimoSpeciesDefinition(string id, string name, double oimoPowerCost, OimoPowerEffectType oimoPowerEffectType, double oimoPowerValue, string oimoPowerDescription)
    {
        Id = id;
        Name = name;
        OimoPowerCost = oimoPowerCost;
        OimoPowerEffectType = oimoPowerEffectType;
        OimoPowerValue = oimoPowerValue;
        OimoPowerDescription = oimoPowerDescription;
    }
}