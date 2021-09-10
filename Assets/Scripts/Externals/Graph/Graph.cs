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
    
    public bool Contains(T node1, T node2)
    {
        return adjacencyGroups.TryGetValue(node1, out var nextNodes) && nextNodes.Contains(node2);
    }
    
    public void AddDirected(T node1, T node2)
    {
        if (!adjacencyGroups.TryGetValue(node1, out var nextNodes))
        {
            nextNodes = new List<T>();
            adjacencyGroups.Add(node1, nextNodes);
        }

        nextNodes.Add(node2);
    }

    public void PrintGraph()
    {
        foreach (var node in adjacencyGroups)
        {
            Debug.Log(node.Key + ": " + string.Join(", ", node.Value.ToArray()));
        }
    }
}
