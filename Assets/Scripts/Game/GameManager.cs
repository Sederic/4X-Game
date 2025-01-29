using System;
using System.Collections.Generic;
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
        List<Civilization> civs = new List<Civilization>();
        civs.Add(new Civilization("p1"));
        civs.Add(new Civilization("p2"));

        int seed = 1;
        game = new Game(false, seed, civs);

        // Display world
        
    }

    public void JoinMultiplayerGame() {
        isMultiplayer = true;
        isHost = false;
    }
}

