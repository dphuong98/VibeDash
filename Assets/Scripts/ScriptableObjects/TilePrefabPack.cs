using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TilePack))]
public class TilePackDrawer : DictionaryDrawer<TileType, GameObject> { }

[Serializable]
public class TilePack : SerializableDictionary<TileType, GameObject> { }

[CreateAssetMenu(fileName = "Pack", menuName = "ScriptableObjects/TilePrefabPack", order = 1)]
public class TilePrefabPack : ScriptableObject
{
    [SerializeField] public TilePack TilePack;
}
