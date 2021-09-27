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
        var arrowTip = start + (end - start) / 3;
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
        var center = (start + end) / 2;
        var normal = Vector3.back;
        var from = start - end;
        from.x = Math.Abs(from.x);
        from.y = Math.Abs(from.y);
        var angle = 180;
        var radius = (start - end).magnitude / 2;
        Handles.DrawWireArc(center, normal, from, angle, radius); 
    }
}
