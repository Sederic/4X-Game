using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a GameTile in the game world, storing its biome, terrain, features, resources, and tileImprovements.
///
/// <para>**Methods in GameTile Class:**</para>
/// <list type="bullet">
/// <item><description>GameTile(int) - Initializes a tile with a given biome.</description></item>
/// <item><description>GetNeighborsArray(GameTile[][], int, int) - Gets up to 6 neighboring tiles in a hexagonal grid. Top, Top-Right, Bottom-Right, etc.</description></item>
/// <item><description>GetYields() - Calculates and returns the tile’s food and production yields.</description></item>
/// </list>
/// </summary>
public class GameTile
{
    public static GameTile[][] AllTiles { get; private set; }
    public int x { get; private set; }
    public int y { get; private set; }

    public Biome biome { get; private set; }
    public Terrain terrain { get; private set; }
    public Feature feature { get; private set; }
    public Resource resource { get; private set; }
    public TileImprovement tileImprovement { get; private set; }

    public bool[] riverEdges { get; private set; } = new bool[6]; // Are the GameTile edges Adjacent to a river? -> [0,1,2,3,4,5] Represent edges on a hexagon starting from the Top moving clockwise.
    public bool hasFreshWater { get; private set; } = false;

    public Settlement settlement { get; private set; }
    public Unit unit { get; private set; }

    /// <summary>
    /// Initializes a new GameTile with the specified Biome.
    /// </summary>
    public GameTile(int x, int y, Biome biome)
    {
        this.x = x;
        this.y = y;
        this.biome = biome;
        terrain = Terrain.Flat;
        feature = Feature.None;
        resource = Resource.None;
        tileImprovement = TileImprovement.NoFeature;
    }
    
    public GameTile(GameTileData data, GameStateData gameState)
    {
        x = data.x;
        y = data.y;
        biome = data.biome;
        terrain = data.terrain;
        feature = data.feature;
        resource = data.resource;
        tileImprovement = data.tileImprovement;
        riverEdges = data.riverEdges;
        hasFreshWater = data.hasFreshWater;

        // if (data.unitID.HasValue && unitDictionary.ContainsKey(data.unitID.Value))
        // {
        //     unit = unitDictionary[data.unitID.Value];
        // }
    }

    public static void SetAllTiles(GameTile[][] allTiles) { AllTiles = allTiles; }

    /// <summary>
    /// Gets the six neighboring tiles in a hexagonal grid. Top, Top-Right, Bottom-Right, etc.
    /// </summary>
    /// <returns>List of neighboring tiles.</returns>
    public static GameTile?[] GetNeighborsArray(GameTile[][] tiles, int tileX, int tileY)
    {
        GameTile?[] neighbors = new GameTile?[6];

        // Check if tileX and tileY are out of bounds
        if (tileX < 0 || tileY < 0 || tileX >= tiles.Length || tileY >= tiles[0].Length)
            return neighbors;

        // Define offsets for hexagonal grid
        int[][] evenRowOffsets = new int[][]
        {
            new int[] {  0, -1 }, // Top
            new int[] { -1,  1 }, // Top-Right
            new int[] {  1,  1 }, // Bottom-Right
            new int[] {  0,  1 }, // Bottom
            new int[] {  1,  0 }, // Bottom-Left
            new int[] { -1,  0 }, // Top-Left
        };

        int[][] oddRowOffsets = new int[][]
        {
            new int[] {  0, -1 }, // Top
            new int[] { -1,  0 }, // Top-Right
            new int[] {  1,  0 }, // Bottom-Right
            new int[] {  0,  1 }, // Bottom
            new int[] {  1, -1 }, // Bottom-Left
            new int[] { -1, -1 }, // Top-Left
        };

        // Use the correct offset list based on the row index
        int[][] offsets = (tileX % 2 == 0) ? evenRowOffsets : oddRowOffsets;

        for (int i=0; i<offsets.Length; i++)
        {
            int newX = tileX + offsets[i][0];
            int newY = tileY + offsets[i][1];

            // Ensure the new coordinates are within bounds
            if (newX >= 0 && newX < tiles.Length && newY >= 0 && newY < tiles[newX].Length)
            {
                neighbors[i] = tiles[newX][newY];
            }
        }

        return neighbors;
    }
    public static GameTile?[] GetNeighborsArray(int tileX, int tileY) { return GameTile.GetNeighborsArray(AllTiles, tileX, tileY); }
    public GameTile?[] GetNeighborsArray() { return GameTile.GetNeighborsArray(AllTiles, x, y); }

