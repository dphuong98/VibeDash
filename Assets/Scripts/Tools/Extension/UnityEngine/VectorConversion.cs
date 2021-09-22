using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorConversion
{
    /// <summary>
    /// z becomes 0
    /// </summary>
    /// <param name="vector3"></param>
    /// <returns></returns>
    public static Vector2Int ToVector2Int(this Vector3Int vector3)
    {
        return new Vector2Int(vector3.x, vector3.y);
    }
}
