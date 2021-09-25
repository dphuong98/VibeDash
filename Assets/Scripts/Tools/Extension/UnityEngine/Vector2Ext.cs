using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Ext
{
    public static Vector2 RotateClockwise(this Vector2 vector)
    {
        vector.x += vector.y;
        vector.y = vector.x - vector.y;
        vector.x -= vector.y;

        vector.y = -vector.y;

        return vector;
    }
    
    public static Vector2 RotateCounterClockwise(this Vector2 vector)
    {
        vector.x += vector.y;
        vector.y = vector.x - vector.y;
        vector.x -= vector.y;

        vector.x = -vector.x;
        
        return vector;
    }
}
