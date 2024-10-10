using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Random = Unity.Mathematics.Random;

public class WorldGenerator : MonoBehaviour
{
    private int _continents;
    public Random rand;
    
    /* Returns a fully generated game world. */
    public World GenerateWorld(int length, int height, int continents)
    {
        rand.InitState();
        int seed = UnityEngine.Random.Range(0, int.MaxValue);
        rand = new Random((uint) seed);
        _continents = continents;
        World world = new World(length, height);
        world.FillEmptyWorld(7);
        world.SetTileAdjacency();
        DetermineContinents(world);
        DetermineLand(world);
        DetermineBiomes(world);
        DetermineTerrain(world);
        DetermineRivers(world);
        DetermineFeatures(world);
        DetermineResources(world);

        return world;
    }
    
    /* Determine Continent Numbers */
    private void DetermineContinents(World world)
    {
        int worldLength = world.GetLength();
        int worldHeight = world.GetHeight();
    }

    /* Determine the area of Continents proportional to world size.
        - Put basic Plains tiles around the ContinentStartPoints
        - Randomly expand tiles around it to form continent. 
        - Stop when it reaches a % of world coverage.
     */
    
    private void DetermineLand(World world)
    {
        // Different Procedures given different numbers of continents
        int totalWorldSize = world.GetLength() * world.GetHeight();
        float desiredWorldCoverage = totalWorldSize * rand.NextFloat((float) .40, (float) .50); // Some random percentage of world size between 45-55%
        int currentWorldCoverage = 2; // How many Tiles have been turned to land so far.
        int probabilityThreshold = 35; // Base percentage of likelihood to NOT place Tile. (is increased by many factors)
        int consecutiveFailures = 0; // Keeps track of how many times the procedure has failed to place a Tile. (Makes it more likely to succeed if it failed a lot)
        int failureFactor = 12; // The probability factor power of each consecutive failure.
        
        switch (_continents)
        {
            case 1: // One Continent
                int StartX = world.GetLength()/2;
                int StartY = world.GetHeight()/2;
                int numWalkers = 5; //creating this many walkers
                WorldGenWalker[] walkers = new WorldGenWalker[numWalkers]; //see WorldGenWalker class
                GameTile startTile = world.GetTile(StartX, StartY); //start tile for walkers is just center of map
                //startTile.SetTerrain(2);
                
                for (int i = 0; i < numWalkers; i++)
                {
                    walkers[i] = new WorldGenWalker(world, startTile,"biome", 7, 1, rand); //fills list with walkers
                }
                
                while (currentWorldCoverage < desiredWorldCoverage)
                {
                    foreach (WorldGenWalker walker in walkers) //goes through all the walkers in the list
                    {
                        if (walker.move()) //the walker moves and if it returns true(made a land tile),
                        {
                            currentWorldCoverage++; //world coverage increases, otherwise keep iterating
                        }

                        if (walker.CurrTile != null)
                        {
                            if (walker.CurrTile.GetYPos() > world.GetHeight() * 0.7) //being above 70% of world height is too high
                            {
                                walker.tooFarUp = true;
                            }
                            else {
                                walker.tooFarUp = false;
                            }
                            if (walker.CurrTile.GetYPos() < world.GetHeight() * 0.3) //being above 30% of world height is too high
                            {
                                walker.tooFarDown = true;
                            }
                            else {
                                walker.tooFarDown = false;
                            }
                        }
                    }
                }
                break;
            case 2: // Two Continents
                // Determine the random X & Y starting points of 2 continents
    
                int continentStartXWest = rand.NextInt((int)(world.GetLength()  * .25), (int)(world.GetLength() * .35));
                int continentStartYWest = rand.NextInt((int)(world.GetHeight() * .25), (int)(world.GetHeight() * .75));
                int continentStartXEast = rand.NextInt((int)(world.GetLength() * .65), (int)(world.GetLength() * .75));
                int continentStartYEast = rand.NextInt((int)(world.GetHeight() * .25), (int)(world.GetHeight() * .75));


                // Store those X & Y in a ContinentStart Point for each Continent
                Point continentStart1 = new Point(continentStartXWest, continentStartYWest);
                Point continentStart2 = new Point(continentStartXEast, continentStartYEast);

                // Store some important factors
                // int totalWorldSize = world.GetLength() * world.GetHeight();
                // double desiredWorldCoverage = totalWorldSize * random.NextDouble(.40,.50); // Some random percentage of world size between 45-55%
                // int currentWorldCoverage = 2; // How many Tiles have been turned to land so far.
                // int probabilityThreshold = 35; // Base percentage of likelihood to NOT place Tile. (is increased by many factors)
                // int consecutiveFailures = 0; // Keeps track of how many times the procedure has failed to place a Tile. (Makes it more likely to succeed if it failed a lot)
                // int failureFactor = 12; // The probability factor power of each consecutive failure.

                // Instantiate a queue of Points (to reference the points of Tiles) Queues are lines - first come, first served
                Queue<Point> queue = new Queue<Point>();

                // Add all neighbors of the first continent to the queue
                foreach (GameTile t in world.GetTile(continentStart1).GetNeighbors())
                {
                    queue.Enqueue(new Point(t.GetXPos(), t.GetYPos()));
                }
                
                // Turn both continent starting points to land.
                world.ModifyTileBiome(continentStart1, 0);
                world.ModifyTileBiome(continentStart2, 0);
                
                // The percentage of land coverage that the first continent will take before switching to building the second.
                float continentSwitch = rand.NextFloat((float)0.4, (float)0.6);
                // Tells the while loop when the first continent is done.
                bool continentSwitched = false;
                
                // Stop when both continents have reached the desired LandCoverage
                while (currentWorldCoverage < desiredWorldCoverage)
                {
                    // If current coverage has reached the point to switch to the other continent
                    if (currentWorldCoverage >= desiredWorldCoverage * continentSwitch && !continentSwitched)
                    {
                        // Set to true so this does not repeat
                        continentSwitched = true;
                        // Clear the previous continent's queue
                        queue.Clear();
                        
                        // Add all the neighbors of continent #2 to the queue.
                        foreach (GameTile t in world.GetTile(continentStart2).GetNeighbors())
                        {
                            queue.Enqueue(new Point(t.GetXPos(), t.GetYPos()));
                        }
                    }

                    // Deque the first Tile
                    GameTile currentGameTile = world.GetTile(queue.Dequeue());
                    // Instantiate an empty List of possible neighbors
                    List<GameTile> possibleNeighbors = new List<GameTile>();

                    // If its neighbors are not null and are Ocean, store them in possibleNeighbors.
                    foreach (GameTile t in currentGameTile.GetNeighbors())
                    {
                        if (t is not null && t.GetBiome() == 7)
                        {
                            possibleNeighbors.Add(t);
                        }
                    }

                    while (possibleNeighbors.Count > 0)
                    {
                        // Randomly choose the next Neighbor Tile to expand to and set it to currentNeighbor
                        int nextNeighborIndex = rand.NextInt(0, possibleNeighbors.Count - 1);
                        // Reference to the current neighbor
                        GameTile currentNeighbor = possibleNeighbors[nextNeighborIndex];
                        // Store its location
                        Point neighborLocation = new Point(currentNeighbor.GetXPos(), currentNeighbor.GetYPos());
                        
                        // Probability - a random number from 1 to 100
                        int probability = rand.NextInt(0, 100); 
                        
                        // Set this to 1, at the extremes of the map to make it way more likely to stop tiles from spreading. 
                        int divisionFactor = 2;
                        
                        // Increases the closer currentNeighbor's Y is to 0 or worldHeight
                        int heightFactor; 
                        
                        if (currentNeighbor.GetYPos() < world.GetHeight() / 2)
                        {
                            if (currentNeighbor.GetYPos() < world.GetHeight() * .15)
                            {
                                divisionFactor = 1;
                            }
                            heightFactor = (world.GetHeight() / 2) - currentNeighbor.GetYPos() / divisionFactor;
                        }
                        else
                        {
                            if (currentNeighbor.GetYPos() > world.GetHeight() * .85)
                            {
                                divisionFactor = 1;
                            }
                            heightFactor = (currentNeighbor.GetYPos() - (world.GetHeight() / 2)) / divisionFactor;
                        }
                        
                        // Increases the closer currentNeighbor's X is to the center X of the world length.
                        int distanceToCenterFactor; 
                        
                        if (currentNeighbor.GetXPos() < world.GetLength() / 2)
                        {
                            if (currentNeighbor.GetXPos() > world.GetLength() * .40)
                            {
                                divisionFactor = 1;
                            }
                            distanceToCenterFactor = currentNeighbor.GetXPos() / divisionFactor;
                        }
                        else
                        {
                            if (currentNeighbor.GetXPos() < world.GetLength() * .60)
                            {
                                divisionFactor = 1;
                            }
                            distanceToCenterFactor = ((world.GetLength() - (currentNeighbor.GetXPos())) / divisionFactor);
                        }
                        // Increases the closer currentNeighbor's X is to 0 or to max world.Length
                        int distanceToEdgeFactor; 
                        if (currentNeighbor.GetXPos() < world.GetLength() / 2)
                        {
                            if (currentNeighbor.GetXPos() < world.GetLength() * .15)
                            {
                                divisionFactor = 1;
                            }
                            distanceToEdgeFactor = ((world.GetLength() / 2) - currentNeighbor.GetXPos()) / divisionFactor;
                        }
                        else
                        {
                            if (currentNeighbor.GetXPos() > world.GetLength() * .85)
                            {
                                divisionFactor = 1;
                            }
                            distanceToEdgeFactor = (currentNeighbor.GetXPos() - (world.GetLength() / 2)) / divisionFactor;
                        }
                        
                        // Add all the factors to the probability threshold. Roll a number from 0-100 and see if it expands the tile.
                        if (probability > probabilityThreshold + heightFactor + distanceToCenterFactor + distanceToEdgeFactor - (consecutiveFailures * failureFactor))
                        {
                            // Reset consecutive failures
                            consecutiveFailures = 0;
                            // Add the neighbor to our Point Queue
                            queue.Enqueue(neighborLocation);

                            // For Testing - need to see where the continent started
                            if (world.GetTile(neighborLocation).GetBiome() != 0)
                            {
                                // Modify the Tile's Biome 
                                world.ModifyTileBiome(neighborLocation, 1);
                            }

                            // Updates World Coverage
                            currentWorldCoverage++;
                        }
                        else
                        {
                            // Update consecutiveFailures
                            consecutiveFailures++;
                        }
                        // Once it's been processed remove it from the possibleNeighbors list.
                        possibleNeighbors.Remove(currentNeighbor);
                    }
                }
                break;
            case 3: // Three Continents
                break;
        }
        DetermineBiomes(world);
    }
    

