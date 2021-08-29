using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

public class Testspam : MonoBehaviour
{
    [SerializeField, HideInInspector] private int i;

    [ContextMenu("SetInt")]
    public void SetInt()
    {
        i = 3;
    }

    [ContextMenu("GetInt")]
    public void GetInt()
    {
        Debug.Log(i);
    }
}