using System;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class Director : MonoBehaviour
{
    // Serialized Variables
    [Header("Game Manager")] 
    public GameManager gm;
    [Header("Cameras")] public Camera mainCam;
    [Header("Canvases")] 
    public GameObject menuCanvas;
    public GameObject guiCanvas;
    public GameObject saveCanvas;
    public TMP_InputField saveIF;
    [Header("Owner")] 
    public Civilization civilization;
    [Header("Tilemaps")] 
    public Tilemap baseTilemap;
    public Tilemap terrainTilemap;
    public Tilemap featureTilemap;
    public Tilemap unitTilemap;
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
    [Header("Settlements")] 
    public Tile village;
    [Header("Units")] 
    public Tile warrior;

    // Instance Attributes
    private bool _needsDirection;
    
    // Camera Values
    private const float dragSpeed = 10f;
    private const float edgeScrollSpeed = 10f;
    private const float edgeScrollThreshold = 10f;
    private const float smoothTime = 0.5f;
    
    private const float zoomSpeed = 2f;
    private const float minZoom = 2f;
    private const float maxZoom = 15f;
    private Vector3 dragOrign;
    private Vector3 targetPosition;
    private float camHeight, camWidth;

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();

        // If Game is Single player
        if (gm.game.singlePlayer)
        {
            foreach (Civilization civ in gm.game.civilizations)
            {
                if (!civ.IsNPC)
                {
                    civilization = civ;
                }
            }
        }

        RenderGame();

        _needsDirection = true;
    }
    // Update is called every frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuCanvas.SetActive(!menuCanvas.activeSelf);
            saveCanvas.SetActive(false);
        }
        
        CameraControl();
    }

    /* Render the Game. */
    private void RenderGame()
    {
        World gameWorld = gm.game.world;
        RenderTilemaps(gameWorld);
        RenderSettlementUI(gameWorld);

        /* Renders all Tilemaps */
        void RenderTilemaps(World world)
        {
            // Grid Dimensions
            double tileHeight = 0.95f;
            double tileWidth = 1f;

            for (int x = 0; x < world.GetLength(); x++)
            {
                for (int y = 0; y < world.GetHeight(); y++)
                {
                    GameTile currTile = world.GetTile(x, y);

                    // Tile Position Variables - Jason knows how they work don't ask me.
                    double bigX = tileWidth * x * .75f;
                    double bigY = (float)(y * tileHeight + (tileHeight / 2) * (x % 2));

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
                    // Coast
                    else if (currTile.GetBiome() == 6)
                    {
                        baseTilemap.SetTile(new Vector3Int(y, x, 0), coastTile);
                        //tile.color = new Color32(110, 187, 255, 255);
                    }
                    // Ocean
                    else if (currTile.GetBiome() == 7)
                    {
                        baseTilemap.SetTile(new Vector3Int(y, x, 0), oceanTile);
                        //tile.color = new Color32(20, 102, 184, 255);
                    }
                    // Lake
                    else if (currTile.GetBiome() == 8)
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
                    // Woods
                    if (currTile.GetFeature() == 1)
                    {
                        featureTilemap.SetTile(new Vector3Int(y, x, 0), woodsTile);
                    }
                    // Floodplains
                    else if (currTile.GetFeature() == 2)
                    {
                        featureTilemap.SetTile(new Vector3Int(y, x, 0), floodplainsTile);
                    }
                    // Marshes
                    else if (currTile.GetFeature() == 3)
                    {
                        featureTilemap.SetTile(new Vector3Int(y, x, 0), marshTile);
                    }
                    // Rainforest
                    else if (currTile.GetFeature() == 4)
                    {
                        featureTilemap.SetTile(new Vector3Int(y, x, 0), rainforestTile);
                    }
                    // Oasis
                    else if (currTile.GetFeature() == 5)
                    {
                        featureTilemap.SetTile(new Vector3Int(y, x, 0), oasisTile);
                    }

                    /* Render Settlements */
                    if (currTile.GetSettlement() is not null)
                    {
                        featureTilemap.SetTile(new Vector3Int(y, x, 0), village);
                    }
                }
            }
        }
        
        /* Renders Settlement UI above Settlement Tiles */
        void RenderSettlementUI(World world)
        {
            // Grid Dimensions
            double tileHeight = 0.95f;
            double tileWidth = 1f;

            for (int x = 0; x < world.GetLength(); x++)
            {
                for (int y = 0; y < world.GetHeight(); y++)
                {
                    // Tile Position Variables - Jason knows how they work don't ask me.
                    double bigX = tileWidth * x * .75f;
                    double bigY = (float)(y * tileHeight + (tileHeight / 2) * (x % 2));

                    GameTile currTile = world.GetTile(x, y);

                    if (currTile.GetSettlement() is not null)
                    {

                    }

                }
            }
        }
    }

    // Open the Save Game Menu
    public void OpenSaveGameCanvas()
    {
        saveCanvas.SetActive(true);
    }

    // Send Save Game Command to Game
    public void SendSaveToGm()
    {
        gm.SaveGame(saveIF.text);
    }

    // Load Main Menu Scene
    public void QuitToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    /* Calls all the other Camera methods. */
    void CameraControl()
    {
        DragCamera();
        HandleZoom();
    }

    /* Moves the Camera by dragging the world with Left-Click. */
    void DragCamera()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrign = mainCam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 difference = dragOrign - mainCam.ScreenToWorldPoint(Input.mousePosition);
            mainCam.transform.position += difference * (dragSpeed * Time.deltaTime);
        }
    }
    
    /* Changes the size of the camera with Mouse Scroll wheel. */
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        mainCam.orthographicSize -= scroll * zoomSpeed;
        mainCam.orthographicSize = Mathf.Clamp(mainCam.orthographicSize, minZoom, maxZoom);
    }



}
