using System;
using System.Collections.Generic;
using System.IO;
using City_Projects;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Units;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{ 
    [HideInInspector] 
    public Game game;

    private bool savedGame = false;
    
    public void EndTurn()
   {
       game.gameTurn++;
       Debug.Log("Turn: " + game.gameTurn);
       
       foreach (Civilization civilization in game.civilizations)
       {
           civilization.OnTurnEnded();
       }
   }
    
    public void NewDemoGame(uint worldSeed)
    {
        game = new Game();
        game.singlePlayer = true;
        Civilization player1 = new Civilization(new Color32(255, 181, 43, 255));
        player1.IsNPC = false;
        game.civilizations = new List<Civilization>();
        game.civilizations.Add(player1);
        game.world = new WorldGenerator().GenerateWorld(100, 50, 2, worldSeed, game.civilizations.Count);
        Test();
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
    
    // Places some settlements down for testing
    private void Test()
    {
        List<Point> spawnPoints = game.world.GetSpawnPoints();
        
        GameTile spawnPointTile = game.world.GetTile(spawnPoints[0]);

        SpawnUnit(new Settler(spawnPointTile, game.civilizations[0]));

        // Spawn a Warrior around the Spawn Point
        foreach (GameTile neighbor in spawnPointTile.GetNeighbors())
        {
            if (neighbor.IsWalkable())
            {
                if (neighbor.GetUnit() is null)
                {
                    SpawnUnit(new Warrior(neighbor, game.civilizations[0]));
                    break;
                }
            }
        }
    } 

    public void SaveGame(string filename) {
        game.StageForSerialization();
        string json = JsonConvert.SerializeObject(game, Formatting.Indented);
        string path = Path.Combine(Application.persistentDataPath, filename + ".json");
        Debug.Log("Save file path: " + path);
        File.WriteAllText(path, json);
    }

    public void LoadGame(string filenamePlusJson) {
        try {
            string path = Path.Combine(Application.persistentDataPath, filenamePlusJson);
            Debug.Log("Game loading from: " + path);
            string json = File.ReadAllText(path);
            game = JsonConvert.DeserializeObject<Game>(json);
            game.RestoreAfterDeserialization(this);
            Debug.Log("Game loaded from: " + path);

            SceneManager.LoadScene(1, LoadSceneMode.Single);
        } catch (Exception ex) {
            Debug.Log("An error occurred while loading the game: " + ex.Message);
            Debug.Log("Stack Trace: " + ex.StackTrace);
        }
    }
    
    public void SaveGame2(string filename) 
    {
        if (!savedGame)
        {
            game.StageForSerialization();
            if (filename == "")
            {
                filename = "saveFile";
            }
            string fileRelativePath = filename + ".json";
            SaveData(fileRelativePath, game, false);
            savedGame = true;
        }
        
        void SaveData<T>(string relativePath, T data, bool encrypted)
        {
            // File Path
            string path = Path.Combine(Application.persistentDataPath, relativePath);
            Debug.Log("Save file path: " + path);


            // If it exists, delete it (in order to replace it)
            if (File.Exists(path))
            {
                Debug.Log("Data exists. Deleting old file and writing a new one.");
                File.Delete(path);
            }

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.All,
                Converters = new List<JsonConverter>
                {
                    new GameTileArrayConverter()
                },
                ContractResolver = new DefaultContractResolver
                {
                    IgnoreSerializableAttribute = false
                }
            };
       
            //Serialize the data to JSON
            string jsonData = JsonConvert.SerializeObject(data, settings);
       
            // Encode string to bytes[]
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
       
            //Write File
            using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate))
            {
                fileStream.Write(jsonBytes, 0, jsonBytes.Length);
            }
        }
        Debug.Log("Game successfully saved as: " + filename);
    }

    public void LoadGame2(string filePath)
    {
        string path = Path.Combine(Application.persistentDataPath, filePath);

        if (File.Exists(path))
        {
            game = LoadData<Game>(path, false);
            game.RestoreAfterDeserialization(this);
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("File not found.");
        }
        
        T LoadData<T>(string relativePath, bool encrypted)
        {
            string path = Path.Combine(Application.persistentDataPath, relativePath);

            try
            {
                byte[] jsonBytes = File.ReadAllBytes(path);

                string jsonData = System.Text.Encoding.UTF8.GetString(jsonBytes);
                
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    TypeNameHandling = TypeNameHandling.All,
                    Converters = new List<JsonConverter>
                    {
                        new GameTileArrayConverter()
                    },
                    ContractResolver = new DefaultContractResolver
                    {
                        IgnoreSerializableAttribute = false
                    }
                };

                return JsonConvert.DeserializeObject<T>(jsonData, settings);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load data: " + e.Message);
                return default;
            }
        }
    }

    public void MoveUnit(Unit unit, Point target)
    {
        // Shortest path from Unit's position to target point
        List<GameTile> path = Pathfinder.FindShortestPath(unit._gameTile, game.world.GetTile(target));
        
        // Movement Cost of moving across that Path.
        int moveCost = Pathfinder.CalculatePathCost(path);
        
        // Subtract Movement Points from Unit
        unit._currMP -= moveCost;
        
        // Store Game Tile references
        GameTile previousTile = unit._gameTile;
        GameTile newTile = game.world.GetTile(target);
        
        // Update Tiles
        previousTile.SetUnit(null);
        newTile.SetUnit(unit);
        
        // Update Unit's own Tile
        unit._gameTile = newTile;
        
        // Restart get possible moves
        unit.GetPossibleMoves(unit._gameTile, unit._currMP, true);
    }

    public void SpawnUnit(Unit unit)
    {
        // Set it on the Tile
        GameTile tile = unit._gameTile;
        tile.SetUnit(unit);
        // Add into it's Civilization
        Civilization owner = unit._civilization;
        owner._units.Add(unit);
    }

    public void SpawnSettlement(Settlement settlement)
    {
        // Set it on the Tile
        GameTile tile = settlement._gameTile;
        tile.SetSettlement(settlement);
        
        // Add it to it's Civilization
        Civilization owner = settlement._civilization;
        owner.AddSettlement(settlement);

        // Give reference to the GM to the Settlement's Projects (so the projects can call Spawn UNit/Add Building, etc)
        foreach (CityProject project in settlement.GetProjects())
        {
            project.gameManager = this;
            project.settlement = settlement;
        }
        
        settlement.UpdateYields();
    }

    public void AddBuilding(Settlement settlement, Building building)
    {
        settlement._buildings.Add(building);
        
        settlement.UpdateYields();
    }

    public void DestroyUnit(Unit unit)
    {
        Civilization owner = unit._civilization;
        
        // Remove its tracking by owner Civilization
        owner._units.Remove(unit);
        
        // Remove it from the world map
        unit._gameTile.SetUnit(null);
        unit._gameTile = null;
        
        // Nullify it
        unit = null;
    }

    public void CampUnit(Unit unit)
    {
        unit._camping = !unit._camping;
    }

    public void PassUnit(Unit unit)
    {
        unit._passing = true;
    }

    public void SettleUnit(Unit unit)
    {
        String settlmentName = "Jersey";

        if (unit._civilization._settlements.Count > 0)
        {
            for (int i = 0; i < unit._civilization._settlements.Count; i++)
            {
                settlmentName = "New" + " " + settlmentName;
            }
        }
        
        Settlement settlement = new Settlement(settlmentName, unit._civilization, unit._gameTile);
        
        SpawnSettlement(settlement);
        
        DestroyUnit(unit);
    }

    public void AddDiscoveredTiles(Civilization civilization, Point tile)
    {
        civilization.discoveredTiles.Add(tile);
    }

}
