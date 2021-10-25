using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class testspam : MonoBehaviour
{
    [ContextMenu("Test")]
    public void Test()
    {
        Debug.Log(new Vector2Int(0, 0).IsSameDirection(new Vector2Int(0, 0)));
    }
}