    /* Determine Biomes on landmasses and on Coasts */
    private void DetermineBiomes(World world)
    {
        // Convert Tiles at the North and South edges to snow.
        int northSnowLine = world.GetHeight() - (world.GetHeight() / 7);
        int southSnowLine = world.GetHeight() / 7;
        int totalSnowCoverage = world.GetHeight() * world.GetLength()/25;//% of world coverage in snow, bugged rn so it is higher than this number
        int currentSnowCoverage = 0;
        
        int numSnowStarts = rand.NextInt(10, 20);//10-20 random starting points for snow
        GameTile[] snowStarts = new GameTile[numSnowStarts];
        WorldGenWalker[] walkers = new WorldGenWalker[numSnowStarts];
        currentSnowCoverage+=numSnowStarts;
        for (int i = 0; i < numSnowStarts; i++)
        {
            if (rand.NextInt(0, 2) == 0)//50/50 chance to make a SnowStart at top or bottom
            {
                snowStarts[i] = world.GetTile(rand.NextInt(0, world.GetLength()), rand.NextInt(0, southSnowLine));
            }
            else
            {
                snowStarts[i] = world.GetTile(rand.NextInt(0, world.GetLength()), rand.NextInt(northSnowLine, world.GetHeight()));
            }
        }

        for (int i = 0; i < numSnowStarts; i++)
        {
            walkers[i] = new WorldGenWalker(world, snowStarts[i],"biome", 1, 5, rand);
        }

        while (currentSnowCoverage < totalSnowCoverage)
        {
            foreach (WorldGenWalker walker in walkers) //goes through all the walkers in the list
            {
                if (walker.move()) //the walker moves and if it returns true(made a snow tile),
                {
                    currentSnowCoverage++; //snow coverage increases, otherwise keep iterating
                }

                if (walker.CurrTile != null)
                {
                    if (walker.CurrTile.GetYPos() > world.GetHeight() / 2 && walker.CurrTile.GetYPos() < northSnowLine)
                    {
                        walker.tooFarDown = true;
                    }
                    else
                    {
                        walker.tooFarDown = false;
                    }

                    if (walker.CurrTile.GetYPos() < world.GetHeight() / 2 && walker.CurrTile.GetYPos() > southSnowLine)
                    {
                        walker.tooFarUp = true;
                    }
                    else
                    {
                        walker.tooFarUp = false;
                    }
                }
            }
        }
        
        // Convert all Plains Tiles adjacent to Snow into Tundra 
        int totalTundraCoverage = totalSnowCoverage * 3/2;
        int currentTundraCoverage = 0;
        int northTundraLine = northSnowLine - (world.GetHeight() / 12);
        int southTundraLine = southSnowLine + (world.GetHeight() / 12);
        for (int x = 0; x < world.GetLength(); x++)
        {
            for (int y = 0; y < world.GetHeight(); y++)
            {
                // Any Plains adjacent to Snow should be Tundra.
                if (world.GetTile(x, y).GetBiome() == 5)
                {
                    foreach (GameTile neighbor in world.GetTile(x, y).GetNeighbors())
                    {
                        if (neighbor is not null)
                        {
                            if (neighbor.GetBiome() == 1)
                            {
                                neighbor.SetBiome(3);
                                currentTundraCoverage++;
                            }
                        }
                    }
                }
            }
        }

        foreach (WorldGenWalker walker in walkers)
        {
            walker.newTrait = 3;
        }

        while (currentTundraCoverage < totalTundraCoverage)
        {
            foreach (WorldGenWalker walker in walkers) //goes through all the walkers in the list
            {
                if (walker.move()) //the walker moves and if it returns true(made a snow tile),
                {
                    currentTundraCoverage++; //tundra coverage increases, otherwise keep iterating
                }
                if (walker.CurrTile != null)
                {
                    if (walker.CurrTile.GetYPos() > world.GetHeight() / 2 && walker.CurrTile.GetYPos() < northTundraLine)
                    {
                        walker.tooFarDown = true;
                    }
                    else
                    {
                        walker.tooFarDown = false;
                    }

                    if (walker.CurrTile.GetYPos() < world.GetHeight() / 2 && walker.CurrTile.GetYPos() > southTundraLine)
                    {
                        walker.tooFarUp = true;
                    }
                    else
                    {
                        walker.tooFarUp = false;
                    }
                }
            }
        }
        
        int totalDesertCoverage = totalSnowCoverage/3;
        int currentDesertCoverage = 0;
        int northDesertLine = world.GetHeight()/2 + (world.GetHeight() / 8);
        int southDesertLine = world.GetHeight()/2 - (world.GetHeight() / 8);
        foreach (WorldGenWalker walker in walkers)
        {
            walker.newTrait = 4;
        }
        while (currentDesertCoverage < totalDesertCoverage)
        {
            foreach (WorldGenWalker walker in walkers) //goes through all the walkers in the list
            {
                if (walker.move()) //the walker moves and if it returns true(made a desert tile),
                {
                    currentDesertCoverage++; //desert coverage increases, otherwise keep iterating
                }
                if (walker.CurrTile != null)
                {
                    if (walker.CurrTile.GetYPos() > world.GetHeight() / 2 && walker.CurrTile.GetYPos() > northDesertLine)
                    {
                        walker.tooFarUp = true;
                    }
                    else
                    {
                        walker.tooFarUp = false;
                    }

                    if (walker.CurrTile.GetYPos() < world.GetHeight() / 2 && walker.CurrTile.GetYPos() < southDesertLine)
                    {
                        walker.tooFarDown = true;
                    }
                    else
                    {
                        walker.tooFarDown = false;
                    }
                }
            }
        }
        
        int totalGrassCoverage = totalSnowCoverage*2;
        int currentGrassCoverage = 0;
        foreach (WorldGenWalker walker in walkers)
        {
            walker.newTrait = 2;
        }
        while (currentGrassCoverage < totalGrassCoverage)
        {
            foreach (WorldGenWalker walker in walkers) //goes through all the walkers in the list
            {
                if (walker.move()) //the walker moves and if it returns true(made a grass tile),
                {
                    currentGrassCoverage++; //grass coverage increases, otherwise keep iterating
                }
            }
        }

        
        CleanUpTiles(world);
        
        
        // Add Coast Tiles
        for (int x = 0; x < world.GetLength(); x++)
        {
            for (int y = 0; y < world.GetHeight(); y++)
            {
                if (world.GetTile(x, y).GetBiome() == 7)
                {
                    foreach (GameTile neighbor in world.GetTile(x, y).GetNeighbors())
                    {
                        if (neighbor is not null && neighbor.GetBiome() != 7 && neighbor.GetBiome() != 6)
                        {
                            world.GetTile(x, y).SetBiome(6);
                        }
                    }
                }
            }
        }
        
    }

