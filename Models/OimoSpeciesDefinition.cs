namespace OimoHorihori.Models;

public class OimoSpeciesDefinition
{
    public string Id { get; }
    public string Name { get; }
    public OimoSpeciesDefinition(string id, string name)
    {
        Id = id;
        Name = name;
    }
}