using Newtonsoft.Json;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameTile : ISerialization
{
    /*ID INDEX
        Biomes ID:
            1. Plains
            2. Grassland
            3. Tundra
            4. Desert
            5. Snow
            6. Coast
            7. Ocean

        Terrain ID:
            0. Flat
            1. Hill
            2. Mountain

        Feature ID:
            0. None
            1. Woods
            2. Floodplains
            3. Marsh
            4. Rainforest
            5. Oasis
            6. Lake

        Resources ID:
             0. (No resource)
             1.Wheat 2.Rice 3.Maize 4.Stone 5.Coal 6.Deer 7.Cattle 8.Sheep 9.Bananas 10. Fish
             11.Crabs 12.Horses 13.Copper 14.Iron 15.Silk 16.Spices 17.Incense 18.Wine 19.Cotton 20.Citrus
             21.Dyes 22.Cacao 23.Pomegranate 24.Furs 25.Ivory 26.Pearls 27.Whales 28.Marble 29.Salt 30.Amber
             31.Jade 32.Silver 33.Gold

         Tile Improvement ID:
             0 - No Feature
             1 - Farm
             2 - Mine
             3 - Lumber Camp
             4 - Pasture
             5 - Camp
             6 - Plantation
             7 - Fishing Boats
    */

    private static int _nextUID = 1; // TODO: NOT THREAD SAFE

    // Instance Attributes
    [JsonProperty]
    public int UID { get; private set; }

    [JsonProperty]
    private int _xPos; // The Tile's X Position on a 2D Array
    [JsonProperty]
    private int _yPos; // The Tile's Y Position on a 2D Array
    [JsonProperty]
    private int _biome; // The base layer of a Tile (Plains: 1, Grassland: 2, Tundra: 3, Desert: 4, Snow: 5, Coast: 6, Ocean: 7)
    [JsonProperty]
    private int _terrain; // The topography of a Tile (Flat: 0, Hill: 1, Mountain: 2)
    [JsonProperty]
    private int _feature; // The natural feature of a Tile (None: 0, Woods: 1, Floodplains: 2, Marsh: 3, Rainforest: 4, Oasis: 5)
    [JsonProperty]
    private int _resource; // The resource on this Tile. Could be a specific Bonus, Luxury, Strategic Resource, or no Resource. CHECK ID INDEX ABOVE^
    [JsonProperty]
    private int _improvement; // The Tile Improvement on this Tile or 0 for No Improvement. CHECK ID INDEX ABOVE^
    [JsonProperty]
    private int _mc; // Movement cost - the amount of Movement Points a Unit must spend to move unto that Tile.
    [JsonIgnore]
    private GameTile?[] _neighbors; // Adjacent Tiles to these tiles. Index corresponds to Edge assuming flat top/bottom hexagons. Flat Top is 0, Flat Bottom is 3, Right sides are 1,2, Left Sides are 3,4.
    [JsonProperty] // Serialized list of neighbor UIDs
    private List<int?> _neighborUIDs;
    [JsonProperty]
    private bool[] _riverEdges; // Are the Tile edges Adjacent to a river? -> [0,1,2,3,4,5] Represent edges on a hexagon starting from the Top moving clockwise.
    [JsonProperty]
    private bool _freshWaterAccess; // Does the Tile have fresh water access?
    [JsonProperty] 
    private bool _riverAdjacency; // Is the Tile adjacent to a River?
    [JsonProperty]
    private Unit _unit; // The Unit on this Tile. May be null (no unit on Tile). 
    [JsonProperty]
    private Settlement _settlement; // The Settlement on this Tile. May be null (no Settlement on Tile).
    [JsonProperty]
    private int[] _yields; // An int array of a Tile's Yields. [Food, Production, Gold, Culture, Science] -> [0,1,2,3,4]

    // Constants
    private const int TotalYields = 5;
    private const int Zero = 0;
    private const int TileEdges = 6;

    // Static map for UID-to-GameTile lookup
    private static Dictionary<int, GameTile> _gameTileRegistry = new Dictionary<int, GameTile>();
    public static void SetGameTileRegistry(int uid, GameTile gameTile) { _gameTileRegistry[uid] = gameTile; }
    public static void ClearGameTileRegistry() { _gameTileRegistry = new Dictionary<int, GameTile>(); }

    // Class Methods
    public GameTile() {
    }

    /* Natural Tile Constructor - Only Biome, Terrain, Feature, and Resource. (Good for world gen) */
    public GameTile(int biome, int terrain, int feature, int resource)
    {
        UID = _nextUID++;
        _neighborUIDs = new List<int?>();
        _gameTileRegistry[UID] = this;

        _biome = biome;
        _terrain = terrain;
        _feature = feature;
        _resource = resource;
        _improvement = Zero;
        _neighbors = new GameTile[TileEdges];
        _unit = null;
        _settlement = null;
        _mc = CalculateMovementCost();
        _riverEdges = CalculateRiverAdjacency();
        _freshWaterAccess = false;
        _yields = GetYields();
    }

    /* Full Tile Constructor (Good for testing) */
    public GameTile(int biome, int terrain, int feature, int resource, int tileImprovement, Unit unit,
        Settlement settlement)
    {
        UID = _nextUID++;
        _neighborUIDs = new List<int?>();
        _gameTileRegistry[UID] = this;

        _biome = biome;
        _terrain = terrain;
        _feature = feature;
        _resource = resource;
        _improvement = tileImprovement;
        _neighbors = new GameTile[TileEdges];
        _unit = unit;
        _settlement = settlement;
        _riverEdges = CalculateRiverAdjacency();
        _freshWaterAccess = false;
        _mc = CalculateMovementCost();
        _yields = GetYields();
    }

    /* Calculate/Update the Yields of a Tile by going through its properties. */
    public int[] GetYields()
    {
        _yields = new int[TotalYields];

        // Set base Biome yields.
        switch (_biome)
        {
            case 1: // Plains
                _yields[0] = 1; // +1 Food
                _yields[1] = 1; // +1 Production
                break;
            case 2: // Grassland
                _yields[0] = 2; // +2 Food
                _yields[1] = 1; // +1 Production
                break;
            case 3: // Tundra
                _yields[0] = 1; // +1 Food
                _yields[1] = 1; // +1 Production
                break;
            case 4: // Desert
                _yields[1] = 1; // +1 Production
                break;
            case 5: // Snow
                break;
            case 6: // Coast
                _yields[0] = 1; // +1 Food
                _yields[2] = 1; // +1 Gold
                break;
            case 7: // Ocean
                _yields[0] = 1; // +1 Food
                break;
        }

        // Factor in Terrain Yields
        switch (_terrain)
        {
            case 0: // Flat
                break;
            case 1: // Hills
                _yields[1] += 1; // +1 Production
                break;
            case 2: // Mountain (has no yields)
                foreach (int y in _yields)
                {
                    _yields[y] = 0;
                }

                return _yields;
        }

        // Factor in Tile Feature
        switch (_feature)
        {
            case 0: // No Tile Feature
                break;
            case 1: // Woods
                _yields[1] += 1; // +1 Production
                break;
            case 2: // Floodplains
                if (_biome == 4)
                {
                    _yields[0] += 2;
                }
                else
                {
                    _yields[0] += 1; // +1 Food
                }
                break;
            case 3: // Marsh
                _yields[0] += 1; // +1 Food
                break;
            case 4: // Rainforest
                _yields[0] += 1; // +1 Food
                break;
            case 5: // Oasis
                _yields[0] += 3; // +3 Food
                _yields[2] += 1; // +1 Gold
                break;
        }

        // Factor in Tile Resource
        switch (_resource)
        {
            case 0: // No Resource
                break;
            case 1: // Wheat
                break;
            case 2: // Maize
                break;
            case 3: // Rice
                break;
            case 4: // Stone
                break;
            case 5: // Coal
                break;
            case 6: // Deer
                break;
            case 7: // Cattle
                break;
            case 8: // Sheep
                break;
            case 9: // Bananas
                break;
            case 10: // Fish
                break;
            case 11: // Crabs
                break;
            case 12: // Horses
                break;
            case 13: // Copper
                break;
            case 14: // Iron
                break;
            case 15: // Silk
                break;
            case 16: // Spices
                break;
            case 17: // Incense
                break;
            case 18: // Wine
                break;
            case 19: // Cotton
                break;
            case 20: // Citrus
                break;
            case 21: // Dyes
                break;
            case 22: // Cacao
                break;
            case 23: // Pomegranate
                break;
            case 24: // Furs
                break;
            case 25: // Ivory
                break;
            case 26: // Pearls
                break;
            case 27: // Whales
                break;
            case 28: // Marble
                break;
            case 29: // Salt
                break;
            case 30: // Amber
                break;
            case 31: // Jade
                break;
            case 32: // Silver
                break;
            case 33: // Gold
                break;
        }

        // Factor in Tile Improvement
        switch (_improvement)
        {
            case 0: // No Improvement
                break;
            case 1: // Farm
                _yields[0] += 1; // +1 Food
                break;
            case 2: // Mine
                _yields[1] += 1; // +1 Production
                break;
            case 3: // Lumber Camp
                _yields[1] += 1; // +1 Production
                break;
            case 4: // Pasture
                _yields[1] += 1; // +1 Production
                break;
            case 5: // Camp
                _yields[2] += 1; // +1 Gold
                break;
            case 6: // Plantation
                _yields[2] += 2; // +2 Gold
                break;
            case 7: // Fishing Boats
                _yields[0] += 1; // +1 Food
                break;
        }

        if (_settlement is not null)
        {
            // Settlement Tiles always have 2 Food
            _yields[0] = 2;
        }

        return _yields;
    }
    
    public float TileValue()
    {
        float yieldsValue = 0;
        
        yieldsValue += _yields[0];
        yieldsValue += (_yields[1] * 1.25f);
        yieldsValue += _yields[2];
        yieldsValue += (_yields[3] * 1.5f);
        yieldsValue += (_yields[4] * 1.5f);

        return yieldsValue;
    }

    private bool[] CalculateRiverAdjacency()
    {
        bool[] riverAdj = new bool[TileEdges];
        // To be implemented
        return riverAdj;
    }

    private int CalculateMovementCost()
    {
        // If Hills
        if (_terrain == 1)
        {
            return 2;
        }
        if (_terrain == 2)
        {
            return 10000;
        }

        // If Woods, Marsh, or Rainforest
        if (_feature is 1 or 3 or 4)
        {
            return 2;
        }

        //Otherwise
        return 1;
    }

    // Comparison Methods
    public bool IsLand()
    {
        if (_biome == 6 || _biome == 7 || _biome == 8)
        {
            return false;
        }

        return true;
    }

    // Setter Methods
    public void SetXPos(int xPos)
    {
        _xPos = xPos;
    }

    public void SetYPos(int yPos)
    {
        _yPos = yPos;
    }
    
    public void SetBiome(int biome)
    {
        _biome = biome;
        _yields = GetYields();
    }

    public void SetTerrain(int terrain)
    {
        _terrain = terrain;
        _yields = GetYields();
    }

    public void SetFeature(int feature)
    {
        _feature = feature;
        _yields = GetYields();
    }

    public void SetResource(int resource)
    {
        _resource = resource;
        _yields = GetYields();
    }

    public void SetImprovement(int improvement)
    {
        _improvement = improvement;
        _yields = GetYields();
    }

    public void SetNeighbor(int edge, GameTile neighbor)
    {
        if (_neighbors is null)
        {
            _neighbors = new GameTile[6];
        }
        _neighbors[edge] = neighbor;
    }

    public void SetRiverEdge(int edge, bool value)
    {
        _riverEdges[edge] = value;
    }

    public void SetFreshWaterAccess(bool value)
    {
        _freshWaterAccess = value;
    }

    public void SetRiverAdjacency(bool value)
    {
        _riverAdjacency = value;
    }

    public void SetUnit(Unit unit)
    {
        _unit = unit;
        if (unit != null)
        {
            _unit._gameTile = this;
        }
        
    }

    public void SetSettlement(Settlement settlement)
    {
        _settlement = settlement;
    }
    
    // Getter Methods
    public int GetXPos()
    {
        return _xPos;
    }

    public int GetYPos()
    {
        return _yPos;
    }
    
    public int GetBiome()
    {
        return _biome;
    }

    public int GetTerrain()
    {
        return _terrain;
    }

    public int GetFeature()
    {
        return _feature;
    }

    public int GetResource()
    {
        return _resource;
    }

    public int GetImprovement()
    {
        return _improvement;
    }

    public GameTile[] GetNeighbors()
    {
        return _neighbors;
    }

    public bool GetRiverEdge(int edge)
    {
        return _riverEdges[edge];
    }

    public bool GetFreshWaterAccess()
    {
        return _freshWaterAccess;
    }

    public bool GetRiverAdjacency()
    {
        return _riverAdjacency;
    }

    public int GetMovementCost()
    {
        // If Hill
        if (_terrain == 1)
        {
            return 2;
        }

        // If Woods, Marsh, or Rainforest
        if (_feature is 1 or 3 or 4)
        {
            return 2;
        }

        return 1;
    }

    public Unit GetUnit()
    {
        return _unit;
    }

    public Settlement GetSettlement()
    {
        return _settlement;
    }
    
    public bool IsWalkable()
    {
        if (IsLand() && _terrain != 2)
        {
            return true;
        }

        return false;
    }

    public static GameTile GetTileByUID(int uid) { return _gameTileRegistry[uid]; }

    public void StageForSerialization()
    {
        // This is reset by world's set adjacency
        _neighborUIDs.Clear();
        for (int i=0; i<TileEdges; i++) {
            _neighborUIDs.Add(_neighbors[i]?.UID);
        }

        // Civilization's will hold Units and Settlements
        _settlement = null;
        _unit = null;
    }

    public void RestoreAfterDeserialization(GameManager gameManager)
    {
        // Doesn't need to be Restored
        // Settlements and Units will give this Tile it's reference back
        _neighbors = new GameTile[TileEdges];
        for (int i=0; i<TileEdges; i++) {
            int? uid = _neighborUIDs[i];
            _neighbors[i] = (uid is null) ? null : _gameTileRegistry[(int)uid];
        }
    }

    public bool ObstructsVision()
    {
        // Woods, Rainforest, Hills, and Mountain, obstruct vision
        if (_feature is 1 or 4 || _terrain is 1 or 2)
        {
            return true;
        }

        return false;
    }
}
