using System;
using System.Collections.Generic;

[Serializable]
public class GameTileData {
    public int x, y;
    public Biome biome;
    public Terrain terrain;
    public Feature feature;
    public Resource resource;
    public TileImprovement tileImprovement;
    public bool[] riverEdges;
    public bool hasFreshWater;
    public int? unitID; // Store the Unit ID if a unit is present
    // public int? settlementCivID; // Store the Civ ID if there is a settlement

    public GameTileData() {}
    public GameTileData(GameTile tile) {
        x = tile.x;
        y = tile.y;
        biome = tile.biome;
        terrain = tile.terrain;
        feature = tile.feature;
        resource = tile.resource;
        tileImprovement = tile.tileImprovement;
        riverEdges = tile.riverEdges;
        hasFreshWater = tile.hasFreshWater;

        unitID = tile.unit != null ? tile.unit.GetHashCode() : (int?)null;
        // settlementCivID = tile.settlement != null ? tile.settlement.Civ.ID : (int?)null;
    }
}
