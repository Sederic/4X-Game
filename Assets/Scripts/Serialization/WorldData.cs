using System;
using System.Collections.Generic;

[Serializable]
public class WorldData {
    public int length;
    public int height;
    public List<Point> spawnPoints;

    public WorldData() {}
    public WorldData(World world) {
        length = world.length;
        height = world.height;
        spawnPoints = new List<Point>(world.SpawnPoints);
    }
}
