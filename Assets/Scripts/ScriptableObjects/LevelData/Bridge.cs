using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Bridge
{
    public List<Vector3Int> BridgeParts { get; private set; }

    public Bridge(List<Vector3Int> bridgeParts)
    {
        BridgeParts = new List<Vector3Int>(bridgeParts);
    }

    public Bridge ReverseBridge()
    {
        var bridgeClone = new List<Vector3Int>(BridgeParts);
        bridgeClone.Reverse();
        return new Bridge(bridgeClone);
    }
}
