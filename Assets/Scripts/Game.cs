using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TextCore.Text;

[System.Serializable]
public class Game : ISerialization
{
    [JsonProperty] public World world;
    [JsonProperty] public List<Civilization> civilizations;
    [JsonProperty] public int gameTurn;
    [JsonProperty] public bool singlePlayer;

    /* Constructor */
    public Game()
    {
        gameTurn = 0;
    }

    public void StageForSerialization()
    {
        // World
        world.StageForSerialization();
        
        // Civilizations

        if (civilizations != null)
        {
            foreach (Civilization civilization in civilizations)
            {
                civilization.StageForSerialization();
            }
        }
    }

    public void RestoreAfterDeserialization(Game game)
    {
        world.RestoreAfterDeserialization(game);

        if (civilizations is not null)
        {
            foreach (Civilization civilization in civilizations)
            {
                civilization.RestoreAfterDeserialization(game);
            }
        }
    }
}
