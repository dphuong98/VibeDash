using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightedGraph<T>
{
    private Dictionary<T, Dictionary<T, int>> adjacencyGroups;
    
    public WeightedGraph()
    {
        adjacencyGroups = new Dictionary<T, Dictionary<T, int>>();
    }

    public bool Exist(T node1, T node2)
    {
        if (adjacencyGroups.TryGetValue(node1, out var nextNodes))
        {
            return nextNodes.TryGetValue(node2, out var x);
        }
        
        return false;
    }
    public void AddDirected(T node1, T node2, int weight)
    {
        if (!adjacencyGroups.TryGetValue(node1, out var nextNodes))
        {
            nextNodes = new Dictionary<T, int>();
            adjacencyGroups.Add(node1, nextNodes);
        }

        nextNodes.Add(node2, weight);
    }

    public void PrintGraph()
    {
        foreach (var node in adjacencyGroups)
        {
            Debug.Log(node.Key);
            foreach (var next in node.Value)
            {
                Debug.LogFormat("{0}: {1}", next.Key, next.Value);
            }
        }
    }
}
