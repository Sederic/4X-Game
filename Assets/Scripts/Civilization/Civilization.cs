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

    public Civilization(string name) {
        ID = this.GetHashCode();
        Name = name;

        TileVisibility = new int[50][];
        for (int i=0; i<TileVisibility.Length; i++)
        {
            TileVisibility[i] = new int[100];
        }
    }

    public Civilization(CivilizationData civData, GameStateData gameState) {
        ID = civData.civID;
        Name = civData.civName;

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

        // Units
        foreach (var unit in Units) {
            int vision = unit.Vision;
            int blockingCount = unit.tile.UnitBlockingCount();
            List<(GameTile, int)> frontier = new List<(GameTile, int)>();
            for (int i=0; i<6; i++) { frontier.Add((unit.tile, i)); }
            while (vision-- > 0) {
                List<(GameTile, int)> newFrontier = new List<(GameTile, int)>();
                foreach ((GameTile tile, int dir) in frontier) {
                    GameTile?[] neighbors = tile.GetNeighborsArray();
                    for (int i=0; i<6; i++) {
                        GameTile? t = neighbors[i];
                        if (t != null && (dir == i || dir == (i + 1) % 6 || dir == (i - 1) % 6)) {
                            int x = t.x;
                            int y = t.y;

                            if (TileVisibility[x][y] != 2) {
                                TileVisibility[x][y] = 2;
                                UpdatedTileVisibility.Add(new Point(x, y));
                            }

                            if (blockingCount >= t.BlockingCount() && (unit.Vision != vision + 1 || dir == i))
                                newFrontier.Add((t, dir));
                        }
                    }
                }
                frontier = newFrontier;
            }
        }
    }
    public void ClearTileVisibilityList() { UpdatedTileVisibility.Clear(); }

}