    /// <summary>
    /// Calculates and returns the yields of the tile (Food, Production).
    /// </summary>
    /// <returns>A tuple (int food, int production) representing tile yields.</returns>
    public (int food, int production) GetYields()
    {
        int food = 0;
        int production = 0;

        // Set base Biome yields
        switch (biome)
        {
            case Biome.Plains:
                food = 1;
                production = 1;
                break;
            case Biome.Grassland:
                food = 2;
                production = 1;
                break;
            case Biome.Tundra:
                food = 1;
                production = 1;
                break;
            case Biome.Desert:
                production = 1;
                break;
            case Biome.Snow:
                break;
            case Biome.Coast:
                food = 1;
                break;
            case Biome.Ocean:
                food = 1;
                break;
        }

        // Factor in Terrain Yields
        switch (terrain)
        {
            case Terrain.Flat:
                break;
            case Terrain.Hill:
                production += 1; // +1 Production
                break;
            case Terrain.Mountain:
                return (0, 0); // Mountains have no yields
        }

        // Factor in GameTile Feature
        switch (feature)
        {
            case Feature.None:
                break;
            case Feature.Woods:
                production += 1; // +1 Production
                break;
            case Feature.Floodplains:
                food += (biome == Biome.Desert) ? 2 : 1;
                break;
            case Feature.Marsh:
                food += 1; // +1 Food
                break;
            case Feature.Rainforest:
                food += 1; // +1 Food
                break;
            case Feature.Oasis:
                food += 3; // +3 Food
                break;
        }

        // Factor in GameTile Improvement
        switch (tileImprovement)
        {
            case TileImprovement.NoFeature:
                break;
            case TileImprovement.Farm:
                food += 1; // +1 Food
                break;
            case TileImprovement.Mine:
                production += 1; // +1 Production
                break;
            case TileImprovement.LumberCamp:
                production += 1; // +1 Production
                break;
            case TileImprovement.Pasture:
                production += 1; // +1 Production
                break;
            case TileImprovement.Camp:
                break; // Gold yield (ignored for now)
            case TileImprovement.Plantation:
                break; // Gold yield (ignored for now)
            case TileImprovement.FishingBoats:
                food += 1; // +1 Food
                break;
        }

        // If there's a Settlement, enforce at least 2 Food ? this should go after biome ? or ... eg Granary
        if (settlement is not null)
        {
            food = Math.Max(food, 2);
        }

        return (food, production);
    }

    /* -- Setters for biome/terrain/feature/resource/improvement -- */
    public void SetBiome(Biome biome) { this.biome = biome; }
    public void SetTerrain(Terrain terrain) { this.terrain = terrain; }
    public void SetFeature(Feature feature) { this.feature = feature; }
    public void SetResource(Resource resource) { this.resource = resource; }
    public void SetImprovement(TileImprovement tileImprovement) { this.tileImprovement = tileImprovement; }

    /* -- Terrain helper functions -- */
    public void SetRiverEdge(int index, bool value) { riverEdges[index] = value; }
    public void SetHasFreshWater(bool value) { hasFreshWater = value; }
    public bool IsLand()
    {
        return biome == Biome.Plains ||
            biome == Biome.Grassland ||
            biome == Biome.Tundra ||
            biome == Biome.Desert ||
            biome == Biome.Snow;
    }
    public bool IsWalkable() { return IsLand() && terrain != Terrain.Mountain; }
    public bool HasRiver() { return riverEdges.Any(edge => edge); }

    /* -- Overload Boolean Comparisons -- */
    public static bool operator ==(GameTile a, GameTile b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a is null || b is null) return false;
        return a.x == b.x && a.y == b.y;
    }
    public static bool operator !=(GameTile a, GameTile b) { return !(a == b); }
    public override bool Equals(object obj) { return (obj is GameTile otherGameTile) ? this == otherGameTile : false; }
    public override int GetHashCode() { return HashCode.Combine(x, y); }


    /* -- Units -- */
    public void SetUnit(Unit unit) { this.unit = unit; }
    public void UnsetUnit() { this.unit = null; }
}
