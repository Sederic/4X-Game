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

public class GameManager : MonoBehaviour {
    private Game game;
    private bool isSinglePlayer = true;
    private bool isHost = false;
    private UnityTransport _transport;
    public string relayCode;

    private bool isInGame = false;
    private double lastUpdateTime = (double)0;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // Keeps this object across scenes (GameUI refers to this)
    }

    void Start() {
        lastUpdateTime = Util.GetUnixTimeMilliseconds();
    }

    void Update() {

    }

    /* -----------------------------------------------
        Start Singleplayer Game
    ----------------------------------------------- */
    /* -----------------------------------------------
        Host Multiplayer Game
    ----------------------------------------------- */
    public async void HostNewMultiplayerGame() {
        isSinglePlayer = false;
        isHost = true;

        // Create Game + World
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

            Allocation a = await RelayService.Instance.CreateAllocationAsync(2); // Up to 2 players
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

            Debug.Log($"Relay Created! Join Code: {joinCode}");

            // Set up Transport
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

            // Set up Transport
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient(); // Start client mode
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to Join Relay: {e.Message}");
        }
    }
}

