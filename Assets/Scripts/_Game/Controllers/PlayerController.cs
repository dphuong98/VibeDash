using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlayerController
{
    private Transform playerTransform;
    private Level level;
    
    public PlayerController(Transform playerTransform, Level level)
    {
        this.playerTransform = playerTransform;
        this.level = level;
        InputManager.OnSwipeDirection.AddListener(HandleSwipe);
    }

    private void HandleSwipe(Vector2Int direction)
    {
        var tmp = TryMove(direction.ToVector3Int(), out var path);
        if (path.Count != 0)
            playerTransform.position = level.LevelGrid.GetCellCenterWorld(path.Last());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="path"></param>
    /// <returns>false if the player fell out of the map</returns>
    private bool TryMove(Vector3Int direction, out List<Vector3Int> path)
    {
        path = new List<Vector3Int>();

        if (playerTransform == null) return false;
        var currentGridPosition = level.LevelGrid.WorldToCell(playerTransform.position);

        while (true)
        {
            currentGridPosition += direction;

            var currentTileType = level.GetTileType(currentGridPosition);

            if (currentTileType == TileType.Air)
            {
                //Temporary let air behave like wall
                break;
                //return false
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
            
            if (currentTileType == TileType.Road || currentTileType == TileType.Entrance || currentTileType == TileType.Exit)
            {
                path.Add(currentGridPosition);
                continue;
            }

            if (currentTileType == TileType.Bridge)
            {
                //Perform score check here
                var bridge = level.GetBridge(currentGridPosition);
                for (var i = 0; i < bridge.bridgeParts.Count; i++)
                {
                    if (i == 0 || i == bridge.bridgeParts.Count - 1) continue;
                    path.Add(bridge.bridgeParts[i].ToVector3Int());
                }

                currentGridPosition = bridge.bridgeParts[bridge.bridgeParts.Count - 2].ToVector3Int();
                direction = (bridge.bridgeParts[bridge.bridgeParts.Count - 1] -
                            bridge.bridgeParts[bridge.bridgeParts.Count - 2]).ToVector3Int();
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

        return true;
    }
}
