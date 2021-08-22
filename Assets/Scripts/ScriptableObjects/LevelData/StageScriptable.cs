using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum TileType
{
    Wall,
    Road
}

[CreateAssetMenu(fileName = "Stage", menuName = "Scriptables/Stage", order = 1)]
public class StageScriptable : ScriptableObject
{
    public const int maxSize = 50;
    public Vector2Int Entrance;
    public Vector2Int Exit;

    private Vector2Int stageSize;
    public Vector2Int StageSize
    {
        get => stageSize;
        set
        {
            if (value.x <= maxSize && value.y <= maxSize)
            {
                stageSize = value;
            }
        }
    }
    public TileType[,] tileData = new TileType[maxSize,maxSize];
}
