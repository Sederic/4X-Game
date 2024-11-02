using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class Settlement : ISerialization
{
    // Serializable Instance Variables
    [JsonProperty]
    private string _name; // The name the player set for the Settlement.
    [JsonProperty]
    private int[] _yieldsPt; // [Food, Production, Gold, Culture, Science] -> [0,1,2,3,4] YieldsPT -> Yields Per Turn
    [JsonProperty]
    private int _population; // The size of Settlement and the units you can assign to Tiles
    [JsonProperty]
    private int _foodSurplus; // The buildup of extra food needed to grow a Settlement.
    [JsonProperty]
    private int _combatStrength;
    [JsonProperty]
    private List<Building> _buildings;
    [JsonProperty]
    private List<CityProject> _projects;
    [JsonProperty]
    private CityProject _currentCityProject;
    [JsonProperty]
    private int _tier; // Settlement tier. 1 = Village, 2 = Town, 3 = City
    [JsonProperty]
    public Point[] _territoryPoints;
    [JsonProperty]
    public Point[] _workedTilesPoints;
    [JsonProperty]
    public Point _settlementPoint;
    
    // Circular Instance References
    public Civilization _civilization; // Owner
    public GameTile _gameTile;
    public List<GameTile> _territory; // Tiles a Settlement controls.
    public List<GameTile> _workedTiles; // Tiles in Territory that are being worked by a Population
    
    // Constants
    private const int FoodSurplusRequirement = 15;
    private const int Food = 0;
    private const int Production = 1;
    private const int Gold = 2;
    private const int Culture = 3;
    private const int Science = 4;

    /* New Settlement Constructor - for Gameplay */
    public Settlement(string name, Civilization civilization, GameTile gameTile)
    {
        _name = name;
        _gameTile = gameTile;
        _civilization = civilization;
        _territory = StartingTerritory(gameTile);
        _workedTiles = new List<GameTile>();
        _workedTiles.Add(gameTile);
        _projects = new List<CityProject>();
        _population = 1;
        _tier = 1;
        
        /* Adds all adjacent Tiles to territory */
        List<GameTile> StartingTerritory(GameTile gameTile)
        {
            List<GameTile> territory = new List<GameTile>();
            territory.Add(gameTile);

            foreach (GameTile t in gameTile.GetNeighbors())
            {
                territory.Add(t);
            }
            return territory;
        }
    }
    
    // End the turn
    public void OnTurnEnd()
    {
        // Update Settlement Production
        ProgressCityProject();
        
        // Update Settlement Growth
        ProgressFoodSurplus();
        
        // Update Settlement Yields
        UpdateYields();

        // Update Settlement Tier
        UpdateTier();
        
        void ProgressCityProject()
        {
            // Add current Production per turn to the City Project's progress
            _currentCityProject.AddToProgress(_yieldsPt[Production]);
        }

        void ProgressFoodSurplus()
        {
            // Updated Growth
            _foodSurplus += _yieldsPt[Food] - (_population * 2);
            
            // Check if Settlement will grow
            if (_foodSurplus >= FoodSurplusRequirement)
            {
                // If so, 50% of remaining food surplus is carried over.
                _foodSurplus = (_foodSurplus - FoodSurplusRequirement) / 2 ;
            
                // Increase population
                _population += 1;
            }
        }

        void UpdateCivilizationYields()
        {
            
        }
    }

    /* Called frequently to make sure Settlement updates yields in GUI */
    public void UpdateYields()
    {
        // Reset Yields
        _yieldsPt = new int[5];
        
        // Tiles
        for (int yield = 0; yield < _yieldsPt.Length; yield++)
        {
            foreach (GameTile t in _workedTiles)
            {
                _yieldsPt[yield] += t.GetYields()[yield];
            }
        }
        
        // Buildings
        for (int yield = 0; yield < _yieldsPt.Length; yield++)
        {
            foreach (Building b in _buildings)
            {
                _yieldsPt[yield] += b.GetYields()[yield];
            }
        }
    }

    /* Calculate a settlement's tier */
    private void UpdateTier()
    {
        if (_population <= 3)
        {
            _tier = 0;
        }
        else if (_population <= 7)
        {
            _tier = 1;
        }
        else
        {
            _tier = 2;
        }
    }

    /* Add a city project to settlement */
    public void AddProject(int name, int cost)
    {
        _projects.Add(new CityProject(name, cost));
    }

    /* Switch the project the settlement is working on */
    public void SwitchProject(int index)
    {
        _currentCityProject = _projects[index];
    }
    
    
    // Getter Methods
    public int[] GetYieldsPt()
    {
        return _yieldsPt;
    }

    public string GetName()
    {
        return _name;
    }

    public Civilization GetCivilization()
    {
        return _civilization;
    }

    public int GetPopulation()
    {
        return _population;
    }

    public List<GameTile> GetTerritory()
    {
        return _territory;
    }

    public List<GameTile> GetWorkedTiles()
    {
        return _workedTiles;
    }
    
    public int GetFoodSurplus()
    {
        return _foodSurplus;
    }
    
    public int GetCombatStrength()
    {
        return _combatStrength;
    }
    
    public List<Building> GetBuildings()
    {
        return _buildings;
    }
    
    public List<CityProject> GetProjects()
    {
        return _projects;
    }
    
    public CityProject GetCurrentCityProject()
    {
        return _currentCityProject;
    }
    
    public GameTile GetTile()
    {
        return _gameTile;
    }
    
    public int GetTier()
    {
        return _tier;
    }

    public void StageForSerialization()
    {
        // Civilization will restore this for each settlement.
        _civilization = null;
        
        StageSettlementTile();
        StageTerritoryTiles();
        StageWorkedTiles();
        
        // Store the Settlement's Tile
        void StageSettlementTile()
        {
            _settlementPoint = new Point(_gameTile.GetXPos(), _gameTile.GetYPos());
            _gameTile = null;
        }
        // Transfer all _territory Tiles into an array of Points for serialization
        void StageTerritoryTiles()
        {
            if (_territory is not null)
            {
                _territoryPoints = new Point[_territory.Count];
                int index = 0;
                foreach (GameTile t in _territory)
                {
                    _territoryPoints[index] = new Point(t.GetXPos(), t.GetYPos());
                    index++;
                }
                _territory = null;
            }    
        }
        // Transfer _workedTiles List into an array of Points for serialization
        void StageWorkedTiles()
        {
            if (_workedTiles is not null)
            {
                _workedTilesPoints = new Point[_workedTiles.Count];
                int index = 0;
                foreach (GameTile t in _workedTiles)
                {
                    _workedTilesPoints[index] = new Point(t.GetXPos(), t.GetYPos());
                    index++;
                }
                _workedTiles = null;
            }
        }
        
    }

    public void RestoreAfterDeserialization(Game game)
    {
        RestoreSettlementTile();
        RestoreTerritory();
        RestoreWorkedTiles();

        // Restore GameTile Reference to Settlement (Settlement Location)
        void RestoreSettlementTile()
        {
            _gameTile = game.world.GetTile(_settlementPoint);
            _gameTile.SetSettlement(this);
        }
        // Restore Territory Tile References to Settlement
        void RestoreTerritory()
        {
            foreach (Point point in _territoryPoints)
            {
                _territory.Add(game.world.GetTile(point));
            }
        }
        // Restore Worked Tile References to Settlement
        void RestoreWorkedTiles()
        {
            foreach (Point point in _workedTilesPoints)
            {
                _workedTiles.Add(game.world.GetTile(point));
            }
        }
    }
}
    
    
