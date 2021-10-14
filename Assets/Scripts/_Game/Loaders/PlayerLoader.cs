
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerLoader
{
    public GameObject PlayerPrefab;

    public PlayerLoader()
    {
        PlayerPrefab = Resources.Load<GameObject>("Prefabs/Avatars/PlayerObject");
    }
    
    public GameObject LoadPlayerObject(Level level)
    {
        var entranceStage = level.LevelData.StagePositions.First();
        var playerGridPos = entranceStage.Key.GetEntrance();
        var playerPos = level.LevelGrid.CellToWorld(entranceStage.Value) + level.LevelGrid.GetCellCenterWorld(playerGridPos);

        return Object.Instantiate(PlayerPrefab, playerPos, Quaternion.identity);
    }
}
