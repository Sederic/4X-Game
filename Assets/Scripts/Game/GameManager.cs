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

    ----------------------------------------------- */
    public async void HostNewMultiplayerGame() {
        isMultiplayer = true;
        isHost = true;

        // Create Game + World
        List<Civilization> civs = new List<Civilization>();
        civs.Add(new Civilization("p1"));
        civs.Add(new Civilization("p2"));

        int seed = 100;
        await Game.Instance.Initialize(false, seed, civs);
        game = Game.Instance;
    }

    public async void JoinMultiplayerGame() {
        isMultiplayer = true;
        isHost = false;
    }
}

