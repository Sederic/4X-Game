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
    public int ID { get; protected set; }

    public string Name { get; protected set; } // The Unit's name.
    public int Vision { get; protected set; } = 2; // Vision # of tiles
    public int BaseMP { get; protected set; } // A Unit's current Movement Points per turn.
    public int CurrMP { get; protected set; } // A Unit's remaining movement points this turn.
    public int Supplies { get; protected set; } // Determines how many turns it can stay out of your territory.

    public int XP { get; protected set; } = 0; // A Unit's current XP. Needs X amount for a Promotion.
    
    public bool HasOrder { get; protected set; } = false; // Determines whether a Unit has already been given an order.
    public bool Exhausted { get; protected set; } = false; // Determines if a Unit still has moves left this turn.
    public bool Camping { get; protected set; } = false; // Determines if a Unit was ordered to Fortify.
    public bool Passing { get; protected set; } = false; // Determines if a Unit is passing.
    
    // public List<Promotion> Promotions { get; protected set; } // Unlocked Promotions

    public UnitPrefab unitPrefab { get; protected set; }

    public UnitClass unitClass { get; protected set; }
    public UnitType unitType { get; protected set; }
    public GameTile tile { get; protected set; }
    public Civilization civ { get; protected set; }

    public Unit(GameTile tile, Civilization civ)
    {
        ID = GetHashCode();
        this.tile = tile;
        this.civ = civ;
        tile.SetUnit(this);
        civ.AddUnit(this);

        Debug.Log("Unit: " + civ.Name + ": " + tile.x.ToString() + ", " + tile.y.ToString());
    }

    public void Reinitialize(UnitData unit, GameStateData gameState)
    {
        ID = unit.unitID;
        
        CurrMP = unit.currMP;
        XP = unit.experience;
        HasOrder = unit.hasOrder;
        Exhausted = unit.exhausted;
        Camping = unit.camping;
        Passing = unit.passing;
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
        XP += xp;
    }

    /* ------------------------------------------------
        UNIT MECHANICS
    ------------------------------------------------ */
    public int VisionBlockingCount()
    {
        if (tile.terrain == Terrain.Mountain) return 100;
        return ((tile.terrain == Terrain.Hill) ? 1 : 0);
    }

    private List<(int, GameTile)>? _PossibleMovement;
    public List<(int, GameTile)> PossibleMovement { get
    {
        if (_PossibleMovement != null) return _PossibleMovement;
        List<(int, GameTile)> possibleMoves = new List<(int, GameTile)>();

        if (CurrMP == 0) return possibleMoves;

        bool[] visited = new bool[Game.Instance.tiles.Length * Game.Instance.tiles[0].Length];
        List<(int, GameTile)> frontier = new List<(int, GameTile)>();
        frontier.Add((CurrMP, tile));

        while (frontier.Count > 0) {
            List<(int, GameTile)> newFrontier = new List<(int, GameTile)>();
            foreach ((int mp, GameTile tile_) in frontier) {
                GameTile?[] neighbors = tile_.GetNeighborsArray();
                for (int i=0; i<6; i++) {
                    GameTile? t = neighbors[i];
                    if (t != null) {
                        if (visited[t.x * Game.Instance.tiles[0].Length + t.y]) continue;
                        else visited[t.x * Game.Instance.tiles[0].Length + t.y] = true;

                        int newMP = mp;

                        // MC = inf
                        if (!t.IsWalkable()) continue;

                        // units are impassable rn
                        else if (t.unit != null) continue;

                        // MC = 3
                        else if (
                            t.terrain == Terrain.Hill && (
                                t.feature == Feature.Woods ||
                                t.feature == Feature.Rainforest
                            )
                        ) newMP -= 3;

                        // MC = 2
                        else if (
                            t.terrain == Terrain.Hill ||
                            t.feature == Feature.Woods ||
                            t.feature == Feature.Rainforest ||
                            t.feature == Feature.Marsh
                        ) newMP -= 2;

                        // MC = 1
                        else newMP -= 1;

                        // aditional
                        // MC +3 - river
                        if (t.riverEdges[i]) newMP -= 3;

                        possibleMoves.Add((CurrMP - newMP, t));
                        if (newMP > 0) {
                            newFrontier.Add((newMP, t));
                        }
                    }
                }
            }
            frontier = newFrontier;
        }

        return possibleMoves;
    } }
    public void ResetPossibleMovement() { _PossibleMovement = null; }

    public void MoveTo(GameTile gameTile)
    {
        // Does not validate if it can be reached
        int mpRequired = -1;
        foreach ((int mp, GameTile t) in PossibleMovement) {
            if (gameTile == t) {
                mpRequired = mp;
            }
        }
        if (mpRequired == -1) return;

        tile.UnsetUnit();
        tile = gameTile;
        tile.SetUnit(this);
        CurrMP -= mpRequired;
        /* TBD: XP gain?? */ XP += mpRequired;

        // Reset cache
        ResetPossibleMovement();
    }
}
