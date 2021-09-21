using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge
{
    public readonly int MaxLength;
    public List<Vector2Int> bridgeParts;

    public Bridge(int maxLength)
    {
        MaxLength = maxLength;
        bridgeParts = new List<Vector2Int>();
    }
}
