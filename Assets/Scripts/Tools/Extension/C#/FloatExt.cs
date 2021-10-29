using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatExt
{
    public static void Swap(ref float num1, ref float num2)
    {
        var tmp = num1;
        num1 = num2;
        num2 = tmp;
    }
}
