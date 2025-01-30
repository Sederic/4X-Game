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
    public List<Civilization> civs { get; private set; }
    public int gameTurn { get; private set; } = 0;
    public bool isSinglePlayer { get; private set; }

    private Game() {}
    
    public async Task Initialize (bool isSinglePlayer, int seed, List<Civilization> civs) {
        this.isSinglePlayer = isSinglePlayer;
        this.civs = civs;
        
        world = new World(seed, civs.Count);
        Debug.Log("done");

        // Game UI object
        UI = UnityEngine.Object.FindObjectOfType<GameUI>();
        if (UI == null)
        {
            await LoadGameSceneAsync();
        }

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
            // civs[i]; world.spawnPoints[i];
        }
    }


    /* -----------------------------------------------

    ----------------------------------------------- */

}