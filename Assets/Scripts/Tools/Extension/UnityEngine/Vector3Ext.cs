using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Axis
{
    X, Y, Z
}

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
    
    public static Vector3 RotateCounterClockwiseXZ(this Vector3 vector)
    {
        vector.x += vector.z;
        vector.z = vector.x - vector.z;
        vector.x -= vector.z;

        vector.x = -vector.x;

        return vector;
    }
    
    public static bool IsCBetweenAB (Vector3 A, Vector3 B, Vector3 C ) {
        return Vector3.Dot( (B-A).normalized , (C-B).normalized ) <0f && Vector3.Dot( (A-B).normalized , (C-A).normalized ) <0f;
    }
}
