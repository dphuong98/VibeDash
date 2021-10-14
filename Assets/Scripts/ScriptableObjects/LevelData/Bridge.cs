using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Bridge
{
    [SerializeField] public readonly int MaxLength;
    [SerializeField] public List<Vector2Int> bridgeParts;

    public Bridge(int maxLength)
    {
        MaxLength = maxLength;
        bridgeParts = new List<Vector2Int>();
    }

    public bool IsValid()
    {
        return bridgeParts.Count <= MaxLength;
    }
}
