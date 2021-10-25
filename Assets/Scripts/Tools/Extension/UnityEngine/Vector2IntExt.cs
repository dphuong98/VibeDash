using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2IntExt
{
    public static Vector2Int RotateClockwise(this Vector2Int vector)
    {
        vector.x += vector.y;
        vector.y = vector.x - vector.y;
        vector.x -= vector.y;

        vector.y = -vector.y;

        return vector;
    }
    
    public static Vector2Int RotateCounterClockwise(this Vector2Int vector)
    {
        vector.x += vector.y;
        vector.y = vector.x - vector.y;
        vector.x -= vector.y;

        vector.x = -vector.x;
        
        return vector;
    }

    public static bool IsAdjacent(this Vector2Int v1, Vector2Int v2)
    {
        return (v1 - v2).magnitude == 1;
    }

    public static bool IsSameDirection(this Vector2Int v1, Vector2Int v2)
    {
        return (v1.x == v2.x || v1.x * v2.x > 0) &&
               (v1.y == v2.y || v1.y * v2.y > 0);
    }
}
