using System;

[Serializable]
public class UnitData {
    public int unitID;
    public int civID;
    public int tileX, tileY;
    public UnitClass unitClass; // Determines the subclass (Military, Civilian, etc.)
    public string unitType; // Stores the unit’s type (Settler, Warrior, etc.)

    public int currMP;
    public int experience;
    public bool hasOrder;
    public bool exhausted;
    public bool camping;
    public bool passing;

    // Additional Fields for Military Units
    public int? health;
    public int? combatStrength;
    public int? attackRange;

    public UnitData() {}
    public UnitData(Unit unit) {
        unitID = unit.ID;
        civID = unit.civ.ID;
        tileX = unit.tile.x;
        tileY = unit.tile.y;
        unitClass = unit.unitClass;
        unitType = unit.unitType.ToString();

        currMP = unit.CurrMP;
        experience = unit.XP;
        hasOrder = unit.HasOrder;
        exhausted = unit.Exhausted;
        camping = unit.Camping;
        passing = unit.Passing;

        // Military-Specific Fields
        if (unit is MilitaryUnit military) {
            health = military.Health;
            combatStrength = military.CombatStrength;
            attackRange = military.AttackRange;
        }
    }
}
