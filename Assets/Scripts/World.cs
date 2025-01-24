using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class World : ISerialization
{
    // Instance Attributes
    [JsonProperty]
    private int _length;
    [JsonProperty]
    private int _height;
    [JsonProperty]
    private GameTile[,] _world;
    [JsonProperty]
    public List<Point> spawnPoints;
    
    // Constants
    private const int DefaultBiomeFill = 7; // 1.Plains, 2.Grassland, 3.Tundra, 4.Desert, 5.Snow, 6.Coast, 7.Ocean
    
    // Class Methods
    
    /* World Constructor */
    public World() {}
    public World(int length, int height)
    {
        // Length must be odd in order to connect world horizontally.
        if (length % 2 != 0)
        {
            throw new System.ArgumentException("Length must be even.");
        }
        _length = length;  
        _height = height;
        _world = new GameTile[length, height];
    }
    
    /* Fill an empty world with a starting Tile. */
    public void FillEmptyWorld(int biome)
    {
        for (int x = 0; x < _length; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                // Instantiate default/background tile to fill world with
                GameTile gameTile = new GameTile(DefaultBiomeFill, 0, 0, 0);
                
                // Record its X & Y position within its instance attributes
                gameTile.SetXPos(x);
                gameTile.SetYPos(y);
                
                // Store it in 2D array.
                _world[x, y] = gameTile;
            }
        }
    }

    /* Sets Tile Adjacency (neighbors) based on a Flat Top/Bottom hexagon grid with an even-q orientation (0,0 must be at bottom left of grid). */
    public void SetTileAdjacency()
    {
        for (int x = 0; x < _length; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                // Edge 0
                if (y < _height - 1)
                {
                    _world[x, y].SetNeighbor(0, _world[x, y + 1]);
                    _world[x, y + 1].SetNeighbor(3, _world[x, y]);
                }
                
                // Edge 3
                if (y > 0)
                {
                    _world[x,y].SetNeighbor(3, _world[x , y - 1]);
                    _world[x, y - 1].SetNeighbor(0, _world[x, y]);
                }
                
                // Every Odd X Tile
                if (x % 2 != 0)
                {
                    // Edge 1 
                    if (x < _length - 1 && y < _height - 1)
                    {
                        _world[x, y].SetNeighbor(1, _world[x + 1, y + 1]);
                        _world[x + 1, y + 1].SetNeighbor(4, _world[x, y]);
                    }

                    // Edge 2 
                    if (x < _length - 1)
                    {
                        _world[x, y].SetNeighbor(2, _world[x + 1, y]);
                        _world[x + 1, y].SetNeighbor(5, _world[x, y]);
                    }

                    // Edge 4
                    if (x > 0)
                    {
                        _world[x,  y].SetNeighbor(4, _world[x - 1, y]);
                        _world[x - 1, y].SetNeighbor(1, _world[x, y]);
                    }

                    // Edge 5
                    if (x > 0 && y < _height - 1)
                    {
                        _world[x, y].SetNeighbor(5, _world[x - 1, y + 1]);
                        _world[x - 1, y + 1].SetNeighbor(2, _world[x, y]);
                    }
                } 
            }
        }

        // Set the horizontal edges of world adjacent to each other.
        for (int y = _height - 1; y >= 0; y--)
        {
            // Edge 1
            if (y < _height - 1 && y > 0)
            {
                _world[_length - 1, y].SetNeighbor(1, _world[0, y + 1]);
                _world[0, y + 1].SetNeighbor(4, _world[_length - 1, y]);
            }
            
            // Edge 2
            _world[_length - 1, y].SetNeighbor(2, _world[0, y]);
            _world[0, y].SetNeighbor(5, _world[_length - 1, y]);
            
            // Edge 4
            if (y > 0 && y < _height - 1)
            {
                _world[0, y].SetNeighbor(4, _world[_length - 1, y - 1]);
                _world[_length - 1, y - 1].SetNeighbor(1, _world[0, y]);
            }
            
            // Edge 5
            _world[0, y].SetNeighbor(5, _world[_length - 1, y]);
            _world[_length - 1, y].SetNeighbor(2, _world[0, y]);
            
        }
    }

    public void StageForSerialization()
    {
        // Remove Tile adjacencies
        for (int x = 0; x < _length; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _world[x, y].StageForSerialization();
            }
        }
    }

    public void RestoreAfterDeserialization(GameManager gameManager)
    {
        // Set GameTile neighbor properties back to normal.
        // gameManager.game.world.SetTileAdjacency();

        for (int x = 0; x < _length; x++) {
            for (int y = 0; y < _height; y++) {
                GameTile.SetGameTileRegistry(_world[x,y].UID, _world[x, y]);
            }
        }
        for (int x = 0; x < _length; x++) {
            for (int y = 0; y < _height; y++) {
                _world[x, y].RestoreAfterDeserialization(gameManager);
            }
        }
    }

    /* Print the world to console. (Bad way to test but will do for now) */
    public void PrintWorld()
    {
        string worldString = "";
        for (int y = _height - 1; y >= 0; y--)
        {
            for (int x = 0; x < _length; x++)
            {
                worldString += _world[x,y].GetBiome().ToString() + " ";
            }
            worldString += "\n";
        }
        Debug.Log(worldString);
    }

    /* Takes a Tile's X & Y Position on a 2D grid and prints a list of its neighbors' X & Y.  */
    public void TestTileAdjacency(int xPos, int yPos)
    {
        string output = "Tile ("+xPos+","+yPos+") is adjacent to: " + "\n";

        for (int x = 0; x < _world[xPos, yPos].GetNeighbors().Length; x++)
        {
            if (_world[xPos, yPos].GetNeighbors()[x] is not null)
            {
                output += "Edge: " + x + " " + "(" + _world[xPos, yPos].GetNeighbors()[x].GetXPos() + 
                          "," + _world[xPos, yPos].GetNeighbors()[x].GetYPos() + ")";
                
                output += "\n";
            }
        }
        Debug.Log(output);
    }
    
    // Tile Modification Methods
    public void ModifyTile(String tileProperty, Point point, int value)
    {
        if (tileProperty == "biome")
        {
            ModifyTileBiome(point, value);
        }
        else if (tileProperty == "terrain")
        {
            ModifyTileTerrain(point, value);
        } else if (tileProperty == "feature")
        {
            ModifyTileFeature(point, value);
        }
    }

    public void ModifyTileBiome(Point point, int biome)
    {
        _world[point.x, point.y].SetBiome(biome); 
    }

    public void ModifyTileTerrain(Point point, int terrain)
    {
        _world[point.x, point.y].SetTerrain(terrain);
    }

    public void ModifyTileFeature(Point point, int feature)
    {
        _world[point.x, point.y].SetFeature(feature);
    }

    public void ModifyTileResource(Point point, int resource)
    {
        _world[point.x, point.y].SetResource(resource);
    }
    
    // Getter Methods

    public int GetLength()
    {
        return _length;
    }

    public int GetHeight()
    {
        return _height;
    }
    
    public GameTile[,] GetWorld()
    {
        return _world;
    }

    public GameTile GetTile(int x, int y)
    {
        return _world[x, y];
    }

    public GameTile GetTile(Point point)
    {
        return _world[point.x, point.y];
    }

    public List<Point> GetSpawnPoints()
    {
        return spawnPoints;
    }
   
}

