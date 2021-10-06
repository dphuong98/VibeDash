using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class StagePosition : SerializableDictionary<Vector3, Stage>
{
    public StagePosition() : base() {}
    public StagePosition(StagePosition otherStagePositions) : base(otherStagePositions) { }

    public StagePosition(IDictionary<Vector3, Stage> dictionary) : base(dictionary) { }
}

[Serializable]
public class Level : ScriptableObject, IInit, ICopiable<Level>
{
    //TODO into serializableDictionary
    [SerializeField, HideInInspector] private StagePosition stagePositions = new StagePosition();
    public StagePosition StagePositions => new StagePosition(stagePositions);
    
    [SerializeField, HideInInspector] private List<Bridge> bridges = new List<Bridge>();
    public List<Bridge> Bridges => new List<Bridge>(bridges);

    public void Init()
    {
        
    }

    public void CopyFrom(Level other)
    {
        this.stagePositions = new StagePosition(other.stagePositions);
        this.bridges = new List<Bridge>(other.bridges);
    }

    public void Import(Dictionary<Vector3, Stage> stagePositions, List<Bridge> bridges)
    {
        this.stagePositions = new StagePosition(stagePositions);
        this.bridges = new List<Bridge>(bridges);
    }
}
