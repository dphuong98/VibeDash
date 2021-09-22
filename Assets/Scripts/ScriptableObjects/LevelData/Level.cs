using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Level : ScriptableObject, IInit, ICopiable<Level>
{
    [SerializeField, HideInInspector] private Dictionary<Stage, Vector2Int> stages = new Dictionary<Stage, Vector2Int>();
    public Dictionary<Stage, Vector2Int> Stages => new Dictionary<Stage, Vector2Int>(stages);
    
    [SerializeField, HideInInspector] private List<Bridge> bridges = new List<Bridge>();
    public List<Bridge> Bridges => new List<Bridge>(bridges);

    public void Init()
    {
        
    }

    public void CopyFrom(Level other)
    {
        this.stages = new Dictionary<Stage, Vector2Int>(other.stages);
        this.bridges = new List<Bridge>(other.bridges);
    }

    public void Import(Dictionary<Stage, Vector2Int> stages, List<Bridge> bridges)
    {
        this.stages = new Dictionary<Stage, Vector2Int>(stages);
        this.bridges = new List<Bridge>(bridges);
    }
}
