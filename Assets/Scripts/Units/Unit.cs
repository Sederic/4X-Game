using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit
{
    // Instance Properties with public getters and private setters
    public string Name { get; private set; } // The Unit's name.
    public int Health { get; private set; } // A Unit's current Health Points (Default of 100)
    public int BaseMP { get; private set; } // A Unit's current Movement Points per turn.
    public int CurrMP { get; private set; } // A Unit's remaining movement points this turn.
    public int CombatStrength { get; private set; } // Base Combat Strength Stat
    public int Supplies { get; private set; } // Determines how many turns it can stay out of your territory.
    public int AttackRange { get; private set; } = 0; // The range of Tiles a Unit can attack from (ranged).
    public int Experience { get; private set; } = 0; // A Unit's current XP. Needs X amount for a Promotion.
    public bool HasOrder { get; private set; } = false; // Determines whether a Unit has already been given an order.
    public bool Exhausted { get; private set; } = false; // Determines if a Unit still has moves left this turn.
    public bool Camping { get; private set; } = false; // Determines if a Unit was ordered to Fortify.
    public bool Passing { get; private set; } = false; // Determines if a Unit is passing.
    // public List<Promotion> Promotions { get; private set; } // Unlocked Promotions
    public Point Position { get; private set; } // Unit's position on the map.

    public void TakeDamage(int damage)
    {
        Health = Health - damage;
        if (Health <= 0) {
            Debug.Log("Unit died");
        }
    }

    public void GainExperience(int xp)
    {
        Experience += xp;
    }

    public void MoveTo(Point newPosition)
    {
        Position = newPosition;
    }
}
