using System.Collections;
using System.Collections.Generic;
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
}
