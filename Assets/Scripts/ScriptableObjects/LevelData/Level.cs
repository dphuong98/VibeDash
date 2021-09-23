using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class Level : ScriptableObject, IInit, ICopiable<Level>
{
    [SerializeField, HideInInspector] private List<Stage> stageItems = new List<Stage>();
    [SerializeField, HideInInspector] private List<Vector2Int> stagesPositions = new List<Vector2Int>();
    public Dictionary<Stage, Vector2Int> Stages => stageItems.Zip(stagesPositions, (k, v) => new { Key = k, Value = v })
        .ToDictionary(x => x.Key, x => x.Value);
    
    [SerializeField, HideInInspector] private List<Bridge> bridges = new List<Bridge>();
    public List<Bridge> Bridges => new List<Bridge>(bridges);

    public void Init()
    {
        
    }

    public void CopyFrom(Level other)
    {
        this.stageItems = new List<Stage>(other.stageItems);
        this.stagesPositions = new List<Vector2Int>(other.stagesPositions);
        this.bridges = new List<Bridge>(other.bridges);
    }

    public void Import(Dictionary<Stage, Vector2Int> stages, List<Bridge> bridges)
    {
        this.stageItems = new List<Stage>(stages.Keys);
        this.stagesPositions = new List<Vector2Int>(stages.Values);
        this.bridges = new List<Bridge>(bridges);
    }
}
