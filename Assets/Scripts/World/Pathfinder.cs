// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;

// public static class Pathfinder
// {
//     public static List<GameTile> FindShortestPath(GameTile start, GameTile target)
//     {
//         // Open set of tiles to be evaluated
//         var openSet = new PriorityQueue<GameTile>();
//         var cameFrom = new Dictionary<GameTile, GameTile>();
//         var gScore = new Dictionary<GameTile, int>();
//         var fScore = new Dictionary<GameTile, int>();

//         // Initialize the scores
//         gScore[start] = 0;
//         fScore[start] = HeuristicCostEstimate(start, target);
//         openSet.Enqueue(start, fScore[start]);

//         while (openSet.Count > 0)
//         {
//             var current = openSet.Dequeue();

//             // If we reached the target
//             if (current == target)
//                 return ReconstructPath(cameFrom, current);

//             foreach (var neighbor in current.GetNeighbors())
//             {
//                 if (neighbor == null) continue;

//                 int tentativeGScore = gScore[current] + neighbor.GetMovementCost();

//                 if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
//                 {
//                     cameFrom[neighbor] = current;
//                     gScore[neighbor] = tentativeGScore;
//                     fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, target);

//                     if (!openSet.Contains(neighbor))
//                         openSet.Enqueue(neighbor, fScore[neighbor]);
//                 }
//             }
//         }

//         // If we get here, no path was found
//         return null;
//     }

//     public static int CalculatePathCost(List<GameTile> path)
//     {
//         if (path == null || path.Count == 0)
//             return 0;

//         return path.Sum(tile => tile.GetMovementCost());
//     }

//     private static int HeuristicCostEstimate(GameTile a, GameTile b)
//     {
//         // Using Manhattan distance for a hex grid
//         int dx = Mathf.Abs(a.GetXPos() - b.GetXPos());
//         int dy = Mathf.Abs(a.GetYPos() - b.GetYPos());
//         return dx + dy;
//     }

//     private static List<GameTile> ReconstructPath(Dictionary<GameTile, GameTile> cameFrom, GameTile current)
//     {
//         var path = new List<GameTile>();
    
//         // Start tracing back from the target to the start
//         while (cameFrom.ContainsKey(current))
//         {
//             path.Add(current);
//             current = cameFrom[current];
//         }

//         // Since the start tile is the last tile in the chain, do not add it to the path
//         path.Reverse(); // Reverse to get the correct order (target to start -> start to target)
//         return path;
//     }
// }

