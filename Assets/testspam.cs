using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class testspam : MonoBehaviour
{
    [ContextMenu("Test")]
    public void Test()
    {
        EditorApplication.ExecuteMenuItem("Tools/ProGrids/Close ProGrids");
    }
}
