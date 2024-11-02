using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;
using Random = Unity.Mathematics.Random;

public class WorldGenerator : MonoBehaviour
{
    private int _continents;
    public Random _random;
    
    /* Returns a fully generated game world. */
    public World GenerateWorld(int length, int height, int continents, uint seed, int civCount)
    {
        _random.InitState();
        _random = new Random(seed);
        _continents = continents;
        World world = new World(length, height);
        world.FillEmptyWorld(7);
        world.SetTileAdjacency();
        DetermineContinents(world);
        DetermineLand(world);
        DetermineBiomes(world);
        DetermineTerrain(world);
        DetermineRiversAndLakes(world);
        DetermineFeatures(world);
        DetermineResources(world);
        DetermineSpawnPoints(world, civCount);

        return world;
        
        /* Determine Continent Numbers */
        void DetermineContinents(World world)
        {
            int worldLength = world.GetLength();
            int worldHeight = world.GetHeight();
        }

        /* Determine the area of Continents proportional to world size. */
        void DetermineLand(World world)
        {
            // Different Procedures given different numbers of continents
            int totalWorldSize = world.GetLength() * world.GetHeight();
            float desiredWorldCoverage = totalWorldSize * _random.NextFloat((float) .40, (float) .50); // Some random percentage of world size between 45-55%
            int currentWorldCoverage = 2; // How many Tiles have been turned to land so far.
            int probabilityThreshold = 35; // Base percentage of likelihood to NOT place Tile. (is increased by many factors)
            int consecutiveFailures = 0; // Keeps track of how many times the procedure has failed to place a Tile. (Makes it more likely to succeed if it failed a lot)
            int failureFactor = 12; // The probability factor power of each consecutive failure.
            
            switch (_continents)
            {
                case 1: // Fractal Map
                    int StartX = world.GetLength()/2;
                    int StartY = world.GetHeight()/2;
                    int numWalkers = 5; //creating this many walkers
                    WorldGenWalker[] walkers = new WorldGenWalker[numWalkers]; //see WorldGenWalker class
                    GameTile startTile = world.GetTile(StartX, StartY); //start tile for walkers is just center of map
                    //startTile.SetTerrain(2);
                    
                    for (int i = 0; i < numWalkers; i++)
                    {
                        walkers[i] = new WorldGenWalker(world, startTile,"biome", 7, 1, _random); //fills list with walkers
                    }
                    
                    while (currentWorldCoverage < desiredWorldCoverage)
                    {
                        foreach (WorldGenWalker walker in walkers) //goes through all the walkers in the list
                        {
                            if (walker.Move()) //the walker moves and if it returns true(made a land tile),
                            {
                                currentWorldCoverage++; //world coverage increases, otherwise keep iterating
                            }

                            if (walker._currTile != null)
                            {
                                if (walker._currTile.GetYPos() > world.GetHeight() * 0.7) //being above 70% of world height is too high
                                {
                                    walker.tooFarUp = true;
                                }
                                else {
                                    walker.tooFarUp = false;
                                }
                                if (walker._currTile.GetYPos() < world.GetHeight() * 0.3) //being above 30% of world height is too high
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
                case 2: // Two Standard Continents
                    // Determine the random X & Y starting points of 2 continents
        
                    int continentStartXWest = _random.NextInt((int)(world.GetLength()  * .25), (int)(world.GetLength() * .35));
                    int continentStartYWest = _random.NextInt((int)(world.GetHeight() * .25), (int)(world.GetHeight() * .75));
                    int continentStartXEast = _random.NextInt((int)(world.GetLength() * .65), (int)(world.GetLength() * .75));
                    int continentStartYEast = _random.NextInt((int)(world.GetHeight() * .25), (int)(world.GetHeight() * .75));


                    // Store those X & Y in a ContinentStart Point for each Continent
                    Point continentStart1 = new Point(continentStartXWest, continentStartYWest);
                    Point continentStart2 = new Point(continentStartXEast, continentStartYEast);

                    // Set important factors for this world gen 
                    currentWorldCoverage = 2; // How many Tiles have been turned to land so far.
                    probabilityThreshold = 25; // Base percentage of likelihood to NOT place Tile. (is increased by many factors)
                    consecutiveFailures = 0; // Keeps track of how many times the procedure has failed to place a Tile. (Makes it more likely to succeed if it failed a lot)
                    failureFactor = 12; // The probability factor power of each consecutive failure.

                    // Instantiate a queue of Points (to reference the points of Tiles) Queues are lines - first come, first served
                    Queue<Point> queue = new Queue<Point>();

                    // Add all neighbors of the first continent to the queue
                    foreach (GameTile t in world.GetTile(continentStart1).GetNeighbors())
                    {
                        queue.Enqueue(new Point(t.GetXPos(), t.GetYPos()));
                    }
                    
                    // Turn both continent starting points to land.
                    world.ModifyTileBiome(continentStart1, 1);
                    world.ModifyTileBiome(continentStart2, 1);
                    
                    // The percentage of land coverage that the first continent will take before switching to building the second.
                    float continentSwitch = _random.NextFloat((float)0.4, (float)0.6);
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
                            int nextNeighborIndex = _random.NextInt(0, possibleNeighbors.Count - 1);
                            // Reference to the current neighbor
                            GameTile currentNeighbor = possibleNeighbors[nextNeighborIndex];
                            // Store its location
                            Point neighborLocation = new Point(currentNeighbor.GetXPos(), currentNeighbor.GetYPos());
                            
                            // Probability - a random number from 1 to 100
                            int probability = _random.NextInt(0, 100); 
                            
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
        void DetermineBiomes(World world)
        {
            // Convert Tiles at the North and South edges to snow.
            int northSnowLine = world.GetHeight() - (world.GetHeight() / 7);
            int southSnowLine = world.GetHeight() / 7;
            int totalSnowCoverage = world.GetHeight() * world.GetLength()/25;//% of world coverage in snow, bugged rn so it is higher than this number
            int currentSnowCoverage = 0;
            
            int numSnowStarts = _random.NextInt(10, 20);//10-20 random starting points for snow
            GameTile[] snowStarts = new GameTile[numSnowStarts];
            WorldGenWalker[] walkers = new WorldGenWalker[numSnowStarts];
            currentSnowCoverage+=numSnowStarts;
            for (int i = 0; i < numSnowStarts; i++)
            {
                if (_random.NextInt(0, 2) == 0)//50/50 chance to make a SnowStart at top or bottom
                {
                    snowStarts[i] = world.GetTile(_random.NextInt(0, world.GetLength()), _random.NextInt(0, southSnowLine));
                }
                else
                {
                    snowStarts[i] = world.GetTile(_random.NextInt(0, world.GetLength()), _random.NextInt(northSnowLine, world.GetHeight()));
                }
            }

            for (int i = 0; i < numSnowStarts; i++)
            {
                walkers[i] = new WorldGenWalker(world, snowStarts[i],"biome", 1, 5, _random);
            }

            while (currentSnowCoverage < totalSnowCoverage)
            {
                foreach (WorldGenWalker walker in walkers) //goes through all the walkers in the list
                {
                    if (walker.Move()) //the walker moves and if it returns true(made a snow tile),
                    {
                        currentSnowCoverage++; //snow coverage increases, otherwise keep iterating
                    }

                    if (walker._currTile != null)
                    {
                        if (walker._currTile.GetYPos() > world.GetHeight() / 2 && walker._currTile.GetYPos() < northSnowLine)
                        {
                            walker.tooFarDown = true;
                        }
                        else
                        {
                            walker.tooFarDown = false;
                        }

                        if (walker._currTile.GetYPos() < world.GetHeight() / 2 && walker._currTile.GetYPos() > southSnowLine)
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
                    if (walker.Move()) //the walker moves and if it returns true(made a snow tile),
                    {
                        currentTundraCoverage++; //tundra coverage increases, otherwise keep iterating
                    }
                    if (walker._currTile != null)
                    {
                        if (walker._currTile.GetYPos() > world.GetHeight() / 2 && walker._currTile.GetYPos() < northTundraLine)
                        {
                            walker.tooFarDown = true;
                        }
                        else
                        {
                            walker.tooFarDown = false;
                        }

                        if (walker._currTile.GetYPos() < world.GetHeight() / 2 && walker._currTile.GetYPos() > southTundraLine)
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
                    if (walker.Move()) //the walker moves and if it returns true(made a desert tile),
                    {
                        currentDesertCoverage++; //desert coverage increases, otherwise keep iterating
                    }
                    if (walker._currTile != null)
                    {
                        if (walker._currTile.GetYPos() > world.GetHeight() / 2 && walker._currTile.GetYPos() > northDesertLine)
                        {
                            walker.tooFarUp = true;
                        }
                        else
                        {
                            walker.tooFarUp = false;
                        }

                        if (walker._currTile.GetYPos() < world.GetHeight() / 2 && walker._currTile.GetYPos() < southDesertLine)
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
                    if (walker.Move()) //the walker moves and if it returns true(made a grass tile),
                    {
                        currentGrassCoverage++; //grass coverage increases, otherwise keep iterating
                    }
                }
            }
            
            CorrectBiomes(world);
            
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
            
            /* This method is meant to be a scan to correct Biome placings.  */
        void CorrectBiomes(World world)
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
                    GameTile currTile = world.GetTile(x, y);
                    
                    if (currTile.IsLand())
                    {
                        totalLand++;
                    }

                    if (currTile.GetBiome() == 1)
                    {
                        totalPlains++;
                    } else if (currTile.GetBiome() == 2)
                    {
                        totalGrass++;
                    } else if (currTile.GetBiome() == 4)
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
                int newBiome;

                if (totalPlains < totalGrass)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
            
            
        }
            
        }

        /* Determine Hills and Mountains  */
        void DetermineTerrain(World world)
        {
            // Determine Mountain Ranges
            int randomX = _random.NextInt(world.GetLength()/4, world.GetLength() * 3/4);
            int randomY = _random.NextInt(0, world.GetHeight());
            WorldGenWalker[] walkers = new WorldGenWalker[_random.NextInt(3, 6)];
            
            int mountainSize = 0;
            int desiredMountainSize;
            for (int i = 0; i < walkers.Length; i++)
            {
                randomX = _random.NextInt(world.GetLength()/4, world.GetLength() * 3/4);
                randomY = _random.NextInt(0, world.GetHeight());
                walkers[i] = new WorldGenWalker(world, world.GetTile(randomX, randomY), "terrain", 0, 2, _random);
                Debug.Log("x = " + randomX + ", y =" + randomY);
            }
            foreach (WorldGenWalker walker in walkers)
            {
                if (_random.NextInt(0, 2) == 0)
                {
                    walker.tooFarUp = true;
                }
                else
                {
                    walker.tooFarDown = true;
                }
                if (_random.NextInt(0, 3) ==  0)
                {
                    walker.tooFarLeft = true;
                }
                else if (_random.NextInt(0, 3) == 1)
                {
                    walker.tooFarRight = true;
                }

                desiredMountainSize = _random.NextInt(100, 150);//random numbers, can be adjusted
                while (mountainSize < desiredMountainSize)
                {
                    if (walker.Move())
                    {
                        mountainSize++;
                    }
                }

                mountainSize = 0;
            }
            
            CleanUpMountains(world);

            void CleanUpMountains(World world)
            {
                // Instantiate list of all Mountain Tiles
                List<GameTile> mountains = new List<GameTile>();
                
                // Scan world
                for (int x = 0; x < world.GetLength(); x++)
                {
                    for (int y = 0; y < world.GetHeight(); y++)
                    {
                        GameTile currtile = world.GetTile(x, y);
                        // If Tile has a Moutain
                        if (currtile.GetTerrain() == 2)
                        {
                            mountains.Add(world.GetTile(x, y));

                            // If Tile is Coast or Ocean
                            if (!currtile.IsLand())
                            {
                                // Remove mountain
                                currtile.SetTerrain(0);
                            }
                            
                            // If Tile is Desert
                            if (currtile.GetBiome() == 4)
                            {
                                // Randomly set the Tile to either Flat or Hills
                                currtile.SetTerrain(_random.NextInt(0, 2));
                                
                                // If a neighbor is not null, is land, and is not desert
                                foreach (GameTile neighbor in currtile.GetNeighbors())
                                {
                                    if (neighbor is not null && neighbor.IsLand() && neighbor.GetBiome() != 4)
                                    {
                                        // Put a mountain next to it.
                                        neighbor.SetTerrain(2);
                                    }
                                }
                            }
                        }
                    }
                }
                
                // For all mountains in world
                foreach (GameTile mountain in mountains)
                {
                    // For all their neighbors
                    foreach (GameTile neighbor in mountain.GetNeighbors())
                    {
                        // If none of that mountain's neighbors are null or anything other than Mountains
                        if (neighbor is null || neighbor.GetTerrain() != 2)
                        {
                            break;
                        }
                        //Make them Flat. - This means this mountain was surrounded by mountains.
                        mountain.SetTerrain(0);
                        //neighbor.SetTerrain(0);
                    }
                }
                
                // For all mountains in world give a slight probabilty of deleting if it is adjacent to coast
                foreach (GameTile mountain in mountains)
                {
                    int coastalNeighbors = 0;
                    int coastalFactor = 30 * coastalNeighbors;
                    // For all their neighbors
                    foreach (GameTile neighbor in mountain.GetNeighbors())
                    {
                        
                        if (neighbor is not null && neighbor.GetBiome() == 6)
                        {
                            coastalNeighbors++;
                        }
                    }

                    if (_random.NextInt(0, 100) < 0 + coastalFactor)
                    {
                        mountain.SetTerrain(0);
                    }
                }
                
                List<GameTile> hills = new List<GameTile>();
                
                // Dot random Hills around the world - give every flat tile a 15% chance to spawn a Hill. 
                for (int x = 0; x < world.GetLength(); x++)
                {
                    for (int y = 0; y < world.GetHeight(); y++)
                    {
                        // If it's flat already, isn't Coast or Ocean
                        if (world.GetTile(x, y).GetTerrain() == 0 && world.GetTile(x, y).GetBiome() != 6 &&
                            world.GetTile(x, y).GetBiome() != 7)
                        {
                            // If next int is less than 15
                            if (_random.NextInt(0, 100) < 15)
                            {
                                // Maker it hills
                                world.GetTile(x, y).SetTerrain(1);
                                hills.Add(world.GetTile(x, y));
                            }
                        }
                    }
                }
                
                // For reach hill, give it another 15% chance it's neighbors will be hills
                foreach (GameTile hill in hills)
                {
                    foreach (GameTile neighbor in hill.GetNeighbors())
                    {
                        if (neighbor is not null && neighbor.GetTerrain() == 0 && neighbor.GetBiome() != 6 &&
                            neighbor.GetBiome() != 7)
                        {
                            if (_random.NextInt(0, 100) < 15)
                            {
                                neighbor.SetTerrain(1);
                            }
                        }
                    }
                }
            }
        }
        
        /* Determine Rivers & Lakes - From Mountains to Coast  */ 
        void DetermineRiversAndLakes(World world)
        {
            HashSet<GameTile> scannedTiles = new HashSet<GameTile>();
            
            // Empty List where we will store the Tiles to start rivers from.
            List<GameTile> riverStartLocations = new List<GameTile>();
            
            foreach (GameTile start in DetermineTilesWithinLandDistance(7))
            {
                SetRiverEdges(FormRiver(start, _random.NextInt(10, 15)));
            }

            foreach (GameTile start in DetermineTilesWithinLandDistance(5))
            {
                SetRiverEdges(FormRiver(start, _random.NextInt(7, 12)));
            }

            foreach (GameTile start in DetermineTilesWithinLandDistance(4))
            {
                SetRiverEdges(FormRiver(start, _random.NextInt(7, 9)));
            }
            
            foreach (GameTile start in DetermineTilesWithinLandDistance(3))
            {
                SetRiverEdges(FormRiver(start, _random.NextInt(5, 9)));
            }

            foreach (GameTile start in DetermineTilesWithinLandDistance(2))
            {
                SetRiverEdges(FormRiver(start, _random.NextInt(3, 7)));
            }

            List<GameTile> DetermineTilesWithinLandDistance(int maxDistance)
            {
                List<GameTile> validTiles = new List<GameTile>();

                for (int x = 0; x < world.GetLength(); x++)
                {
                    for (int y = 0; y < world.GetHeight(); y++)
                    {
                        GameTile currTile = world.GetTile(x, y);

                        if (scannedTiles.Contains(currTile)) continue;
                        
                        if (IsSurroundedByLand(currTile, maxDistance))
                        {
                            validTiles.Add(currTile);
                        }
                    }
                }
                return validTiles;
            }

            bool IsSurroundedByLand(GameTile tile, int maxDistance)
            {
                HashSet<GameTile> visited = new HashSet<GameTile>();
                Queue<(GameTile tile, int distance)> queue = new Queue<(GameTile tile, int distance)>();
                queue.Enqueue((tile, 0));
                visited.Add(tile);
                
                List<GameTile> localTilesScanned = new List<GameTile>();

                while (queue.Count > 0)
                {
                    var (currentTile, distance) = queue.Dequeue();
                    
                    // If we've reached max distance, stop checking further neighbors
                    if (distance > maxDistance) continue;

                    // If the current tile has already been scanned, stop
                    if (scannedTiles.Contains(currentTile))
                    {
                        return false; // This tile intersects with a previous valid tile's radius
                    }
                    
                    // If the current tile is not land, return false
                    if (!currentTile.IsLand())
                    {
                        return false;
                    }
                    
                    // Add to local list of tiles being scanned for this search
                    localTilesScanned.Add(currentTile);
                    
                    //Enqueue the neighbors to continue exploring within the distance limit.
                    GameTile[] neighbors = currentTile.GetNeighbors();
                    foreach (GameTile neighbor in neighbors)
                    {
                        if (neighbor is not null && !visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue((neighbor, distance + 1));
                        }
                    }
                }

                foreach (GameTile gameTile in localTilesScanned)
                {
                    scannedTiles.Add(gameTile);
                }
                
                // If all tiles within the distance are land, return true
                return true;
            }
            
            // Clear extra rivers that are not necessary
            ClearTShapedRiverIntersections();
            ClearTShapedRiverIntersections();
            ClearTShapedRiverIntersections();
            
            // Make sure Rivers end at Coast
            FixEdgesAtCoasts();
            
            // Set FreshWaterAccess to true for all Tiles adjacent to River.
            AssignRemainingFreshWaterAccess(world);
            
            /* Returns a List(Path) of Tiles to create a River */
            List<GameTile> FormRiver(GameTile start, int riverLength)
            {
                List<GameTile> tileList = new List<GameTile>();
                
                int currentLength = 0;
                int prevEdge = _random.NextInt(0, 6);
                int nextEdge;
                int[] possibleNeighbors = new int[3];
                
                // Set it's edge on.
                start.SetRiverEdge(prevEdge, true);
                // Add the initial Tile to the List
                tileList.Add(start);
                
                bool wentLeft = false;
                bool wentRight = false;
                
                GameTile currTile = start;
                // Until we reach the river's lenght, keep adding tiles to the List to make river.
                for (int i = 0; i < riverLength; i++)
                {
                    // Determine the edge for the river to expand to (it should always be adjacent to the previous edge)
                    if (prevEdge == 0)
                    {
                        possibleNeighbors[0] = 5;
                        possibleNeighbors[1] = 0;
                        possibleNeighbors[2] = 1;
                    } else if (prevEdge == 5)
                    {
                        possibleNeighbors[0] = 4;
                        possibleNeighbors[1] = 5;
                        possibleNeighbors[2] = 0;
                    }
                    else
                    {
                        possibleNeighbors[0] = prevEdge - 1;
                        possibleNeighbors[1] = prevEdge;
                        possibleNeighbors[2] = prevEdge + 1;
                    }
                    
                    // Determine a new random edge within the right parameters
                    if (wentLeft)
                    {
                        nextEdge = possibleNeighbors[_random.NextInt(1,possibleNeighbors.Length)];
                        wentLeft = false;
                    } else if (wentRight)
                    {
                        nextEdge = possibleNeighbors[_random.NextInt(possibleNeighbors.Length - 1)];
                        wentRight = false;
                    }
                    else
                    {
                        nextEdge = possibleNeighbors[_random.NextInt(possibleNeighbors.Length)];
                    }

                    GameTile nextTile = currTile.GetNeighbors()[nextEdge];

                    // If the next Tile is null, return List and end river path.
                    if (nextTile is null)
                    {
                        return tileList;
                    }
                    
                    // Add it to the list
                    tileList.Add(currTile.GetNeighbors()[nextEdge]);
                    // Set the current Tile's river Adjacency to true.
                    currTile.SetFreshWaterAccess(true);
                    currTile.SetRiverAdjacency(true);
                    
                    // Check if we are currently adjacent to a Coast.
                    foreach (GameTile neighbor in currTile.GetNeighbors())
                    {
                        if (neighbor is not null)
                        {
                            // If so
                            if (neighbor.GetBiome() == 6 || neighbor.GetBiome() == 7)
                            { 
                                // Deactivate previous edge on this tile.
                                currTile.SetRiverEdge(prevEdge, false);
                            
                                // End the River
                                return tileList;
                            }
                        }
                        else
                        {
                            // Deactivate previous edge on this tile.
                            currTile.SetRiverEdge(prevEdge, false);
                            
                            return tileList;
                        }
                        
                    }
                    
                    // Update currTile to the next neighbor
                    currTile = currTile.GetNeighbors()[nextEdge];
                    
                    // Update what the direction of previous edge was.
                    if (nextEdge == 0 && prevEdge == 5)
                    {
                        wentLeft = true;
                    } else if (nextEdge < prevEdge)
                    {
                        wentLeft = true;
                    }

                    if (nextEdge == 5 && prevEdge == 0)
                    {
                        wentRight = true;
                    } else if (nextEdge > prevEdge)
                    {
                        wentRight = true;
                    }
                    
                    prevEdge = nextEdge;
                }
                
                // Return a List (path) of Tiles for the river.
                return tileList;
            }
            
            /* Takes a List (Path) of Tiles and sets the edges in order to create a River. */
            void SetRiverEdges(List<GameTile> riverPath)
            {
                if (riverPath.Count == 1)
                {
                    return;
                }
                
                for (int i = 0; i < riverPath.Count; i++)
                {
                    GameTile currentTile = riverPath[i];

                    // Handle first tile
                    if (i == 0)
                    {
                        GameTile nextTile = riverPath[i + 1];
                        int nextTileEdgeIndex = GetSharedEdgeIndex(currentTile, nextTile);
                        currentTile.SetRiverEdge(nextTileEdgeIndex, true);
                    }
                    // Handle last tile
                    else if (i == riverPath.Count - 1)
                    {
                        GameTile previousTile = riverPath[i - 1];
                        int prevTileEdgeIndex = GetSharedEdgeIndex(currentTile, previousTile);
                        currentTile.SetRiverEdge(prevTileEdgeIndex, true);
                    }
                    // Handle middle tiles
                    else
                    {
                        GameTile previousTile = riverPath[i - 1];
                        GameTile nextTile = riverPath[i + 1];
                
                        int prevTileEdgeIndex = GetSharedEdgeIndex(currentTile, previousTile);
                        int nextTileEdgeIndex = GetSharedEdgeIndex(currentTile, nextTile);
                        
                        // Now connect the shared edges insided the tile
                        ConnectInternalEdges(currentTile, prevTileEdgeIndex, nextTileEdgeIndex);
                    }
                }
            }
            
            // Determines the shared edge between two tiles
            int GetSharedEdgeIndex(GameTile currentTile, GameTile nextTile)
            {
                // Iterate over the neighbors array to find the shared edge
                for (int i = 0; i < 6; i++)
                {
                    if (currentTile.GetNeighbors()[i] == nextTile)
                    {
                        return i;
                    }
                }

                // Return -1 if no shared edge is found (this should not happen if the path is valid)
                return -1;
            }
            
            // Sets the River Edges in a hexagon to true from startEdge to endEdge.
            void ConnectInternalEdges(GameTile tile, int startEdge, int endEdge)
            {
                // Clockwise or counterclockwise distance
                int clockwiseDistance = (endEdge - startEdge + 6) % 6;
                int counterClockwiseDistance = (startEdge - endEdge + 6) % 6;
                int edgeStartIndex = 0;
                
                // Decide which direction to connect the edges - random 50% chance
                if (_random.NextInt(0, 3) > 0)
                {
                    if (clockwiseDistance < counterClockwiseDistance)
                    {
                        
                        // Set Clockwise edges
                        for (int i = edgeStartIndex; i <= clockwiseDistance; i++)
                        {
                        
                            int edgeToSet = (startEdge + i) % 6;
                            tile.SetRiverEdge(edgeToSet, true);
                        }
                    }
                    else if (clockwiseDistance > counterClockwiseDistance)
                    {
                        // Set Clockwise edges
                        for (int i = edgeStartIndex; i < clockwiseDistance; i++)
                        {
                        
                            int edgeToSet = (startEdge + i) % 6;
                            tile.SetRiverEdge(edgeToSet, true);
                        }
                    }
                    else
                    {
                        // Set Clockwise edges
                        for (int i = edgeStartIndex; i < clockwiseDistance; i++)
                        {
                        
                            int edgeToSet = (startEdge + i) % 6;
                            tile.SetRiverEdge(edgeToSet, true);
                        }
                    }
                }
                else
                {
                    if (counterClockwiseDistance < clockwiseDistance)
                    {
                          
                        // Set Counter-clockwise edges
                        for (int i = edgeStartIndex; i < counterClockwiseDistance; i++)
                        {
                            int edgeToSet = (startEdge - i + 6) % 6;
                            tile.SetRiverEdge(edgeToSet, true);
                        }
                    }
                    else if (counterClockwiseDistance > clockwiseDistance)
                    {
                        // Set Counter-clockwise edges
                        for (int i = edgeStartIndex; i < counterClockwiseDistance; i++)
                        {
                            int edgeToSet = (startEdge - i + 6) % 6;
                            tile.SetRiverEdge(edgeToSet, true);
                        }
                    } else 
                    {
                        // Set Counter-clockwise edges
                        for (int i = edgeStartIndex; i < counterClockwiseDistance; i++)
                        {
                            int edgeToSet = (startEdge - i + 6) % 6;
                            tile.SetRiverEdge(edgeToSet, true);
                        }
                    }
                }
            }
            
            /* Does a final scan over every river and makes sure there are no needless T shaped river segments. */
            void ClearTShapedRiverIntersections()
            {
                // Scan the world
                for (int x = 0; x < world.GetLength(); x++)
                {
                    for (int y = 0; y < world.GetHeight(); y++)
                    {
                        GameTile currTile = world.GetTile(x, y);

                        // If a Tile has River adjacency
                        if (currTile.GetFreshWaterAccess())
                        {
                            // For each of its neighbors
                            foreach (GameTile neighbor in currTile.GetNeighbors())
                            {
                                // If they have river adjacency
                                if (neighbor is not null && neighbor.GetFreshWaterAccess())
                                {
                                    // Set to true by default.
                                    bool sameEdges = true;
                                    
                                    List<int> edgesSetToTrue = new List<int>();
                                    // Add all the current Tile's river edges set to True to a list.
                                    for (int index = 0; index < 6; index++)
                                    {
                                        if (currTile.GetRiverEdge(index) && index != GetSharedEdgeIndex(currTile, neighbor))
                                        {
                                            edgesSetToTrue.Add(index);
                                        }
                                    }
                                    
                                    // If neighbor has those same edges set to true as well
                                    foreach (int edge in edgesSetToTrue)
                                    {
                                        if (!neighbor.GetRiverEdge(edge))
                                        {
                                            sameEdges = false;
                                        }
                                    }

                                    if (sameEdges)
                                    {
                                        // Deactivate their shared edge on both Tile's ends.
                                        currTile.SetRiverEdge(GetSharedEdgeIndex(currTile, neighbor), false);
                                        neighbor.SetRiverEdge(GetSharedEdgeIndex(neighbor, currTile), false);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            /* Corrects any river segments past coasts. */
            void FixEdgesAtCoasts()
            {
                // Scan the world
                for (int x = 0; x < world.GetLength(); x++)
                {
                    for (int y = 0; y < world.GetHeight(); y++)
                    {
                        GameTile currTile = world.GetTile(x, y);
                        bool isAdjacentToCoat = false;

                        // If a Tile has River Adjacency
                        if (currTile.GetFreshWaterAccess())
                        {
                            int index = 0;
                            int edgeAdjacentToCoast;
                            // Figure out where each edge is at
                            
                            foreach (GameTile neighbor in currTile.GetNeighbors())
                            {
                                // if Neighbor is Coast
                                if (neighbor is not null && neighbor.GetBiome() == 6)
                                {
                                    // Set to true
                                    isAdjacentToCoat = true;
                                    // EdgeAdjacentToCoast
                                    edgeAdjacentToCoast = index;
                                    // Set it to False
                                    currTile.SetRiverEdge(edgeAdjacentToCoast, false);
                                }
                                index++;
                            }

                            if (isAdjacentToCoat)
                            {
                                // Check through the Tile's river edges
                                for (int i = 0; i < 6; i++)
                                {
                                    // If this edge is Set to True.
                                    if (currTile.GetRiverEdge(i))
                                    {
                                        // If this edge is 5 and the Tile towards this edge is not in the River Path
                                        if (i == 5 && !currTile.GetNeighbors()[5].GetFreshWaterAccess())
                                        {
                                            // If Both it's neighboring edges don't lead to a Tile on the River Path
                                            if (!currTile.GetNeighbors()[4].GetFreshWaterAccess() && !currTile.GetNeighbors()[0].GetFreshWaterAccess())
                                            {
                                                // Then set the isolated river edge to false.
                                                currTile.SetRiverEdge(5, false);
                                            }
                                        } 
                                        // If this edge is 0 and the Tile towards this edge is not in the River Path
                                        else if (i == 0 && !currTile.GetNeighbors()[0].GetFreshWaterAccess())
                                        {
                                            // If Both it's neighboring edges don't lead to a Tile on the River Path
                                            if (!currTile.GetNeighbors()[5].GetFreshWaterAccess() && !currTile.GetNeighbors()[1].GetFreshWaterAccess())
                                            {
                                                // Then set the isolated river edge to false.
                                                currTile.SetRiverEdge(0, false);
                                            }
                                        }
                                        // If any other number and the Tile towards this edge is not in the River Path
                                        else if (!currTile.GetNeighbors()[i].GetFreshWaterAccess())
                                        {
                                            // If Both it's neighboring edges don't lead to a Tile on the River Path
                                            if (!currTile.GetNeighbors()[i - 1].GetFreshWaterAccess() && !currTile.GetNeighbors()[i + 1].GetFreshWaterAccess())
                                            {
                                                // Then set the isolated river edge to false.
                                                currTile.SetRiverEdge(i, false);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        
                        
                    }
                }
            }
            
            /* Determine Lakes */
            void DetermineLakes()
            {
                
            }

            /* Makes sure that all Tile's adjacent to River's have Fresh Water Access set to True.  */
            void AssignRemainingFreshWaterAccess(World world)
            {
                for (int x = 0; x < world.GetLength(); x++)
                {
                    for (int y = 0; y < world.GetHeight(); y++)
                    {
                        GameTile currTile = world.GetTile(x, y);
                        
                        if (currTile.GetFreshWaterAccess())
                        {
                            int edge = 0; // Keep track of which neighbor
                            foreach (GameTile neighbor in currTile.GetNeighbors())
                            {
                                if (neighbor is not null && currTile.GetRiverEdge(edge) && neighbor.IsLand())
                                {
                                    // Set the opposing edge to true
                                    currTile.GetNeighbors()[edge].SetRiverEdge((edge + 3) % 6, true);
                                    // Set the neighbor's fresh water access to true.
                                    currTile.GetNeighbors()[edge].SetFreshWaterAccess(true);
                                    currTile.GetNeighbors()[edge].SetRiverAdjacency(true);
                                }
                                edge++;
                            }

                            // Check if the Tile has river edges.
                            bool hasRiverEdges = false;
                            for (int i = 0; i < 6; i++)
                            {
                                if (currTile.GetRiverEdge(i))
                                {
                                    hasRiverEdges = true;
                                    break;
                                }
                            }
                            
                            // If it doesn't 
                            if (!hasRiverEdges)
                            {
                                // Remove it's fresh water access
                                currTile.SetFreshWaterAccess(false);
                                currTile.SetRiverAdjacency(false);
                            }
                        }
                        // If it's an inland Coast tile
                        else if (currTile.GetBiome() == 6)
                        {
                            bool isSurrounderByLand = true;

                            foreach (GameTile neighbor in currTile.GetNeighbors())
                            {
                                if (neighbor is not null && !neighbor.IsLand())
                                {
                                    isSurrounderByLand = false;
                                }
                            }

                            if (isSurrounderByLand)
                            {
                                // Set it to Lake
                                currTile.SetBiome(8);

                                // Give all of it's neighbor that are land fresh water access
                                foreach (GameTile neighbor in currTile.GetNeighbors())
                                {
                                    if (neighbor is not null && neighbor.IsLand())
                                    {
                                        neighbor.SetFreshWaterAccess(true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        /* Determine the features on Tiles. */
        void DetermineFeatures(World world)
        {
            DetermineWoods();
            DetermineFloodPlains();
            DetermineMarshes();
            DetermineRainforest();
            DetermineOasis();
            
            void DetermineWoods()
            {
                Queue<GameTile> woodsQueue = new Queue<GameTile>();
                
                SpreadAFewRandomWoods();
                
                void SpreadAFewRandomWoods() 
                {
                    for (int x = 0; x < world.GetLength(); x++)
                    {
                        for (int y = 0; y < world.GetHeight(); y++)
                        {
                            GameTile currTile = world.GetTile(x, y);
                            
                            // If it is Land, and it is not Snow, and it is not Desert, and it doesn't have a Mountain
                            if (currTile.IsLand() && currTile.GetBiome() != 5 && currTile.GetBiome() != 4 &&
                                currTile.GetTerrain() != 2)
                            {
                                int probability = _random.NextInt(0, 100);

                                // 10% chance 
                                if (probability < 15)
                                {
                                    // Add a Woods
                                    currTile.SetFeature(1);
                                    // Add it to our Queue
                                    woodsQueue.Enqueue(currTile);
                                }
                            }
                        }
                    }
                }
                
                // Small chance to spread to nearby Woods
                while (woodsQueue.Count > 0)
                {
                    GameTile currTile = woodsQueue.Dequeue();

                    foreach (GameTile neighbor in currTile.GetNeighbors())
                    {
                        // If neighbor is not null, if it is land, if it is not a Mountain, if it is not Snow or Desert
                        if (neighbor is not null && neighbor.IsLand() && neighbor.GetTerrain() != 2 &&
                            neighbor.GetBiome() != 5 && neighbor.GetBiome() != 4)
                        {
                            int probability = _random.NextInt(0, 100);
                            // 15% chance.
                            if (probability < 50)
                            {
                                currTile.SetFeature(1);
                            }
                        }
                    }
                }
                
            }
            
            void DetermineFloodPlains()
            {
                Queue<GameTile> floodPlainsQueue = new Queue<GameTile>();
                
                PlaceAFewFloodplains();
                
                void PlaceAFewFloodplains()
                {
                    for (int x = 0; x < world.GetLength(); x++)
                    {
                        for (int y = 0; y < world.GetHeight(); y++)
                        {
                            GameTile currTile = world.GetTile(x, y);

                            // If the Tile is adjacent to River, if it's Desert, and if it's Flat
                            if (currTile.GetRiverAdjacency() && currTile.GetBiome() == 4 && currTile.GetTerrain() == 0)
                            {
                                // Set it to Foodplains
                                currTile.SetFeature(2);
                            } 
                            // If the Tile is adjacent to River, if it's Plains or Grassland, and if it's Flat
                            else if (currTile.GetRiverAdjacency() &&
                                     (currTile.GetBiome() == 1 || currTile.GetBiome() == 2) && currTile.GetTerrain() == 0)
                            {

                                bool isAdjacentToAnotherFloodplain = false;
                                foreach (GameTile neighbor in currTile.GetNeighbors())
                                {
                                    if (neighbor is not null && neighbor.GetFeature() == 2)
                                    {
                                        isAdjacentToAnotherFloodplain = true;
                                    }
                                }

                                if (!isAdjacentToAnotherFloodplain)
                                {
                                    int probability = _random.NextInt(0, 100);
                                    // 80% chance
                                    if (probability < 10)
                                    {
                                        // Set it to Floodplains
                                        currTile.SetFeature(2);
                                        // Add it to Floodplains Queue
                                        floodPlainsQueue.Enqueue(currTile);
                                    }
                                }
                            }
                        }
                    }
                }

                while (floodPlainsQueue.Count > 0)
                {
                    GameTile currTile = floodPlainsQueue.Dequeue();

                    foreach (GameTile neighbor in currTile.GetNeighbors())
                    {
                        // If neighbor is not null, if its land, if it's next to a river, if it's plains or grassland, and if its flat.
                        if (neighbor is not null && neighbor.GetRiverAdjacency() && neighbor.IsLand() &&
                            (neighbor.GetBiome() == 2 || neighbor.GetBiome() == 1) && neighbor.GetTerrain() == 0 )
                        {
                            int probability = _random.NextInt(0, 100);
                            // 50% Chance to spread to neighbor
                            if (probability < 50)
                            {
                                neighbor.SetFeature(2);
                            }
                        }
                    }
                }
            }

            void DetermineMarshes()
            {
                for (int x = 0; x < world.GetLength(); x++)
                {
                    for (int y = 0; y < world.GetHeight(); y++)
                    {
                        GameTile currTile = world.GetTile(x, y);

                        // If the Tile is Grassland and Flat.
                        if (currTile.GetBiome() == 2 && currTile.GetTerrain() == 0 && !currTile.GetRiverAdjacency())
                        {
                            int probability = _random.NextInt(0, 100);

                            // 5% Chance
                            if (probability < 5)
                            {
                                // Set it to Marsh
                                currTile.SetFeature(3);
                            }
                        }
                    }
                }
            }

            void DetermineRainforest()
            {
                for (int x = 0; x < world.GetLength(); x++)
                {
                    for (int y = 0; y < world.GetHeight(); y++)
                    {
                        GameTile currTile = world.GetTile(x, y);

                        if (currTile.GetBiome() == 1 && currTile.GetTerrain() != 2 && !currTile.GetRiverAdjacency())
                        {
                            int probability = _random.NextInt(0, 100);
                            
                            // If it's right on the Ecuator
                            if (y < world.GetHeight() * .525 && y > world.GetHeight() * .475)
                            {
                                // 80% Chance
                                if (probability < 80)
                                {
                                    currTile.SetFeature(4);
                                }
                            }
                            else
                            {
                                // Chance decreases relative to distance to equator
                                int distanceToEquator = Math.Abs(y - (world.GetHeight() / 2));
                                // Multiply each distance by this.
                                int distanceFactor = distanceToEquator * 5;
                                if (probability + distanceFactor < 60)
                                {
                                    currTile.SetFeature(4);
                                }
                            }
                            
                            
                            
                            
                            
                        }
                    }
                }
            }

            void DetermineOasis()
            {
                for (int x = 0; x < world.GetLength(); x++)
                {
                    for (int y = 0; y < world.GetHeight(); y++)
                    {
                        GameTile currTile = world.GetTile(x, y);

                        // If it's Desert, if It's Flat, and if it's not on a River.
                        if (currTile.GetBiome() == 4 && currTile.GetTerrain() == 0 && !currTile.GetRiverAdjacency())
                        {
                            int probability = _random.NextInt(0, 100);

                            // 15% Chance to Spawn Oasis
                            if (probability < 15)
                            {
                                currTile.SetFeature(5);
                            }
                        }
                    }
                }
            }
        }

        /* Determine the Resources and Resource spread across the game world.  */
        void DetermineResources(World world)
        {
            
        }
        
        /* Determines Spawn Points */
        void DetermineSpawnPoints(World world, int civCount)
        {
            List<Point> validPoints = new List<Point>();
            List<Point> spawnPoints = new List<Point>();
            
            for (int x = 0; x < world.GetLength(); x++) //looking through all tiles near middle vertically
            {
                for (int y = world.GetHeight()/4; y < world.GetHeight() * 3/4; y++)
                {
                    GameTile currTile = world.GetTile(x, y);
                    if (currTile.GetRiverAdjacency() && currTile.IsLand() && currTile.GetTerrain() != 2)
                    {
                        validPoints.Add(new Point(x, y)); //adds tile if 1. next to river 2. is land 3.is not mountain
                    }
                }
            }
            int randomNum = _random.NextInt(0, validPoints.Count); 
            spawnPoints.Add(validPoints[randomNum]);//set a random first spawn

            double eucDist(Point p1, Point p2) //calculates the Euclidean distance between 2 points
            {
                int x = p2.x - p1.x;
                int y = p2.y - p1.y;
                return Math.Abs(Math.Sqrt(x * x + y * y));
            }

            while (civCount > spawnPoints.Count) //while there needs to be more spawns
            {
                double maxDist = 0; //updates every while loop
                Point nextPoint = spawnPoints[0]; //updates every while loop, point to be added
                foreach (Point vPoint in validPoints) //goes through all valid tiles,
                {
                    List<double> dist = new List<double>();
                    foreach (Point sPoint in spawnPoints)
                    {
                        dist.Add(eucDist(vPoint, sPoint)); //creates a list of distances between the current valid point
                    }                                       // and all the current spawn points

                    if (dist.Min() > maxDist) //if the lowest of those distances is > max dist
                    {
                        maxDist = dist.Min(); //updates max dist
                        nextPoint = vPoint; //updates nextpoint
                    }
                }
                spawnPoints.Add(nextPoint); //adds the next point that is as far from every spawn as possible
            }

            foreach (Point sPoint in spawnPoints)
            {
                GameTile currTile = world.GetTile(sPoint); //for all spawnpoints do something
                
            }
        }
    }
}
