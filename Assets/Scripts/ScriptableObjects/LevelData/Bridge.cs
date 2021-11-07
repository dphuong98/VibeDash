using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PathCreation;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Bridge
{
    [SerializeField] private List<Vector3Int> bridgeParts;

    public List<Vector3Int> BridgeParts => bridgeParts;

    public Bridge(Bridge bridge)
    {
        this.bridgeParts = new List<Vector3Int>(bridge.bridgeParts);
    }
    
    public Bridge(List<Vector3Int> bridgeParts)
    {
        this.bridgeParts = new List<Vector3Int>(bridgeParts);
    }

    public Bridge ReverseBridge()
    {
        var bridgeClone = new List<Vector3Int>(bridgeParts);
        bridgeClone.Reverse();
        return new Bridge(bridgeClone);
    }
}
