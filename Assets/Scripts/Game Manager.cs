using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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

       if (game.civilizations != null)
       {
           foreach (Civilization civilization in game.civilizations)
           {
               civilization.OnTurnEnded();
           }
       }
   }
    
    public void NewDemoGame(uint worldSeed)
    {
        game = new Game();
        game.singlePlayer = true;
        Civilization player = new Civilization();
        player.IsNPC = false;
        game.civilizations = new List<Civilization>();
        game.civilizations.Add(player);
        game.world = new WorldGenerator().GenerateWorld(100, 50, 2, worldSeed, game.civilizations.Count);
        SceneManager.LoadScene(1);
    }
    
    public void SaveGame(string filename) 
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
    }

    public void LoadGame(string filePath)
    {
        string path = Path.Combine(Application.persistentDataPath, filePath);

        if (File.Exists(path))
        {
            game = LoadData<Game>(path, false);
            game.RestoreAfterDeserialization(game);
            SceneManager.LoadScene(1);
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
}
