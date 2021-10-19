using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PathGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="path"></param>
    /// <returns>false if the player fell out of the map</returns>
    public static bool TryMove(Transform playerTransform, Vector3Int direction, Level level, out List<Vector3Int> path)
    {
        path = new List<Vector3Int>();
        
        var currentGridPosition = level.LevelGrid.WorldToCell(playerTransform.position);
        path.Add(currentGridPosition);
        
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
            
            if (currentTileType == TileType.Road || currentTileType == TileType.Blank ||
                currentTileType == TileType.Entrance || currentTileType == TileType.Exit)
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
                    path.Add(bridge.bridgeParts[i]);
                }

                currentGridPosition = bridge.bridgeParts[bridge.bridgeParts.Count - 2];
                direction = (bridge.bridgeParts[bridge.bridgeParts.Count - 1] -
                            bridge.bridgeParts[bridge.bridgeParts.Count - 2]);
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
