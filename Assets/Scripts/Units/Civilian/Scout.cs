
using UnityEngine;

public class Scout : CivilianUnit
{
    
    public Scout(GameTile tile, Civilization civ)
        : base(tile, civ)
    {
        unitType = UnitType.Scout;
        Name = unitType.ToString();
        
        Vision = 3;
        BaseMP = 3;
        CurrMP = BaseMP;
    }
}

