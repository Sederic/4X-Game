using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Test : MonoBehaviour
{
    public GameObject square;
    public Tilemap tilemap;
    public Tile tile;
    public GameObject riverSegment;
    public Tile prairieTile;
    public Tile grassTile;
    public Tile tundraTile;
    public Tile desertTile;
    public Tile oceanTile;
    public Tile coastTile;
    public Tile snowTile;
    public Tile prairieHillsTile;
    public Tile grassHillsTile;
    public Tile tundraHillsTile;
    public Tile desertHillsTile;
    public Tile snowHillsTile;
    public Tile prairieMountainsTile;
    public Tile grassMountainsTile;
    public Tile tundraMountainsTile;
    public Tile desertMountainsTile;
    public Tile snowMountainsTile;

    // Start is called before the first frame update
    void Start()
    {
        TestWorldGeneration();
    }

    private void TestWorldGeneration()
    {
        World gameWorld = new WorldGenerator().GenerateWorld(100, 50,1);
        //World gameWorld = new WorldGenerator().GenerateWorld(100, 50, 2);

        DrawTilemap(gameWorld);
        gameWorld.SetTileAdjacency();
    }

    public void DrawTilemap(World world)
    {

        List<GameTile> path = new List<GameTile>();
        List<Tuple<GameTile, int>> list = Pathfinder.AStarWithLimit(world.GetTile(16, 15), world.GetTile(3, 16), 15);

        foreach (Tuple<GameTile, int> t in list)
        {
            path.Add(t.Item1);
        }

        for (int x = 0; x < world.GetLength(); x++)
        {
            for (int y = 0; y < world.GetHeight(); y++)
            {
                GameTile currTile = world.GetTile(x, y);
                
                // Grid can be switched, check if Height is Width
                double tileHeight = 0.9555f;
                float tileWidth = 1f;
                float edge = 4f;

                
                square.transform.position =
                    new Vector3(x * .75f * tileWidth, (float)(y * tileHeight + (tileHeight / 2) * (x % 2)));



                float bigX = tileWidth * x * .75f;
                float bigY = (float)(y * tileHeight + (tileHeight / 2) * (x % 2));

                square.transform.position = new Vector3(
                    (float)(bigX + Math.Pow(-1f, Math.Pow(0f, (5f - edge) * (4f - edge))) *
                        Math.Pow(0f, Math.Pow(0f, edge % 3f)) * tileWidth * 3 / 8),
                    (float)(bigY + Math.Pow(-1f, Math.Pow(0f, Math.Abs((edge - 2f) * (edge - 3f) * (edge - 4f)))) *
                        (tileHeight / 4f + tileHeight / 4f * Math.Abs(Math.Pow(0f, Math.Pow(0f, edge % 3f)) - 1f))),
                    0f);
                

                // Plains
                if (world.GetTile(x, y).GetBiome() == 1)
                {
                    // Hills
                    if (world.GetTile(x, y).GetTerrain() == 1)
                    {
                        tilemap.SetTile(new Vector3Int(y, x, 0), prairieHillsTile);
                    }
                    // Mountain
                    else if (world.GetTile(x, y).GetTerrain() == 2)
                    {
                        tilemap.SetTile(new Vector3Int(y, x, 0), prairieMountainsTile);
                    }
                    // Flat 
                    else
                    {
                        tilemap.SetTile(new Vector3Int(y, x, 0), prairieTile);
                    }

                    //tile.color = new Color32(145, 158, 11, 255);

                }
                // Grassland
                else if (world.GetTile(x, y).GetBiome() == 2)
                {
                    // Hills
                    if (world.GetTile(x, y).GetTerrain() == 1)
                    {
                        tilemap.SetTile(new Vector3Int(y, x, 0), grassHillsTile);
                    }
                    // Mountain
                    else if (world.GetTile(x, y).GetTerrain() == 2)
                    {
                        tilemap.SetTile(new Vector3Int(y, x, 0), grassMountainsTile);
                    }
                    // Flat 
                    else
                    {
                        tilemap.SetTile(new Vector3Int(y, x, 0), grassTile);
                    }

                    //tile.color = new Color32(92, 128, 82, 255);
                }
                // Tundra
                else if (world.GetTile(x, y).GetBiome() == 3)
                {
                    // Hills
                    if (world.GetTile(x, y).GetTerrain() == 1)
                    {
                        tilemap.SetTile(new Vector3Int(y, x, 0), tundraHillsTile);
                    }
                    // Mountain
                    else if (world.GetTile(x, y).GetTerrain() == 2)
                    {
                        tilemap.SetTile(new Vector3Int(y, x, 0), tundraMountainsTile);
                    }
                    // Flat 
                    else
                    {
                        tilemap.SetTile(new Vector3Int(y, x, 0), tundraTile);
                    }

                    //tile.color = new Color32(144, 158, 141, 255);
                }
                // Desert
                else if (world.GetTile(x, y).GetBiome() == 4)
                {
                    // Hills
                    if (world.GetTile(x, y).GetTerrain() == 1)
                    {
                        tilemap.SetTile(new Vector3Int(y, x, 0), desertHillsTile);
                    }
                    // Mountain
                    else if (world.GetTile(x, y).GetTerrain() == 2)
                    {
                        tilemap.SetTile(new Vector3Int(y, x, 0), desertMountainsTile);
                    }
                    // Flat 
                    else
                    {
                        tilemap.SetTile(new Vector3Int(y, x, 0), desertTile);
                    }

                    //tile.color = new Color32(255, 217, 112, 255);
                }
                // Snow
                else if (world.GetTile(x, y).GetBiome() == 5)
                {
                    // Hills
                    if (world.GetTile(x, y).GetTerrain() == 1)
                    {
                        tilemap.SetTile(new Vector3Int(y, x, 0), snowHillsTile);
                    }
                    // Mountain
                    else if (world.GetTile(x, y).GetTerrain() == 2)
                    {
                        tilemap.SetTile(new Vector3Int(y, x, 0), snowMountainsTile);
                    }
                    // Flat 
                    else
                    {
                        tilemap.SetTile(new Vector3Int(y, x, 0), snowTile);
                    }

                    //tile.color = Color.white;
                }
                else if (world.GetTile(x, y).GetBiome() == 6)
                {
                    tilemap.SetTile(new Vector3Int(y, x, 0), coastTile);
                    //tile.color = new Color32(110, 187, 255, 255);
                }
                else if (world.GetTile(x, y).GetBiome() == 7)
                {
                    tilemap.SetTile(new Vector3Int(y, x, 0), oceanTile);
                    //tile.color = new Color32(20, 102, 184, 255);
                }

                if (currTile.GetFreshWaterAccess())
                {
                    // For testing FreshWaterAccess
                    /*tilemap.SetTile(new Vector3Int(y, x, 0), tile);
                    tile.color = Color.white;*/
                    
                    for (int index = 0; index < 6; index++)
                    {
                        if (currTile.GetRiverEdge(index))
                        {
                            // Formula for River Position
                            Vector3 riverPosition = new Vector3((float)(bigX + Math.Pow(-1f, Math.Pow(0f, (5f - index) * (4f - index))) *
                                Math.Pow(0f, Math.Pow(0f, index % 3f)) * tileWidth * 3 / 8),
                            (float)(bigY + Math.Pow(-1f, Math.Pow(0f, Math.Abs((index - 2f) * (index - 3f) * (index - 4f)))) *
                                (tileHeight / 4f + tileHeight / 4f * Math.Abs(Math.Pow(0f, Math.Pow(0f, index % 3f)) - 1f))),
                            0f);
                            
                            Quaternion riverRotation;

                            if (index == 1 || index == 4)
                            {
                                riverRotation = Quaternion.Euler(0f, 0f, -63f);
                            } else if (index == 5 || index == 2)
                            {
                                riverRotation = Quaternion.Euler(0f, 0f, 63f);
                            }
                            else
                            {
                                riverRotation = Quaternion.Euler(0f, 0f, 0f);
                            }
                            
                            Instantiate(riverSegment, riverPosition, riverRotation);
                        }
                    }
                }
                

            }
        }
    }
}
