using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    bool isMultiplayer = false;
    bool isHost = false;

    bool isInGame = false;
    private double lastUpdateTime = (double)0;

    private Game game;

    void Start() {
        lastUpdateTime = Util.GetUnixTimeMilliseconds();
    }

    void Update() {

    }

    /* -----------------------------------------------

    ----------------------------------------------- */
    public void HostNewMultiplayerGame() {
        isMultiplayer = true;
        isHost = true;

        // Create Game + World
        game = new Game();
        
    }

    public void JoinMultiplayerGame() {
        isMultiplayer = true;
        isHost = false;
    }
}

