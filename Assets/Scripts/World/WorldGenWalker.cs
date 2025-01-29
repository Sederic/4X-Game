using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class WorldGenWalker
{
    public GameTile currGameTile; //the tile that the walker is standing on
    private GameTile[][] tiles;
    public int direction; //a number from 0-5, which will pull from the list of neighbors
    public bool tooFarUp = false;
    public bool tooFarDown = false;
    public bool tooFarLeft = false;
    public bool tooFarRight = false;
    public int oldTrait;
    public int newTrait;
    public string modify;
    public Random rand;
    
    public bool DefaultIsValidNeighbor(GameTile tile)
    {
        return true;
    }

    public WorldGenWalker(GameTile[][] tiles, GameTile startGameTile, string _modify, int _oldTrait, int _newTrait, Random Rand) //initializing variables
    {
        rand = Rand;
        this.tiles = tiles;
        currGameTile = startGameTile;
        oldTrait = _oldTrait;
        newTrait = _newTrait;
        modify = _modify;
        direction = rand.NextInt(0, 6);
    }

    /*
     * the walker will take a step
     * changes the tile it is currently standing on,
     * then move in the current direction and sets a new random number
     * as its new direction
     *
     * several fail safes in place:
     * if the currGameTile is null, which will happen
     * if the walker traverses off of the map,
     * it will start again from a new completely random tile
     *
     * if the direction would take the walker onto land or off the map,
     * it looks for a valid neighbor that is neither null nor land
     *(the above is done through the findNewNeighbor function)
     *
     * if all else fails it just takes a random step which will
     * sometimes take it off of the map, in which case the first failsafe
     * triggers
     *
     * returns true if it modified a tile, false if it didn't 
     */ 
    public bool Move() 
    {
        if (currGameTile == null || rand.NextInt(0, 100) > 98) // 1% chance for walker to go rogue
        {
            int randomX = rand.NextInt(0, tiles.Length);
            int randomY = rand.NextInt(0, tiles[0].Length);
            currGameTile = tiles[randomX][randomY]; //teleports to a completely random spot
            return false;
        }
        GameTile?[] neighbors = GameTile.GetNeighborsArray(tiles, currGameTile.x, currGameTile.y);
        GameTile? currNeighbor = neighbors[direction];

        switch (modify)
        {
            case "biome":
                if (currNeighbor == null || (int)currNeighbor.biome != oldTrait)
                {
                    currNeighbor = findNewNeighbor(neighbors);
                    if (currNeighbor == null)
                    {
                        currGameTile = neighbors[direction];
                        direction = RandomDirectionNotNearEdges();
                        return false;
                    }
                }

                break;
            case "terrain":
                if (currNeighbor == null || (int)currNeighbor.terrain != oldTrait || currNeighbor.biome == Biome.Ocean)
                {
                    currNeighbor = findNewNeighbor(neighbors);
                    if (currNeighbor == null || currNeighbor.biome == Biome.Desert)
                    {
                        currGameTile = neighbors[direction];
                        direction = rand.NextInt(0, 6);
        
                        if (tooFarUp && (direction == 0 || direction == 1 || direction == 5)) //too far up causes re-roll for upward tiles
                        { 
                            direction = rand.NextInt(2, 5); // Pick one of three directions away from Up
                        }
        
                        if (tooFarDown && (direction == 2 || direction == 3 || direction == 4)) //too far down causes a re-roll for downward tiles
                        {
                            int[] choices = { 0, 1, 5 };
                            direction = choices[rand.NextInt(0, 3)]; // Pick one of three directions away from Down
                        }
                        if (tooFarLeft && (direction == 5 || direction == 4)) //too far left causes re-roll for leftward tiles
                        {
                            direction = rand.NextInt(1, 3); // Pick one of two directions away from Left
                        }
        
                        if (tooFarRight && (direction == 1 || direction == 2)) //too far right causes a re-roll for rightward tiles
                        {
                            direction = rand.NextInt(4, 6); // Pick one of two directions away from Right
                        }
                        return false;
                    }
                }

                break;
            case "feature":
                if (currNeighbor == null || currNeighbor.feature != (Feature)oldTrait)
                {
                    currNeighbor = findNewNeighbor(neighbors);
                    if (currNeighbor == null)
                    {
                        currGameTile = neighbors[direction];
                        direction = RandomDirectionNotNearEdges();


                        return false;
                    }
                }

                break;
        }

        switch (modify)
        {
            case "biome":
                currNeighbor.SetBiome((Biome)newTrait);
                break;
            case "terrain":
                currNeighbor.SetTerrain((Terrain)newTrait);
                break;
            case "feature":
                currNeighbor.SetFeature((Feature)newTrait);
                break;
        }
        
        currGameTile = neighbors[direction];
        direction = RandomDirectionNotNearEdges();
    
        return true;
    }

    private int RandomDirectionNotNearEdges()
    {
        int direction = rand.NextInt(0, 6);
        int maxAttempts = 4; // Prevent infinite loops
        int attempts = 0;
        int probability = rand.NextInt(0, 3);
        
        do
        {
            direction = rand.NextInt(0, 6);
            attempts++;
        }
        while (
            attempts < maxAttempts &&
            (
                (tooFarUp && (direction == 0 || direction == 1 || direction == 5)) || 
                (tooFarDown && (direction == 2 || direction == 3 || direction == 4)) || 
                (tooFarLeft && probability != 0 && (direction == 5 || direction == 4)) || 
                (tooFarRight && probability != 0 && (direction == 1 || direction == 2))
            )
        );

        return direction;
    }

    /*goes through the list of neighbors and finds all non-null water tiles
     stores all valid tiles in a linked list, then randomly selects one
     returns the randomly selected GameTile to be used as currNeighbor in move()
     */
    public GameTile findNewNeighbor(GameTile[] neighbors)
    {
        LinkedList<GameTile> validNeighbors = new LinkedList<GameTile>();

        switch (modify)
        {
            case "biome":
                foreach (GameTile neighbor in neighbors)
                {
                    if (neighbor != null && (int)neighbor.biome == oldTrait)
                    {
                        validNeighbors.AddLast(neighbor);
                    }
                }

                break;
            case "terrain":
                foreach (GameTile neighbor in neighbors)
                {
                    if (neighbor != null && neighbor.terrain == (Terrain)oldTrait && neighbor.biome != Biome.Ocean)
                    {
                        validNeighbors.AddLast(neighbor);
                    }
                }

                break;
            case "feature":
                foreach (GameTile neighbor in neighbors)
                {
                    if (neighbor != null && neighbor.feature == (Feature)oldTrait)
                    {
                        validNeighbors.AddLast(neighbor);
                    }
                }

                break;
        }
        
        for (int i = 0; i < rand.NextInt(0, validNeighbors.Count); i++)
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
