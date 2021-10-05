using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TileLogic
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="stage"></param>
    /// <param name="start"></param>
    /// <param name="direction"></param>
    /// <param name="path"></param>
    /// <returns>false if player fell out of the map</returns>
    public static bool TryMove(this Stage stage, Vector2Int start, Vector2Int direction, out List<Vector2Int> path)
    {
        var currentTilePosition = start;
        path = new List<Vector2Int>();

        while (true)
        {
            currentTilePosition += direction;
            //Out of bound
            if (0 > currentTilePosition.x || currentTilePosition.x >= stage.Size.x ||
                0 > currentTilePosition.y || currentTilePosition.y >= stage.Size.y)
                break;
            
            var currentTileType = stage[currentTilePosition.x, currentTilePosition.y];

            //Exit
            if (currentTileType == TileType.Exit)
            {
                path.Add(currentTilePosition);
                return true;
            }
            
            //Player fell out of the map
            if (currentTileType == TileType.Air)
            {
                return false;
            }
            
            //Impassibles
            if (currentTileType == TileType.Wall)
                break;

            if (currentTileType == TileType.Stop)
            {
                path.Add(currentTilePosition);
                break;
            }
            
            if (currentTileType == TileType.PortalBlue)
            {
                path.Add(currentTilePosition);
                
                //If exit does not exist act as stop
                var portal = stage.PortalPairs.Where(s => s.Blue == currentTilePosition);
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
                var portal = stage.PortalPairs.Where(s => s.Orange == currentTilePosition);
                if (portal.Any() && portal.First().Blue != -Vector2Int.one)
                {
                    path.Add(portal.First().Blue);
                }
                
                break;
            }

            if (currentTileType == TileType.Road || currentTileType == TileType.Entrance)
            {
                path.Add(currentTilePosition);
                continue;
            }

            if (currentTileType == TileType.Push)
            {
                path.Add(currentTilePosition);
                direction = stage.TileDirections[currentTilePosition];
                continue;
            }
            
            if (currentTileType == TileType.Corner)
            {
                var upVector = stage.TileDirections[currentTilePosition];
                var rightVector = upVector.RotateClockwise();
                
                path.Add(currentTilePosition);

                if (direction == -upVector) direction = rightVector;
                if (direction == -rightVector) direction = upVector;
                continue;
            }

            break; //Treat unknown tile as wall
        }

        return true;
    }
}
