using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LinqExt
{
    public static IEnumerable<int> IndicesOf<TSource>(this IEnumerable<TSource> source, TSource value)
    {
        var sourceList = source.ToList();
        var result = Enumerable.Range(0, sourceList.Count)
            .Where(i => EqualityComparer<TSource>.Default.Equals(sourceList[i], value));
        return result;
    }
}
