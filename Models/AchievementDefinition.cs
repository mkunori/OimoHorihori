namespace OimoHorihori.Models;

public class AchievementDefinition
{
    public string Id { get; }

    public string Name { get; }

    public string Description { get; }

    public Func<GameState, bool> Condition { get; }

    public AchievementDefinition(
        string id,
        string name,
        string description,
        Func<GameState, bool> condition)
    {
        Id = id;
        Name = name;
        Description = description;
        Condition = condition;
    }
}