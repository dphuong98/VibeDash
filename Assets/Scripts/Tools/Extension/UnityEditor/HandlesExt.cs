using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HandlesExt
{
    public static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        Handles.color = color;
        Handles.DrawLine(start, end);
    }
    
    public static void DrawPath(Vector3 start, Vector3 end, Color color)
    {
        var size = 0.1f;
        Handles.color = color;
        Handles.DrawLine(start, end);
        var headLength = (start - end).normalized * size;
        var perpendicularVector = (start - end).RotateClockwiseXY().normalized * size;
        var arrowTip = start + (end - start).normalized / 4;
        var arrowTriangle = new [] { arrowTip, arrowTip + headLength + perpendicularVector, arrowTip + headLength - perpendicularVector };
        Handles.DrawAAConvexPolygon(arrowTriangle);
    }

    public static void DrawArrow(Vector3 start, Vector3 end, Color color)
    {
        var size = 0.1f;
        Handles.color = color;
        Handles.DrawLine(start, end);
        var headLength = (start - end).normalized * size;
        var perpendicularVector = (start - end).RotateClockwiseXY().normalized * size;
        var arrowTriangle = new [] { end, end + headLength + perpendicularVector, end + headLength - perpendicularVector };
        Handles.DrawAAConvexPolygon(arrowTriangle);
    }

    public static void DrawSimpleArc(Vector3 start, Vector3 end, Color color)
    {
        Handles.color = color;
        var offsetRatio = 0.3f;
        var centerOffset = (end - start).RotateClockwiseXY().normalized * (end - start).magnitude * offsetRatio;
        centerOffset.x = -Math.Abs(centerOffset.x);
        centerOffset.y = -Math.Abs(centerOffset.y);
        var center = (start + end) / 2 + centerOffset;
        var normal = Vector3.back;
        var from = new Vector3();
        if (start.y == end.y) from = start.x < end.x ? start - center : end - center;
        if (start.x == end.x) from = start.y < end.y ? end - center : start - center;
        var angle = (float)(Math.Atan(1 / ( 2 * offsetRatio)) * (180/Math.PI) * 2);
        var radius = from.magnitude;
        Handles.DrawWireArc(center, normal, from, angle, radius); 
    }
}
