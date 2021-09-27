﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Ext
{
    //TODO Doesn't make much sense to rotate a vector3
    public static Vector3 RotateClockwiseXY(this Vector3 vector)
    {
        vector.x += vector.y;
        vector.y = vector.x - vector.y;
        vector.x -= vector.y;

        vector.y = -vector.y;

        return vector;
    }
    
    public static Vector3 RotateCounterClockwiseXY(this Vector3 vector)
    {
        vector.x += vector.y;
        vector.y = vector.x - vector.y;
        vector.x -= vector.y;

        vector.x = -vector.x;

        return vector;
    }
}