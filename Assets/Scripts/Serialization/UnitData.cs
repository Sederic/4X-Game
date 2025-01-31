using System;

[Serializable]
public class UnitData {
    public int unitID;
    public int civID;
    public int tileX, tileY;
    public string unitType; // Stores the unit’s type (Settler, Warrior, etc.)
    public UnitClass unitClass; // Determines the subclass (Military, Civilian, etc.)
    public int currMP;
    public int experience;
    public bool exhausted;

    // Additional Fields for Military Units
    public int? health;
    public int? combatStrength;
    public int? attackRange;

    public UnitData(Unit unit) {
        unitID = unit.GetHashCode();
        civID = unit.civ.ID;
        tileX = unit.tile.x;
        tileY = unit.tile.y;
        unitType = unit.unitType.ToString();
        unitClass = unit.unitClass;
        currMP = unit.CurrMP;
        experience = unit.Experience;
        exhausted = unit.Exhausted;

        // Military-Specific Fields
        if (unit is MilitaryUnit military) {
            health = military.Health;
            combatStrength = military.CombatStrength;
            attackRange = military.AttackRange;
        }
    }
}
