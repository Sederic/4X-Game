// using System;
// using System.Collections.Generic;
// using System.Linq;
// using City_Projects;
// using TMPro;
// using UnityEngine;
// using UnityEngine.Serialization;
// using UnityEngine.Tilemaps;

// public class SettlementWindow : MonoBehaviour
// {
//     public GameObject settlementWindow;
//     public Canvas _worldCanvas;
//     public RectTransform unitProjectsContent;
//     public RectTransform buildingProjectsContent;
//     public RectTransform buildingsContent;
//     public Director director;
//     public TMP_Text settlementName;
//     public GameObject citizenUI;
//     public Settlement _settlement;
//     public Tilemap tilemap;
//     public GameObject cityProjectButton;
//     public GameObject buildingButton;
//     public TMP_Text food;
//     public TMP_Text production;
//     public TMP_Text gold;
//     public TMP_Text culture;
//     public TMP_Text science;
    
//     // Private Properties
//     private HashSet<GameObject> citizenUIs = new HashSet<GameObject>();
//     private Dictionary<CityProject, GameObject> projects = new Dictionary<CityProject, GameObject>();

//     public void Start()
//     {
//         director = FindObjectOfType<Director>();
//         citizenUIs = new HashSet<GameObject>();
//         FillProjectTabs();
//         FillBuildingTabs();
//         UpdateYieldsTab();
//     }
    
//     public void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Escape))
//         {
//             DestoryWindow();
//             DestroyCitizenUIs();
//             // Turn base UI back on
//             director.guiCanvas.SetActive(true);
//         }
//     }

//     public void UpdateYieldsTab()
//     {
//         food.text = _settlement.GetYieldsPt()[0].ToString();
//         production.text = _settlement.GetYieldsPt()[1].ToString();
//         gold.text = _settlement.GetYieldsPt()[2].ToString();
//         culture.text = _settlement.GetYieldsPt()[3].ToString();
//         science.text = _settlement.GetYieldsPt()[4].ToString();
//     }

//     public void RenderCitizenUIs()
//     {
//         // Initialize
//         citizenUIs = new HashSet<GameObject>();

//         foreach (GameTile tile in _settlement._territory)
//         {
//             // Get a Vector 3 Position of that tile
//             Vector3Int tilePosition = new Vector3Int(tile.GetYPos(), tile.GetXPos(), 0);
//             Vector3 worldPosition = tilemap.CellToWorld(tilePosition);
            
//             // Instance citizen Obj and the UI prefab
//             GameObject citizen = Instantiate(citizenUI, _worldCanvas.transform);
            
//             // Position it in world canvas
//             RectTransform rectTransform = citizen.GetComponentInChildren<RectTransform>();
//             rectTransform.position = worldPosition;
            
//             // Set Citizen scripts references
//             citizen.GetComponent<Citizen>().settlementWindow = this;
//             citizen.GetComponent<Citizen>().settlement = _settlement;
//             citizen.GetComponent<Citizen>().gameTile = tile;
            
//             // Keep track of it in HashSet
//             citizenUIs.Add(citizen);
//         }
//     }

//     /* Renders buttons on top of Citizen tiles. */
//     public void ToggleManageCitizens()
//     {
//         if (citizenUIs.Count <= 0)
//         {
//             RenderCitizenUIs();
//         }
//         else
//         {
//             DestroyCitizenUIs();
//             _settlement.AutoAssignWorkedTiles();
//         }
//     }

//     private void DestroyCitizenUIs()
//     {
//         foreach (GameObject ui in citizenUIs)
//         {
//             Destroy(ui);
//         }

//         citizenUIs = new HashSet<GameObject>();
//     }
    
//     private void DestoryWindow()
//     {
//         settlementWindow.transform.SetParent(null);
//         director._zoomedIn = false;
//         director.RestoreCameraState();
//         Destroy(settlementWindow);
//     }

//     public void FillProjectTabs()
//     {
//         // Fill Project Tabs with the Settlement's added projects
//         foreach (CityProject project in _settlement.GetProjects())
//         {
//             // Initialize variable
//             GameObject projectPrefab = null;
            
//             // Place Units in the Units content
//             if (project.projectType == "unit")
//             {
//                 projectPrefab = Instantiate(cityProjectButton, unitProjectsContent);
//             }
//             // Place Buildings in the Buildings content
//             else if (project.projectType == "building")
//             {
//                 // If that Building hasn't already been built
//                 if (project.alreadyBuilt)
//                 {
//                     if (projects.Keys.Contains(project))
//                     {
//                         // Remove it from our tracked projects
//                         projects.Remove(project);
//                     }
//                     continue;
//                 }
                
//                 projectPrefab = Instantiate(cityProjectButton, buildingProjectsContent);
//             }

//             ProjectButton projectButtonScript = projectPrefab.GetComponent<ProjectButton>();
                
//             projectButtonScript.name.text = project.projectName;
//             projectButtonScript.turns.text = Math.Ceiling((((float) project.projectCost - (float) project.currentProductionProgress) / (float) _settlement.GetYieldsPt()[1])).ToString();
//             projectButtonScript.cost.text = project.projectCost.ToString();
//             projectButtonScript.settlement = _settlement;
//             projectButtonScript.project = project;
//             projectButtonScript.director = director;

//             projects.Add(project, projectPrefab);
//         }
//     }

//     public void FillBuildingTabs()
//     {
//         if (_settlement._buildings.Count > 0)
//         {
//             GameObject buildingPrefab = null;
            
//             // Fill Buildings Tab with a Settlement's buildings
//             foreach (Building building in _settlement._buildings)
//             {
//                 // Instantiate the prefab
//                 buildingPrefab = Instantiate(buildingButton, buildingsContent);

//                 BuildingButton buttonScript = buildingPrefab.GetComponent<BuildingButton>();
//                 buttonScript.building = building;
//                 buttonScript.DisplayText();
//             }
//         }
//     }

//     /* Goes through each Project Prefab button and updates its cost. (Useful when Settlement changes yields) */
//     public void UpdateProjectTabs()
//     {
//         foreach (CityProject project in projects.Keys)
//         {
//             GameObject projectPrefab = projects[project];
            
//             projectPrefab.GetComponent<ProjectButton>().turns.text = Math.Ceiling((double)((project.projectCost - project.currentProductionProgress) / _settlement.GetYieldsPt()[1])).ToString();
//         }
//     }
// }
