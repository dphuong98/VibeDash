using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool IsReceivingInput
    {
        get;
        private set;
    }
    
    public PlayerObject playerObject;
    public Grid grid;

    public void MoveTo(Vector3Int destination)
    {
        var destinationPosition = grid.GetCellCenterWorld(destination);
        playerObject.transform.position = destinationPosition;
    }
}
