using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum UnitClass
{
    Civilian = 0,
    Military = 1,
}

public enum UnitType
{
    // Civilian
    Settler = 0,
    Scout = 1,

    // Military
    Warrior = 100,
}

public class Unit
{
    public string Name { get; protected set; } // The Unit's name.
    public int BaseMP { get; protected set; } // A Unit's current Movement Points per turn.
    public int CurrMP { get; protected set; } // A Unit's remaining movement points this turn.
    public int Supplies { get; protected set; } // Determines how many turns it can stay out of your territory.

    public int Experience { get; protected set; } = 0; // A Unit's current XP. Needs X amount for a Promotion.
    
    public bool HasOrder { get; protected set; } = false; // Determines whether a Unit has already been given an order.
    public bool Exhausted { get; protected set; } = false; // Determines if a Unit still has moves left this turn.
    public bool Camping { get; protected set; } = false; // Determines if a Unit was ordered to Fortify.
    public bool Passing { get; protected set; } = false; // Determines if a Unit is passing.
    
    // public List<Promotion> Promotions { get; protected set; } // Unlocked Promotions
    // public Point Position { get; protected set; } // Unit's position on the map.

    public UnitPrefab unitPrefab { get; protected set; }

    public UnitClass unitClass { get; protected set; }
    public UnitType unitType { get; protected set; }
    public GameTile tile { get; protected set; }
    public Civilization civ { get; protected set; }

    public Unit(GameTile tile, Civilization civ)
    {
        this.tile = tile;
        this.civ = civ;
        tile.SetUnit(this);
        civ.AddUnit(this);

        Debug.Log("Unit: " + civ.name + ": " + tile.x.ToString() + ", " + tile.y.ToString());
    }

    public void SetUnitPrefab(UnitPrefab unitPrefab, GameUI UI)
    {
        unitPrefab.SetUnitPrefab(this, UI);
        this.unitPrefab = unitPrefab;
    }

    public void DeleteUnit()
    {
        unitPrefab?.DestroyUnit();
    }

    public void GainExperience(int xp)
    {
        Experience += xp;
    }

    public void MoveTo(GameTile tile)
    {
        this.tile = tile;
    }
}
