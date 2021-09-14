using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2IntExt
{
    public static void RotateClockwise(ref this Vector2Int vector)
    {
        vector.x += vector.y;
        vector.y = vector.x - vector.y;
        vector.x -= vector.y;

        vector.y = -vector.y;
    }
}
