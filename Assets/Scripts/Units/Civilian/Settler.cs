
using UnityEngine;

public class Settler : CivilianUnit
{
    
    public Settler(GameTile tile, Civilization civ)
        : base(tile, civ)
    {
        unitType = UnitType.Settler;
        Name = unitType.ToString();

        BaseMP = 2;
    }
}

