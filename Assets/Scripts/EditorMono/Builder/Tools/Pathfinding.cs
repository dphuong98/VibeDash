using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
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
            
        //Brute force
        //Find all path from entrance to exit -> Get path that covers the most tiles -> Get shortest path from which
        if (MapNode(stageData, new List<Vector2Int>(), stageData.GetEntrance(), out var allExitPaths))
        {
            var fullPath = allExitPaths.GroupBy(s => s.Distinct().Count()).Aggregate((i1,i2) => i1.Key > i2.Key ? i1 : i2);
            var shortestFullPath = fullPath.GroupBy(s => s.Count).Aggregate((i1,i2) => i1.Key < i2.Key ? i1 : i2);
            solution = new List<Vector2Int>(shortestFullPath.First());
            solution.Insert(0, stageData.GetEntrance());
        }

        return solution;
    }
    
    //TODO Refactor this
    private static bool MapNode(StageData stageData, List<Vector2Int> nodeCache, Vector2Int currentNode, out List<List<Vector2Int>> exitPaths)
    {
        exitPaths = new List<List<Vector2Int>>();
        
        if (stageData[currentNode.x, currentNode.y] == TileType.Exit)
            return true;

        var direction = Vector2Int.up;

        do
        {
            //Exit path detected
            if (stageData.TryMove(currentNode, direction, out var scoutPath))
            {
                if (scoutPath.Count == 0
                    //HasJiggle(nodeCache, scoutPath.Last()) ||
                    //HasInfiniteLoop(nodeCache, scoutPath.Last())
                    )
                {
                    direction = direction.RotateClockwise();
                    continue;
                }
                
                //Trace Stacks
                nodeCache.Add(scoutPath.Last());

                //Recursion ends when DFS meet an exit. Only return false when exit doesnt exist
                if (MapNode(stageData, nodeCache, scoutPath.Last(), out var endPaths))
                {
                    if (endPaths.Count == 0)
                    {
                        exitPaths.Add(scoutPath);
                    }

                    foreach (var i in endPaths)
                    {
                        exitPaths.Add(scoutPath.Concat(i).ToList());
                    }
                }

                //Remove Trace Stacks
                nodeCache.RemoveAt(nodeCache.Count - 1);
            }

            direction = direction.RotateClockwise();
        } while (direction != Vector2Int.up);

        return exitPaths.Count > 0;
    }

    private static bool HasJiggle(List<Vector2Int> nodeCache, Vector2Int nextNode)
    {
        //Jiggle: a -> b -> c -> b -> a -> b -> c
        var indices = nodeCache.IndicesOf(nextNode).ToList();
        if (indices.Count == 0)
            return false;
        
        var c2 = nodeCache.Count;
        var c1 = indices[indices.Count - 1];
        if ((c1 + c2) % 2 != 0) return false;
        
        var a2 = (c1 + c2) / 2;
        var a1 = c1 - (a2 - c1);

        if (a1 < 0) return false;

        var firstSegment = nodeCache.GetRange(a1, c1 - a1 + 1);
        var secondSegment = nodeCache.GetRange(c1, a2 - c1 + 1);
        secondSegment.Reverse();
        var thirdSegment = nodeCache.GetRange(a2, c2 - a2);
        thirdSegment.Add(nextNode);

        if (firstSegment.SequenceEqual(secondSegment) &&
            firstSegment.SequenceEqual(thirdSegment))
        {
            return true;
        }
        
        return false;
    }

    private static bool HasInfiniteLoop(List<Vector2Int> nodeCache, Vector2Int nextNode)
    {
        //If a loop happens twice, it can only lead to an infinite loop, only apply for this pathfinding algorithm
        var indices = nodeCache.IndicesOf(nextNode).ToList();
        if (indices.Count < 2) return false;
        
        var lastLoop = nodeCache.GetRange(indices[indices.Count - 1],
            nodeCache.Count - indices[indices.Count - 1]);

        for (var i = 0; i != indices.Count - 1; i++)
        {
            var currentLoop = nodeCache.GetRange(indices[i], indices[i + 1] - indices[i]);

            if (currentLoop.SequenceEqual(lastLoop))
            {
                return true;
            }
        }

        return false;
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
