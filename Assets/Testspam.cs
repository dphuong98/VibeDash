using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

public class Testspam : MonoBehaviour
{
    [ContextMenu("Test")]
    private void Test()
    {
        var graph = new WeightedGraph<Vector2Int>();
        graph.AddDirected(new Vector2Int(1,2), new Vector2Int(3, 4), 10);
        graph.AddDirected(new Vector2Int(1,2), new Vector2Int(2, 2), 20);
        graph.AddDirected(new Vector2Int(2,2), new Vector2Int(3, 4), 30);
        graph.PrintGraph();
    }
}