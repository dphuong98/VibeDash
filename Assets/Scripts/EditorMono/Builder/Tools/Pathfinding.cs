using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEngine;

public static class Pathfinding
{
    public static int CountUniqueTiles(IEnumerable<Vector2Int> path)
    {
        return path.Distinct().Count();
    }
    
    public static List<Vector2Int> GetSolution(StageData stageData)
    {
        var solution = new List<Vector2Int>();
        
        if (stageData == null || stageData.GetEntrance() == -Vector2Int.one || stageData.GetExit() == -Vector2Int.one)
        {
            return solution;
        }
        
        // var scoutGraph = new Graph<Vector2Int>();
        // var visited = new List<Vector2Int>();
        // GraphNode(scoutGraph, stageData, visited, stageData.GetEntrance());
        // scoutGraph.PrintGraph();
        
        //Brute force
        //Find all path from entrance to exit -> Get path that covers the most tiles -> Get shortest path from which
        try
        {
            if (MapNode(stageData, new List<Vector2Int>(), stageData.GetEntrance(), out var allExitPaths))
            {
                var fullPath = allExitPaths.GroupBy(s => s.Distinct().Count()).Aggregate((i1,i2) => i1.Key > i2.Key ? i1 : i2);
                var shortestFullPath = fullPath.GroupBy(s => s.Count).Aggregate((i1,i2) => i1.Key < i2.Key ? i1 : i2);
                solution = new List<Vector2Int>(shortestFullPath.First());
                solution.Insert(0, stageData.GetEntrance());
            }

            return solution;
        }
        catch (Exception ex)
        {
            Debug.Log("goon");
        }
        
        return solution;
    }

    private static void GraphNode(Graph<Vector2Int> graph, StageData stageData, List<Vector2Int> visited, Vector2Int currentNode)
    {
        visited.Add(currentNode);
        var direction = Vector2Int.up;

        do
        {
            if (stageData.TryMove(currentNode, direction, out var scoutPath))
            {
                if (scoutPath.Count == 0)
                {
                    direction = direction.RotateClockwise();
                    continue;
                }
                
                graph.AddDirected(currentNode, scoutPath.Last());
                
                if (!visited.Contains(scoutPath.Last()))
                    GraphNode(graph, stageData, visited, scoutPath.Last());
            }

            direction = direction.RotateClockwise();
        } while (direction != Vector2Int.up);
    }
    
    //TODO Refactor this
    private static bool MapNode(StageData stageData, List<Vector2Int> nodeCache, Vector2Int currentNode, out List<List<Vector2Int>> exitPaths)
    {
        exitPaths = new List<List<Vector2Int>>();
        
        if (stageData[currentNode.x, currentNode.y] == TileType.Exit)
            return true;
        
        //Trace Stacks
        nodeCache.Add(currentNode);

        var direction = Vector2Int.up; //

        do
        {
            //Exit path detected
            if (stageData.TryMove(currentNode, direction, out var scoutPath)) //
            {
                if (scoutPath.Count == 0 ||
                    HasInfiniteLoop(nodeCache, scoutPath.Last())
                    )
                {
                    direction = direction.RotateClockwise();
                    continue;
                } //

                //Recursion ends when DFS meet an exit. Only return false when exit doesnt exist
                if (MapNode(stageData, nodeCache, scoutPath.Last(), out var endPaths))
                {
                    if (endPaths.Count == 0)s
                    {
                        exitPaths.Add(scoutPath);
                    }

                    foreach (var i in endPaths)
                    {
                        exitPaths.Add(scoutPath.Concat(i).ToList());
                    }
                }
            }

            direction = direction.RotateClockwise(); //
        } while (direction != Vector2Int.up); //
        
        //Remove Trace Stacks
        nodeCache.RemoveAt(nodeCache.Count - 1);

        return exitPaths.Count > 0;
    }

    private static bool HasInfiniteLoop(List<Vector2Int> nodeCache, Vector2Int nextNode)
    {
        var latestOccurence = nodeCache.LastIndexOf(nextNode);
        if (latestOccurence == -1) return false;

        for (var i = latestOccurence + 1; i != nodeCache.Count; i++)
        {
            if (nodeCache.IndexOf(nodeCache[i], 0, latestOccurence) == -1)
            {
                return false;
            }
        }

        return true;
    }
    
    /// <returns>true if there is a line with the direction from start to end</returns>
    public static bool ExistDirectedPath(List<Vector2Int> path, params Vector2Int[] subPath)
    {
        var startNodeOccurrences = path.IndicesOf(subPath[0]);

        foreach (var nodeIndex in startNodeOccurrences)
        {
            var match = true;
            for (var i = 1; i < subPath.Length; i++)
            {
                if (nodeIndex + i >= path.Count) return false;
                if (path[nodeIndex + i] != subPath[i])
                {
                    match = false;
                    break;
                }
            }

            if (match) return true;
        }
        
        return false;
    }

    public static List<Vector2Int> GetNextNodes(List<Vector2Int> path, params Vector2Int[] subPath)
    {
        var nextNodes = new List<Vector2Int>();

        var currentNodeOccurrences = path.IndicesOf(subPath[0]);
        foreach (var nodeIndex in currentNodeOccurrences)
        {
            var match = true;
            for (var i = 1; i < subPath.Length; i++)
            {
                if (nodeIndex + i + 1 >= path.Count || path[nodeIndex + i] != subPath[i])
                {
                    match = false;
                    break;
                }
            }

            if (match) nextNodes.Add(path[nodeIndex + subPath.Length]);
        }

        return nextNodes;
    }
}
