
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class GameManager : NetworkBehaviour {
    private static GameManager _instance;
    public static GameManager Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<GameManager>();
            }
            return _instance;
        }
    }

    private Game game;
    private bool isSinglePlayer = true;
    private bool isHost = false;
    private UnityTransport _transport;
    public string relayCode;

    private bool isInGame = false;
    private double lastUpdateTime = (double)0;
    
    // Temporary buffer to store chunks before reassembly
    private List<byte> _receivedChunks = new List<byte>();
    private const int ChunkSize = 1024;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() {
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnDisable() {
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    void Start() {
        lastUpdateTime = Util.GetUnixTimeMilliseconds();
    }

    /* -----------------------------------------------
        Host Multiplayer Game
    ----------------------------------------------- */
    public async Task<List<Lobby>> GetAvailableLobbies()
    {
        try
        {
            await Authenticate();

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            return queryResponse.Results;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to get lobbies: {e.Message}");
            return new List<Lobby>();
        }
    }

    public async Task ShowLobbies()
    {
        Debug.Log("Lobbies:");
        List<Lobby> lobbies = await GetAvailableLobbies();
        Debug.Log("LobbiesX:");
        foreach (Lobby lobby in lobbies)
        {
            string lobbyCode = lobby.Data.ContainsKey("LobbyCode") ? lobby.Data["LobbyCode"].Value : "N/A";
            Debug.Log($"Lobby: {lobby.Name}, Players: {lobby.Players.Count}/{lobby.MaxPlayers}, Code: {lobbyCode}");
        }
    }

    public bool IsLobbyPrivate = true;
    public async void HostNewMultiplayerGame__Lobby(bool isPrivate=false) {
        await CreateLobby("My Game Name", 4, isPrivate);
    }

    public async void HostNewMultiplayerGame(bool isPrivate=false) {
        isSinglePlayer = false;
        isHost = true;

        List<Civilization> civs = new List<Civilization>();
        civs.Add(new Civilization("p1"));
        civs.Add(new Civilization("p2"));

        // int seed = 1000;
        int seed = UnityEngine.Random.Range(0, int.MaxValue);
        await Game.Instance.Initialize(false, seed, civs);
        game = Game.Instance;

        relayCode = await CreateRelay();

        // Update the lobby with the actual lobby code
        await Lobbies.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, relayCode) }
            }
        });
    }

    public async Task<string> CreateRelay()
    {
        try
        {
            await Authenticate();

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"Relay Created! Join Code: {joinCode}");

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Relay Creation Failed: {e.Message}");
            // TODO: incorrect relay join code -- maybe set error string (red) saying incorrect code
            return null;
        }

    }

    private Lobby currentLobby;
    public string lobbyCode;

    public async Task CreateLobby(string lobbyName, int maxPlayers, bool isPrivate=true)
    {
        try
        {
            await Authenticate();

            Lobby createdLobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { "LobbyCode", new DataObject(DataObject.VisibilityOptions.Public, "Generating...") }
                }
            });

            // Wait for Unity Lobby to process
            await Task.Delay(1000);  

            // Fetch the actual lobby with its generated code
            createdLobby = await Lobbies.Instance.GetLobbyAsync(createdLobby.Id);

            // Update the lobby with the actual lobby code
            await Lobbies.Instance.UpdateLobbyAsync(createdLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "LobbyCode", new DataObject(DataObject.VisibilityOptions.Public, createdLobby.LobbyCode) }
                }
            });

            Debug.Log($"Lobby Created: {createdLobby.Name}, Code: {createdLobby.LobbyCode}");
            currentLobby = createdLobby;
            lobbyCode = createdLobby.LobbyCode;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
        }
    }

    public async Task JoinLobbyByCode()
    {
        try
        {
            await Authenticate();

            currentLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);

            Debug.Log($"Joined Lobby: {currentLobby.Name}, Players: {currentLobby.Players.Count}/{currentLobby.MaxPlayers}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");

            // Check if the error means "Player is already in a lobby"
            if (e.Message == "player is already a member of the lobby") // alr in lobby (same pc for dev purposes)
            {
                Debug.Log("Player is already in a lobby. Retrieving existing instance...");

                // Get the current lobby the player is in
                QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

                foreach (Lobby lobby in queryResponse.Results)
                {
                    if (lobby.Players.Any(p => p.Id == AuthenticationService.Instance.PlayerId))
                    {
                        currentLobby = lobby;
                        Debug.Log($"Found existing lobby: {currentLobby.Name}");
                        break;
                    }
                }

                PollLobbyForRelayCode();
            }
        }
    }

    private bool isCheckingForRelay = false;

    public async Task PollLobbyForRelayCode()
    {
        if (currentLobby == null)
        {
            Debug.LogError("Not in a lobby.");
            return;
        }

        isCheckingForRelay = true;

        while (isCheckingForRelay)
        {
            try
            {
                currentLobby = await Lobbies.Instance.GetLobbyAsync(currentLobby.Id);

                if (currentLobby.Data.ContainsKey("RelayCode") && currentLobby.Data["RelayCode"].Value != "N/A")
                {
                    Debug.Log($"Relay Code Found: {currentLobby.Data["RelayCode"].Value}");
                    relayCode = currentLobby.Data["RelayCode"].Value;
                    await JoinMultiplayerGame();
                    isCheckingForRelay = false; // Stop checking
                    return;
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Error checking for Relay Code: {e.Message}");
            }

            await Task.Delay(2000); // Check every 2 seconds
        }
    }
    /* -----------------------------------------------
        Join Multiplayer Game
    ----------------------------------------------- */
    public async Task Authenticate() {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async Task JoinMultiplayerGame() {
        isSinglePlayer = false;
        isHost = false;
        string joinCode = relayCode;

        game = Game.Instance;
        await JoinRelay(joinCode);

        await game.SetupUI();
    }

    private async Task JoinRelay(string joinCode)
    {
        try
        {
            await Authenticate();

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log($"Joined Relay with Code: {joinCode}");

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to Join Relay: {e.Message}");
        }
    }

    /* -----------------------------------------------
        Multiplayer Communication
    ----------------------------------------------- */
    private void OnClientConnected(ulong clientId) {
        Debug.Log($"Client {clientId} joined the game!");

        if (NetworkManager.Singleton.IsHost) {  
            Debug.Log($"Client {clientId} joined (Host Perspective)");
        } else {
            Debug.Log("Client connected, requesting board state...");
            RequestBoardStateServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ClientRpc]
    private void NotifyClientJoinedClientRpc(ulong clientId) {
        Debug.Log($"Client {NetworkManager.Singleton.LocalClientId} received join notification for Client {clientId}");
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestBoardStateServerRpc(ulong clientId) {
        Debug.Log($"Client {clientId} requested the board state.");

        if (IsHost) {
            byte[] gameState = new GameStateData(game).Serialize();
            int totalChunks = Mathf.CeilToInt((float)gameState.Length / ChunkSize);

            // Send the data in chunks
            for (int i = 0; i < totalChunks; i++) {
                int startIndex = i * ChunkSize;
                int chunkLength = Mathf.Min(ChunkSize, gameState.Length - startIndex);
                byte[] chunk = new byte[chunkLength];
                Array.Copy(gameState, startIndex, chunk, 0, chunkLength);

                // Send each chunk with its index and total chunks
                ReceiveBoardStateClientRpc(clientId, chunk, i, totalChunks);
            }

            Debug.Log($"Sent {totalChunks} chunks");
        }
    }

    [ClientRpc]
    private void ReceiveBoardStateClientRpc(ulong clientId, byte[] chunkJson, int chunkIndex, int totalChunks) {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log($"Client {NetworkManager.Singleton.LocalClientId} received the board state.");

            // Store or append the chunk to a buffer
            if (chunkIndex == 0) {
                _receivedChunks = new List<byte>();
            }

            _receivedChunks.AddRange(chunkJson);

            // Check if all chunks are received
            if (chunkIndex == totalChunks - 1) {
                Debug.Log("Received all chunks");
                byte[] fullData = _receivedChunks.ToArray();
                Game.Instance.ReinitializeFromJson(fullData);

                Debug.Log($"Client {clientId} has successfully reassembled and loaded the board state.");
                _receivedChunks.Clear();
            }
        }
        
    }

    /* ------------------------------------------------
        NETWORK - UNIT
    ------------------------------------------------ */
    [ServerRpc(RequireOwnership=false)]
    public void Network_Unit_MoveTo__ServerRpc(ulong originID, int civID, int unitID, int tileX, int tileY)
    {
        // should lock or smthg prolly
        
        Network_Unit_MoveTo__ClientRpc(originID, civID, unitID, tileX, tileY);
    }

    [ClientRpc]
    public void Network_Unit_MoveTo__ClientRpc(ulong originID, int civID, int unitID, int tileX, int tileY)
    {
        Civilization civ = Game.Instance.civs.Find(civ => civ.ID == civID);
        Unit unit = civ.Units.Find(unit => unit.ID == unitID);
        GameTile tile = Game.Instance.tiles[tileX][tileY];
        unit.MoveTo(tile);
        Game.Instance.UI.RenderUnits();

        if (NetworkManager.Singleton.LocalClientId == originID) {
            Game.Instance.UI.MoveSelectedUnitConfirmed();
        }
    }
}
