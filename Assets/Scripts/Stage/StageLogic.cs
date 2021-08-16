using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StageLogic : MonoBehaviour
{
    public PlayerObject playerObject;
    public Grid grid;
    public Tilemap tilemap;

    public bool TryMove(Vector3Int direction, out Vector3Int destination)
    {
        var playerGridPosition = grid.WorldToCell(playerObject.transform.position);
        while (true)
        {
            var cellObject = PrefabGrid.GetObjectInCell(grid, tilemap.transform, playerGridPosition + direction);
            if (cellObject == null || cellObject.name == "Wall")
                break;
            
            if (cellObject.name == "Road")
            {
                playerGridPosition += direction;
                continue;
            }

            break; //In case of unknown tile objects
        }
        
        destination = playerGridPosition;
        return playerGridPosition == destination;
    }
}
