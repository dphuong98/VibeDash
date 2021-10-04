
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExt
{
    public static void AddUnique<T>(this List<T> list, T value)
    {
        if (!list.Contains(value))
            list.Add(value);
    }
}
