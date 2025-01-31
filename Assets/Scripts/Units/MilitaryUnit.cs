
using UnityEngine;

public class MilitaryUnit : Unit
{
    public int Health { get; protected set; } = 100; // A Unit's current Health Points (Default of 100)
    public int CombatStrength { get; protected set; } // Base Combat Strength Stat
    public int AttackRange { get; protected set; } = 0; // The range of Tiles a Unit can attack from (ranged).

    public MilitaryUnit(GameTile tile, Civilization civ)
        : base(tile, civ)
    {
        unitClass = UnitClass.Military;
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0) {
            Debug.Log("Unit died");
        }
    }

    public void Attack(MilitaryUnit unit)
    {
        // TBD
    }
}
