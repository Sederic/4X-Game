using System;
using System.Collections.Generic;

public class UnitFactory
{
    private static readonly Dictionary<UnitType, Func<GameTile, Civilization, Unit>> unitCreators = new()
    {
        // Civilian Units
        { UnitType.Settler, (tile, civ) => new Settler(tile, civ) },
        { UnitType.Scout, (tile, civ) => new Scout(tile, civ) },

        // Military Units
        { UnitType.Warrior, (tile, civ) => new Warrior(tile, civ) },
    };

    public static bool TryCreateUnit(UnitType unitType, GameTile tile, Civilization civ, out Unit newUnit)
    {
        if (tile.unit is null && unitCreators.TryGetValue(unitType, out var creator))
        {
            newUnit = creator(tile, civ);
            return true;
        }
        
        newUnit = null;
        return false;
    }

    public static void ReinitializeUnit(UnitData unit, GameStateData gameState)
    {
        Enum.TryParse<UnitType>(unit.unitType, out UnitType parsedUnitType);
        if (!unitCreators.TryGetValue(parsedUnitType, out var creator)) { return; }

        GameTile tile = gameState._tiles[unit.tileX][unit.tileY];
        Civilization civ = gameState._civsDict[unit.civID];
        Unit newUnit = creator(tile, civ);
        newUnit.Reinitialize(unit, gameState);
    }
}
