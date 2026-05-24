using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static bool IsEmpty(this IEnumerable source)
    {
        if (source == null) return true;

        var enumerator = source.GetEnumerator();
        using (enumerator as IDisposable)
        {
            return !enumerator.MoveNext();
        }
    }

    public static bool IsEmpty(this IList source)
    {
        return source == null || source.Count <= 0;
    }

    public static T Get<T>(this IList<T> source, int idx)
    {
        if (source != null)
        {
            if (idx >= 0 && idx < source.Count)
            {
                return source[idx];
            }
        }

        return default;
    }
}