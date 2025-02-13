using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
    [Header("Game Manager")]
    public GameManager gameManager;

    [Header("Menu Canvases")]
    public GameObject mainMenuCanvas;
    public GameObject singleplayerMenuCanvas;
    public GameObject singleplayerNewGameCanvas;
    public GameObject singleplayerLoadGameCanvas;
    public GameObject multiplayerMenuCanvas;
    public GameObject multiplayerHostGameCanvas;
    public GameObject multiplayerJoinGameCanvas;
    
    [Header("Prefabs")]
    public GameObject playerButtonPrefab;
    public GameObject saveButtonPrefab;
    public Transform gamePlayersContainer;
    public Transform saveListContainer;
    
    [Header("World Seed Input")]
    public TMP_InputField seedInput;

    [Header("Input Fields")]
    public TextMeshProUGUI lobbyCodeInput;
    
    private string filePath;
    private GameObject currentCanvas;
    private List<GameObject> playerButtons;


    // Start is called before the first frame update
    void Start()
    {
        currentCanvas = mainMenuCanvas;
    }
    
    // Closes any Menu and returns to Main Menu
    public void GoToMainMenuCanvas()
    {
        if (currentCanvas != mainMenuCanvas)
        {
            // Deactivate Current Canvas
            currentCanvas.SetActive(false);
            
            // Activate Main Menu Canvas
            mainMenuCanvas.SetActive(true);
            
            // Set Current Canvas
            currentCanvas = mainMenuCanvas;
        }
    }
    
    public void GoToSingleplayerMenu()
    {
        // Deactivate Main Menu Canvas
        currentCanvas.SetActive(false);
        
        // Activate Singleplayer Menu Canvas
        singleplayerMenuCanvas.SetActive(true);

        // Set Current Canvas
        currentCanvas = singleplayerMenuCanvas;
    }

    public void GoToMultiplayerMenu()
    {
        // Deactivate Main Menu Canvas
        currentCanvas.SetActive(false);
        
        // Activate Multiplayer Menu Canvas
        multiplayerMenuCanvas.SetActive(true);
        
        // Set Current Canvas
        currentCanvas = multiplayerMenuCanvas;
    }

    public void GoToSingleplayerNewGame()
    {
        // Deactivate Singleplayer Menu Canvas
        currentCanvas.SetActive(false);
        
        // Activate Singleplayer New Game Canvas
        singleplayerNewGameCanvas.SetActive(true);
        
        currentCanvas = singleplayerNewGameCanvas;
    }

    public void GoToSingleplayerLoadGame()
    {
        // Deactivate Current Canvas
        currentCanvas.SetActive(false);
        
        // Activate Singleplayer Load Game Canvas
        singleplayerLoadGameCanvas.SetActive(true);
        
        // Set Current Canvas
        currentCanvas = singleplayerLoadGameCanvas;
    }

    public void GoToMultiplayerHostGame()
    {
        CreateLobby();

        // Deactivate Current Canvas
        currentCanvas.SetActive(false);
        
        // Activate Multiplayer Host Game Canvas
        multiplayerHostGameCanvas.SetActive(true);
        
        // Set Current Canvas
        currentCanvas = multiplayerHostGameCanvas;
    }

    public void GoToMultiplayerJoinGame()
    {
        ListMultiplayerGames();

        // Deactivate Current Canvas
        currentCanvas.SetActive(false);
        
        // Activate Multiplayer Join Game Canvas
        multiplayerJoinGameCanvas.SetActive(true);
        
        // Set Current Canvas
        currentCanvas = multiplayerJoinGameCanvas;
    }
    
    public void ApplicationQuit()
    {
        Application.Quit();
    }
    
    public void NewMultiplayerGame() {
        gameManager.HostNewMultiplayerGame();
    }

    public void LoadMultiplayerGmae() {
        gameManager.JoinMultiplayerGame();
    }

    /* Will be called every second to update the players in host's game */
    void UpdatePlayersInHostGame()
    {
        foreach (GameObject player in playerButtons)
        {
            if (playerButtons.Contains(player))
            {
                playerButtons.Remove(player);
            }
        }
        
        /*
         *    foreach (player connected){
         *   {
         *          
         *   }
         */
        
        GameObject playerButton = Instantiate(playerButtonPrefab, gamePlayersContainer);
        PlayerButton playerButtonScript = playerButton.GetComponent<PlayerButton>();
        playerButtonScript.playerName.text = "player name (replace me!)";
        playerButtonScript.playerID.text = "player ID (replace me!)";
        
        // Track it in List
        playerButtons.Add(playerButton);
    }
    
    // Opens Load Game Menu 
    public void OpenLoadGameMenu()
    {
        mainMenuCanvas.SetActive(false);
        /*newGameCanvas.SetActive(false);
        loadGameCanvas.SetActive(true);*/
        
        PopulateSaveList();
        
        void PopulateSaveList()
        {
            string[] files = Directory.GetFiles(Application.persistentDataPath, "*.json");

            foreach (string filePath in files)
            {
                GameObject saveButton = Instantiate(saveButtonPrefab, saveListContainer);
                string filename = Path.GetFileNameWithoutExtension(filePath);
                saveButton.GetComponentInChildren<TMP_Text>().text = filename;
            
                saveButton.GetComponent<Button>().onClick.AddListener(() => LoadSelectedSave(filePath));
            }
        }
        
        void LoadSelectedSave(string filePath)
        {
            // gameManager.LoadGame(filePath);
        }
    }

    // public void StartNewGame()
    // {
    //     int input = int.Parse(seedInput.text);
    //     uint seed = Convert.ToUInt32(input);
    //     // gameManager.NewDemoGame(seed);
    // }

    public void CreateLobby() {
        gameManager.HostNewMultiplayerGame__Lobby(gameManager.IsLobbyPrivate);
    }

    public void StartGameAsHost() {
        gameManager.HostNewMultiplayerGame();
    }

    public void JoinLobby() {
        gameManager.JoinLobbyByCode(lobbyCodeInput.text.Replace("\u200B", ""));
    }

    public void ListMultiplayerGames() {
        gameManager.ShowLobbies();
    }
}
