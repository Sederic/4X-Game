using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class World {
    public int length { get; private set; } = 100;
    public int height { get; private set; } = 50;
    public GameTile[][] tiles { get; private set; }
    public List<Point> spawnPoints { get; private set; }= new List<Point>();
    
    public World (int seed, int civCount) {
        // 2 continents
        tiles = (new WorldGenerator()).GenerateWorld(length, height, 2, (uint)seed);
        spawnPoints = WorldGenerator.DetermineSpawnPoints(tiles, civCount);
    }
}