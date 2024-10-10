using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settlement : MonoBehaviour
{
    
    // Instance Variables
    private string _name; // The name the player set for the Settlement.
    private Civilization _civilization; // Owner
    private List<GameTile> _territory; // Tiles a Settlement controls.
    private List<GameTile> _workedTiles; // Tiles in Territory that are being worked by a Population
    private int[] _yieldsPt; // [Food, Production, Gold, Culture, Science] -> [0,1,2,3,4] YieldsPT -> Yields Per Turn
    private int _population; // The size of Settlement and the units you can assign to Tiles
    private int _foodSurplus; // The buildup of extra food needed to grow a Settlement.
    private int _combatStrength;
    private List<Building> _buildings;
    private List<CityProject> _projects;
    private CityProject _currentCityProject;
    private GameTile _gameTile;
    private int _Tier; // Settlement tier. 0 = Village, 1 = Town, 2 = City
    
    // Constants
    private const int FoodSurplusRequirement = 15;
    private const int Food = 0;
    private const int Production = 1;
    private const int Gold = 2;
    private const int Culture = 3;
    private const int Science = 4;

    private void Start()
    {
        // Listen to GameManager OnTurnEnd event
        GameManager.Instance.OnTurnEnd += Instance_OnTurnEnd;
    }

    // End the turn
    private void Instance_OnTurnEnd(object sender, System.EventArgs e)
    {
        // Update Settlement Yields
        CalculateYields();

        // Update Settlement Tier
        CalculateTier();

        // Add current Production per turn to the City Project's progress
        _currentCityProject.AddToProgress(_yieldsPt[Production]);

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
        _Tier = 0;
        CalculateYields();
    }

    /* Adds all adjacent Tiles to territory */
    private List<GameTile> StartingTerritory(GameTile gameTile)
    {
        List<GameTile> territory = new List<GameTile>();
        territory.Add(gameTile);

        foreach (GameTile t in gameTile.GetNeighbors())
        {
            territory.Add(t);
        }
        return territory;
    }

    /* Calculate a Settlement's yields per turn by summing  */
    public void CalculateYields()
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
    private void CalculateTier()
    {
        if (_population <= 3)
        {
            _Tier = 0;
        }
        else if (_population <= 7)
        {
            _Tier = 1;
        }
        else
        {
            _Tier = 2;
        }
    }

    private void CalculateFoodSurplus()
    {
        
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
    
    public GameTile GetGameTile()
    {
        return _gameTile;
    }
    
    public int GetTier()
    {
        return _Tier;
    }

}
    
    
