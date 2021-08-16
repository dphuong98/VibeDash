using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    public StageLogic stageLogic;
    public PlayerController playerController;
    
    public void Move(Vector3Int direction)
    {
        Vector3Int destination;
        if (stageLogic.TryMove(direction, out destination))
        {
            playerController.MoveTo(destination);
        }
    }
}
