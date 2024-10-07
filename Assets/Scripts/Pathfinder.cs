using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.FilePathAttribute;

public static class Pathfinder
// if _terrain is 2 the tile is impassable.
// C# version 9 doees not have a Priority Queue, so for now I'm going to borrow one from Red Blob
{
    public static Dictionary<GameTile,GameTile> AStar(GameTile start, GameTile goal)
    {
        Dictionary<GameTile,GameTile> cameFrom = new Dictionary<GameTile, GameTile>();
        Dictionary<GameTile, double>  costSoFar = new Dictionary<GameTile, double>();

        PriorityQueue<GameTile, double> frontier = new PriorityQueue<GameTile, double>();
        Debug.Log(start);
        Debug.Log(goal);
        Debug.Log(frontier);
        frontier.Enqueue(start, 0);

        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0) {
            GameTile current = frontier.Dequeue();
            if (current.GetXPos() == goal.GetXPos() && current.GetYPos() == goal.GetYPos())
            {
                break;
            }
            Debug.Log(current.GetNeighbors().Length);
            for (int x = 0; x < current.GetNeighbors().Length; x++)
            {
                Debug.Log(x);
                GameTile neighbour = current.GetNeighbors()[x];
                if (neighbour == null)
                {
                    continue;
                }
                Debug.Log(neighbour);
                /* if (neighbour.GetTerrain() == 2)
                {
                    continue; 
                } */
                
                double new_cost = costSoFar[current] + neighbour.GetMovementCost();
                if (!costSoFar.ContainsKey(neighbour) || new_cost < costSoFar[neighbour])
                {
                    costSoFar[neighbour] = new_cost;
                    double priority = new_cost + Position.cost_distance(neighbour, goal);
                    frontier.Enqueue(neighbour, priority);
                    cameFrom[neighbour] = current;
                }
            } 
        }
        
        return cameFrom;
    }
    // AStarwithLimit returns a List of tuples with Tiles and the total movement cost to move to that tile.
    // Unlike the algorithm before, only the neighbour with the least cost is added to the list of paths.
    public static List<Tuple<GameTile, int>> AStarWithLimit(GameTile start, GameTile goal, int movementPoints)
    {
        Dictionary<GameTile, GameTile> cameFrom = new Dictionary<GameTile, GameTile>();
        List<Tuple<GameTile, int>> path = new List<Tuple<GameTile, int>>();
        Dictionary<GameTile, double> costSoFar = new Dictionary<GameTile, double>();
        PriorityQueue<GameTile, double> frontier = new PriorityQueue<GameTile, double>();

        costSoFar[start] = 0;
        frontier.Enqueue(start, 0);

        while (frontier.Count > 0)
        {
            GameTile current = frontier.Dequeue();
            // We have a path to our destination.
            if (current == goal)
            {
                break;
            }
            // We check to see if we have used ALL our movement points and haven't reached the goal.
            // We'll need a check later making sure that even if we were not able to, we still have a valid path.
            if ((int)costSoFar[current] == movementPoints)
            {
                goal = current;
                break;
            }
            

            foreach (GameTile neighbor in current.GetNeighbors())
            {
                // Mountains are impassable. Void/null tiles shouldn't be considered.
                if (neighbor == null || neighbor.GetTerrain() == 2 || neighbor.GetBiome() == 7 || neighbor.GetBiome() == 6)
                {
                    continue;
                }
    
                double newCost = costSoFar[current] + neighbor.GetMovementCost();
                
                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    // We need to ensure that going to the neighbor doesn't exceed our movementpoints.
                    if (newCost<= movementPoints){
                        costSoFar[neighbor] = newCost;
                        double priority = newCost + Position.cost_distance(neighbor, goal);
                        frontier.Enqueue(neighbor, priority);
                        cameFrom[neighbor] = current;
                    }
                }
            }
            // We run a check to see if the frontier is empty(meaning that the loop will terminate)
            // If it is, it means we haven't teminated in any other way(movement points used up, found goal)
            // It also means that no neighbours in this iteration were added to the frontier, and therefore do not form a valid path.
            // Therefore, we can maintain a valid path by setting our goal to the last reached current.
            if (frontier.Count == 0)
            {
                goal = current;
            }
        }

        // Reconstruct the path from goal to start
        GameTile pathCurrent = goal;
        if (cameFrom.ContainsKey(goal) || goal == start) // Ensure there's a valid path
        {
            while (pathCurrent != start)
            {
                path.Insert(0, new Tuple<GameTile, int>(pathCurrent, (int)costSoFar[pathCurrent]));
                pathCurrent = cameFrom[pathCurrent];
            }
        }

        return path;
    }


    // UnitAstar assumes the unit on the start tile then return the tile it would get to by the end of its turn.
    public static List<Tuple<GameTile, int>> UnitAstar(GameTile start, GameTile goal)
    {
        if (start.GetUnit() is not null)
        {
            return AStarWithLimit(start, goal, start.GetUnit().GetMovementPoints());
        }
        else return null;

    }
    
    public static List<Tuple<GameTile, int>> AStarWithoutLimit(GameTile start, GameTile goal)
    {
        Dictionary<GameTile, GameTile> cameFrom = new Dictionary<GameTile, GameTile>();
        List<Tuple<GameTile, int>> path = new List<Tuple<GameTile, int>>();
        Dictionary<GameTile, double> costSoFar = new Dictionary<GameTile, double>();
        PriorityQueue<GameTile, double> frontier = new PriorityQueue<GameTile, double>();

        costSoFar[start] = 0;
        frontier.Enqueue(start, 0);

        while (frontier.Count > 0)
        {
            GameTile current = frontier.Dequeue();
            // We have a path to our destination.
            if (current == goal)
            {
                break;
            }
            foreach (GameTile neighbor in current.GetNeighbors())
            {
                // Mountains are impassable. Void/null tiles shouldn't be considered.
                if (neighbor == null || neighbor.GetTerrain() == 2 || neighbor.GetBiome() == 7 || neighbor.GetBiome() == 6)
                {
                    continue;
                }
    
                double newCost = costSoFar[current] + neighbor.GetMovementCost();
                
                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    // We need to ensure that going to the neighbor doesn't exceed our movementpoints.
                        costSoFar[neighbor] = newCost;
                        double priority = newCost + Position.cost_distance(neighbor, goal);
                        frontier.Enqueue(neighbor, priority);
                        cameFrom[neighbor] = current;
                }
            }
            // We run a check to see if the frontier is empty(meaning that the loop will terminate)
            // If it is, it means we haven't teminated in any other way(movement points used up, found goal)
            // It also means that no neighbours in this iteration were added to the frontier, and therefore do not form a valid path.
            // Therefore, we can maintain a valid path by setting our goal to the last reached current.
        }

        // Reconstruct the path from goal to start
        GameTile pathCurrent = goal;
        if (cameFrom.ContainsKey(goal) || goal == start) // Ensure there's a valid path
        {
            while (pathCurrent != start)
            {
                path.Insert(0, new Tuple<GameTile, int>(pathCurrent, (int)costSoFar[pathCurrent]));
                pathCurrent = cameFrom[pathCurrent];
            }
        }

        return path;
    }

    public class PriorityQueue<TElement, TPriority>
    {
        private List<Tuple<TElement, TPriority>> elements = new List<Tuple<TElement, TPriority>>();

        public int Count
        {
            get { return elements.Count; }
        }

        public void Enqueue(TElement item, TPriority priority)
        {
            elements.Add(Tuple.Create(item, priority));
        }

        public TElement Dequeue()
        {
            Comparer<TPriority> comparer = Comparer<TPriority>.Default;
            int bestIndex = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (comparer.Compare(elements[i].Item2, elements[bestIndex].Item2) < 0)
                {
                    bestIndex = i;
                }
            }

            TElement bestItem = elements[bestIndex].Item1;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }
    
    

    // Implement early-early exit A* - cost_limit based on movement points, exit when movement points are depleted 
    // Split the movement paths into turns 
}
