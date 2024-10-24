using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class Director : MonoBehaviour
{
    [Header("Game Manager")] 
    public GameManager gm;
    [Header("Tilemaps")]
    public Tilemap baseTilemap;
    public Tilemap terrainTilemap;
    public Tilemap featureTilemap;
    [Header("Flat Tiles")]
    public Tile tile;
    public Tile prairieTile;
    public Tile grassTile;
    public Tile tundraTile;
    public Tile desertTile;
    public Tile oceanTile;
    public Tile coastTile;
    public Tile snowTile;
    public Tile lakeTile;
    [Header("Hills Tiles")]
    public Tile prairieHillsTile;
    public Tile grassHillsTile;
    public Tile tundraHillsTile;
    public Tile desertHillsTile;
    public Tile snowHillsTile;
    [Header("Features")]
    public Tile woodsTile;
    public Tile floodplainsTile;
    public Tile marshTile;
    public Tile rainforestTile;
    public Tile oasisTile;
    [Header("Terrain")]
    public Tile mountain;
    [Header("Rivers")]
    public GameObject riversParent;
    public GameObject riverSegment;

    // Start is called before the first frame update
    void Start()
    {
        RenderWorld();
    }

    private void RenderWorld()
    {
        World gameWorld = gm.GetWorld();

        DrawTilemaps(gameWorld);
        gameWorld.SetTileAdjacency();
    }

    public void DrawTilemaps(World world)
    {
        for (int x = 0; x < world.GetLength(); x++)
        {
            for (int y = 0; y < world.GetHeight(); y++)
            {
                GameTile currTile = world.GetTile(x, y);
                
                // Grid can be switched, check if Height is Width
                double tileHeight = 0.95f;
                float tileWidth = 1f;
                float edge = 4f;
                
                // Tile Position Variables - Jason knows how they work don't ask me.
                float bigX = tileWidth * x * .75f;
                float bigY = (float)(y * tileHeight + (tileHeight / 2) * (x % 2));
                

                /* Render Base Tiles */
                // Plains
                if (currTile.GetBiome() == 1)
                {
                    // Hills
                    if (currTile.GetTerrain() == 1)
                    {
                        baseTilemap.SetTile(new Vector3Int(y, x, 0), prairieHillsTile);
                    }
                    // Flat 
                    else
                    {
                        baseTilemap.SetTile(new Vector3Int(y, x, 0), prairieTile);
                    }
                    

                    //tile.color = new Color32(145, 158, 11, 255);

                }
                // Grassland
                else if (currTile.GetBiome() == 2)
                {
                    // Hills
                    if (currTile.GetTerrain() == 1)
                    {
                        baseTilemap.SetTile(new Vector3Int(y, x, 0), grassHillsTile);
                    }
                    // Flat 
                    else
                    {
                        baseTilemap.SetTile(new Vector3Int(y, x, 0), grassTile);
                    }

                    //tile.color = new Color32(92, 128, 82, 255);
                }
                // Tundra
                else if (currTile.GetBiome() == 3)
                {
                    // Hills
                    if (currTile.GetTerrain() == 1)
                    {
                        baseTilemap.SetTile(new Vector3Int(y, x, 0), tundraHillsTile);
                    }
                    // Flat 
                    else
                    {
                        baseTilemap.SetTile(new Vector3Int(y, x, 0), tundraTile);
                    }

                    //tile.color = new Color32(144, 158, 141, 255);
                }
                // Desert
                else if (currTile.GetBiome() == 4)
                {
                    // Hills
                    if (currTile.GetTerrain() == 1)
                    {
                        baseTilemap.SetTile(new Vector3Int(y, x, 0), desertHillsTile);
                    }
                    // Flat 
                    else
                    {
                        baseTilemap.SetTile(new Vector3Int(y, x, 0), desertTile);
                    }

                    //tile.color = new Color32(255, 217, 112, 255);
                }
                // Snow
                else if (currTile.GetBiome() == 5)
                {
                    // Hills
                    if (world.GetTile(x, y).GetTerrain() == 1)
                    {
                        baseTilemap.SetTile(new Vector3Int(y, x, 0), snowHillsTile);
                    }
                    // Flat 
                    else
                    {
                        baseTilemap.SetTile(new Vector3Int(y, x, 0), snowTile);
                    }

                    //tile.color = Color.white;
                }
                else if (currTile.GetBiome() == 6)
                {
                    baseTilemap.SetTile(new Vector3Int(y, x, 0), coastTile);
                    //tile.color = new Color32(110, 187, 255, 255);
                }
                else if (currTile.GetBiome() == 7)
                {
                    baseTilemap.SetTile(new Vector3Int(y, x, 0), oceanTile);
                    //tile.color = new Color32(20, 102, 184, 255);
                } else if (currTile.GetBiome() == 8)
                {
                    baseTilemap.SetTile(new Vector3Int(y, x, 0), lakeTile);
                }
                
                
                /* Render Terrain */
                // Mountains
                if (currTile.GetTerrain() == 2)
                {
                    terrainTilemap.SetTile(new Vector3Int(y, x, 0), mountain);
                }
                

                /* Render Rivers */
                if (currTile.GetFreshWaterAccess())
                {
                    // For testing FreshWaterAccess
                    /*baseTilemap.SetTile(new Vector3Int(y, x, 0), tile);
                    tile.color = Color.white;*/

                    for (int index = 0; index < 6; index++)
                    {
                        if (currTile.GetRiverEdge(index))
                        {
                            // Instiate Vector3 for Position at Formula for River Position
                            Vector3 riverPosition = new Vector3((float)(bigX +
                                                                        Math.Pow(-1f,
                                                                            Math.Pow(0f,
                                                                                (5f - index) * (4f - index))) *
                                                                        Math.Pow(0f, Math.Pow(0f, index % 3f)) *
                                                                        tileWidth * 3 / 8),
                                (float)(bigY + Math.Pow(-1f,
                                        Math.Pow(0f, Math.Abs((index - 2f) * (index - 3f) * (index - 4f)))) *
                                    (tileHeight / 4f + tileHeight / 4f *
                                        Math.Abs(Math.Pow(0f, Math.Pow(0f, index % 3f)) - 1f))),
                                0f);
                            // Declare riverRotation variable
                            Quaternion riverRotation;

                            if (index == 1 || index == 4)
                            {
                                // Set the rotation of the river based on it's edge
                                riverRotation = Quaternion.Euler(0f, 0f, -63f);
                            }
                            else if (index == 5 || index == 2)
                            {
                                // Set the rotation of the river based on it's edge
                                riverRotation = Quaternion.Euler(0f, 0f, 63f);
                            }
                            else
                            {
                                // Set the rotation of the river based on it's edge
                                riverRotation = Quaternion.Euler(0f, 0f, 0f);
                            }
                            
                            // Instantiate as part of the Rivers obj in order to not clog up hierarchy
                            GameObject riverPiece = Instantiate(riverSegment, riverPosition, riverRotation);
                            riverPiece.transform.SetParent(riversParent.transform);
                            
                        }
                    }
                }
                
                /* Render Features */
                if (currTile.GetFeature() == 1)
                {
                    featureTilemap.SetTile(new Vector3Int(y, x, 0), woodsTile);
                } else if (currTile.GetFeature() == 2)
                {
                    featureTilemap.SetTile(new Vector3Int(y, x, 0), floodplainsTile);
                } else if (currTile.GetFeature() == 3)
                {
                    featureTilemap.SetTile(new Vector3Int(y, x, 0), marshTile);
                } else if (currTile.GetFeature() == 4)
                {
                    featureTilemap.SetTile(new Vector3Int(y, x, 0), rainforestTile);
                } else if (currTile.GetFeature() == 5)
                {
                    featureTilemap.SetTile(new Vector3Int(y, x, 0), oasisTile);
                }
            }
        }
    }


}
