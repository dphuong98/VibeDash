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
        var currentGridPosition = level.LevelGrid.WorldToCell(playerTransform.position);
        path = new List<Vector3Int>();

        while (true)
        {
            currentGridPosition += direction;

            var currentTileType = level.GetTileType(currentGridPosition);

            if (currentTileType == TileType.Air)
            {
                return false;
            }
            
            //Impassible
            if (currentTileType == TileType.Wall)
                break;
            
            if (currentTileType == TileType.Stop)
            {
                path.Add(currentGridPosition);
                break;
            }
            
            /*if (currentTileType == TileType.PortalBlue)
            {
                path.Add(currentGridPosition);
                
                //If exit does not exist act as stop
                var portal = stageData.PortalPairs.Where(s => s.Blue == currentTilePosition);
                if (portal.Any() && portal.First().Orange != -Vector2Int.one)
                {
                    path.Add(portal.First().Orange);
                }
                
                break;
            }
            
            if (currentTileType == TileType.PortalOrange)
            {
                path.Add(currentTilePosition);
                
                //If there is no portal act as stop
                var portal = stageData.PortalPairs.Where(s => s.Orange == currentTilePosition);
                if (portal.Any() && portal.First().Blue != -Vector2Int.one)
                {
                    path.Add(portal.First().Blue);
                }
                
                break;
            }*/
            
            if (currentTileType == TileType.Road || currentTileType == TileType.Entrance || currentTileType == TileType.Exit)
            {
                path.Add(currentGridPosition);
                continue;
            }
            
            /*
            if (currentTileType == TileType.Push)
            {
                path.Add(currentTilePosition);
                direction = stageData.TileDirections[currentTilePosition];
                continue;
            }
            
            if (currentTileType == TileType.Corner)
            {
                var upVector = stageData.TileDirections[currentTilePosition];
                var rightVector = upVector.RotateClockwise();
                
                path.Add(currentTilePosition);

                if (direction == -upVector) direction = rightVector;
                if (direction == -rightVector) direction = upVector;
                continue;
            }*/

            break;
        }

        return true;
    }
}
