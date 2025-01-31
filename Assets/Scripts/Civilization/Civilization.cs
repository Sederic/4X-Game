using System;
using System.Collections.Generic;

public enum TileVisibility
{
    Unexplored = 0,     // Fully unkown
    Unknown = 1,        // Met at one point
    Visible,            // Full Visibility
}

public class Civilization {
    public int ID { get; private set; }
    public string Name { get; private set; }
    public List<Unit> Units { get; private set; } = new List<Unit>();

    public int[][] TileVisibility { get; private set; }
    public List<Point> UpdatedTileVisibility { get; private set; } = new List<Point>();

    public static List<Civilization> AllCivs { get; private set; } = new List<Civilization>();

    public Civilization(string name) {
        Name = name;

        TileVisibility = new int[50][];
        for (int i=0; i<TileVisibility.Length; i++)
        {
            TileVisibility[i] = new int[100];
        }

    }

    public void AddUnit(Unit unit) { Units.Add(unit); }

    public void UpdateTileVisibilityList()
    {
        GameTile[][] tiles = Game.Instance.tiles;
        for (int x=0; x<tiles.Length; x++)
        {
            for (int y=0; y<tiles[0].Length; y++)
            {
                GameTile tile = tiles[x][y];
                int prevVisibility = TileVisibility[x][y];
                int visibility = prevVisibility;
                
                // units
                if (tile.unit != null && tile.unit.civ.Name == Name)
                {
                    visibility = 2;
                }

                // settlement


                TileVisibility[x][y] = visibility;
                if (visibility != prevVisibility)
                {
                    UpdatedTileVisibility.Add(new Point(x, y));
                }
            }
        }
    }
    public void ClearTileVisibilityList() { UpdatedTileVisibility.Clear(); }

    public static void SetAllCivs(List<Civilization> civs) { AllCivs = civs; }
}