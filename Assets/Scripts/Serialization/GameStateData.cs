using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameStateData {
    public int gameTurn;
    public bool isSinglePlayer;
    public WorldData world;

    public Dictionary<int, GameTileData> tiles;
    public Dictionary<int, CivilizationData> civs;

    public GameStateData() {}
    public GameStateData(Game game) {
        gameTurn = game.gameTurn;
        isSinglePlayer = game.isSinglePlayer;

        world = new WorldData(game.world);

        // Initialize Tile Dictionary (Key: "x,y" to allow easy lookup)
        tiles = new Dictionary<int, GameTileData>();
        foreach (var row in game.tiles) {
            foreach (var tile in row) {
                tiles[tile.x * game.tiles[0].Length + tile.y] = new GameTileData(tile);
            }
        }

        // Initialize Civ Dictionary (Key: Civilization ID)
        // civs = new Dictionary<int, CivilizationData>();
        // foreach (var civ in game.civs) {
        //     civs[civ.ID] = new CivilizationData(civ);
        // }
    }

    public byte[] Serialize() {
        Debug.Log(JsonConvert.SerializeObject(this));
        return Util.CompressJson(JsonConvert.SerializeObject(this));
    }

    public static GameStateData Deserialize(byte[] json) {
        return JsonConvert.DeserializeObject<GameStateData>(Util.DecompressJson(json));
    }
}
