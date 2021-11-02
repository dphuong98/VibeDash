using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MakeSetPlayerModelButtons : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    
    private void Awake()
    {
        var playerModels = Resources.LoadAll<GameObject>(ResourcePaths.PlayerModelFolder);
        foreach (var playerModel in playerModels)
        {
            
        }
    }
}
