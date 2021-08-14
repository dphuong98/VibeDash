using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageLogic : MonoBehaviour
{
    public PlayerObject playerObject;
    public Grid grid;

    public Vector3Int MoveToEnd(Vector3Int direction)
    {
        var playerGridPosition = grid.WorldToCell(playerObject.transform.position);
        
        
        return new Vector3Int();
    }
}
