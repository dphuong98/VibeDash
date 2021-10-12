
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
    
    public GameObject LoadPlayerObject(Grid levelGrid, LevelData levelData)
    {
        var entranceStage = levelData.StagePositions.First();
        var playerGridPos = entranceStage.Key.GetEntrance();
        var playerPos = entranceStage.Value + levelGrid.GetCellCenterWorld(playerGridPos);

        return Object.Instantiate(PlayerPrefab, playerPos, Quaternion.identity);
    }
}
