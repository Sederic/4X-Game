using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
// Serialized Variables
    [Header("Game Manager")] private GameManager gm;
    [Header("Cameras")] public Camera mainCam;
    [Header("Canvases")] public Canvas worldCanvas;
    public GameObject menuCanvas;
    public GameObject guiCanvas;
    public GameObject saveCanvas;
    public GameObject unitWindowCanvas;
    public TMP_InputField saveIF;
    public GameObject settlementWindowCanvas;
    [Header("Owner")] public Civilization civilization;
    [Header("Tilemaps")] public Tilemap baseTilemap;
    public Tilemap hillsTilemap;
    public Tilemap mountainTilemap;
    public Tilemap featureTilemap;
    public Tilemap shadingTilemap;
    public Tilemap visibilityTilemap;
    [Header("Flat Tiles")] public Tile tile;
    public Tile prairieTile;
    public Tile grassTile;
    public Tile tundraTile;
    public Tile desertTile;
    public Tile oceanTile;
    public Tile coastTile;
    public Tile snowTile;
    [Header("Hills Tiles")] public Tile prairieHillsTile;
    public Tile grassHillsTile;
    public Tile tundraHillsTile;
    public Tile desertHillsTile;
    public Tile snowHillsTile;
    [Header("Features")] public Tile woodsTile_grassland;
	public Tile woodsTile_plains;
    public Tile floodplainsTile;
    public Tile marshTile;
    public Tile rainforestTile;
    public Tile oasisTile;
    public Tile lakeTile;
    [Header("Terrain")] 
    public Tile mountainTile;
    [Header("Shading Tiles")] 
    public Tile highlight;
    public Tile unexplored;
    public Tile darkened;
    [Header("Rivers")] 
    public GameObject riversParent;
    public GameObject riverSegment;
    [Header("Settlements")] public Tile village;
    [Header("UI Prefabs")] public GameObject settlementUI;
    public GameObject settlementWindow;
    public GameObject territoryParent;
    public GameObject territoryLines;
    public TMP_Text endTurnText;
    [Header("Unit Selection")] 
    public Unit selectedUnit;
    public GameObject selectedUnitPrefab;
    public TMP_Text selectedUnitName;
    public TMP_Text selectedUnitMovementPoints;
    public GameObject unitPrefab;
    public GameObject unitsParent;
    public Image moveButton;
    public Image passButton;
    public Image attackButton;
    public Image campButton;
    public Image settleButton;

    // Private Instance Attributes
    // private bool _needsDirection;
    // private Dictionary<GameTile, GameObject> settlementUIs = new Dictionary<GameTile, GameObject>();
    // private Dictionary<Unit, GameObject> Units = new Dictionary<Unit, GameObject>();
    // private HashSet<Point> HighlightedTiles = new HashSet<Point>();
    // private HashSet<Point> VisibleTiles = new HashSet<Point>();

    // Camera Values
    public bool _zoomedIn;
    private Vector3 _prevPos;
    private float _prevSize;

    // Camera Constants 
    private const float dragSpeed = 10f;
    private const float zoomSpeed = 2f;
    private const float minZoom = 2f;

    private const float maxZoom = 15f;

    // Grid Dimensions
    private const double tileHeight = 0.95f;
    private const double tileWidth = 1f;

    // Camera References
    private Vector3 dragOrign;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        // Toggle vision on/off for testing
        if (Input.GetKeyDown(KeyCode.Period))
        {
            visibilityTilemap.gameObject.SetActive(!visibilityTilemap.gameObject.activeSelf);
        }

        CameraControl();
    }

    public void SetUpWorldCanvas()
    {
        // Set exact dimensions based on editor observations
        RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
        canvasRect.transform.position = new Vector3(worldCanvas.transform.position.x / 2,
            worldCanvas.transform.position.y / 2, worldCanvas.transform.position.z);
        canvasRect.sizeDelta = new Vector2(74.2f, 47);
        canvasRect.pivot = new Vector2(74.2f / 2, 47f / 2);
    }

    public void RenderGame()
    {
        RenderTilemaps();
        // RenderSettlementUI(gameWorld);
        // RenderTerritoryLines();
        RenderUnits();
        RenderPlayerVision(true);

        // Start at Settler
        List<Unit> units = Game.Instance.civ.Units;
        Unit settler = (units[0].unitType == UnitType.Settler) ? units[0] : units[1];
        SelectUnit(settler);
        PositionCamera(settler.tile);
    }


    /* ------------------------------------------------
        TILEMAPS
    ------------------------------------------------ */
    public void RenderTilemaps()
    {
        GameTile[][] tiles = Game.Instance.world.tiles;
        for (int x=0; x<tiles.Length; x++)
        {
            for (int y=0; y<tiles[0].Length; y++)
            {
                GameTile tile = tiles[x][y];
                Biome biome = tile.biome;
                Terrain terrain = tile.terrain;

                // Tile Position Variables - Jason knows how they work don't ask me.
                double bigX = tileWidth * y * .75f;
                double bigY = (float)(x * tileHeight + (tileHeight / 2) * (y % 2));

                Vector3Int tilePosition = new Vector3Int(-x, y, 0);

                /* Render Base Tiles */
                switch (biome)
                {
                    case Biome.Plains:
                        baseTilemap.SetTile(tilePosition, prairieTile);
                        if (terrain == Terrain.Hill) hillsTilemap.SetTile(tilePosition, prairieHillsTile);
                        break;
                    
                    case Biome.Grassland:
                        baseTilemap.SetTile(tilePosition, grassTile);
                        if (terrain == Terrain.Hill) hillsTilemap.SetTile(tilePosition, grassHillsTile);
                        break;

                    case Biome.Tundra:
                        baseTilemap.SetTile(tilePosition, tundraTile);
                        if (terrain == Terrain.Hill) hillsTilemap.SetTile(tilePosition, tundraHillsTile);
                        break;

                    case Biome.Desert:
                        baseTilemap.SetTile(tilePosition, desertTile);
                        if (terrain == Terrain.Hill) hillsTilemap.SetTile(tilePosition, desertHillsTile);
                        break;

                    case Biome.Snow:
                        baseTilemap.SetTile(tilePosition, snowTile);
                        if (terrain == Terrain.Hill) hillsTilemap.SetTile(tilePosition, snowHillsTile);
                        break;

                    case Biome.Coast:
                        baseTilemap.SetTile(tilePosition, coastTile);
                        break;

                    case Biome.Ocean:
                        baseTilemap.SetTile(tilePosition, oceanTile);
                        break;
                }

                if (terrain == Terrain.Mountain)
                {
                    mountainTilemap.SetTile(tilePosition, mountainTile);
                }

                /* Render Features */
                switch (tile.feature)
                {
                    case Feature.Woods:
                        featureTilemap.SetTile(tilePosition, (biome == Biome.Plains) ? woodsTile_plains : woodsTile_grassland);
                        break;

                    case Feature.Floodplains:
                        featureTilemap.SetTile(tilePosition, floodplainsTile);
                        break;

                    case Feature.Marsh:
                        featureTilemap.SetTile(tilePosition, marshTile);
                        break;

                    case Feature.Rainforest:
                        featureTilemap.SetTile(tilePosition, rainforestTile);
                        break;

                    case Feature.Oasis:
                        featureTilemap.SetTile(tilePosition, oasisTile);
                        break;

                    case Feature.Lake:
                        featureTilemap.SetTile(tilePosition, lakeTile);
                        break;
                }
            }
        }

        /* Rivers */
        for (int x=0; x<tiles.Length; x++)
        {
            for (int y=0; y<tiles[0].Length; y++)
            {
                GameTile tile = tiles[x][y];
                if (tile.hasFreshWater || true)
                {
                    for (int i=0; i<3; i++) // only the 0-2 for each tile
                    {
                        // tile.riverEdges[i] 
                        if (true)
                        {
                            float[] angles = { 90f, 27f, -27f };
                            Vector3 direction(float distance, float angle) {
                                float radians = Mathf.Deg2Rad * angle;
                                return distance * new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0);
                            }

                            // // Declare riverPosition variable
                            // Vector3[] directions = new Vector3[]
                            // {
                            //     new Vector3(0, 1, 0),  // i=0 → Up
                            //     new Vector3(Mathf.Cos(Mathf.Deg2Rad * 27), Mathf.Sin(Mathf.Deg2Rad * 27), 0) // i=1 → 60° (top right)
                            //         + new Vector3()
                            //     new Vector3(Mathf.Cos(Mathf.Deg2Rad * -27), Mathf.Sin(Mathf.Deg2Rad * -27), 0) // i=2 → 120° (bottom right)
                            // };
                            // float[] displacements = { 0.45f, 0.45f, 0.45f };
                            // Vector3 riverPosition = tile.Pos + (directions[i] * displacements[i]);

                            // // Declare riverRotation variable
                            // float[] rotations = { 90f, 120f, 150f };
                            // Quaternion riverRotation = Quaternion.Euler(0f, 0f, rotations[i % 3]);

                            float angle = angles[i]; // Hexagon angles (0, 60, 120, 180, 240, 300)
                            float radians = Mathf.Deg2Rad * angle; // Convert to radians

                            float radius = 0.45f;

                            Vector3[] displacements = new Vector3[]
                            {
                                new Vector3(0, 0, 0),
                                new Vector3(-0.035f, 0.035f, 0),
                                new Vector3(-0.035f, -0.035f, 0)
                            };

                            float[] distances = { 0.47f, 0.45f, 0.45f };

                            // Calculate position using trigonometry
                            Vector3 position = tile.Pos + new Vector3(
                                distances[i] * Mathf.Cos(radians),
                                distances[i] * Mathf.Sin(radians),
                                0
                            ) + displacements[i];

                            // Calculate rotation (perpendicular to edge)
                            Quaternion rotation = Quaternion.Euler(0, 0, angle + 90);

                            // Instantiate as part of the Rivers obj in order to not clog up hierarchy
                            // GameObject riverPiece = Instantiate(riverSegment, riverPosition, riverRotation);
                            GameObject riverPiece = Instantiate(riverSegment, position, rotation);
                            riverPiece.transform.SetParent(riversParent.transform);
                        }
                    }
                }
            }
        }
    }

    /* ------------------------------------------------
        VISIBILITY
    ------------------------------------------------ */
    void RenderPlayerVision(bool updateAll=false)
    {
        // Tile[] shadingArray = { darkened, unexplored, null };
        Tile[] shadingArray = { unexplored, unexplored, null };
        GameTile[][] tiles = Game.Instance.tiles;
        Civilization civ = Game.Instance.civ;
        civ.UpdateTileVisibilityList();

        if (updateAll)
        {
            for (int x=0; x<tiles.Length; x++)
            {
                for (int y=0; y<tiles[0].Length; y++)
                {
                    visibilityTilemap.SetTile(tiles[x][y].TilemapLoc(), shadingArray[civ.TileVisibility[x][y]]);
                }
            }
        }
        else
        {
            foreach (Point p in civ.UpdatedTileVisibility)
            {
                visibilityTilemap.SetTile(GameTile.TilemapLoc(p), shadingArray[civ.TileVisibility[p.x][p.y]]);
            }
            civ.ClearTileVisibilityList();
        }
    }


    /* ------------------------------------------------
        UNITS
    ------------------------------------------------ */
    public void RenderUnits()
    {
        GameTile[][] tiles = Game.Instance.world.tiles;
        for (int x=0; x<tiles.Length; x++)
        {
            for (int y=0; y<tiles[0].Length; y++)
            {
                Unit unit = tiles[x][y].unit;
                if (unit is not null)
                {
                    if (unit.unitPrefab is null)
                    {
                        GameObject unitOBJ = Instantiate(unitPrefab, unitsParent.transform);
                        unit.SetUnitPrefab(unitOBJ.GetComponent<UnitPrefab>(), this);
                    }
                    unit.unitPrefab.transform.position = tiles[x][y].Pos;
                }
            }
        }
    }

    /* ------------------------------------------------
        CAMERA CONTROL
    ------------------------------------------------ */
    public void PositionCamera(GameTile tile) {
        mainCam.transform.position = tile.PosCamera();
        mainCam.orthographicSize = 4f;
    }

    /* Calls all the other Camera methods. */
    void CameraControl()
    {
        DragCamera();
        HandleZoom();
        
        /* Moves the Camera by dragging the world with Left-Click. */
        void DragCamera()
        {
            // When left mouse is clicked
            if (Input.GetMouseButtonDown(0))
            {
                dragOrign = mainCam.ScreenToWorldPoint(Input.mousePosition);
            }

            // While left mouse is held
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

    /* Selects the Unit in the parameter, open's the unit window. */
    public void SelectUnit(Unit unit)
    {
        // // Assign Selected Unit
        // selectedUnit = unit;
        
        // // Set UnitWindow Active and Update its text
        // OpenUnitWindow();

        // if (!selectedUnit._passing && !selectedUnit._camping)
        // {
        //     // Display that Unit's possible moves through the Shading Tilemap (highlight tiles)
        //     DisplayPossibleMoves(unit);
        // }
    }
}