    /* Removes Snow, Tundra, and corrects Tile positions.  */
    public void CleanUpTiles(World world)
    {
        int totalLand = 0;
        int totalPlains = 0;
        int totalGrass = 0;
        int totalDesert = 0;
        
        
        // Calculate total tiles and types.
        for (int x = 0; x < world.GetLength(); x++)
        {
            for (int y = 0; y < world.GetHeight(); y++)
            {
                if (world.GetTile(x, y).GetBiome() != 6 || world.GetTile(x, y).GetBiome() != 7)
                {
                    totalLand++;
                }

                if (world.GetTile(x, y).GetBiome() == 1)
                {
                    totalPlains++;
                } else if (world.GetTile(x, y).GetBiome() == 2)
                {
                    totalGrass++;
                } else if (world.GetTile(x, y).GetBiome() == 4)
                {
                    totalDesert++;
                }
            }
        }
        
        // Change the Biomes on tiles
        for (int x = 0; x < world.GetLength(); x++)
        {
            for (int y = 0; y < world.GetHeight(); y++)
            {
                // If Tile is Tundra
                if (world.GetTile(x, y).GetBiome() == 3)
                {
                    if (y > world.GetHeight() * .25 && y < world.GetHeight() * .75)
                    {
                        world.GetTile(x, y).SetBiome(ChangeBiome());
                    } 
                } 
                // If Tile is Desert
                else if (world.GetTile(x, y).GetBiome() == 4)
                {
                    if (y < world.GetHeight() * .20 || y > world.GetHeight() * .80)
                    {
                        world.GetTile(x, y).SetBiome(ChangeBiome());
                    }
                    
                    // For any Desert that has Tundra adjacent to it. Change the Tile to something else.
                    foreach (GameTile neighbor in world.GetTile(x, y).GetNeighbors())
                    {
                        if (neighbor is not null && neighbor.GetBiome() == 3)
                        {
                            world.GetTile(x, y).SetBiome(ChangeBiome());
                        }
                    }
                }
                // If Tile is Snow 
                else if (world.GetTile(x, y).GetBiome() == 5)
                {
                    // And it's inside the restricted zone.
                    if (y > world.GetHeight() * .15 && y < world.GetHeight() * .85)
                    {
                        // Change it to either Grass or Plains depending on what is needed
                        world.GetTile(x, y).SetBiome(ChangeBiome());
                        continue;
                    }
                    
                    // For every Snow Tile's neighbors. If it's neighbors are not Tundra, change it
                    foreach (GameTile neighbor in world.GetTile(x, y).GetNeighbors())
                    {
                        if (neighbor is not null && neighbor.GetBiome() != 7 && neighbor.GetBiome() != 6 && neighbor.GetBiome() != 5)
                        {
                            neighbor.SetBiome(3);
                        }
                    }
                } 
                // If near the edges
                else if (y < world.GetHeight() * .15 || y > world.GetHeight() * .85)
                {
                    // And it's a Grass or Plains
                    if (world.GetTile(x, y).GetBiome() == 1 || world.GetTile(x, y).GetBiome() == 2)
                    {
                        // Make it Tundra
                        world.GetTile(x, y).SetBiome(3);
                    }
                }
                
                
            }
        }
        
        
        // Return the desired Biome type to match.
        int ChangeBiome()
        {
            if (totalPlains < totalGrass)
            {
                return 1;
            }
            return 2;
        }
        
        
    }

