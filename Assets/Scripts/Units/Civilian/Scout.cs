
using UnityEngine;

public class Scout : CivilianUnit
{
    
    public Scout(GameTile tile, Civilization civ)
        : base(tile, civ)
    {
        unitType = UnitType.Scout;
        Name = unitType.ToString();
        
        BaseMP = 3;
    }
}

