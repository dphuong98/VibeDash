using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Level : ScriptableObject, IInit, ICopiable<Level>
{
    [SerializeField, HideInInspector] private Dictionary<Stage, Vector2Int> stages = new Dictionary<Stage, Vector2Int>();

    public void Init()
    {
        
    }

    public void CopyFrom(Level other)
    {
        stages = new Dictionary<Stage, Vector2Int>(other.stages);
    }

    public void Import(Dictionary<Stage, Vector2Int> stages)
    {
        this.stages = new Dictionary<Stage, Vector2Int>(stages);
    }

    public Dictionary<Stage, Vector2Int> GetStages()
    {
        return stages;
    }
}
