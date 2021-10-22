using System;
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

    private void Update()
    {
        transform.position += Vector3.down * 0.1f;
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger");
    }
}
