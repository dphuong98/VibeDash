using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Level Level;

    private int points;
    private bool isTraversing;
    private float elapsedTime;
    private float nextEdge;
    private const float speed = 12f; //tile/s
    
    private List<Vector3Int> waitingPath;
    public List<Vector3Int> WaitingPath => waitingPath != null ? new List<Vector3Int>(waitingPath) : null;

    private void Awake()
    {
        InputManager.OnSwipeDirection.AddListener(ProbePath);
    }

    private void FixedUpdate()
    {
        PlayerMove();

        var text = "IsMoving: " + GetComponent<Animator>().GetBool("IsMoving") + "\n";
        text += "Points: " + points;
        DebugUI.Instance.SetText(text);
    }

    private void PlayerMove()
    {
        if (!isTraversing) return;
        
        elapsedTime += Time.deltaTime;
        var totalTime = waitingPath.Count / speed;
        var lerpValue = elapsedTime / totalTime;
        transform.position = Path.LerpPath(Level, waitingPath, lerpValue);

        if (lerpValue >= 1)
        {
            ResetPath();
            return;
        }
        
        if (lerpValue * (waitingPath.Count - 1) >= nextEdge)
        {
            LeaveTile(waitingPath[(int)Math.Floor(nextEdge)]);

            if ((int) Math.Ceiling(nextEdge) == waitingPath.Count)
            {
                nextEdge += 1f;
                return;
            }
            
            EnterTile(waitingPath[(int)Math.Ceiling(nextEdge)]);
            nextEdge += 1f;
        }
    }

    private void LeaveTile(Vector3Int gridPos)
    {
        TraverseRoad(gridPos);
    }

    private void TraverseRoad(Vector3Int gridPos)
    {
        var position = Level.LevelGrid.GetCellCenterWorld(gridPos);
        var hits = Physics.RaycastAll(position + Vector3.down, Vector3.up, Mathf.Infinity, 1 << LevelLoader.tileLayerMask);

        foreach (var hitInfo in hits)
        {
            if (hitInfo.transform.GetComponent<Tile>() is var tileComponent &&
                tileComponent != null &&
                tileComponent.TileType == TileType.Road)
            {
                points++;
                LevelLoader.PlaceTile(tileComponent.transform.position, TileType.Blank, Level.LevelGrid.transform);
                Destroy(tileComponent.gameObject);
            }
        }
    }

    private void EnterTile(Vector3Int gridPos)
    {
        
    }

    private void ProbePath(Vector3Int direction)
    {
        //TODO Handle when player fall off the map
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
        nextEdge = 0.5f;
        isTraversing = true;
    }
}
