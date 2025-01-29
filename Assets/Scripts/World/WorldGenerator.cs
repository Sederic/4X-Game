using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class WorldGenerator {
    private int continents;
    private static Random random;
    private Tile[][] tiles;
    
    /* Returns a fully generated game world. */
    public Tile[][] GenerateWorld(int length, int height, int continents, uint seed)
    {
        random.InitState();
        random = new Random(seed);
        this.continents = continents;

        tiles = new Tile[height][];
        for (int i=0;  i<height; i++) {
            tiles[i] = new Tile[length];
            for (int j=0; j<length; j++) {
                tiles[i][j] = new Tile(i, j, Biome.Ocean); // ocean start
            }
        }

        DetermineContinents();
        DetermineLand();
        Debug.Log("done land");
        DetermineBiomes();
        Debug.Log("done biomes");
        DetermineTerrain();
        Debug.Log("done terrain");
        DetermineRiversAndLakes();
        Debug.Log("done rivers and lakes");
        DetermineFeatures();
        DetermineResources();

        return tiles;
    }
    
    /* Determine Continent Numbers */
    private void DetermineContinents()
    {
        // int worldLength = tiles[0].Length;
        // int worldHeight = tiles.Length;
    }

    /* Determine the area of Continents proportional to world size. */
    private void DetermineLand()
    {
        // Different Procedures given different numbers of continents
        int totalWorldSize = tiles[0].Length * tiles.Length;
        float desiredWorldCoverage = totalWorldSize * random.NextFloat(0.4f, 0.5f); // Some random percentage of world size between 45-55%
        int currentWorldCoverage = 2; // How many Tiles have been turned to land so far.
        int probabilityThreshold = 35; // Base percentage of likelihood to NOT place Tile. (is increased by many factors)
        int consecutiveFailures = 0; // Keeps track of how many times the procedure has failed to place a Tile. (Makes it more likely to succeed if it failed a lot)
        int failureFactor = 12; // The probability factor power of each consecutive failure.
        
        switch (continents)
        {
            case 1: // Fractal Map
                int StartX = tiles.Length/2;
                int StartY = tiles[0].Length/2;
                int numWalkers = 5; //creating this many walkers
                WorldGenWalker[] walkers = new WorldGenWalker[numWalkers]; //see WorldGenWalker class
                Tile startTile = tiles[StartX][StartY]; //start tile for walkers is just center of map
                
                for (int i = 0; i < numWalkers; i++)
                {
                    walkers[i] = new WorldGenWalker(tiles, startTile, "biome", 7, 1, random); //fills list with walkers
                }
                
                while (currentWorldCoverage < desiredWorldCoverage)
                {
                    foreach (WorldGenWalker walker in walkers) //goes through all the walkers in the list
                    {
                        if (walker.Move()) //the walker moves and if it returns true(made a land tile),
                        {
                            currentWorldCoverage++; //world coverage increases, otherwise keep iterating
                        }

                        // being above 70% of world height is too high
                        walker.tooFarUp = (walker.currTile.x > tiles.Length * 0.7);

                        // being below 30% of world height is too low
                        walker.tooFarDown = (walker.currTile.x < tiles.Length * 0.3);
                    }
                }
                break;
            case 2: // Two Standard Continents
                // Determine the random X & Y starting points of 2 continents
                int continentStart1X = random.NextInt((int)(tiles.Length  * .25), (int)(tiles.Length * .75));
                int continentStart1Y = random.NextInt((int)(tiles[0].Length * .25), (int)(tiles[0].Length * .35));
                int continentStart2X = random.NextInt((int)(tiles.Length * .25), (int)(tiles.Length * .75));
                int continentStart2Y = random.NextInt((int)(tiles[0].Length * .65), (int)(tiles[0].Length * .75));

                // Store those X & Y in a ContinentStart Point for each Continent
                Point continentStart1 = new Point(continentStart1X, continentStart1Y);
                Point continentStart2 = new Point(continentStart2X, continentStart2Y);

                // Set important factors for this world gen 
                currentWorldCoverage = 2; // How many Tiles have been turned to land so far.
                probabilityThreshold = 15; // Base percentage of likelihood to NOT place Tile. (is increased by many factors)
                consecutiveFailures = 0; // Keeps track of how many times the procedure has failed to place a Tile. (Makes it more likely to succeed if it failed a lot)
                failureFactor = 12; // The probability factor power of each consecutive failure.

                // Instantiate a queue of Points (to reference the points of Tiles) Queues are lines - first come, first served
                Queue<Point> queue = new Queue<Point>();

                // Add all neighbors of the first continent to the queue
                foreach (Tile? t in Tile.GetNeighborsArray(tiles, continentStart1.x, continentStart1.y))
                {
                    if (t is not null) queue.Enqueue(new Point(t.x, t.y));
                }
                
                // Turn both continent starting points to land.
                tiles[continentStart1.x][continentStart1.y].SetBiome(Biome.Plains);
                tiles[continentStart2.x][continentStart2.y].SetBiome(Biome.Plains);
                
                // The percentage of land coverage that the first continent will take before switching to building the second.
                float continentSwitch = random.NextFloat(0.4f, 0.6f);
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
                        foreach (Tile? t in Tile.GetNeighborsArray(tiles, continentStart2.x, continentStart2.y))
                        {
                            if (t is not null) queue.Enqueue(new Point(t.x, t.y));
                        }
                    }

                    // Deque the first Tile
                    Point dequed = queue.Dequeue();
                    Tile currentTile = tiles[dequed.x][dequed.y];

                    // Instantiate an empty List of possible neighbors
                    List<Tile> possibleNeighbors = new List<Tile>();
                    foreach (Tile t in Tile.GetNeighborsArray(tiles, currentTile.x, currentTile.y))
                    {
                        if (t is not null && t.biome == Biome.Ocean)
                        {
                            possibleNeighbors.Add(t);
                        }
                    }

                    while (possibleNeighbors.Count > 0)
                    {
                        // Randomly choose the next Neighbor Tile to expand to and set it to currentNeighbor
                        int nextNeighborIndex = random.NextInt(0, possibleNeighbors.Count);
                        // Reference to the current neighbor
                        Tile currentNeighbor = possibleNeighbors[nextNeighborIndex];
                        // Store its location
                        Point neighborLocation = new Point(currentNeighbor.x, currentNeighbor.y);
                        
                        // Probability - a random number from 1 to 100
                        int probability = random.NextInt(0, 100); 
                        
                        // Set this to 1, at the .xtremes of the map to make it way more likely to stop tiles from spreading. 
                        int divisionFactor = 2;
                        
                        // Increases the closer currentNeighbor's Y is to 0 or worldHeight
                        divisionFactor = ((currentNeighbor.x < tiles.Length * .15) || (currentNeighbor.x > tiles.Length * .85)) ? 1 : 2;
                        int heightFactor = Math.Abs(tiles.Length / 2 - currentNeighbor.x) / divisionFactor;
                        
                        // Increases the closer currentNeighbor's X is to the center X of the world length.
                        divisionFactor = (currentNeighbor.y > tiles[0].Length * 0.4 && currentNeighbor.y < tiles[0].Length * 0.6) ? 1 : 2;
                        int distanceToCenterFactor = Math.Min(currentNeighbor.y, tiles[0].Length - currentNeighbor.y) / divisionFactor;
                        
                        // Increases the closer currentNeighbor's X is to 0 or to max world.Length
                        divisionFactor = (currentNeighbor.y < tiles[0].Length * 0.15 || currentNeighbor.y > tiles[0].Length * 0.85) ? 1 : 2;
                        int distanceToEdgeFactor = Math.Max(currentNeighbor.y, tiles[0].Length - currentNeighbor.y) / divisionFactor; 
                        
                        // Add all the factors to the probability threshold. Roll a number from 0-100 and see if it expands the tile.
                        if (probability > probabilityThreshold + heightFactor + distanceToCenterFactor + distanceToEdgeFactor - (consecutiveFailures * failureFactor))
                        {
                            consecutiveFailures = 0;
                            queue.Enqueue(neighborLocation);
                            tiles[neighborLocation.x][neighborLocation.y].SetBiome(Biome.Plains);
                            currentWorldCoverage++;
                        }
                        else
                        {
                            consecutiveFailures++;
                        }

                        // Once it's been processed remove it from the possibleNeighbors list.
                        possibleNeighbors.RemoveAt(nextNeighborIndex);
                    }
                }
                break;
            case 3: // Three Continents
                break;
        }
    }
    
    /* Determine Biomes on landmasses and on Coasts */
    private void DetermineBiomes()
    {
        /* Snow */
        // Determine Snow world boundaries
        int northSnowLine = tiles.Length - (tiles.Length / 7);
        int southSnowLine = tiles.Length / 7;
        // Track's total Snow coverage in world
        int desiredSnowCoverage = tiles.Length * tiles[0].Length/25; //% of world coverage in snow, bugged rn so it is higher than this number
        int currentSnowCoverage = 0;
        // Determines 10-20 random starting points for snow
        int numSnowStarts = random.NextInt(10, 20);
        Tile[] snowStarts = new Tile[numSnowStarts];
        WorldGenWalker[] walkers = new WorldGenWalker[numSnowStarts];
        
        /* Tundra */
        // Determine Tundra world boundaries
        int northTundraLine = northSnowLine - (tiles.Length / 12);
        int southTundraLine = southSnowLine + (tiles.Length / 12);
        // Convert all Plains Tiles adjacent to Snow into Tundra 
        int desiredTundraCoverage = desiredSnowCoverage * 3/2;
        int currentTundraCoverage = 0;
        
        /* Desert */
        // Determine Desert world boundaries
        int northDesertLine = tiles.Length/2 + (tiles.Length / 8);
        int southDesertLine = tiles.Length/2 - (tiles.Length / 8);
        // Track's total Desert coverage in World
        int desiredDesertCoverage = desiredSnowCoverage/3;
        int currentDesertCoverage = 0;
        
        /* Grassland */
        // Track's total Grassland coverage in World
        int desiredGrassCoverage = desiredSnowCoverage*2;
        int currentGrassCoverage = 0;
        
        DetermineSnow();
        DetermineTundra();
        DetermineDesert();
        DetermineGrassland();
        DetermineCoasts();
        CorrectBiomes();
        
        void DetermineSnow()
        {
            // Add the Walker starting points to currSnowCoverage
            currentSnowCoverage+=numSnowStarts;
            
            // Determine Snow Walker starts
            for (int i = 0; i < numSnowStarts; i++)
            {
                if (random.NextInt(0, 2) == 0)//50/50 chance to make a SnowStart at top or bottom
                {
                    snowStarts[i] = tiles[random.NextInt(0, southSnowLine)][random.NextInt(0, tiles[0].Length)];
                }
                else
                {
                    snowStarts[i] = tiles[random.NextInt(northSnowLine, tiles.Length)][random.NextInt(0, tiles[0].Length)];
                }
            }

            // Instantiate Walkers
            for (int i = 0; i < numSnowStarts; i++)
            {
                walkers[i] = new WorldGenWalker(tiles, snowStarts[i],"biome", 1, 5, random);
            }

            // Convert Tiles to Snow until we reach the desired world coverage
            while (currentSnowCoverage < desiredSnowCoverage)
            {
                foreach (WorldGenWalker walker in walkers) //goes through all the walkers in the list
                {
                    if (walker.Move()) //the walker moves and if it returns true(made a snow tile),
                    {
                        currentSnowCoverage++; //snow coverage increases, otherwise keep iterating
                    }

                    if (walker.currTile != null)
                    {
                        if (walker.currTile.x > tiles.Length / 2 && walker.currTile.x < northSnowLine)
                        {
                            walker.tooFarDown = true;
                        }
                        else
                        {
                            walker.tooFarDown = false;
                        }

                        if (walker.currTile.x < tiles.Length / 2 && walker.currTile.x > southSnowLine)
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
        }
        
        void DetermineTundra()
        {
            // Scan World to change any Plains adjacent to Snow to Tundra.
            for (int x = 0; x < tiles.Length; x++)
            {
                for (int y = 0; y < tiles[0].Length; y++)
                {
                    if (tiles[x][y].biome == Biome.Snow)
                    {
                        foreach (Tile? neighbor in Tile.GetNeighborsArray(tiles, x, y))
                        {
                            if (neighbor?.biome == Biome.Plains)
                            {
                                neighbor.SetBiome(Biome.Tundra);
                                currentTundraCoverage++;
                            }
                        }
                    }
                }
            }

            // Set Walkers to change biome into Tundra
            foreach (WorldGenWalker walker in walkers)
            {
                walker.newTrait = (int)Biome.Tundra;
            }
            // Convert tiles to Tundra until we have achieved total desired coverage
            while (currentTundraCoverage < desiredTundraCoverage)
            {
                foreach (WorldGenWalker walker in walkers) //goes through all the walkers in the list
                {
                    if (walker.Move()) //the walker moves and if it returns true(made a snow tile),
                    {
                        currentTundraCoverage++; //tundra coverage increases, otherwise keep iterating
                    }
                    if (walker.currTile != null)
                    {
                        if (walker.currTile.x > tiles.Length / 2 && walker.currTile.x < northTundraLine)
                        {
                            walker.tooFarDown = true;
                        }
                        else
                        {
                            walker.tooFarDown = false;
                        }

                        if (walker.currTile.x < tiles.Length / 2 && walker.currTile.x > southTundraLine)
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
        }
        
        void DetermineDesert()
        {
            // Set Walkers to change biome into Desert
            foreach (WorldGenWalker walker in walkers)
            {
                walker.newTrait = (int)Biome.Desert;
            }
            
            // Convert Tiles to Desert until achieved desired coverage
            while (currentDesertCoverage < desiredDesertCoverage)
            {
                foreach (WorldGenWalker walker in walkers) //goes through all the walkers in the list
                {
                    if (walker.Move()) //the walker moves and if it returns true(made a desert tile),
                    {
                        currentDesertCoverage++; //desert coverage increases, otherwise keep iterating
                    }

                    if (walker.currTile is not null) {
                        walker.tooFarUp = (walker.currTile.x > (double)tiles.Length / 2 && walker.currTile.x > northDesertLine);
                        walker.tooFarDown = (walker.currTile.x < (double)tiles.Length / 2 && walker.currTile.x < southDesertLine);
                    }
                }
            }
        }
        
        void DetermineGrassland()
        {
            // Change Walkers to change biome to Grassland
            foreach (WorldGenWalker walker in walkers)
            {
                walker.newTrait = 2;
            }
            
            // Convert Tiles to Grassland until achieved desired coverage
            while (currentGrassCoverage < desiredGrassCoverage)
            {
                foreach (WorldGenWalker walker in walkers) //goes through all the walkers in the list
                {
                    if (walker.Move()) //the walker moves and if it returns true(made a grass tile),
                    {
                        currentGrassCoverage++; //grass coverage increases, otherwise keep iterating
                    }
                }
            }
        }
        
        void DetermineCoasts()
        {
            // Scan World and change any land tile adjacent to ocean into Coast.
            for (int x = 0; x < tiles.Length; x++)
            {
                for (int y = 0; y < tiles[0].Length; y++)
                {
                    if (tiles[x][y].biome == Biome.Ocean)
                    {
                        foreach (Tile? neighbor in Tile.GetNeighborsArray(tiles, x, y))
                        {
                            if (neighbor is not null && neighbor.IsLand())
                            {
                                tiles[x][y].SetBiome(Biome.Ocean);
                            }
                        }
                    }
                }
            }
        }
        
        /* This method is meant to be a world scan to correct Biome placings.  */
        void CorrectBiomes()
        {
            int totalLand = 0;
            int totalPlains = 0;
            int totalGrass = 0;
            int totalDesert = 0;
            
            // Add up total tiles and types.
            for (int x = 0; x < tiles.Length; x++)
            {
                for (int y = 0; y < tiles[0].Length; y++)
                {
                    Tile currTile = tiles[x][y];
                    
                    if (currTile.IsLand())
                    {
                        totalLand++;
                    }

                    if (currTile.biome == Biome.Plains)
                    {
                        totalPlains++;
                    } else if (currTile.biome == Biome.Grassland)
                    {
                        totalGrass++;
                    } else if (currTile.biome == Biome.Desert)
                    {
                        totalDesert++;
                    }
                }
            }
            
            // Change the Biomes on tiles based on their positions
            for (int x = 0; x < tiles.Length; x++)
            {
                for (int y = 0; y < tiles[0].Length; y++)
                {
                    Tile currTile = tiles[x][y];

                    // If Tile is Tundra
                    if (currTile.biome == Biome.Tundra)
                    {
                        if (x > tiles.Length * .25 && x < tiles.Length * .75)
                        {
                            currTile.SetBiome(ChangeBiome());
                        } 
                    } 
                    // If Tile is Desert
                    else if (currTile.biome == Biome.Desert)
                    {
                        if (x < tiles.Length * .20 || x > tiles.Length * .80)
                        {
                            currTile.SetBiome(ChangeBiome());
                        }
                        
                        // For any Desert that has Tundra adjacent to it. Change the Tile to something else.
                        foreach (Tile? neighbor in Tile.GetNeighborsArray(tiles, currTile.x, currTile.y))
                        {
                            if (neighbor is not null && neighbor.biome == Biome.Tundra)
                            {
                                currTile.SetBiome(ChangeBiome());
                            }
                        }
                    }
                    // If Tile is Snow 
                    else if (currTile.biome == Biome.Snow)
                    {
                        // And it's inside the restricted zone.
                        if (x > tiles.Length * .15 && x < tiles.Length * .85)
                        {
                            // Change it to either Grass or Plains depending on what is needed
                            currTile.SetBiome(ChangeBiome());
                            continue;
                        }
                        
                        // For every Snow Tile's neighbors. If it's neighbors are not Tundra, change it
                        foreach (Tile? neighbor in Tile.GetNeighborsArray(tiles, x, y))
                        {
                            if (neighbor is not null && neighbor.biome != Biome.Ocean && neighbor.biome != Biome.Coast && neighbor.biome != Biome.Snow)
                            {
                                neighbor.SetBiome(Biome.Tundra);
                            }
                        }
                    } 
                    // If near the edges
                    else if (x < tiles.Length * .15 || x > tiles.Length * .85)
                    {
                        // And it's a Grass or Plains
                        if (tiles[x][y].biome == Biome.Plains || tiles[x][y].biome == Biome.Grassland)
                        {
                            // Make it Tundra
                            tiles[x][y].SetBiome(Biome.Tundra);
                        }
                    }
                }
            }
            
            // Return the desired Biome type to match.
            Biome ChangeBiome()
            {
                if (totalPlains < totalGrass)
                {
                    return Biome.Plains;
                }
                return Biome.Grassland;
            }
            
            
        }
        
    }

    /* Determine Hills and Mountains  */
    private void DetermineTerrain()
    {
        // Determine Mountain Ranges
        int randomX, randomY;
        WorldGenWalker[] walkers = new WorldGenWalker[random.NextInt(3, 6)];
        
        int mountainSize = 0;
        int desiredMountainSize;
        for (int i = 0; i < walkers.Length; i++)
        {
            randomX = random.NextInt(0, tiles.Length);
            randomY = random.NextInt(tiles[0].Length/4, tiles[0].Length * 3/4);
            walkers[i] = new WorldGenWalker(tiles, tiles[randomX][randomY], "terrain", 0, 2, random);
            Debug.Log("x = " + randomX + ", y =" + randomY);
        }
        foreach (WorldGenWalker walker in walkers)
        {
            if (random.NextInt(0, 2) == 0)
            {
                walker.tooFarUp = true;
            }
            else
            {
                walker.tooFarDown = true;
            }
            if (random.NextInt(0, 3) ==  0)
            {
                walker.tooFarLeft = true;
            }
            else if (random.NextInt(0, 3) == 1)
            {
                walker.tooFarRight = true;
            }

            desiredMountainSize = random.NextInt(100, 150);//random numbers, can be adjusted
            while (mountainSize < desiredMountainSize)
            {
                if (walker.Move())
                {
                    mountainSize++;
                }
            }

            mountainSize = 0;
        }
        
        CleanUpMountains();

        void CleanUpMountains()
        {
            // Instantiate list of all Mountain Tiles
            List<Tile> mountains = new List<Tile>();
            
            // Scan world
            for (int x = 0; x < tiles.Length; x++)
            {
                for (int y = 0; y < tiles[0].Length; y++)
                {
                    Tile currTile = tiles[x][y];
                    // If Tile has a Moutain
                    if (currTile.terrain == Terrain.Mountain)
                    {
                        mountains.Add(currTile);

                        // If Tile is Coast or Ocean
                        if (!currTile.IsLand())
                        {
                            // Remove mountain
                            currTile.SetTerrain(Terrain.Flat);
                        }
                        
                        // If Tile is Desert
                        if (currTile.biome == Biome.Desert)
                        {
                            // Randomly set the Tile to either Flat or Hills
                            currTile.SetTerrain((Terrain)random.NextInt(0, 2));
                            
                            // If a neighbor is not null, is land, and is not desert
                            foreach (Tile? neighbor in Tile.GetNeighborsArray(tiles, currTile.x, currTile.y))
                            {
                                if (neighbor is not null && neighbor.IsLand() && neighbor.biome != Biome.Desert)
                                {
                                    // Put a mountain next to it.
                                    neighbor.SetTerrain(Terrain.Mountain);
                                }
                            }
                        }
                    }
                }
            }
            
            // For all mountains in world
            foreach (Tile mountain in mountains)
            {
                // For all their neighbors
                foreach (Tile? neighbor in Tile.GetNeighborsArray(tiles, mountain.x, mountain.y))
                {
                    // If none of that mountain's neighbors are null or anything other than Mountains
                    if (neighbor is null || neighbor.terrain != Terrain.Mountain)
                    {
                        break;
                    }
                    //Make them Flat. - This means this mountain was surrounded by mountains.
                    mountain.SetTerrain(Terrain.Flat);
                }
            }
            
            // For all mountains in world give a slight probabilty of deleting if it is adjacent to coast
            foreach (Tile mountain in mountains)
            {
                int coastalNeighbors = 0;
                int coastalFactor = 30 * coastalNeighbors;
                // For all their neighbors
                foreach (Tile? neighbor in Tile.GetNeighborsArray(tiles, mountain.x, mountain.y))
                {
                    
                    if (neighbor is not null && neighbor.biome == Biome.Coast)
                    {
                        coastalNeighbors++;
                    }
                }

                if (random.NextInt(0, 100) < 0 + coastalFactor)
                {
                    mountain.SetTerrain(Terrain.Flat);
                }
            }
            
            List<Tile> hills = new List<Tile>();
            
            // Dot random Hills around the world - give every flat tile a 15% chance to spawn a Hill. 
            for (int x = 0; x < tiles.Length; x++)
            {
                for (int y = 0; y < tiles[0].Length; y++)
                {
                    // If it's flat already, isn't Coast or Ocean
                    if (tiles[x][y].terrain == Terrain.Flat && tiles[x][y].biome != Biome.Coast &&
                        tiles[x][y].biome != Biome.Ocean)
                    {
                        // If next int is less than 15
                        if (random.NextInt(0, 100) < 15)
                        {
                            // Maker it hills
                            tiles[x][y].SetTerrain(Terrain.Hill);
                            hills.Add(tiles[x][y]);
                        }
                    }
                }
            }
            
            // For reach hill, give it another 15% chance it's neighbors will be hills
            foreach (Tile hill in hills)
            {
                foreach (Tile? neighbor in Tile.GetNeighborsArray(tiles, hill.x, hill.y))
                {
                    if (neighbor is not null && neighbor.terrain == Terrain.Flat && neighbor.biome != Biome.Coast &&
                        neighbor.biome != Biome.Ocean)
                    {
                        if (random.NextInt(0, 100) < 15)
                        {
                            neighbor.SetTerrain(Terrain.Hill);
                        }
                    }
                }
            }
        }
    }
    
    /* Determine Rivers & Lakes - From Mountains to Coast  */ 
    private void DetermineRiversAndLakes()
    {
        HashSet<Tile> scannedTiles = new HashSet<Tile>();
        
        foreach (Tile start in DetermineTilesWithinLandDistance(7))
        {
            SetRiverEdges(FormRiver(start, random.NextInt(10, 15)));
        }

        foreach (Tile start in DetermineTilesWithinLandDistance(5))
        {
            SetRiverEdges(FormRiver(start, random.NextInt(7, 12)));
        }

        foreach (Tile start in DetermineTilesWithinLandDistance(4))
        {
            SetRiverEdges(FormRiver(start, random.NextInt(7, 9)));
        }
        
        foreach (Tile start in DetermineTilesWithinLandDistance(3))
        {
            SetRiverEdges(FormRiver(start, random.NextInt(5, 9)));
        }

        foreach (Tile start in DetermineTilesWithinLandDistance(2))
        {
            SetRiverEdges(FormRiver(start, random.NextInt(3, 7)));
        }

        List<Tile> DetermineTilesWithinLandDistance(int maxDistance)
        {
            List<Tile> validTiles = new List<Tile>();

            for (int x = 0; x < tiles.Length; x++)
            {
                for (int y = 0; y < tiles[0].Length; y++)
                {
                    Tile currTile = tiles[x][y];

                    if (scannedTiles.Contains(currTile)) continue;
                    
                    if (IsSurroundedByLand(currTile, maxDistance))
                    {
                        validTiles.Add(currTile);
                    }
                }
            }
            return validTiles;
        }

        bool IsSurroundedByLand(Tile tile, int maxDistance)
        {
            HashSet<Tile> visited = new HashSet<Tile>();
            Queue<(Tile tile, int distance)> queue = new Queue<(Tile tile, int distance)>();
            queue.Enqueue((tile, 0));
            visited.Add(tile);
            
            List<Tile> localTilesScanned = new List<Tile>();

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
                
                //Enqueue the neighbors to continue .xploring within the distance limit.
                Tile?[] neighbors = Tile.GetNeighborsArray(tiles, currentTile.x, currentTile.y);
                foreach (Tile? neighbor in neighbors)
                {
                    if (neighbor is not null && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, distance + 1));
                    }
                }
            }

            foreach (Tile gameTile in localTilesScanned)
            {
                scannedTiles.Add(gameTile);
            }
            
            // If all tiles within the distance are land, return true
            return true;
        }
        
        // Clear .xtra rivers that are not necessary
        ClearTShapedRiverIntersections();
        ClearTShapedRiverIntersections();
        ClearTShapedRiverIntersections();
        
        // Make sure Rivers end at Coast
        FixEdgesAtCoasts();
        
        ClearTShapedRiverIntersections();
        
        // Set FreshWaterAccess to true for all Tiles adjacent to River.
        AssignRemainingFreshWaterAccess();
        
        /* Returns a List(Path) of Tiles to create a River */
        List<Tile> FormRiver(Tile start, int riverLength)
        {
            List<Tile> tileList = new List<Tile>();
            
            int currentLength = 0;
            int prevEdge = random.NextInt(0, 6);
            int nextEdge;
            int[] possibleNeighbors = new int[3];
            
            // Set it's edge on.
            start.SetRiverEdge(prevEdge, true);
            // Add the initial Tile to the List
            tileList.Add(start);
            
            bool wentLeft = false;
            bool wentRight = false;
            
            Tile currTile = start;
            // Until we reach the river's lenght, keep adding tiles to the List to make river.
            for (int i = 0; i < riverLength; i++)
            {
                // Determine the edge for the river to .xpand to (it should always be adjacent to the previous edge)
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
                    nextEdge = possibleNeighbors[random.NextInt(1,possibleNeighbors.Length)];
                    wentLeft = false;
                } else if (wentRight)
                {
                    nextEdge = possibleNeighbors[random.NextInt(possibleNeighbors.Length - 1)];
                    wentRight = false;
                }
                else
                {
                    nextEdge = possibleNeighbors[random.NextInt(possibleNeighbors.Length)];
                }

                Tile? nextTile = Tile.GetNeighborsArray(tiles, currTile.x, currTile.y)[nextEdge];

                // If the next Tile is null, return List and end river path.
                if (nextTile is null)
                {
                    return tileList;
                }
                
                // Add it to the list
                tileList.Add(nextTile);
                // Set the current Tile's river Adjacency to true.
                currTile.SetHasFreshWater(true);
                
                // Check if we are currently adjacent to a Coast.
                foreach (Tile? neighbor in Tile.GetNeighborsArray(tiles, currTile.x, currTile.y))
                {
                    if (neighbor is not null)
                    {
                        // If so
                        if (neighbor.biome == Biome.Coast || neighbor.biome == Biome.Ocean)
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
                currTile = Tile.GetNeighborsArray(tiles, currTile.x, currTile.y)[nextEdge];
                
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
        void SetRiverEdges(List<Tile> riverPath)
        {
            if (riverPath.Count == 1)
            {
                return;
            }
            
            for (int i = 0; i < riverPath.Count; i++)
            {
                Tile currentTile = riverPath[i];

                // Handle first tile
                if (i == 0)
                {
                    Tile nextTile = riverPath[i + 1];
                    int nextTileEdgeIndex = GetSharedEdgeIndex(currentTile, nextTile);
                    currentTile.SetRiverEdge(nextTileEdgeIndex, true);
                }
                // Handle last tile
                else if (i == riverPath.Count - 1)
                {
                    Tile previousTile = riverPath[i - 1];
                    int prevTileEdgeIndex = GetSharedEdgeIndex(currentTile, previousTile);
                    currentTile.SetRiverEdge(prevTileEdgeIndex, true);
                }
                // Handle middle tiles
                else
                {
                    Tile previousTile = riverPath[i - 1];
                    Tile nextTile = riverPath[i + 1];
            
                    int prevTileEdgeIndex = GetSharedEdgeIndex(currentTile, previousTile);
                    int nextTileEdgeIndex = GetSharedEdgeIndex(currentTile, nextTile);
                    
                    // Now connect the shared edges insided the tile
                    ConnectInternalEdges(currentTile, prevTileEdgeIndex, nextTileEdgeIndex);
                }
            }
        }
        
        // Determines the shared edge between two tiles
        int GetSharedEdgeIndex(Tile currentTile, Tile nextTile)
        {
            // Iterate over the neighbors array to find the shared edge
            for (int i = 0; i < 6; i++)
            {
                if (Tile.GetNeighborsArray(tiles, currentTile.x, currentTile.y)[i] == nextTile)
                {
                    return i;
                }
            }

            // Return -1 if no shared edge is found (this should not happen if the path is valid)
            return -1;
        }
        
        // Sets the River Edges in a h.xagon to true from startEdge to endEdge.
        void ConnectInternalEdges(Tile tile, int startEdge, int endEdge)
        {
            // Clockwise or counterclockwise distance
            int clockwiseDistance = (endEdge - startEdge + 6) % 6;
            int counterClockwiseDistance = (startEdge - endEdge + 6) % 6;
            int edgeStartIndex = 0;
            
            // Decide which direction to connect the edges - random 50% chance
            if (random.NextInt(0, 3) > 0)
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
            for (int x = 0; x < tiles.Length; x++)
            {
                for (int y = 0; y < tiles[0].Length; y++)
                {
                    Tile currTile = tiles[x][y];

                    // If a Tile has River adjacency
                    if (currTile.hasFreshWater)
                    {
                        // For each of its neighbors
                        foreach (Tile neighbor in Tile.GetNeighborsArray(tiles, currTile.x, currTile.y))
                        {
                            // If they have river adjacency
                            if (neighbor is not null && neighbor.hasFreshWater)
                            {
                                // Set to true by default.
                                bool sameEdges = true;
                                
                                List<int> edgesSetToTrue = new List<int>();
                                // Add all the current Tile's river edges set to True to a list.
                                for (int index = 0; index < 6; index++)
                                {
                                    if (currTile.riverEdges[index] && index != GetSharedEdgeIndex(currTile, neighbor))
                                    {
                                        edgesSetToTrue.Add(index);
                                    }
                                }
                                
                                // If neighbor has those same edges set to true as well
                                foreach (int edge in edgesSetToTrue)
                                {
                                    if (!neighbor.riverEdges[edge])
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
            for (int x = 0; x < tiles.Length; x++)
            {
                for (int y = 0; y < tiles[0].Length; y++)
                {
                    Tile currTile = tiles[x][y];
                    bool isAdjacentToCoat = false;

                    // If a Tile has River Adjacency
                    if (currTile.hasFreshWater)
                    {
                        int index = 0;
                        int edgeAdjacentToCoast;
                        // Figure out where each edge is at
                        
                        foreach (Tile neighbor in Tile.GetNeighborsArray(tiles, currTile.x, currTile.y))
                        {
                            // if Neighbor is Coast
                            if (neighbor is not null && neighbor.biome == Biome.Coast)
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
                                if (currTile.riverEdges[i])
                                {
                                    // If this edge is 5 and the Tile towards this edge is not in the River Path
                                    if (i == 5 && !Tile.GetNeighborsArray(tiles, currTile.x, currTile.y)[5].hasFreshWater)
                                    {
                                        // If Both it's neighboring edges don't lead to a Tile on the River Path
                                        if (!Tile.GetNeighborsArray(tiles, currTile.x, currTile.y)[4].hasFreshWater && !Tile.GetNeighborsArray(tiles, currTile.x, currTile.y)[0].hasFreshWater)
                                        {
                                            // Then set the isolated river edge to false.
                                            currTile.SetRiverEdge(5, false);
                                        }
                                    } 
                                    // If this edge is 0 and the Tile towards this edge is not in the River Path
                                    else if (i == 0 && !Tile.GetNeighborsArray(tiles, currTile.x, currTile.y)[0].hasFreshWater)
                                    {
                                        // If Both it's neighboring edges don't lead to a Tile on the River Path
                                        if (!Tile.GetNeighborsArray(tiles, currTile.x, currTile.y)[5].hasFreshWater && !Tile.GetNeighborsArray(tiles, currTile.x, currTile.y)[1].hasFreshWater)
                                        {
                                            // Then set the isolated river edge to false.
                                            currTile.SetRiverEdge(0, false);
                                        }
                                    }
                                    // If any other number and the Tile towards this edge is not in the River Path
                                    else if (!Tile.GetNeighborsArray(tiles, currTile.x, currTile.y)[i].hasFreshWater)
                                    {
                                        // If Both it's neighboring edges don't lead to a Tile on the River Path
                                        if (!Tile.GetNeighborsArray(tiles, currTile.x, currTile.y)[i - 1].hasFreshWater && !Tile.GetNeighborsArray(tiles, currTile.x, currTile.y)[i + 1].hasFreshWater)
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

        /* Makes sure that all Tile's adjacent to River's have Fresh Water Access set to True.  */
        void AssignRemainingFreshWaterAccess()
        {
            for (int x = 0; x < tiles.Length; x++)
            {
                for (int y = 0; y < tiles[0].Length; y++)
                {
                    Tile currTile = tiles[x][y];
                    
                    if (currTile.hasFreshWater)
                    {
                        int edge = 0; // Keep track of which neighbor
                        foreach (Tile neighbor in Tile.GetNeighborsArray(tiles, currTile.x, currTile.y))
                        {
                            if (neighbor is not null && currTile.riverEdges[edge] && neighbor.IsLand())
                            {
                                // Set the opposing edge to true
                                Tile.GetNeighborsArray(tiles, currTile.x, currTile.y)[edge].SetRiverEdge((edge + 3) % 6, true);
                                // Set the neighbor's fresh water access to true.
                                Tile.GetNeighborsArray(tiles, currTile.x, currTile.y)[edge].SetHasFreshWater(true);
                            }
                            edge++;
                        }

                        // Check if the Tile has river edges.
                        bool hasRiverEdges = false;
                        for (int i = 0; i < 6; i++)
                        {
                            if (currTile.riverEdges[i])
                            {
                                hasRiverEdges = true;
                                break;
                            }
                        }
                        
                        // If it doesn't 
                        if (!hasRiverEdges)
                        {
                            // Remove it's fresh water access
                            currTile.SetHasFreshWater(false);
                        }
                    }
                }
            }
        }
    }
    
    /* Determine the features on Tiles. */
    private void DetermineFeatures()
    {
        DetermineWoods();
        DetermineFloodPlains();
        DetermineMarshes();
        DetermineRainforest();
        DetermineOasis();
        
        void DetermineWoods()
        {
            Queue<Tile> woodsQueue = new Queue<Tile>();
            
            SpreadAFewRandomWoods();
            
            void SpreadAFewRandomWoods() 
            {
                for (int x = 0; x < tiles.Length; x++)
                {
                    for (int y = 0; y < tiles[0].Length; y++)
                    {
                        Tile currTile = tiles[x][y];
                        
                        // If it is Land, and it is not Snow, and it is not Desert, and it doesn't have a Mountain
                        if (currTile.IsLand() && currTile.biome != Biome.Snow && currTile.biome != Biome.Desert &&
                            currTile.terrain != Terrain.Mountain)
                        {
                            if (random.NextInt(0, 100) < 15)
                            {
                                // Add a Woods
                                currTile.SetFeature(Feature.Woods);
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
                Tile currTile = woodsQueue.Dequeue();

                foreach (Tile neighbor in Tile.GetNeighborsArray(tiles, currTile.x, currTile.y))
                {
                    // If neighbor is not null, if it is land, if it is not a Mountain, if it is not Snow or Desert
                    if (neighbor is not null && neighbor.IsLand() && neighbor.terrain != Terrain.Mountain &&
                        neighbor.biome != Biome.Snow && neighbor.biome != Biome.Desert)
                    {
                        if (random.NextInt(0, 100) < 50)
                        {
                            currTile.SetFeature(Feature.Woods);
                        }
                    }
                }
            }
            
        }
        
        void DetermineFloodPlains()
        {
            Queue<Tile> floodPlainsQueue = new Queue<Tile>();
            
            PlaceAFewFloodplains();
            
            void PlaceAFewFloodplains()
            {
                for (int x = 0; x < tiles.Length; x++)
                {
                    for (int y = 0; y < tiles[0].Length; y++)
                    {
                        Tile currTile = tiles[x][y];

                        // If the Tile is adjacent to River, if it's Desert, and if it's Flat
                        if (currTile.HasRiver() && currTile.biome == Biome.Desert && currTile.terrain == Terrain.Flat)
                        {
                            // Set it to Foodplains
                            currTile.SetFeature(Feature.Floodplains);
                        } 
                        // If the Tile is adjacent to River, if it's Plains or Grassland, and if it's Flat
                        else if (currTile.HasRiver() &&
                                 (currTile.biome == Biome.Plains || currTile.biome == Biome.Grassland) && currTile.terrain == Terrain.Flat)
                        {

                            bool isAdjacentToAnotherFloodplain = false;
                            foreach (Tile neighbor in Tile.GetNeighborsArray(tiles, currTile.x, currTile.y))
                            {
                                if (neighbor is not null && neighbor.feature == Feature.Floodplains)
                                {
                                    isAdjacentToAnotherFloodplain = true;
                                }
                            }

                            if (!isAdjacentToAnotherFloodplain)
                            {
                                int probability = random.NextInt(0, 100);
                                // 80% chance
                                if (probability < 10)
                                {
                                    // Set it to Floodplains
                                    currTile.SetFeature(Feature.Floodplains);
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
                Tile currTile = floodPlainsQueue.Dequeue();

                foreach (Tile neighbor in Tile.GetNeighborsArray(tiles, currTile.x, currTile.y))
                {
                    // If neighbor is not null, if its land, if it's next to a river, if it's plains or grassland, and if its flat.
                    if (neighbor is not null && neighbor.HasRiver() && neighbor.IsLand() &&
                        (neighbor.biome == Biome.Grassland || neighbor.biome == Biome.Plains) && neighbor.terrain == Terrain.Flat)
                    {
                        // 50% Chance to spread to neighbor
                        if (random.NextInt(0, 100) < 50)
                        {
                            neighbor.SetFeature(Feature.Floodplains);
                        }
                    }
                }
            }
        }

        void DetermineMarshes()
        {
            for (int x = 0; x < tiles.Length; x++)
            {
                for (int y = 0; y < tiles[0].Length; y++)
                {
                    Tile currTile = tiles[x][y];

                    // If the Tile is Grassland and Flat.
                    if (currTile.biome == Biome.Grassland && currTile.terrain == Terrain.Flat && !currTile.HasRiver())
                    {
                        if (random.NextInt(0, 100) < 5)
                        {
                            // Set it to Marsh
                            currTile.SetFeature(Feature.Marsh);
                        }
                    }
                }
            }
        }

        void DetermineRainforest()
        {
            for (int x = 0; x < tiles.Length; x++)
            {
                for (int y = 0; y < tiles[0].Length; y++)
                {
                    Tile currTile = tiles[x][y];

                    if (currTile.biome == Biome.Plains && currTile.terrain != Terrain.Mountain && !currTile.HasRiver())
                    {
                        int probability = random.NextInt(0, 100);
                        // If it's right on the Ecuator
                        if (x < tiles.Length * .525 && x > tiles.Length * .475)
                        {
                            // 80% Chance
                            if (probability < 80)
                            {
                                currTile.SetFeature(Feature.Rainforest);
                            }
                        }
                        else
                        {
                            // Chance decreases relative to distance to equator
                            int distanceToEquator = Math.Abs(x - (tiles.Length / 2));
                            // Multiply each distance by this.
                            int distanceFactor = distanceToEquator * 5;
                            if (probability + distanceFactor < 60)
                            {
                                currTile.SetFeature(Feature.Rainforest);
                            }
                        }
                        
                        
                        
                        
                        
                    }
                }
            }
        }

        void DetermineOasis()
        {
            for (int x = 0; x < tiles.Length; x++)
            {
                for (int y = 0; y < tiles[0].Length; y++)
                {
                    Tile currTile = tiles[x][y];

                    // If it's Desert, if It's Flat, and if it's not on a River.
                    if (currTile.biome == Biome.Desert && currTile.terrain == Terrain.Flat && !currTile.HasRiver())
                    {
                        if (random.NextInt(0, 100) < 15)
                        {
                            currTile.SetFeature(Feature.Oasis);
                        }
                    }
                }
            }
        }

        // To be implemented
        void DetermineLakes()
        {
            
        }
    }

    /* Determine the Resources and Resource spread across the game world.  */
    private void DetermineResources()
    {
        
    }
    
    /* Determines Spawn Points */
    public static List<Point> DetermineSpawnPoints(Tile[][] tiles, int civCount)
    {
        // All Possible Spawn Points (along rivers)
        List<Point> possiblePoints = new List<Point>();
        // Final List of Spawn Points
        List<Point> spawnPointList = new List<Point>();
        
        for (int y = 0; y < tiles[0].Length; y++) //looking through all tiles near middle vertically
        {
            for (int x = tiles.Length/4; x < tiles.Length * 3/4; x++)
            {
                Tile currTile = tiles[x][y];
                if (currTile.HasRiver() && currTile.IsWalkable())
                {
                    possiblePoints.Add(new Point(x, y)); //adds tile if 1. next to river 2. is land 3.is not mountain
                }
            }
        }
        int randomNum = random.NextInt(0, possiblePoints.Count); 
        spawnPointList.Add(possiblePoints[randomNum]);//set a random first spawn

        while (civCount > spawnPointList.Count) //while there needs to be more spawns
        {
            double maxDist = 0; //updates every while loop
            Point nextPoint = spawnPointList[0]; //updates every while loop, point to be added
            foreach (Point vPoint in possiblePoints) //goes through all valid tiles,
            {
                List<double> dist = new List<double>();
                foreach (Point sPoint in spawnPointList)
                {
                    dist.Add(Util.eucDist(vPoint, sPoint)); //creates a list of distances between the current valid point
                }                                       // and all the current spawn points

                if (dist.Min() > maxDist) //if the lowest of those distances is > max dist
                {
                    maxDist = dist.Min(); //updates max dist
                    nextPoint = vPoint; //updates nextpoint
                }
            }
            spawnPointList.Add(nextPoint); //adds the next point that is as far from every spawn as possible
        }

        return spawnPointList;
    }
}
