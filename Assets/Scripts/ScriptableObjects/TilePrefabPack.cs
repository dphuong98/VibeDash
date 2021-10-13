using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Pack", menuName = "ScriptableObjects/TilePrefabPack", order = 1)]
public class TilePrefabPack : ScriptableObject
{
    public GameObject WallPrefab;
    public GameObject RoadPrefab;
    public GameObject StopPrefab;
    public GameObject PortalBluePrefab;
    public GameObject PortalOrangePrefab;
    public GameObject PushPrefab;
    public GameObject CornerPrefab;
    public GameObject BlankPrefab;
}
