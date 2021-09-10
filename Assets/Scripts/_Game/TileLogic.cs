using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TileLogic
{
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

            if (currentTileType == TileType.Wall || currentTileType == TileType.Air)
                break;
            
            if (currentTileType == TileType.Road || currentTileType == TileType.Exit || currentTileType == TileType.Entrance)
            {
                path.Add(currentTilePosition);
                continue;
            }

            //Player fall out of the map
            if (currentTileType == TileType.Air)
            {
                return false;
            }

            break; //Treat unknown tile as wall
        }

        return path.Count != 0;
    }
}
