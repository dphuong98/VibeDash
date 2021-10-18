using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Level Level;

    private bool isTraversing = false;
    private float elapsedTime;
    private const float speed = 12f; //tile/s
    
    private List<Vector3Int> waitingPath;
    public List<Vector3Int> WaitingPath => waitingPath != null ? new List<Vector3Int>(waitingPath) : null;

    private void Awake()
    {
        InputManager.OnSwipeDirection.AddListener(GeneratePath);
    }

    private void FixedUpdate()
    {
        if (!isTraversing) return;
        
        elapsedTime += Time.deltaTime;
        var totalTime = waitingPath.Count / speed;
        var lerpValue = elapsedTime / totalTime;
        transform.position = Path.LerpPath(Level, waitingPath, lerpValue);

        if (lerpValue >= 1)
        {
            ResetPath();
        }
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
            waitingPath = path;
        }
    }

    private void ResetPath()
    {
        waitingPath = null;
        isTraversing = false;
    }
    
    public void StartTraversePath()
    {
        elapsedTime = 0f;
        isTraversing = true;
    }
}
