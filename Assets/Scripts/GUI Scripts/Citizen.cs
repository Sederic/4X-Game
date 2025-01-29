// using UnityEngine;
// using UnityEngine.UI;

// public class Citizen : MonoBehaviour
// {
//     public SettlementWindow settlementWindow;
//     public Settlement settlement;
//     public GameTile gameTile;
//     public Image image;
    
//     private bool worked;
//     private bool locked;
//     private Color32 workedColor = new Color32(255, 255, 255, 200);
//     private Color32 unworkedColor = new Color32(100, 100, 100, 150);
//     private Color32 lockedColor = new Color32(180, 255, 180, 200);
    
//     // Start is called before the first frame update
//     void Start()
//     {
//         UpdateCitizenUIDisplay();
//     }

//     public void ToggleLock()
//     {
//         if (locked)
//         {
//             UnlockCitizenTile();
//         }
//         else
//         {
//             LockCitizenTile();
//         }
        
//         // Update the projects in the Settlement Window to reflect Tile changes
//         UpdateSettlementWindow();
//         // Update the Citizen UI itself (change color, for now)
//         UpdateCitizenUIDisplay();
//     }

//     private void UpdateCitizenUIDisplay()
//     {
//         if (settlement._workedTiles.Contains(gameTile))
//         {
//             worked = true;
//             // Ensure image is set to worked color
//             image.color = workedColor;
//         }
//         else
//         {
//             worked = false;
//             // Ensure image is set to unworked color
//             image.color = unworkedColor;
//         }

//         if (settlement._lockedTiles.Contains(gameTile))
//         {
//             locked = true;
//             // Ensure locked sprite is set to true
//             image.color = lockedColor;
//         }
//         else
//         {
//             locked = false;
//         }
//     }

//     private void LockCitizenTile()
//     {
//         if (settlement._lockedTiles.Count >= settlement.GetPopulation() + 1)
//         {
//             Debug.Log("Cannot lock more tiles than population + 1.");
//             return;
//         }
        
//         locked = true;
        
//         // Add it to Settlement's locked tiles
//         settlement._lockedTiles.Add(gameTile);
//     }

//     private void UnlockCitizenTile()
//     {
//         if (gameTile.Equals(settlement.GetTile()))
//         {
//             Debug.Log("Cannot unlock Settlement's home tile.");
//             return;
//         }
        
//         locked = false;
        
//         // Remove it from the Settlement's locked tiles.
//         settlement._lockedTiles.Remove(gameTile);
//     }

//     private void UpdateSettlementWindow()
//     {
//         // Update the Settlement's yields in memory
//         settlement.UpdateYields();
//         // Update ProjectCosts
//         settlementWindow.UpdateProjectTabs();
//         // Update Yields Tab
//         settlementWindow.UpdateYieldsTab();
//     }
    
//     // Get/Set/Is Methods

//     public bool IsWorked()
//     {
//         return worked;
//     }

//     public bool IsLocked()
//     {
//         return locked;
//     }
// }
