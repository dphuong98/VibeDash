using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Because Unity does not serialize generic type, please inherit this class to use it
/// E.g. public class IntStringSerializableDictionary : SerializableDictionary&lt;int, string> { }
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[Serializable]
public abstract class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector]
    private List<TKey> keyData = new List<TKey>();
	
    [SerializeField, HideInInspector]
    private List<TValue> valueData = new List<TValue>();

    public void OnBeforeSerialize()
    {
        this.keyData.Clear();
        this.valueData.Clear();

        foreach (var item in this)
        {
            this.keyData.Add(item.Key);
            this.valueData.Add(item.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        this.Clear();
        for (int i = 0; i < this.keyData.Count && i < this.valueData.Count; i++)
        {
            this[this.keyData[i]] = this.valueData[i];
        }
    }
}