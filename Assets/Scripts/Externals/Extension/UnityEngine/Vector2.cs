﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Ext
{
    public static void RotateClockwise(ref this Vector2 vector)
    {
        vector.x += vector.y;
        vector.y = vector.x - vector.y;
        vector.x -= vector.y;

        vector.y = -vector.y;
    }
    
    public static void RotateCounterClockwise(ref this Vector2 vector)
    {
        vector.x += vector.y;
        vector.y = vector.x - vector.y;
        vector.x -= vector.y;

        vector.x = -vector.x;
    }
}
