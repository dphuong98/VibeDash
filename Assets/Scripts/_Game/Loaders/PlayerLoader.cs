
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerLoader
{
    public static GameObject LoadPlayerObject(Level level)
    {
        var entranceStage = level.LevelData.StagePositions.First();
        var playerGridPos = entranceStage.Key.GetEntrance();
        var playerPos = level.LevelGrid.CellToWorld(entranceStage.Value) + level.LevelGrid.GetCellCenterWorld(playerGridPos);
        var playerPrefab = Resources.Load<GameObject>("Prefabs/Game/PlayerObject");
        
        return Object.Instantiate(playerPrefab, playerPos, Quaternion.identity);
    }
}
