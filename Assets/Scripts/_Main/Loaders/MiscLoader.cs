
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MiscLoader
{
    public static GameObject LoadPlayerObject(Level level)
    {
        var entranceStage = level.LevelData.StagePositions.First();
        var playerGridPos = entranceStage.Key.GetEntrance();
        var playerPos = level.LevelGrid.CellToWorld(entranceStage.Value) + level.LevelGrid.GetCellCenterWorld(playerGridPos);
        var playerPrefab = Resources.Load<GameObject>("Prefabs/Game/PlayerObject");
        var playerObject = Object.Instantiate(playerPrefab, playerPos, Quaternion.identity);
        playerObject.GetComponent<Player>().Level = level;

        return playerObject;
    }

    public static GameObject LoadFinishTrigger(Level level)
    {
        var lastStagePosition = level.LevelData.StagePositions.Last();
        var exitGridPos = lastStagePosition.Value + lastStagePosition.Key.GetExit().ToVector3Int();
        var emptyNeighbor = level.GetEmptyNeighbor(exitGridPos);
        if (emptyNeighbor == null)
        {
            Debug.LogError("There is no empty spot next to last stage's exit. Level loading is halted.");
        }

        var finishTriggerPrefab = Resources.Load<GameObject>("Prefabs/Game/FinishTrigger");
        return Object.Instantiate(finishTriggerPrefab, level.LevelGrid.GetCellCenterWorld((Vector3Int) emptyNeighbor), Quaternion.identity, level.transform);
    }
}
