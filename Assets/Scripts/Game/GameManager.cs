
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    public async void HostNewMultiplayerGame() {
        isSinglePlayer = false;
        isHost = true;

        List<Civilization> civs = new List<Civilization>();
        civs.Add(new Civilization("p1"));
        civs.Add(new Civilization("p2"));

        int seed = 1000;
        await Game.Instance.Initialize(false, seed, civs);
        game = Game.Instance;

        relayCode = await CreateRelay();
    }

    private async Task<string> CreateRelay()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

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
            return null;
        }
    }

    /* -----------------------------------------------
        Join Multiplayer Game
    ----------------------------------------------- */
    public async void JoinMultiplayerGame() {
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
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

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
            }
        }
        
    }

}
