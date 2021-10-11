using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Level DebugLevel;
    
    private static Level AutoloadLevel;

    private void Start()
    {
        if (AutoloadLevel) LevelLoader.Instance.LoadLevel(AutoloadLevel);
    }
    
    public static void SetAutoloadLevel(Level level)
    {
        AutoloadLevel = level;
    }

    [ContextMenu("Load Debug")]
    public void LoadDebug()
    {
        AutoloadLevel = DebugLevel;
        EditorApplication.isPlaying = true;
    }
}
