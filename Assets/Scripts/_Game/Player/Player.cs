using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Level Level;

    private int totalPoints;
    private int stackPoints;
    public int StackPoints => stackPoints;
    
    private bool isTraversing;
    private float elapsedTime;
    private float nextEdge;
    private const float speed = 12; //tile/s
    
    private List<Vector3Int> waitingPath;
    public List<Vector3Int> WaitingPath => waitingPath != null ? new List<Vector3Int>(waitingPath) : null;

    private void Awake()
    {
        GameInput.OnSwipeDirection.AddListener(ProbePath);
    }

    private void FixedUpdate()
    {
        PlayerMove();

        var text = "IsMoving: " + GetComponent<Animator>().GetBool("IsMoving") + "\n";
        text += "Points: " + totalPoints + "\n";
        text += "Stack: " + stackPoints + "\n";
        DebugUI.Instance.SetText(text);
    }

    private void PlayerMove()
    {
        if (!isTraversing) return;
        
        elapsedTime += Time.deltaTime;
        var totalTime = waitingPath.Count / speed;
        var lerpValue = Mathf.Lerp(0, 1, elapsedTime / totalTime);

        var nextGridPos = waitingPath[(int) Math.Ceiling(lerpValue * (waitingPath.Count - 1))];
        if (Level.GetTileType(nextGridPos) == TileType.Bridge &&
            !Level.HasTile(nextGridPos, TileType.Blank) &&
            stackPoints == 0)
        {
            var pastGridPos = waitingPath[(int) Math.Floor(lerpValue * (waitingPath.Count - 1))];
            var pastGridCenter = Level.LevelGrid.GetCellCenterWorld(pastGridPos);
            transform.position = new Vector3(pastGridCenter.x, transform.position.y, pastGridCenter.z);
            ResetPath();
            return;
        }

        var nextWorldPos = Path.LerpPath(Level, waitingPath, lerpValue);
        transform.position = new Vector3(nextWorldPos.x, transform.position.y, nextWorldPos.z);
        if (lerpValue >= 1)
        {
            ResetPath();
            return;
        }
        
        if (lerpValue * (waitingPath.Count - 1) >= nextEdge)
        {
            LeaveTile(waitingPath[(int)Math.Floor(nextEdge)]);
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
                totalPoints++; stackPoints++;
                LevelLoader.PlaceTile(tileComponent.transform.position, TileType.Blank, Level.LevelGrid.transform);
                Destroy(tileComponent.gameObject);
            }
        }
    }

    private void EnterTile(Vector3Int gridPos)
    {
        if (Level.GetTileType(gridPos) == TileType.Bridge &&
            !Level.HasTile(gridPos, TileType.Blank) &&
            stackPoints > 0)
        {
            LevelLoader.PlaceTile(Level.LevelGrid.GetCellCenterWorld(gridPos), TileType.Blank, Level.LevelGrid.transform);
            stackPoints--;
        }
    }

    private void ProbePath(Vector3Int direction)
    {
        //TODO Handle when player fall off the map
        TryMove(transform, direction, Level, out var path);
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
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="path"></param>
    /// <returns>false if the player fell out of the map</returns>
    public void TryMove(Transform playerTransform, Vector3Int direction, Level level, out List<Vector3Int> path)
    {
        path = new List<Vector3Int>();
        
        var currentGridPosition = level.LevelGrid.WorldToCell(playerTransform.position);
        currentGridPosition.z = 0;
        path.Add(currentGridPosition);
        
        while (true)
        {
            currentGridPosition += direction;

            var currentTileType = level.GetTileType(currentGridPosition);

            if (currentTileType == TileType.Air)
            {
                path.Add(currentGridPosition);
                break;
            }

            //Impassible
            if (currentTileType == TileType.Wall)
                break;
            
            if (currentTileType == TileType.Stop)
            {
                path.Add(currentGridPosition);
                break;
            }
            
            if (currentTileType == TileType.PortalBlue)
            {
                path.Add(currentGridPosition);
                
                //If exit does not exist act as stop
                var portal = level.PortalPairs.Where(s => s.Blue == currentGridPosition.ToVector2Int());
                if (portal.Any() && portal.First().Orange != -Vector2Int.one)
                {
                    path.Add(portal.First().Orange.ToVector3Int());
                }
                break;
            }
            
            if (currentTileType == TileType.PortalOrange)
            {
                path.Add(currentGridPosition);
                
                //If there is no portal act as stop
                var portal = level.PortalPairs.Where(s => s.Orange == currentGridPosition.ToVector2Int());
                if (portal.Any() && portal.First().Blue != -Vector2Int.one)
                {
                    path.Add(portal.First().Blue.ToVector3Int());
                }
                break;
            }
            
            if (currentTileType == TileType.Road || currentTileType == TileType.Blank)
            {
                path.Add(currentGridPosition);
                continue;
            }

            if (currentTileType == TileType.Bridge)
            {
                var bridge = level.GetBridge(currentGridPosition, direction);
                for (var i = 0; i < bridge.bridgeParts.Count - 1; i++)
                {
                    path.Add(bridge.bridgeParts[i]);
                    currentGridPosition = bridge.bridgeParts[i];
                    direction = bridge.bridgeParts[i+1] -
                                 bridge.bridgeParts[i];
                }
                continue;
            }
            
            if (currentTileType == TileType.Push)
            {
                path.Add(currentGridPosition);
                //TODO this
                direction = level.TileDirections[currentGridPosition.ToVector2Int()].ToVector3Int();
                continue;
            }
            
            if (currentTileType == TileType.Corner)
            {
                //TODO this
                var upVector = level.TileDirections[currentGridPosition.ToVector2Int()].ToVector3Int();
                var rightVector = upVector.ToVector2Int().RotateClockwise().ToVector3Int();
                
                path.Add(currentGridPosition);

                if (direction == -upVector) direction = rightVector;
                if (direction == -rightVector) direction = upVector;
                continue;
            }

            break;
        }
    }
}
