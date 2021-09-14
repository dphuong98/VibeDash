using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Graph<T>
{
    private readonly Dictionary<T, List<T>> adjacencyGroups;
    
    public Graph()
    {
        adjacencyGroups = new Dictionary<T, List<T>>();
    }

    public void IterateEdge(Action<T, T> action)
    {
        foreach (var group in adjacencyGroups)
        {
            foreach (var node in group.Value)
            {
                action(group.Key, node);
            }
        }
    }

    public List<T> GetNeighbors(T node)
    {
        adjacencyGroups.TryGetValue(node, out var neighbors);
        return new List<T>(neighbors);
    }
    
    public bool ExistDirectedPath(T node1, T node2)
    {
        return adjacencyGroups.TryGetValue(node1, out var neighbors) && neighbors.Contains(node2);
    }

    public bool ExistUndirectedPath(T node1, T node2)
    {
        return ExistDirectedPath(node1, node2) && ExistDirectedPath(node2, node1);
    }
    
    public void AddDirected(T node1, T node2)
    {
        if (!adjacencyGroups.TryGetValue(node1, out var neighbors))
        {
            neighbors = new List<T>();
            adjacencyGroups.Add(node1, neighbors);
        }

        neighbors.Add(node2);
    }

    public void RemoveDirected(T node1, T node2)
    {
        if (adjacencyGroups.TryGetValue(node1, out var neighbors))
        {
            neighbors.Remove(node2);
        }
    }

    public void PrintGraph()
    {
        foreach (var node in adjacencyGroups)
        {
            Debug.Log(node.Key + ": " + string.Join(" -- ", node.Value.ToArray()));
        }
    }
}
