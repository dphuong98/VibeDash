using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Level : ScriptableObject, IInit, ICopiable<Level>
{
    //TODO is this ref?
    [SerializeField, HideInInspector] private Dictionary<Stage, Vector2Int> stages = new Dictionary<Stage, Vector2Int>();

    public void Init()
    {
        
    }

    public Dictionary<Stage, Vector2Int> GetStages()
    {
        return stages;
    }
    
    public void CopyFrom(Level other)
    {
        stages = new Dictionary<Stage, Vector2Int>(other.stages);
    }

    public void AddStage(Stage stage, Vector2Int gridPos)
    {
        stages.Add(stage, gridPos);
    }

    public void UpdateStagePosition(Stage stage, Vector2Int gridPos)
    {
        
    }

    public void RemoveStage(Stage stage)
    {
        stages.Remove(stage);
    }
}
