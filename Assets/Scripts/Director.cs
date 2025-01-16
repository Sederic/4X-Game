using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Director : MonoBehaviour
{
    // Serialized Variables
    [Header("Game Manager")] public GameManager gm;
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
    public Tile mountain;
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
    private bool _needsDirection;
    private Dictionary<GameTile, GameObject> settlementUIs = new Dictionary<GameTile, GameObject>();
    private Dictionary<Unit, GameObject> units = new Dictionary<Unit, GameObject>();
    private HashSet<Point> highlightedTiles = new HashSet<Point>();
    private HashSet<Point> visibleTiles = new HashSet<Point>();

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

    // Start is called before the first frame update
    void Start()
    {
        // Find GM objects
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

        // Situate the World Canvas for UI (based on tilemap size)
        SetUpWorldCanvas();

        // Render the game (once)
        RenderGame();
        
        // Position Camera over starting position (Units)
        GameTile tile = civilization._units[0]._gameTile;
        Vector3 startPos = baseTilemap.CellToWorld(new Vector3Int(tile.GetYPos(), tile.GetXPos(), -1));
        mainCam.transform.position = startPos;
        mainCam.orthographicSize = 4f;
    }

    // Update is called every frame
    void Update()
    {
        // Pressing Esc - Deselected Unit
        if (Input.GetKeyDown(KeyCode.Escape) && !_zoomedIn)
        {
            // Toggle Unit Selected
            if (unitWindowCanvas.activeSelf)
            {
                CloseUnitWindow();
                return;
            }

            // Toggle Menu Canvas
            menuCanvas.SetActive(!menuCanvas.activeSelf);
            // Ensure that save canvas starts off
            saveCanvas.SetActive(false);
            return;
        }

        // Toggle vision on/off for testing
        if (Input.GetKeyDown(KeyCode.Period))
        {
            visibilityTilemap.gameObject.SetActive(!visibilityTilemap.gameObject.activeSelf);
        }

        if (!_zoomedIn)
        {
            CameraControl();
        }

        // Unit Movement
        MoveSelectedUnit();
    }

    private void MoveSelectedUnit()
    {
        if (selectedUnit is not null)
        {
            if (Input.GetMouseButtonDown(1)) // 1 - Right Mouse button
            {
                Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                
                Vector3Int cellPosition = shadingTilemap.WorldToCell(mousePos);

                // If that location is highlighted
                if (shadingTilemap.HasTile(cellPosition))
                {
                    // Move Unit in GM
                    gm.MoveUnit(selectedUnit, new Point(cellPosition.y, cellPosition.x));
                    
                    // Reposition Selected Unit prefab
                    RenderUnits();
                    
                    // Reset previous moves
                    RemovePossibleMoves();
                    
                    // Display New Possible Moves
                    DisplayPossibleMoves(selectedUnit);
                    
                    // Update Unit Window Values
                    OpenUnitWindow();

                    // If Selected Unit can no longer move (out of movement points)
                    if (!selectedUnit.canMove())
                    {
                        RemovePossibleMoves();
                    }
                    
                    // Update End Turn Button in case player can now end turn
                    UpdateEndTurnButton();
                    
                    // Render new visible tiles
                    RenderPlayerVision();
                }
            }
        }
    }

    public void OpenUnitWindow()
    {
        // Activate the Unit Window Canvas
        unitWindowCanvas.SetActive(true);

        // Manage Buttons
        Color32 deactiveColor = new Color32(50, 50, 50, 200);
        Color32 activeColor = new Color32(200, 200, 200, 255);
        
        if (selectedUnit._camping)
        {
            campButton.color = Color.white;
        }
        else
        {
            if (selectedUnit._currMP <= 0)
            {
                campButton.color = deactiveColor;
            }
            else
            {
                campButton.color = activeColor;
            }
        }

        if (selectedUnit._currMP <= 0 || selectedUnit._camping)
        {
            moveButton.color = deactiveColor;
            passButton.color = deactiveColor;
            attackButton.color = deactiveColor;
            settleButton.color = deactiveColor;

        }
        else
        {
            moveButton.color = activeColor;
            passButton.color = activeColor;
            attackButton.color = activeColor;
            settleButton.color = activeColor;
        }
        
        // Unique Settler Button check
        if (selectedUnit._name == "Settler")
        {
            // Activate Settle Button
            settleButton.gameObject.SetActive(true);
        }
        else
        {
            // Deactive Settle Button
            settleButton.gameObject.SetActive(false);
        }
        
        // Name
        selectedUnitName.text = selectedUnit._name;
        // Movement Points
        selectedUnitMovementPoints.text = 
            "Movement Points: " + selectedUnit._currMP + "/" + selectedUnit._baseMP + "\n" +
            "Combat Strength: " + selectedUnit._combatStrength + "\n" +
            "Supplies: " + selectedUnit._supplies + "\n" + 
            "Health: " + selectedUnit._health;
    }

    public void CloseUnitWindow()
    {
        // Deactive it
        unitWindowCanvas.SetActive(false);
        // Unselect Unit
        selectedUnit = null;
        selectedUnitPrefab = null;
        //Remove possibleMoves() highlighted tiles
        RemovePossibleMoves();
    }
    
    public void ToggleSettlementWindow(Settlement settlement)
    {
        // Tell the director we are zoom in
        _zoomedIn = true;

        // Turn off the Base Canvas
        guiCanvas.SetActive(false);
        
        // Store previous camera size and positions
        _prevPos = mainCam.transform.position;
        _prevSize = mainCam.orthographicSize;
        
        // Deactivate the settlementUI
        settlementUIs[settlement.GetTile()].SetActive(false);
        
        // Instantiate Settlement Window Canvas
        settlementWindow = Instantiate(settlementWindowCanvas);
        
        // Ensure it is active
        settlementWindow.SetActive(true);
    
        // Give Settlement Window Script info necessary references
        settlementWindow.GetComponent<SettlementWindow>()._settlement = settlement;
        settlementWindow.GetComponent<SettlementWindow>().tilemap = baseTilemap;
        settlementWindow.GetComponent<SettlementWindow>()._worldCanvas = worldCanvas;
        settlementWindow.GetComponent<SettlementWindow>().settlementName.text = settlement.GetName();
        
        ZoomCameraAtSettlement();
        
        void ZoomCameraAtSettlement()
        {
            // Settlement's Game Tile
            GameTile tile = settlement.GetTile();
            
            // Settlement's position
            Vector3 settlementPosition = baseTilemap.CellToWorld(new Vector3Int(tile.GetYPos(), tile.GetXPos(), -1));
            
            // Move camera into the Settlement Window view
            mainCam.transform.position = settlementPosition;
            mainCam.orthographicSize = 4f;
        }
    }

    public void RestoreCameraState()
    {
        mainCam.transform.position = _prevPos;
        mainCam.orthographicSize = _prevSize;

        foreach (GameObject uiPrefab in settlementUIs.Values)
        {
            uiPrefab.SetActive(true);
        }
        
        RenderSettlementUI(gm.game.world);
    }

    // Load Main Menu Scene
    public void QuitToMainMenu()
    {
        // SceneManager.UnloadSceneAsync(1);

        // // Clear all Tilemaps
        // baseTilemap.ClearAllTiles();
        // hillsTilemap.ClearAllTiles();
        // mountainTilemap.ClearAllTiles();
        // featureTilemap.ClearAllTiles();
        // shadingTilemap.ClearAllTiles();
        // visibilityTilemap.ClearAllTiles();

        // // Clear dictionaries or any tracking objects
        // settlementUIs.Clear();
        // units.Clear();
        // highlightedTiles.Clear();
        // visibleTiles.Clear();

        // // Disable unnecessary objects
        // menuCanvas.SetActive(false);
        // guiCanvas.SetActive(false);
        // saveCanvas.SetActive(false);
        // unitWindowCanvas.SetActive(false);
        // settlementWindowCanvas.SetActive(false);

        // // Clear global/static variables for garbage collection
        GameTile.ClearGameTileRegistry();

        SceneManager.LoadScene(0);
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

        saveCanvas.SetActive(false);
    }

    public void SendEndTurnToGM()
    {
        // Make sure Settlements have a project
        foreach (Settlement settlement in civilization.GetSettlements())
        {
            if (settlement._currentCityProject is null)
            {
                // Change Button Text
                UpdateEndTurnButton();
                
                // Move Camera to Settlement
                GameTile tile = settlement.GetTile();
                Vector3 settlementPos = baseTilemap.CellToWorld(new Vector3Int(tile.GetYPos(), tile.GetXPos(), -1));

                // Move camera to Settlement position, if it's already there, open Settlement Window
                if (mainCam.transform.position == settlementPos)
                {
                    ToggleSettlementWindow(settlement);
                    return;
                }
                else
                {
                    mainCam.transform.position = settlementPos;
                    mainCam.orthographicSize = 5f;
                    return;
                }
            }
        }

        // Make sure Units have been moved
        foreach (Unit unit in civilization._units)
        {
            if (!unit._passing && unit._currMP > 0 && !unit._camping)
            {
                // Change Button Text
                UpdateEndTurnButton();
                
                // Move Camera to Unit
                GameTile tile = unit._gameTile;
                Vector3 unitPos = baseTilemap.CellToWorld(new Vector3Int(tile.GetYPos(), tile.GetXPos(), -1));
                mainCam.transform.position = unitPos;
                mainCam.orthographicSize = 5f;
                
                // Select the unit
                SelectUnit(unit);
                return;
            }
        }
        
        gm.EndTurn();

        // Update Settlement UIs
        foreach (GameTile tile in settlementUIs.Keys)
        {
            UpdateUIFields(settlementUIs[tile], tile.GetSettlement());
        }

        // Update Selected Unit
        if (selectedUnit != null && !selectedUnit._camping)
        {
            DisplayPossibleMoves(selectedUnit);
            OpenUnitWindow();
        }
        
        RenderUnits();
        
        RenderTerritoryLines();
        
        // Redirect player to new moves at the beginning of new turn
        UpdateEndTurnButton();

        if (selectedUnit is not null)
        {
            // Update Selected Unit Window
            OpenUnitWindow();
        }
    }
    
    /* Selects the Unit in the parameter, open's the unit window. */
    public void SelectUnit(Unit unit)
    {
        // Assign Selected Unit
        selectedUnit = unit;
        
        // Set UnitWindow Active and Update its text
        OpenUnitWindow();

        if (!selectedUnit._passing && !selectedUnit._camping)
        {
            // Display that Unit's possible moves through the Shading Tilemap (highlight tiles)
            DisplayPossibleMoves(unit);
        }
    }

    public void StageSelectedUnitForMovement()
    {
        if (selectedUnit._currMP > 0)
        {
            selectedUnit._passing = false;
            selectedUnit._camping = false;
            DisplayPossibleMoves(selectedUnit);
        }
    }

    /* Resizes World Canvas to match Tilemap size (should work with different world sizes or grid/cell sizes) */
    void SetUpWorldCanvas()
    {
        // Set exact dimensions based on editor observations
        RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
        canvasRect.transform.position = new Vector3(worldCanvas.transform.position.x / 2,
            worldCanvas.transform.position.y / 2, worldCanvas.transform.position.z);
        canvasRect.sizeDelta = new Vector2(74.2f, 47);
        canvasRect.pivot = new Vector2(74.2f / 2, 47f / 2);

        // float tileWidth = baseTilemap.cellSize.x;
        // float tileHeight = baseTilemap.cellSize.y;
        // float tileCountX = gm.game.world.GetLength();
        // float tileCountY = gm.game.world.GetHeight();
        //
        // // Calculate the canvas dimensions based on tilemap size and tile dimensions
        // float canvasWidth = tileWidth * (tileCountX * .75f + 0.25f);
        // float canvasHeight = tileHeight * (tileCountY + 0.5f);
        //
        // // Adjust the RectTransform of the Canvas to match these dimensions
        // RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
        // canvasRect.sizeDelta = new Vector2(canvasWidth, canvasHeight);
        //
        // // Position the Canvas in the center of the tilemap.
        // float offsetX = canvasWidth / 2f;
        // float offsetY = canvasHeight / 2f;
        // canvasRect.position = new Vector3(offsetX, offsetY, 0);
    }

    /* Render everything in the Game. */
    void RenderGame()
    {
        World gameWorld = gm.game.world;
        RenderTilemaps(gameWorld);
        RenderSettlementUI(gameWorld);
        RenderTerritoryLines();
        RenderUnits();
        RenderPlayerVision();
    }

    /* Renders Settlement UI above Settlement Tiles */
    void RenderSettlementUI(World world)
    {
        for (int x = 0; x < world.GetLength(); x++)
        {
            for (int y = 0; y < world.GetHeight(); y++)
            {
                GameTile currTile = world.GetTile(x, y);

                // if a Tile has a Settlement
                if (currTile.GetSettlement() is not null)
                {
                    // If a prefab for that Tile's settlement doesn't exist, instantiate it.
                    if (!settlementUIs.ContainsKey(currTile))
                    {
                        // Instantiate UI Prefab
                        GameObject uiInstance = Instantiate(settlementUI, worldCanvas.transform);

                        // Store the Settlement within Prefab
                        SettlementUI ui = uiInstance.GetComponent<SettlementUI>();
                        ui.settlement = currTile.GetSettlement();

                        // Update the UI's fields
                        UpdateUIFields(uiInstance, currTile.GetSettlement());

                        // Add this UI Prefab to our active SettlementUIs
                        settlementUIs.Add(currTile, uiInstance);

                        // Get the tile's world position
                        Vector3Int cellPosition = new Vector3Int(y, x, 0);
                        Vector3 tileWorldPosition = baseTilemap.CellToWorld(cellPosition);

                        // Make the UI Instance appear slightly above the Tile
                        tileWorldPosition += new Vector3(0, 0.7f, 0);

                        // Set the position of the UI element
                        uiInstance.transform.position = tileWorldPosition;
                    }
                    else
                    {
                        // Update that UI prefab's text yields.
                        UpdateUIFields(settlementUIs[currTile], currTile.GetSettlement());
                    }
                }
            }
        }
    }
    
    // Helper method to update UI fields for a given settlement
    void UpdateUIFields(GameObject uiObject, Settlement settlement)
    {
        // Access TMP_Text Objs - These need to be access through a script transform. Find is too expensive 
        TMP_Text population = uiObject.transform.Find("Population Text").GetComponent<TMP_Text>();
        TMP_Text growth = uiObject.transform.Find("Growth Text").GetComponent<TMP_Text>();
        TMP_Text production = uiObject.transform.Find("Production Text").GetComponent<TMP_Text>();
        TMP_Text name = uiObject.transform.Find("Name Text").GetComponent<TMP_Text>();

        // Update the text fields with the settlement's data
        population.text = settlement.GetPopulation().ToString();
        growth.text = settlement.TurnsToGrow();
        production.text = settlement.TurnsToProduce();
        name.text = settlement.GetName();
    }

    /* Spawns Territory Lines on Tile Edges around Settlement's territory. */
    void RenderTerritoryLines()
    {
        foreach (Civilization civ in gm.game.civilizations)
        {
            foreach (Settlement settlement in civ.GetSettlements())
            {
                foreach (GameTile tile in settlement._territory)
                {
                    int edge = 0;
                    foreach (GameTile neighbor in tile.GetNeighbors())
                    {
                        if (neighbor is not null)
                        {
                            // Tile Position Variables - Jason knows how they work don't ask me.
                            double bigX = tileWidth * tile.GetXPos() * .75f;
                            double bigY =
                                (float)(tile.GetYPos() * tileHeight + (tileHeight / 2) * (tile.GetXPos() % 2));

                            // If this edge is at the end of the Settlement's territory 
                            if (!settlement._territory.Contains(neighbor))
                            {
                                // Put a Territory on that edge// Instiate Vector3 for Position at Formula for River Position
                                Vector3 territoryPosition = new Vector3((float)(bigX +
                                        Math.Pow(-1f,
                                            Math.Pow(0f,
                                                (5f - edge) * (4f - edge))) *
                                        Math.Pow(0f, Math.Pow(0f, edge % 3f)) *
                                        tileWidth * 3 / 8),
                                    (float)(bigY + Math.Pow(-1f,
                                            Math.Pow(0f, Math.Abs((edge - 2f) * (edge - 3f) * (edge - 4f)))) *
                                        (tileHeight / 4f + tileHeight / 4f *
                                            Math.Abs(Math.Pow(0f, Math.Pow(0f, edge % 3f)) - 1f))),
                                    0f);

                                // Declare riverRotation variable
                                Quaternion territoryRotation;

                                if (edge == 1 || edge == 4)
                                {
                                    // Set the rotation of the river based on it's edge
                                    territoryRotation = Quaternion.Euler(0f, 0f, -63f);
                                }
                                else if (edge == 5 || edge == 2)
                                {
                                    // Set the rotation of the river based on it's edge
                                    territoryRotation = Quaternion.Euler(0f, 0f, 63f);
                                }
                                else
                                {
                                    // Set the rotation of the river based on it's edge
                                    territoryRotation = Quaternion.Euler(0f, 0f, 0f);
                                }

                                // Instantiate as part of the Rivers obj in order to not clog up hierarchy
                                GameObject territoryLine =
                                    Instantiate(territoryLines, territoryPosition, territoryRotation);
                                territoryLine.GetComponent<TerritroyLine>().color = civ._color;
                                territoryLine.transform.SetParent(territoryParent.transform);
                            }
                        }

                        edge++;
                    }
                }
            }
        }
    }

    /* Renders all Tilemaps */
    void RenderTilemaps(World world)
    {
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
                    // Base Flat Tile
                    baseTilemap.SetTile(new Vector3Int(y, x, 0), prairieTile);

                    // Hills
                    if (currTile.GetTerrain() == 1)
                    {
                        hillsTilemap.SetTile(new Vector3Int(y, x, 0), prairieHillsTile);
                    }
                    //tile.color = new Color32(145, 158, 11, 255);
                }
                // Grassland
                else if (currTile.GetBiome() == 2)
                {
                    // Flat Tile
                    baseTilemap.SetTile(new Vector3Int(y, x, 0), grassTile);

                    // Hills
                    if (currTile.GetTerrain() == 1)
                    {
                        hillsTilemap.SetTile(new Vector3Int(y, x, 0), grassHillsTile);
                    }
                    //tile.color = new Color32(92, 128, 82, 255);
                }
                // Tundra
                else if (currTile.GetBiome() == 3)
                {
                    // Flat Tile
                    baseTilemap.SetTile(new Vector3Int(y, x, 0), tundraTile);

                    // Hills
                    if (currTile.GetTerrain() == 1)
                    {
                        hillsTilemap.SetTile(new Vector3Int(y, x, 0), tundraHillsTile);
                    }

                    //tile.color = new Color32(144, 158, 141, 255);
                }
                // Desert
                else if (currTile.GetBiome() == 4)
                {
                    // Flat Tile
                    baseTilemap.SetTile(new Vector3Int(y, x, 0), desertTile);

                    // Hills
                    if (currTile.GetTerrain() == 1)
                    {
                        hillsTilemap.SetTile(new Vector3Int(y, x, 0), desertHillsTile);
                    }

                    //tile.color = new Color32(255, 217, 112, 255);
                }
                // Snow
                else if (currTile.GetBiome() == 5)
                {
                    // Flat Tile
                    baseTilemap.SetTile(new Vector3Int(y, x, 0), snowTile);

                    // Hills
                    if (world.GetTile(x, y).GetTerrain() == 1)
                    {
                        hillsTilemap.SetTile(new Vector3Int(y, x, 0), snowHillsTile);
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

                // Mountains
                if (currTile.GetTerrain() == 2)
                {
                    mountainTilemap.SetTile(new Vector3Int(y, x, 0), mountain);
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
					if (currTile.GetBiome() == 1)
					{
                    	featureTilemap.SetTile(new Vector3Int(y, x, 0), woodsTile_plains);
					}
					else 
					{
						featureTilemap.SetTile(new Vector3Int(y, x, 0), woodsTile_grassland);
					}
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
                // Lake
                else if (currTile.GetFeature() == 6)
                {
                    featureTilemap.SetTile(new Vector3Int(y, x, 0), lakeTile);
                }

                /* Render Settlements */
                if (currTile.GetSettlement() is not null)
                {
                    featureTilemap.SetTile(new Vector3Int(y, x, 0), village);
                }
            }
        }
    }

    void RenderPlayerVision()
    {
        // Update player civilization's visibleTiles
        CalculateVision();

        for (int x = 0; x < gm.game.world.GetLength(); x++)
        {
            for (int y = 0; y < gm.game.world.GetHeight(); y++)
            {
                Point currentPoint = new Point(x, y);
                Vector3Int cellLocation = new Vector3Int(y, x, 0);

                // If the Tile hasn't been discovered
                if (!civilization.discoveredTiles.Contains(currentPoint))
                {
                    // Place unexplored tile there
                    visibilityTilemap.SetTile(cellLocation, unexplored);
                }
                else
                {
                    // If it is not visible
                    if (!visibleTiles.Contains(currentPoint))
                    {
                        visibilityTilemap.SetTile(cellLocation, darkened);
                    }
                    else
                    {
                        visibilityTilemap.SetTile(cellLocation, null);
                    }
                }
            }
        }
    }

    /* Render Units */
    void RenderUnits()
    {
        for (int x = 0; x < gm.game.world.GetLength(); x++)
        {
            for (int y = 0; y < gm.game.world.GetHeight(); y++)
            {
                GameTile currTile = gm.game.world.GetTile(x, y);

                // If the Tile has a Unit on it
                if (currTile.GetUnit() is not null)
                {
                    // Calculate World Position of that Tile
                    Vector3Int tilePosition = new Vector3Int(y, x, 0);
                    Vector3 worldPosition = baseTilemap.CellToWorld(tilePosition);
                    
                    // Store the Unit on that Tile
                    Unit currUnit = currTile.GetUnit();
                    
                    // If it's already instantiated and tracked
                    if (units.Keys.Contains(currUnit))
                    {
                        // Move GameObj Prefab to it's correct position
                        units[currUnit].transform.position = worldPosition;
                    }
                    else
                    {
                        // Instantiate a Prefab for that new Unit
                        GameObject unitOBJ = Instantiate(unitPrefab, unitsParent.transform);

                        // Place Game Obj Prefab on that Tile
                        unitOBJ.transform.position = worldPosition;
                        
                        UnitPrefab prefabScript = unitOBJ.GetComponent<UnitPrefab>();

                        // Give the Prefab reference to its own Unit
                        prefabScript.unit = currTile.GetUnit();
                        prefabScript.director = this;
                    
                        // Update it's art and values
                        prefabScript.UpdatePrefab();

                        // Add it to our HashSet tracker
                        units.Add(currUnit, unitOBJ);
                    }
                }
            }
        }
    }

    void DeleteUnit(Unit unit)
    {
        if (units.Keys.Contains(unit))
        {
            units[unit].SetActive(false);
            units.Remove(unit);
        }
    }

    void DisplayPossibleMoves(Unit unit)
    {
        // If unit can't move, don't highlight anything
        if (!unit.canMove())
        {
            return;
        }
        
        // Update the Unit's instance possible moves property
        unit.GetPossibleMoves(unit._gameTile, unit._currMP, true);
        
        // If there are moves 
        if (unit.possible_moves.Count > 0)
        {
            foreach (Point point in unit.possible_moves)
            {
                // On a shading Tilemap, place highlights on the possible moves
                shadingTilemap.SetTile(new Vector3Int(point.y, point.x), highlight);

                // Store that point
                highlightedTiles.Add(point);
            }
        }
    }

    void RemovePossibleMoves()
    {
        if (highlightedTiles.Count > 0)
        {
            foreach (Point point in highlightedTiles)
            {
                // Remove that tile
                shadingTilemap.SetTile(new Vector3Int(point.y, point.x), null);
            }
            
            highlightedTiles.Clear();
        }
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

    /* Checks if player has moves left to make. */
    public void UpdateEndTurnButton()
    {
        foreach (Settlement settlement in civilization._settlements)
        {
            if (settlement._currentCityProject is null)
            {
                endTurnText.text = "SETTLEMENT NEEDS ORDER";
                endTurnText.fontSize = 18;
                return;
            }
        }

        foreach (Unit unit in civilization._units)
        {
            if (unit._currMP > 0 && !unit._camping && !unit._passing)
            {
                endTurnText.text = "UNIT NEEDS ORDER";
                endTurnText.fontSize = 18;
                return;
            }
        }

        endTurnText.text = "END TURN";
        endTurnText.fontSize = 24;
    }

    public void CampSelectedUnit()
    {
        gm.CampUnit(selectedUnit);
        
        OpenUnitWindow();

        if (!selectedUnit._camping)
        {
            DisplayPossibleMoves(selectedUnit);
        }
        else
        {
            RemovePossibleMoves();
        }
        
        UpdateEndTurnButton();
    }

    public void PassSelectedUnit()
    {
        gm.PassUnit(selectedUnit);
        
        // Rerender Unit Window
        OpenUnitWindow();
        
        RemovePossibleMoves();
        
        UpdateEndTurnButton();
    }

    public void SettleSelectedUnit()
    {
        // Delete Object Prefab and remove it from units hashset tracking.
        DeleteUnit(selectedUnit);
        
        // Tell GM to Delete it from Game
        gm.SettleUnit(selectedUnit);
        
        // Close the Unit Window
        CloseUnitWindow();
        
        // Remove the PossibleMoves highlight incase the unit has MP left.
        RemovePossibleMoves();
        
        // Render Settlement and delete Unit
        RenderTilemaps(gm.game.world);
        RenderSettlementUI(gm.game.world);
        RenderTerritoryLines();
        
        // Update Player vision with new territory tiles
        RenderPlayerVision();
    }

    void CalculateVision()
    {
        visibleTiles = new HashSet<Point>();
        
        foreach (Settlement settlement in civilization._settlements)
        {
            CalculcateSettlementVision(settlement);
        }

        foreach (Unit unit in civilization._units)
        {
            CalculateUnitVision(unit);
        }
        
        // Adds all visible tile points to visibleTiles;
        void CalculcateSettlementVision(Settlement settlement)
        {
            foreach (GameTile territoryTile in settlement._territory)
            {
                // Convert Tile to Point
                Point territoryPoint = new Point(territoryTile.GetXPos(), territoryTile.GetYPos());
                // Add it to visible tiles (all territory should be visible)
                visibleTiles.Add(territoryPoint);
                
                // // If the tile hasn't been discovered yet (turn 1)
                if (!civilization.discoveredTiles.Contains(territoryPoint))
                {
                    // Tell GM to add it to the Civilization's discovered tiles
                    gm.AddDiscoveredTiles(civilization, territoryPoint);
                }
                
                foreach (GameTile neighbor in territoryTile.GetNeighbors())
                {
                    if (neighbor is null)
                    {
                        continue;
                    }
                    
                    // If it is adjacent to territory
                    if (!settlement._territory.Contains(neighbor))
                    {
                        // Convert Tile to Point
                        Point neighborPoint = new Point(neighbor.GetXPos(), neighbor.GetYPos());
                        // Add it to visible tiles if it's not already there (Hashsets don't store duplicates)
                        visibleTiles.Add(neighborPoint);

                        // // If the tile hasn't been discovered yet (turn 1)
                        if (!civilization.discoveredTiles.Contains(neighborPoint))
                        {
                            // Tell GM to add it to the Civilization's discovered tiles
                            gm.AddDiscoveredTiles(civilization, neighborPoint);
                        }
                    }
                }
            }
        }

        /* Calculates the Vision for each unit (assumes all units can only see 2 tiles away). */
        void CalculateUnitVision(Unit unit)
        {
            // Add the Unit's own Tile Point
            Point unitTilePoint = new Point(unit._gameTile.GetXPos(), unit._gameTile.GetYPos());
            visibleTiles.Add(unitTilePoint);
            
            // Then check neighbors
            foreach (GameTile neighbor in unit._gameTile.GetNeighbors())
            {
                if (neighbor is not null)
                {
                    // Convert Tile to Point
                    Point neighborPoint = new Point(neighbor.GetXPos(), neighbor.GetYPos());
                    // Add it to visible tiles if it's not already there (Hashsets don't store duplicates)
                    visibleTiles.Add(neighborPoint);
                    
                    // If the tile hasn't been discovered yet (turn 1)
                    if (!civilization.discoveredTiles.Contains(neighborPoint))
                    {
                        // Tell GM to add it to the Civilization's discovered tiles
                        gm.AddDiscoveredTiles(civilization, neighborPoint);
                    }

                    // If it obstructs vision don't do this
                    if (!neighbor.ObstructsVision())
                    {
                        // Track current neighbor's edge (GetNeighbors starts at index 0 ends in 5)
                        foreach (GameTile neighbor2 in neighbor.GetNeighbors())
                        {
                            // If the neighbor's neighbor isn't already part of visible tiles
                            if (neighbor2 is not null && !visibleTiles.Contains(new Point(neighbor2.GetXPos(), neighbor2.GetYPos())))
                            {
                                Point neighbor2Point = new Point(neighbor2.GetXPos(), neighbor2.GetYPos());
                                visibleTiles.Add(neighbor2Point);

                                // If the tile hasn't been discovered yet 
                                if (!civilization.discoveredTiles.Contains(neighbor2Point))
                                {
                                    // Tell GM to add it to the Civilization's discovered tiles
                                    gm.AddDiscoveredTiles(civilization, neighbor2Point);
                                }

                                // If Unit is on hill, get an additional vision
                                if (unit._gameTile.GetTerrain() == 1)
                                {
                                    foreach (GameTile neighbor3 in neighbor2.GetNeighbors())
                                    {
                                        // If the next neighbor isn't null, isn't already visible, and the previous neighbor doesn't obstruct vision
                                        if (neighbor3 is not null && !visibleTiles.Contains(new Point(neighbor3.GetXPos(), neighbor3.GetYPos())) && !neighbor2.ObstructsVision())
                                        {
                                            Point neighbor3Point = new Point(neighbor3.GetXPos(), neighbor3.GetYPos());
                                            // Add it to visible tiles
                                            visibleTiles.Add(neighbor3Point);
                                            
                                            // If the tile hasn't been discovered yet 
                                            if (!civilization.discoveredTiles.Contains(neighbor3Point))
                                            {
                                                // Tell GM to add it to the Civilization's discovered tiles
                                                gm.AddDiscoveredTiles(civilization, neighbor3Point);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
