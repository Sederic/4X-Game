using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Game {
    private static Game _instance;
    public static Game Instance => _instance ??= new Game();
    public GameUI UI;

    public World world { get; private set; }
    public GameTile[][] tiles { get { return world.tiles; } }
    public List<Civilization> civs { get; private set; }
    public int gameTurn { get; private set; } = 0;
    public bool isSinglePlayer { get; private set; }
    public Civilization civ { get; private set; }

    private Game() {}
    
    public async Task Initialize (bool isSinglePlayer, int seed, List<Civilization> civs) {
        this.isSinglePlayer = isSinglePlayer;
        this.civs = civs;
        
        world = new World(seed, civs.Count);
        Turn0();

        // Game UI object
        UI = UnityEngine.Object.FindObjectOfType<GameUI>();
        if (UI == null)
        {
            await LoadGameSceneAsync();
        }

        // Set Civ
        SetCiv(civs[0]);

        // Render game
        UI.SetUpWorldCanvas();
        UI.RenderGame();
    }

    private async Task LoadGameSceneAsync()
    {
        var sceneLoad = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
        while (!sceneLoad.isDone) { await Task.Yield(); }
        UI = UnityEngine.Object.FindObjectOfType<GameUI>();
        SceneManager.UnloadSceneAsync("Main Menu"); // Unload MainMenu
    }

    private void Turn0() {
        for (int i=0; i<civs.Count; i++) {
            int tileX = world.SpawnPoints[i].x;
            int tileY = world.SpawnPoints[i].y;
            GameTile settleTile = GameTile.AllTiles[tileX][tileY];
            SpawnUnit(settleTile, civs[i], UnitType.Settler);

            List<GameTile> possibleWarriorTiles = new List<GameTile>();
            foreach (GameTile? tile in settleTile.GetNeighborsArray())
            {
                if (tile is not null && tile.IsWalkable())
                {
                    possibleWarriorTiles.Add(tile);
                }
            }
            System.Random random = new System.Random();
            GameTile warriorTile = possibleWarriorTiles[random.Next(0, possibleWarriorTiles.Count)];
            SpawnUnit(warriorTile, civs[i], UnitType.Warrior);
        }
    }


    /* -----------------------------------------------

    ----------------------------------------------- */
    public void SetCiv(Civilization civ) { this.civ = civ; }

    /* -----------------------------------------------

    ----------------------------------------------- */
    public bool SpawnUnit(GameTile tile, Civilization civ, UnitType unitType)
    {
        return UnitFactory.TryCreateUnit(unitType, tile, civ, out Unit newUnit);
    }
    public bool SpawnUnit(int tileX, int tileY, int civID, string unitName)
    {
        return Enum.TryParse(unitName, out UnitType unitType)
            && SpawnUnit(GameTile.AllTiles[tileX][tileY], Civilization.AllCivs[civID], unitType);
    }

}