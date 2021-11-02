using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExt
{
    public static Transform FindInChildren(this Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;
        }

        return null;
    }
    
    public static void SetLayer(this Transform root, int layer) {
        root.gameObject.layer = layer;
        foreach(Transform child in root)
            SetLayer(child, layer);
    }
}
