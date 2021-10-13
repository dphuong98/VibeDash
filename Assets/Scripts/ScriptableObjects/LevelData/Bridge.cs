using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Bridge
{
    [SerializeField] public readonly int MaxLength;
    [SerializeField] public List<Vector3> bridgeParts;

    public Bridge(int maxLength)
    {
        MaxLength = maxLength;
        bridgeParts = new List<Vector3>();
    }

    public bool IsValid()
    {
        return bridgeParts.Count <= MaxLength;
    }
}
