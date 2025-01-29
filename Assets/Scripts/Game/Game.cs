using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Game {
    public World world { get; private set; }
    public List<Civilization> civs { get; private set; }
    public int gameTurn { get; private set; } = 0;
    public bool isSinglePlayer { get; private set; }
    
    public Game (bool isSinglePlayer, int seed, List<Civilization> civs) {
        world = new World(seed, civs.Count);
        this.civs = civs;
        Debug.Log("done");
    }

    private void Turn0() {

    }
}