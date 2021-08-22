using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader
{
    public static void LoadLevel(Transform parent, LevelScriptable levelData)
    {
        Debug.Log(levelData.name);
    }

    public static void LoadStageAt(Transform parent, StageScriptable stageData, Vector3 entrancePosition)
    {
        
    }
}
