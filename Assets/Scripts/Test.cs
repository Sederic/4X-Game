using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Test : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile tile;
    public Tile praireTile;
    public Tile tundraTile;
    public Tile oceanTile;
    public Tile coastTile;
    public Tile snowTile;


    // Start is called before the first frame update
    void Start()
    {
        TestWorldGeneration();
    }

    private void TestWorldGeneration()
    {
        World gameWorld = new WorldGenerator().GenerateWorld(100, 50, 2);
        DrawTilemap(gameWorld);
        gameWorld.SetTileAdjacency();
        gameWorld.TestTileAdjacency(2, 0);
        gameWorld.TestTileAdjacency(1, 0);
        gameWorld.TestTileAdjacency(3, 0);
        gameWorld.TestTileAdjacency(0, 1);
        gameWorld.TestTileAdjacency(0, 2);
        gameWorld.TestTileAdjacency(99, 49);
        gameWorld.TestTileAdjacency(99, 48);
        gameWorld.TestTileAdjacency(0, 0);
    }

    public void DrawTilemap(World world)
    {

        /*for (int y = 30; y > 15; y--)
        {
            if (y != 28)
            {
                world.ModifyTileTerrain(new Point(30, y), 2);
            }
        }*/
        for (double x = 0; x < world.GetLength(); x++)
        {
            for (int y = world.GetHeight()-1; y > 0; y--)
            {
                if ((Random.Range(1,6)==1) && (world.GetTile((int)x,y).GetBiome() != 7) && (world.GetTile((int)x,y).GetBiome() != 6))
                {
                    world.ModifyTileTerrain(new Point((int)x, y), 2);
                }
            }
        }
        
        // Debug.Log("Trying to reach 27,23 from 25,25 within 3 moves. Testing with movement point based early exits and without MP based early exits");
        List<GameTile> path = new List<GameTile>();
        List<GameTile> path2 = new List<GameTile>();
        List<Tuple<GameTile, int>> list = Pathfinder.AStarWithLimit(world.GetTile(80, 25), world.GetTile(10, 23), 100);
        List<Tuple<GameTile, int>> list2 = Pathfinder.AStarWithoutLimit(world.GetTile(25, 25), world.GetTile(80, 40));
        if (list.Count != 0)
        {
            Debug.Log(list[^1].Item1.GetXPos() + "," + list[^1].Item1.GetYPos() + " " + "reached when factoring limit");
        }
        else
        {
            Debug.Log("A* with limit could not find a path, likely unreachable.");
        }

        if (list2.Count != 0)
        {
            Debug.Log(list2[^1].Item1.GetXPos() + "," + list2[^1].Item1.GetYPos() + " " + "reached when not factoring limit but took" + list2[^1].Item2 + "moves");
        }
        else
        {
            Debug.Log("A* without limit could not find a path, likely unreachable.");
        }
        

        foreach (Tuple<GameTile, int> t in list)
        {
            path.Add(t.Item1);
        }
        foreach (Tuple<GameTile, int> t in list2)
        {
            path2.Add(t.Item1);
        }

        for (int x = 0; x < world.GetLength(); x++)
        {
            for (int y = 0; y < world.GetHeight(); y++)
            {
                if (world.GetTile(x, y).GetBiome() == 1)
                {
                    tile.color = new Color32(145, 158, 11, 255);

                }
                else if (world.GetTile(x, y).GetBiome() == 2)
                {
                    tile.color = new Color32(92, 128, 82, 255);
                }
                else if (world.GetTile(x, y).GetBiome() == 3)
                {
                    tile.color = new Color32(144, 158, 141, 255);
                }
                else if (world.GetTile(x, y).GetBiome() == 4)
                {
                    tile.color = new Color32(255, 217, 112, 255);
                }
                else if (world.GetTile(x, y).GetBiome() == 5 || world.GetTile(x, y).GetBiome() == 0)
                {
                    tile.color = Color.white;
                }
                else if (world.GetTile(x, y).GetBiome() == 6)
                {
                    tile.color = new Color32(110, 187, 255, 255);
                }
                else if (world.GetTile(x, y).GetBiome() == 7)
                {
                    tile.color = new Color32(20, 102, 184, 255);
                }






                // Tile Texture - commented out for easier testing for now
                /*if (world.GetTile(x, y).GetBiome() == 1)
                {
                    tilemap.SetTile(new Vector3Int(y, x, 0), praireTile);
                }
                else if (world.GetTile(x, y).GetBiome() == 3)
                {
                    tilemap.SetTile(new Vector3Int(y, x, 0), tundraTile);
                } else if (world.GetTile(x, y).GetBiome() == 5)
                {
                    tilemap.SetTile(new Vector3Int(y, x, 0), snowTile);
                } else if (world.GetTile(x, y).GetBiome() == 6)
                {
                    tilemap.SetTile(new Vector3Int(y, x, 0), coastTile);
                } else if (world.GetTile(x, y).GetBiome() == 7)
                {
                    tilemap.SetTile(new Vector3Int(y, x, 0), oceanTile);
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(y, x, 0), tile);
                }*/
                    
                if (path.Contains(world.GetTile(x, y)))
                {
                    tile.color = Color.blue;
                }
                if (path2.Contains(world.GetTile(x, y)))
                {
                    tile.color = Color.red;
                }

                if (world.GetTile(x, y).GetTerrain() == 2)
                {
                    tile.color = Color.black;
                }
                tilemap.SetTile(new Vector3Int(y, x, 0), tile);
            }
        }
    }
}
