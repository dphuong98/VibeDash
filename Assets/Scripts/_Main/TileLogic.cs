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
    /// <param name="stageData"></param>
    /// <param name="start"></param>
    /// <param name="direction"></param>
    /// <param name="path"></param>
    /// <returns>false if player fell out of the map</returns>
    public static bool TryMove(this StageData stageData, Vector2Int start, Vector2Int direction, out List<Vector2Int> path)
    {
        path = new List<Vector2Int>();
        var currentTilePosition = start;

        while (true)
        {
            currentTilePosition += direction;
            //Out of bound, count as air
            if (0 > currentTilePosition.x || currentTilePosition.x >= stageData.Size.x ||
                0 > currentTilePosition.y || currentTilePosition.y >= stageData.Size.y)
                return false;
            
            var currentTileType = stageData[currentTilePosition.x, currentTilePosition.y];

            //Player fell out of the map
            if (currentTileType == TileType.Air)
            {
                return false;
            }
            
            //Exit
            if (currentTileType == TileType.Exit)
            {
                path.Add(currentTilePosition);
                return true;
            }

            //Impassible
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
            }

            if (currentTileType == TileType.Road || currentTileType == TileType.Entrance)
            {
                path.Add(currentTilePosition);
                continue;
            }

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
            }

            break; //Treat unknown tile as wall
        }

        return true;
    }
}
