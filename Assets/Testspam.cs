using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

[Serializable]
public class IntStringDictionary : SerializableDictionary<int, string> { }

public class Testspam : MonoBehaviour
{
    [SerializeField] public IntStringDictionary here = new IntStringDictionary();
    
    [ContextMenu("Add")]
    private void Add()
    {
        here.Add(0, "here");
    }

    [ContextMenu("Check")]
    private void Check()
    {
        Debug.Log(here.Count);
    }
}