using System;
using System.Collections.Generic;
using City_Projects;
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
    public List<Building> _buildings;
    [JsonProperty]
    private List<CityProject> _projects;
    [JsonIgnore]
    public CityProject _currentCityProject;
    [JsonProperty("CityProjectName")]
    private string? _currentCityProjectName;
    [JsonProperty]
    private int _tier; // Settlement tier. 1 = Village, 2 = Town, 3 = City
    [JsonProperty]
    public Point[] _territoryPoints;
    [JsonProperty]
    public Point[] _workedTilesPoints;
    [JsonProperty] public Point[] _lockedTilesPoints;
    [JsonProperty] public Point _settlementPoint;
    
    // Circular Instance References
    [JsonIgnore]
    public Civilization _civilization; // Owner
    [JsonIgnore]
    public GameTile _gameTile;
    [JsonIgnore]
    public List<GameTile> _territory; // Tiles a Settlement controls.
    [JsonIgnore]
    public List<GameTile> _workedTiles; // Tiles in Territory that are being worked by a Population.
    [JsonIgnore]
    public List<GameTile> _lockedTiles; // Tiles locked for specific use.

    [JsonProperty("GameTileUIDs")]
    private int _gameTileUID;
    [JsonProperty("TerritoryUIDs")]
    private List<int> _territoryUIDs;
    [JsonProperty("WorkedTileUIDs")]
    private List<int> _workedTileUIDs;
    [JsonProperty("LockedTileUIDs")]
    private List<int> _lockedTileUIDs;
    
    // Private properties
    private GameManager _gm;
    
    // Constants
    private const int FoodSurplusRequirement = 15;
    private const int Food = 0;
    private const int Production = 1;
    private const int Gold = 2;
    private const int Culture = 3;
    private const int Science = 4;

    /* New Settlement Constructor - for Gameplay */
    [JsonConstructor]
    public Settlement()
    {
        _yieldsPt = new int[5];
        _population = 1;
        _foodSurplus = 0;
        _combatStrength = 10;
        _buildings = new List<Building>();
        _projects = new List<CityProject>();
        _currentCityProject = null;
        _territory = new List<GameTile>();
        _workedTiles = new List<GameTile>();
        _lockedTiles = new List<GameTile>();
        _territoryUIDs = new List<int>();
        _workedTileUIDs = new List<int>();
        _lockedTileUIDs = new List<int>();
        _tier = 1;
    }

    public Settlement(string name, Civilization civilization, GameTile gameTile)
    {
        _name = name;
        _yieldsPt = new int[5];
        _population = 1;
        _foodSurplus = 0;
        _combatStrength = 10;
        _buildings = new List<Building>();
        _projects = GetBaseProjects();
        _currentCityProject = null;
        _gameTile = gameTile;
        _civilization = civilization;
        _territory = StartingTerritory(gameTile);
        _workedTiles = new List<GameTile>();
        _lockedTiles = new List<GameTile>();
        _lockedTiles.Add(gameTile);
        AutoAssignWorkedTiles();
        UpdateYields();
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
            if (_currentCityProject is not null)
            {
                // Add current Production per turn to the City Project's progress
                _currentCityProject.AddToProgress(_yieldsPt[Production]);
            }
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
                
                // Auto Assign new Population
                AutoAssignWorkedTiles();
            }
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
                _yieldsPt[yield] += b.yields[yield];
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
    public void AddProject(string name, int cost)
    {
        
    }

    /* Switch the project the settlement is working on */
    public void SwitchProject(int index)
    {
        _currentCityProject = _projects[index];
    }

    /* Gives every Settlement the base projects. */
    private List<CityProject> GetBaseProjects()
    {
        List<CityProject> baseProjects = new List<CityProject>();
        
        baseProjects.Add(new SettlerProject());
        baseProjects.Add(new ScoutProject());
        baseProjects.Add(new WarriorProject());
        baseProjects.Add(new MonumentProject());
        
        return baseProjects;
    }

    /* Auto Assigns Worked Tiles that haven't been locked in by player */
    public void AutoAssignWorkedTiles()
    {
        // Reinitialize _workedTiles
        _workedTiles = new List<GameTile>();
        
        // If there are exactly the same amount of locked tiles as there can possibly be worked
        if (_lockedTiles.Count == _population + 1)
        {
            // Assign them, and return.
            _workedTiles = _lockedTiles;
            return;
        }
        
        // Sort tiles in territory according to best yield value
        List<GameTile> bestTilesInTerritory = _territory;
        
        // Best file in the front
        bestTilesInTerritory.Sort((tile1,tile2) => tile2.TileValue().CompareTo(tile1.TileValue()));
        
        // If there are more locked tiles than the Settlement can work, add the best Locked Tiles to Worked Tiles
        if (_lockedTiles.Count > _population + 1)
        {
            // Sort locked tiles by highest value
            _lockedTiles.Sort((tile1,tile2) => tile2.TileValue().CompareTo(tile1.TileValue()));

            foreach (GameTile t in _lockedTiles)
            {
                // If we are at max capacity
                if (_workedTiles.Count >= _population + 1)
                {
                    Debug.Log("Assigned all locked tiles until full.");
                    return;
                }
                
                // If this tile isn't already being worked
                if (!_workedTiles.Contains(t))
                {
                    // Add it
                    _workedTiles.Add(t);
                }
            } 
        }
        
        // If there are less locked tiles than possible number of worked tiles, Assign them to worked Tiles.
        if (_lockedTiles.Count < _population + 1)
        {
            _workedTiles = _lockedTiles;
        }
        
        // Fill any remaining tiles (population) with the best valued tiles
        foreach (GameTile territoryTile in bestTilesInTerritory)
        {
            // If we are at max capacity
            if (_population + 1 <= _workedTiles.Count)
            {
                return;
            }
            
            if (!_workedTiles.Contains(territoryTile))
            {
                _workedTiles.Add(territoryTile);
            }
        }
        
        UpdateYields();
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
        _gameTileUID = _gameTile.UID;
        _territoryUIDs = ConvertTilesToUIDs(_territory);
        _workedTileUIDs = ConvertTilesToUIDs(_workedTiles);
        _lockedTileUIDs = ConvertTilesToUIDs(_lockedTiles);

        List<int> ConvertTilesToUIDs(List<GameTile> tiles)
        {
            var uids = new List<int>();
            foreach (var tile in tiles)
            {
                if (tile != null)
                {
                    uids.Add(tile.UID);
                }
            }
            return uids;
        }

        _currentCityProjectName = _currentCityProject.projectName;
    }

    public void RestoreAfterDeserialization(GameManager gameManager)
    {
        _gameTile = GameTile.GetTileByUID(_gameTileUID);
        _territory = ConvertUIDsToTiles(_territoryUIDs);
        _workedTiles = ConvertUIDsToTiles(_workedTileUIDs);
        _lockedTiles = ConvertUIDsToTiles(_lockedTileUIDs);

        List<GameTile> ConvertUIDsToTiles(List<int> uids)
        {
            var tiles = new List<GameTile>();
            foreach (var uid in uids)
            {
                if (uid != null && GameTile.GetTileByUID(uid) != null)
                {
                    tiles.Add(GameTile.GetTileByUID(uid));
                }
            }
            return tiles;
        }

        // use _gameTileUID to set GameTile._settlement field
        GameTile.GetTileByUID(_gameTileUID).SetSettlement(this);

        foreach (var p in _projects) {
            p.settlement = this;
            p.gameManager = gameManager;
        }
        if (_currentCityProjectName == null) {
            _currentCityProjectName = null;
        } else {
            foreach (var p in _projects) {
                if (p.projectName == _currentCityProjectName) {
                    _currentCityProject = p; break;
                }
            }
        }
    }

    public int GetProduction()
    {
        return _yieldsPt[1];
    }

    public int GetFood()
    {
        return _yieldsPt[0];
    }

    // Returns a string (integer) to display the turns left to grow.
    public string TurnsToGrow()
    {
        int foodSurplusPerTurn = _yieldsPt[0] - (_population * 2);

        int remainingSurplusNeeded = 15 - _foodSurplus;

        if (foodSurplusPerTurn <= 0)
        {
            return "-";
        }

        return Math.Ceiling((float)remainingSurplusNeeded / (float) foodSurplusPerTurn).ToString();
    }
    
    // Returns a string (integer) to display the turns left to produce.
    public string TurnsToProduce()
    {
        if (_currentCityProject is not null)
        {
            float remainingCost = _currentCityProject.projectCost - _currentCityProject.currentProductionProgress;

            float productionPT = _yieldsPt[1];
            
            float turnToProduce = remainingCost / productionPT;

            if (remainingCost <= 0)
            {
                return "-";
            }
            
            return Math.Ceiling(turnToProduce).ToString();
        }
        

        return "-";
    }

    public void SetCityProject(CityProject project)
    {
        _currentCityProject = project;
    }

    /* Calls on GM to Spawn Unit but only called from City Projects. */
    public void UnitProjectComplete()
    {
        
    }

    public void BuildingProjectComplete()
    {
        
    }
    
}
    
    
