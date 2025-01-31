using System;
using System.Collections.Generic;

public class World {
    public int length { get; private set; }
    public int height { get; private set; }
    public GameTile[][] tiles { get; private set; }
    public List<Point> SpawnPoints { get; private set; }

    public World(int seed, int civCount) {
        length = 100;
        height = 50;
        tiles = (new WorldGenerator()).GenerateWorld(length, height, 2, (uint)seed);
        SpawnPoints = WorldGenerator.DetermineSpawnPoints(tiles, civCount);
    }

    public World(WorldData worldData, GameStateData gameState) {
        length = worldData.length;
        height = worldData.height;
        SpawnPoints = new List<Point>(worldData.spawnPoints);

        // Convert tile list back into a 2D array
        tiles = new GameTile[height][];
        for (int x=0; x<height; x++) {
            tiles[x] = new GameTile[length];
            for (int y=0; y<length; y++) {
                tiles[x][y] = new GameTile(gameState.tiles[x * length + y], gameState);
            }
        }
    }
}