    /* Determine Hills and Mountains  */
    private void DetermineTerrain(World world)
    {
        // To be implemented
        int randomX = rand.NextInt(world.GetLength()/6, world.GetLength() * 5/6);
        int randomY = rand.NextInt(0, world.GetHeight());
        WorldGenWalker[] walkers = new WorldGenWalker[rand.NextInt(30, 40)];
        int longMountainCount = 0;
        for (int i = 0; i < walkers.Length; i++)
        {
            randomX = rand.NextInt(world.GetLength()/4, world.GetLength() * 3/4);
            randomY = rand.NextInt(0, world.GetHeight());
            walkers[i] = new WorldGenWalker(world, world.GetTile(randomX, randomY), "terrain", 0, 2, rand);
        }
        foreach (WorldGenWalker walker in walkers)
        {
            int mountainSize = 0;
            if (rand.NextInt(0, 2) == 0)
            {
                walker.tooFarUp = true;
            }
            else
            {
                walker.tooFarDown = true;
            }
            if (rand.NextInt(0, 3) ==  0)
            {
                walker.tooFarLeft = true;
            }
            else if (rand.NextInt(0, 3) == 1)
            {
                walker.tooFarRight = true;
            }

            int desiredMountainSize = rand.NextInt(3, 10);//random numbers, can be adjusted
            if (longMountainCount < rand.NextInt(3, 7))
            {
                desiredMountainSize+=rand.NextInt(20, 50);
            }
            while (mountainSize < desiredMountainSize)
            {
                if (walker.move())
                {
                    mountainSize++;
                }
            }

            longMountainCount++;
        }
    }
    
    /* Determine Rivers - From Mountains to Coast  */ 
    private void DetermineRivers(World world)
    {
        
    }
    
    /* Determine the features on Tiles. */
    private void DetermineFeatures(World world)
    {
        
    }

    /* Determine the Resources and Resource spread across the game world.  */
    private void DetermineResources(World world)
    {
        
    }
}
