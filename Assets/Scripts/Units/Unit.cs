using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Unit : ISerialization
{
    // Instance Properties
    [JsonProperty] public string _name; // The Unit's name.
    [JsonProperty] public int _health; // A Unit's current Health Points (Default of 100)
    [JsonProperty] public int _baseMP; // A Unit's current Movement Points per turn.
    [JsonProperty] public int _currMP; // A Unit's remaining movement points this turn.
    [JsonProperty] public int _combatStrength; // A Unit's base Combat Strength Stat - used to determine Attack Damage and Defense Damage.
    [JsonProperty] public int _supplies; // A Unit's current Supplies stat - Determines how many turns it can stay out of your territory before taking damage.
    [JsonProperty] public int _attackRange; // The range of Tiles a Unit can attack from. (Melee: 0, Ranged: 1 - X).
    [JsonProperty] public int _experience; // A Unit's current XP. Needs X amount for a Promotion.
    [JsonProperty] public bool _hasOrder; // Determines whether a Unit has already been given an order for this turn.
    [JsonProperty] public bool _exhausted; // Determines if a Unit still has moves to make this turn.
    [JsonProperty] public bool _camping; // Determines if a Unit was ordered to Fortify this turn.
    [JsonProperty] public bool _passing;
    [JsonProperty] public List<Promotion> _promotions; // Unlocked Promotions
    [JsonProperty] public Point _position;
    
    // Private Variables
    public List<Point> possible_moves;
    
    // References
    [JsonIgnore]
    public GameTile _gameTile; //The Tile this Unit is on.
    [JsonProperty("GameTileUID")]
    private int? _gameTileUID;
    [JsonIgnore]
    public Civilization _civilization; // The Civilization that owns this Unit.

    // Constants
    private const int Zero = 0;
    private const int TotalPromotions = 5;
    private const int UnitMaxHealth = 100;
    private const int UnitMaxSupply = 8;
    private const int HillAttackDebuf = 10;
    private const int RiverAttackDebuf = 10;

    public Unit(GameTile tile, Civilization civilization)
    {
        _gameTile = tile;
        _civilization = civilization;
    }

    public void OnTurnEnd()
    {
        if (_camping)
        {
            _health += 10;

            if (_health > UnitMaxHealth)
            {
                _health = UnitMaxHealth;
            }
            
            _supplies += _currMP;

            if (_supplies > UnitMaxSupply)
            {
                _supplies = UnitMaxSupply;
            }
            
        } else if (!IsInFriendlyTerritory())
        {
            if (_supplies > 0)
            {
                _supplies -= 1;
            }
            else 
            {
                Damage(20);
            }
        }
        
        _exhausted = false;
        _hasOrder = false;
        _passing = false;
        _currMP = _baseMP;
    }

    bool IsInFriendlyTerritory()
    {
        foreach (Settlement settlement in _civilization._settlements)
        {
            if (settlement._territory.Contains(_gameTile))
            {
                return true;
            }
        }
        return false;
    }

    public void UpdateUnit()
    {
        if (_currMP <= 0)
        {
            _exhausted = true;
            _hasOrder = true;
        }
    }
    
    /* Move a Unit across Tiles */
    public void Move(GameTile target)
    {
        // To be implemented
        
    }


    /* Attack a Settlement */
    public void Attack(Settlement target)
    {
        // To be implemented
    }

    public void Damage(int damage)
    {
        _health -= damage;

        if (_health <= 0)
        {
            Debug.Log("Unit died");
        }
    }

    public void GetPossibleMoves(GameTile currTile, int movementPoints, bool isInitial)
    {
        if (isInitial)
        {
            possible_moves = new List<Point>();
        }

        if (movementPoints >= 0)
        {
            possible_moves.Add(new Point(currTile.GetXPos(), currTile.GetYPos()));
        }
        if (movementPoints < 0)
        {
            return;
        }

        foreach (GameTile neighbor in currTile.GetNeighbors())
        {
            if (neighbor.IsWalkable())
            {
                GetPossibleMoves(neighbor, movementPoints - neighbor.GetMovementCost(), false);
            }
        }
    }

    public int CalculateMovementCost(GameTile target)
    {
        return 0;
    }

    public bool canMove()
    {
        if (_currMP <= 0)
        {
            return false;
        }
        
        return true;
    }
    
    public void StageForSerialization()
    {
        StageCurrentTile();

        // Turn the current Tile into a Point (location)
        void StageCurrentTile()
        {
            _position = new Point(_gameTile.GetXPos(), _gameTile.GetYPos());
            // _gameTile = null;
        }
        
        // Set its owner to null (this will be restored by the Civilization)
        // _civilization = null;

        _gameTileUID = (_gameTile is null) ? null : _gameTile.UID;
    }

    public void RestoreAfterDeserialization(GameManager gameManager)
    {
        _gameTile = (_gameTileUID is null) ? null : GameTile.GetTileByUID((int)_gameTileUID);
        _gameTile.SetUnit(this);

        // RestoreCurrentTile();

        // // Restore this Unit, and it's Tile's references to each other.
        // void RestoreCurrentTile()
        // {
        //     _gameTile = game.world.GetTile(_position);
        //     _gameTile.SetUnit(this);
        // }
    }
}
