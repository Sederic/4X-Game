using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
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

            Allocation a = await RelayService.Instance.CreateAllocationAsync(2);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            Debug.Log($"Relay Created! Join Code: {joinCode}");

            RelayServerData relayServerData = new RelayServerData(a, "dtls");
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

        await JoinRelay(joinCode);
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

    /* -----------------------------------------------
        Client Requests Board State from Host
    ----------------------------------------------- */
    [ServerRpc(RequireOwnership = false)]
    void RequestBoardStateServerRpc(ulong clientId) {
        Debug.Log($"Client {clientId} requested the board state.");

        if (IsHost) {
            SendBoardStateClientRpc("board state", clientId);
        }
    }

    /* -----------------------------------------------
        Host Sends Board State to Specific Client
    ----------------------------------------------- */
    [ClientRpc]
    void SendBoardStateClientRpc(string boardState, ulong clientId, ClientRpcParams clientRpcParams = default) {
        if (NetworkManager.Singleton.LocalClientId == clientId) {
            Debug.Log($"Received board state from host: {boardState}");
        }
    }

}
