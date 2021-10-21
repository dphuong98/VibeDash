using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Bridge
{
    [SerializeField] public readonly int MaxLength;
    [FormerlySerializedAs("BridgeParts")] [SerializeField] public List<Vector3Int> bridgeParts;

    public Bridge(int maxLength)
    {
        MaxLength = maxLength;
        bridgeParts = new List<Vector3Int>();
    }
    
    //TODO remove max length
    public Bridge(int maxLength, List<Vector3Int> bridgeParts)
    {
        MaxLength = maxLength;
        this.bridgeParts = new List<Vector3Int>(bridgeParts);
    }

    public bool IsValid()
    {
        Debug.Log(bridgeParts.Count);
        Debug.Log(MaxLength);
        return bridgeParts.Count <= MaxLength;
    }

    public Bridge ReverseBridge()
    {
        var bridgeClone = new List<Vector3Int>(bridgeParts);
        bridgeClone.Reverse();
        return new Bridge(MaxLength, bridgeClone);
    }
}
