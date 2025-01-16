using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class Game : ISerialization
{
    public World world;
    public List<Civilization> civilizations;
    public int gameTurn;
    public bool singlePlayer;

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

    public void RestoreAfterDeserialization(GameManager gameManager)
    {
        world.RestoreAfterDeserialization(gameManager);

        if (civilizations is not null)
        {
            foreach (Civilization civilization in civilizations)
            {
                civilization.RestoreAfterDeserialization(gameManager);
            }
        }
    }
}
