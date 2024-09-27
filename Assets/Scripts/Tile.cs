using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;

public class Tile : MonoBehaviour
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
    
    // Instance Attributes
    private int _xPos; // The Tile's X Position on a 2D Array
    private int _yPos; // The Tile's Y Position on a 2D Array
    private int _biome; // The base layer of a Tile (Plains: 1, Grassland: 2, Tundra: 3, Desert: 4, Snow: 5, Coast: 6, Ocean: 7)
    private int _terrain; // The topography of a Tile (Flat: 0, Hill: 1, Mountain: 2)
    private int _feature; // The natural feature of a Tile (None: 0, Woods: 1, Floodplains: 2, Marsh: 3, Rainforest: 4, Oasis: 5)
    private int _resource; // The resource on this Tile. Could be a specific Bonus, Luxury, Strategic Resource, or no Resource. CHECK ID INDEX ABOVE^
    private int _improvement; // The Tile Improvement on this Tile or 0 for No Improvement. CHECK ID INDEX ABOVE^
    private int _mc; // Movement cost - the amount of Movement Points a Unit must spend to move unto that Tile.
    private bool[] _riverAdj; // is the Tile Adjacent to a river? -> [0,1,2,3,4,5] 
    private Unit _unit; // The Unit on this Tile. May be null (no unit on Tile). 
    private Settlement _settlement; // The Settlement on this Tile. May be null (no Settlement on Tile).
    private int[] _yields; // An int array of a Tile's Yields. [Food, Production, Gold, Culture, Science] -> [0,1,2,3,4]
    
    // Constants
    private const int TotalYields = 5;
    private const int Zero = 0;
    private const int Edges = 6;
    
    // Class Methods
    
    /* Natural Tile Constructor - Only Biome, Terrain, Feature, and Resource. (Good for world gen) */
    public Tile(int biome, int terrain, int feature, int resource)
    {
        _biome = biome;
        _terrain = terrain;
        _feature = feature;
        _resource = resource;
        _improvement = Zero;
        _unit = null;
        _settlement = null;
        _mc = CalculateMovementCost();
        _riverAdj = CalculateRiverAdjacency();
        _yields = CalculateYields();
    }
    
    /* Full Tile Constructor (Good for testing) */
    public Tile(int biome, int terrain, int feature,  int resource, int tileImprovement, Unit unit, Settlement settlement)
    {
        _biome = biome;
        _terrain = terrain;
        _feature = feature;
        _resource = resource;
        _improvement = tileImprovement;
        _unit = unit;
        _settlement = settlement;
        _riverAdj = CalculateRiverAdjacency();
        _mc = CalculateMovementCost();
        _yields = CalculateYields();
    }
    
    /* Calculate/Update the Yields of a Tile by going through its properties. */
    private int[] CalculateYields()
    {
        int[] yields = new int[TotalYields];
        
        // Set base Biome yields.
        switch (_biome)
        {
            case 1: // Plains
                yields[0] = 1; // +1 Food
                yields[1] = 1; // +1 Production
                break;
            case 2: // Grassland
                yields[0] = 2; // +2 Food
                yields[1] = 1; // +1 Production
                break;
            case 3: // Tundra
                yields[0] = 1; // +1 Food
                yields[1] = 1; // +1 Production
                break;
            case 4: // Desert
                yields[1] = 1; // +1 Production
                break;
            case 5: // Snow
                break;
            case 6: // Coast
                yields[0] = 1; // +1 Food
                yields[3] = 1; // +1 Gold
                break;
            case 7: // Ocean
                yields[0] = 1; // +1 Food
                break;
        }
        
        // Factor in Terrain Yields
        switch (_terrain)
        {
            case 0: // Flat
                break;
            case 1: // Hills
                yields[1] += 1; // +1 Production
                break;
            case 2: // Mountain (has no yields)
                foreach (int y in yields)
                {
                    yields[y] = 0;
                }
                return yields;
        }
        
        // Factor in Tile Feature
        switch (_feature)
        {
            case 0: // No Tile Feature
                break;
            case 1: // Woods
                yields[1] += 1; // +1 Production
                break;
            case 2: // Floodplains
                yields[0] += 1; // +1 Food
                break;
            case 3: // Marsh
                yields[0] += 1; // +1 Food
                break;
            case 4: // Rainforest
                yields[0] += 1; // +1 Food
                break;
            case 5: // Oasis
                yields[0] += 3; // +3 Food
                yields[2] += 1; // +1 Gold
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
                yields[0] += 1; // +1 Food
                break;
            case 2: // Mine
                yields[1] += 1; // +1 Production
                break;
            case 3: // Lumber Camp
                yields[1] += 1; // +1 Production
                break;
            case 4: // Pasture
                yields[1] += 1; // +1 Production
                break;
            case 5: // Camp
                yields[2] += 1; // +1 Gold
                break;
            case 6: // Plantation
                yields[2] += 2; // +2 Gold
                break;
            case 7: // Fishing Boats
                yields[0] += 1; // +1 Food
                break;
        }

        return yields;
    }
    
    private bool[] CalculateRiverAdjacency()
    {
        bool[] riverAdj = new bool[Edges];
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
        
        // If Woods, Marsh, or Rainforest
        if (_feature is 1 or 3 or 4)
        {
            return 2;
        }
    
        //Otherwise
        return 1;
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
        _yields = CalculateYields();
    }

    public void SetTerrain(int terrain)
    {
        _terrain = terrain;
        _yields = CalculateYields();
    }

    public void SetFeature(int feature)
    {
        _feature = feature;
        _yields = CalculateYields();
    }

    public void SetResource(int resource)
    {
        _resource = resource;
        _yields = CalculateYields();
    }

    public void SetImprovement(int improvement)
    {
        _improvement = improvement;
        _yields = CalculateYields();
    }

    public void SetUnit(Unit unit)
    {
        
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

    public int GetMovementCost()
    {
        return _mc;
    }

    public Unit GetUnit()
    {
        return _unit;
    }

    public Settlement GetSettlement()
    {
        return _settlement;
    }

    public int[] GetYields()
    {
        return _yields;
    }

}
