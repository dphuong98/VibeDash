using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinding
{
    public static int GetMaximumUniqueTile(Stage stage)
    {
        return GetSolution(stage).Distinct().Count();
    }
    
    public static List<Vector2Int> GetSolution(Stage stage)
    {
        var solution = new List<Vector2Int>();
        
        if (stage == null || stage.GetEntrance() == -Vector2Int.one || stage.GetExit() == -Vector2Int.one)
        {
            return null;
        }
            
        //Brute force
        //Find all path from entrance to exit -> Get path that covers the most tiles -> Get shortest path from which
        if (MapNode(stage, new Graph<Vector2Int>(), stage.GetEntrance(), out var allExitPaths))
        {
            var fullPath = allExitPaths.GroupBy(s => s.Distinct().Count()).Aggregate((i1,i2) => i1.Key > i2.Key ? i1 : i2);
            var shortestFullPath = fullPath.GroupBy(s => s.Count).Aggregate((i1,i2) => i1.Key < i2.Key ? i1 : i2);
            solution = shortestFullPath.First();
            solution.Insert(0, stage.GetEntrance());
        }

        return solution;
    }
    
    //TODO Refactor this
    private static bool MapNode(Stage stage, Graph<Vector2Int> traceGraph, Vector2Int currentNode, out List<List<Vector2Int>> exitPaths)
    {
        exitPaths = new List<List<Vector2Int>>();
        
        if (stage[currentNode.x, currentNode.y] == TileType.Exit)
            return true;

        var direction = Vector2Int.up;

        do
        {
            if (traceGraph.ExistDirectedPath(currentNode, currentNode + direction))
            {
                direction = direction.RotateClockwise(); continue;
            }
            
            //Exit path detected
            if (stage.TryMove(currentNode, direction, out var scoutPath))
            {
                if (scoutPath.Count == 0)
                {
                    direction = direction.RotateClockwise(); continue;
                }
                
                //Trace Stacks
                traceGraph.AddDirected(currentNode, scoutPath.First());
                for (int i = 0; i < scoutPath.Count - 1; i++)
                {
                    traceGraph.AddDirected(scoutPath[i], scoutPath[i+1]);
                }

                //Recursion ends when DFS meet an exit. Only return false when exit doesnt exist
                if (MapNode(stage, traceGraph, scoutPath.Last(), out var scoutPath2))
                {
                    if (scoutPath2.Count == 0)
                    {
                        exitPaths.Add(scoutPath);
                    }
                    
                    foreach (var i in scoutPath2)
                    {
                        exitPaths.Add(scoutPath.Concat(i).ToList());
                    }
                }

                //Remove Trace Stacks
                traceGraph.RemoveDirected(currentNode, scoutPath.First());
                for (int i = 0; i < scoutPath.Count - 1; i++)
                {
                    traceGraph.RemoveDirected(scoutPath[i], scoutPath[i+1]);
                }
            }
            
            direction = direction.RotateClockwise();
        } while (direction != Vector2Int.up);

        return exitPaths.Count > 0;
    }

    public static bool ExistPath(List<Vector2Int> path, Vector2Int start, Vector2Int end)
    {
        var startTileOccurences = path.Where(s => s == start).Select(s => path.IndexOf(s));

        foreach (var point in startTileOccurences)
        {
            if (point + 1 < path.Count && path[point + 1] == end)
            {
                return true;
            }

            if (point - 1 >= 0 && path[point - 1] == end)
            {
                return true;
            }
        }
        
        return false;
    }
}
