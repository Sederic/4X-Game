
using UnityEngine;

public class CivilianUnit : Unit
{
    public CivilianUnit(GameTile tile, Civilization civ)
        : base(tile, civ)
    {
        unitClass = UnitClass.Civilian;
    }
}