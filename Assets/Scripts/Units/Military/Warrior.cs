
using UnityEngine;

public class Warrior : MilitaryUnit
{
    
    public Warrior(GameTile tile, Civilization civ)
        : base(tile, civ)
    {
        unitType = UnitType.Warrior;
        Name = unitType.ToString();
        
        Health = 20;
        BaseMP = 2;
        CombatStrength = 20;
    }
}
