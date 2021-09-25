using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorConversion
{
    /// <summary>
    /// z is removed
    /// </summary>
    /// <param name="vector3"></param>
    /// <returns></returns>
    public static Vector2Int ToVector2Int(this Vector3Int vector3)
    {
        return new Vector2Int(vector3.x, vector3.y);
    }
    
    /// <summary>
    /// z is 0
    /// </summary>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static Vector3Int ToVector3Int(this Vector2Int vector2)
    {
        return new Vector3Int(vector2.x, vector2.y, 0);
    }
    
    /// <summary>
    /// z is removed
    /// </summary>
    /// <param name="vector3"></param>
    /// <returns></returns>
    public static Vector2 ToVector2(this Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.y);
    }
    
    /// <summary>
    /// z is 0
    /// </summary>
    /// <param name="vector2"></param>
    /// <returns></returns>
    public static Vector3 ToVector3(this Vector2 vector2)
    {
        return new Vector3(vector2.x, vector2.y, 0);
    }
}
