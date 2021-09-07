﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TileLogic
{
    public static bool TryMove(this Stage stage, Vector2Int start, Vector2Int direction, out Vector2Int destination, out int weight)
    {
        var currentTilePosition = destination = start;
        weight = 0;

        while (true)
        {
            currentTilePosition += direction;
            if (0 > currentTilePosition.x || currentTilePosition.x >= stage.Size.x ||
                0 > currentTilePosition.y || currentTilePosition.y >= stage.Size.y)
                break;
            
            var currentTileType = stage[currentTilePosition.x, currentTilePosition.y];

            if (currentTileType == TileType.Wall || currentTileType == TileType.Air)
                break;

            //Stop at exit
            if (currentTileType == TileType.Road || currentTileType == TileType.Exit || currentTileType == TileType.Entrance)
            {
                destination = currentTilePosition;
                weight++;
                continue;
            }

            if (currentTileType == TileType.Bridge)
            {
                var oneUpPosition = start + direction;
                if (stage[oneUpPosition.x, oneUpPosition.y] != TileType.Bridge)
                    break;

                destination = currentTilePosition;
                weight--;
                continue;
            }

            break; //Treat unknown tile as wall
        }
        
        return start != destination;
    }
}
