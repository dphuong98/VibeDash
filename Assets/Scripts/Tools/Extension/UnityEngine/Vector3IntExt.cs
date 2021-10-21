using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3IntExt
{
    public static Vector3Int RotateClockwise(this Vector3Int vector)
    {
        vector.x += vector.y;
        vector.y = vector.x - vector.y;
        vector.x -= vector.y;

        vector.y = -vector.y;

        return vector;
    }
    
    public static Vector3Int RotateCounterClockwise(this Vector3Int vector)
    {
        vector.x += vector.y;
        vector.y = vector.x - vector.y;
        vector.x -= vector.y;

        vector.x = -vector.x;
        
        return vector;
    }
}
