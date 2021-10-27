using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public interface IPlayer: IBasicObject
{
    Transform Root { get; }
    ILevel Level { get; }
}

public class Player : MonoBehaviour, IPlayer
{
    [SerializeField] private Level level;

    public Transform Root => transform;
    public ILevel Level { get; private set; }

    public void Setup()
    {
        Level = level;
    }

    public void CleanUp()
    {
        
    }
}
