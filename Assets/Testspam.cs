using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Testspam : MonoBehaviour
{
    public PlayerController playerController;
    
    // Start is called before the first frame update
    void Start()
    {
        playerController.MoveTo(new Vector3Int(2, 3, 0));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}