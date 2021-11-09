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

    public static int Div(float num1, float num2)
    {
        var times = 1;
        while (num2 * times <= num1)
        {
            times++;
        }

        return times - 1;
    }

    public static float Mod(float num1, float num2)
    {
        return num1 - num2 * Div(num1, num2);
    }
}
