using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class WorldGenWalker : MonoBehaviour
{
    public GameTile CurrTile; //the tile that the walker is standing on
    public World world;
    public int direction; //a number from 0-5, which will pull from the list of neighbors
    private Random _random;

    public WorldGenWalker(World w, GameTile startTile, int _direction) //initializing variables
    {
        world = w;
        CurrTile = startTile;
        direction = _direction;
        _random.InitState();
        _random = new Random(3);
    }

    /*
     * the walker will take a step
     * changes the tile it is currently standing on,
     * then move in the current direction and sets a new random number
     * as its new direction
     *
     * several failsafes in place:
     * if the currtile is null, which will happen
     * if the walker traverses off of the map,
     * it will start again from a new completely random tile
     *
     * if the direction would take the walker onto land or off the map,
     * it looks for a valid neighbor that is neither null nor land
     *(the above is done through the findnewneighbor function
     *
     * if all else fails it just takes a random step which will
     * sometimes take it off of the map, in which case the first failsafe
     * triggers
     *
     * returns true if it modified a tile, false if it didn't
     */
    public bool move() 
    {
        if (CurrTile == null)
        {
            int randomX = _random.NextInt(0, world.GetLength() - 1);
            int randomY = _random.NextInt(0, world.GetHeight() - 1);
            CurrTile = world.GetTile(new Point(randomX, randomY));
            return false;
        }
        GameTile[] neighbors = CurrTile.GetNeighbors();
        GameTile currNeighbor = neighbors[direction];

        if (currNeighbor == null || currNeighbor.GetBiome() != 7)
        {
            currNeighbor = findNewNeighbor(neighbors);
            if (currNeighbor == null)
            {
                CurrTile = neighbors[direction];
                direction = _random.NextInt(0, 6);
                return false;
            }
        }
        
        
        Point point = new Point(currNeighbor.GetXPos(), currNeighbor.GetYPos());
        world.ModifyTileBiome(point, 1);
        CurrTile = neighbors[direction];
        direction = _random.NextInt(0, 6);
        return true;
        
    }

    public GameTile findNewNeighbor(GameTile[] neighbors)
    {
        LinkedList<GameTile> validNeighbors = new LinkedList<GameTile>();
        foreach (GameTile neighbor in neighbors)
        {
            if (neighbor != null && neighbor.GetBiome() == 7)
            {
                validNeighbors.AddLast(neighbor);
            }
        }
        for (int i = 0; i < _random.NextInt(0, validNeighbors.Count); i++)
        {
            validNeighbors.RemoveFirst();
        }

        if (validNeighbors.Count > 0)
        {
            return validNeighbors.First.Value;
        }

        return null;
    }
}