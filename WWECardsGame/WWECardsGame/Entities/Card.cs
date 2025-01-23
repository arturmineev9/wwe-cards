namespace WWECardsGame.Entities;

class Card
{
    public string Name { get; }
    public string Gender { get; }
    public int Strength { get; }
    public int Toughness { get; }
    public int Endurance { get; }
    public int Charisma { get; }

    public Card(string name, string gender, int strength, int toughness, int endurance, int charisma)
    {
        Name = name;
        Gender = gender;
        Strength = strength;
        Toughness = toughness;
        Endurance = endurance;
        Charisma = charisma;
    }
    
    public int GetAttribute(string attribute)
    {
        return attribute switch
        {
            "Strength" => Strength,
            "Toughness" => Toughness,
            "Endurance" => Endurance,
            "Charisma" => Charisma,
            _ => 0
        };
    }
    

    public override string ToString()
    {
        return $"{Name} ({Gender}) - Сила: {Strength}, Жесткость: {Toughness}, Выносливость: {Endurance}, Харизма: {Charisma}";
    }
    
    
    
}