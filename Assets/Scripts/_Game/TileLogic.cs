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
            if (0 > currentTilePosition.x || currentTilePosition.x >= stage.Size.x ||
                0 > currentTilePosition.y || currentTilePosition.y >= stage.Size.y)
                break;
            
            var currentTileType = stage[currentTilePosition.x, currentTilePosition.y];

            //Player fell out of the map
            if (currentTileType == TileType.Air)
            {
                return false;
            }
            
            //Impassible
            if (currentTileType == TileType.Wall || currentTileType == TileType.Air)
                break;

            if (currentTileType == TileType.Road  || currentTileType == TileType.Entrance)
            {
                path.Add(currentTilePosition);
                continue;
            }
            
            if (currentTileType == TileType.Exit)
            {
                path.Add(currentTilePosition);
                return true;
            }

            break; //Treat unknown tile as wall
        }

        return true;
    }
}
