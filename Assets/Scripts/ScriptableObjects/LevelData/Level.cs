using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Level : ScriptableObject, IInit, ICopiable<Level>
{
    //TODO is this ref?
    [SerializeField, HideInInspector] private Dictionary<Vector2Int, Stage> stages = new Dictionary<Vector2Int, Stage>();
    [SerializeField, HideInInspector] private Dictionary<Stage, Stage> bridges = new Dictionary<Stage, Stage>();

    public void Init()
    {
        
    }
    
    public static Level CreateLevel()
    {
        var newLevel = CreateInstance<Level>();
        newLevel.Init();
        return newLevel;
    }
    
    public void CopyFrom(Level other)
    {
        stages = new Dictionary<Vector2Int, Stage>(other.stages);
        bridges = new Dictionary<Stage, Stage>(other.bridges);
    }
}
