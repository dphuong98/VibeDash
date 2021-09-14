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
    
    public static void DrawPath(Vector3 start, Vector3 end, Color color)
    {
        Handles.color = color;
        Handles.DrawLine(start, end);
        var headLength = (start - end) / 10;
        var pendicularVector = (Vector2)(start - end);
        pendicularVector.RotateClockwise();
        pendicularVector /= 11;
        var pendicularVector3 = new Vector3(pendicularVector.x, pendicularVector.y, 0);
        var arrowTip = start + (end - start) / 3;
        var arrowTriangle = new Vector3[] { arrowTip, arrowTip + headLength + pendicularVector3, arrowTip + headLength - pendicularVector3 };
        Handles.DrawAAConvexPolygon(arrowTriangle);
    }

    public static void DrawArrow(Vector3 start, Vector3 end, Color color)
    {
        Handles.color = color;
        Handles.DrawLine(start, end);
        var headLength = (start - end) / 10;
        var pendicularVector = (Vector2)(start - end);
        pendicularVector.RotateClockwise();
        pendicularVector /= 11;
        var pendicularVector3 = new Vector3(pendicularVector.x, pendicularVector.y, 0);
        var arrowTriangle = new Vector3[] { end, end + headLength + pendicularVector3, end + headLength - pendicularVector3 };
        Handles.DrawAAConvexPolygon(arrowTriangle);
    }
}
