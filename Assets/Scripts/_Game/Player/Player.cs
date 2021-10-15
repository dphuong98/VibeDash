using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Level Level;

    private List<Vector3Int> path;
    public List<Vector3Int> Path => path != null ? new List<Vector3Int>(path) : null;

    private void Awake()
    {
        InputManager.OnSwipeDirection.AddListener(GeneratePath);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = GetComponent<Animator>().GetBool("IsMoving") ? Color.green : Color.grey;
        Gizmos.DrawCube(transform.position + new Vector3 {y = 4}, Vector3.one * 0.5f);
    }

    private void GeneratePath(Vector3Int direction)
    {
        PathGenerator.TryMove(transform, direction, Level, out var path);
        if (path.Count != 0)
        {
            this.path = path;
        }
    }

    public void ResetPath()
    {
        path = null;
    }
}
