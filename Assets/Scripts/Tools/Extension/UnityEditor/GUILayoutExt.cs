using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GUILayoutExt
{
    public static void HorizontalSeparator()
    {
        GUIStyle styleHR = new GUIStyle(GUI.skin.box);
        styleHR.stretchWidth = true;
        styleHR.fixedHeight = 2;
        GUILayout.Box("", styleHR);
    }

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
        Handles.color = color;
        Handles.DrawLine(start, end);
        var headLength = (start - end) / 10;
        var pendicularVector = start - end;
        pendicularVector.RotateClockwiseXY();
        pendicularVector /= 11;
        var arrowTriangle = new [] { end, end + headLength + pendicularVector, end + headLength - pendicularVector };
        Handles.DrawAAConvexPolygon(arrowTriangle);
    }
}
