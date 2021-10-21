using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.Events;

[Serializable] public class Vector3IntEvent : UnityEvent<Vector3Int> {}

public class GameInput : MonoBehaviour
{
    public static Vector3IntEvent OnSwipeDirection { get; } = new Vector3IntEvent();

    public static void EnableInput()
    {
        LeanTouch.OnFingerSwipe += HandleFingerSwipe;
    }

    public static void DisableInput()
    {
        LeanTouch.OnFingerSwipe -= HandleFingerSwipe;
    }

    private static void HandleFingerSwipe(LeanFinger finger)
    {
        if (finger.StartedOverGui ||
            finger.IsOverGui
            )
        {
            return;
        }

        var finalDelta = finger.StartScreenPosition - finger.ScreenPosition;
        var direction = Vector3Int.zero;
        
        if (Math.Abs(finalDelta.x) > Math.Abs(finalDelta.y))
        {
            direction = finalDelta.x > 0 ? Vector3Int.left : Vector3Int.right;
        }
        else
        {
            direction = finalDelta.y > 0 ? Vector3Int.down : Vector3Int.up;
        }
        
        OnSwipeDirection.Invoke(direction);
    }
}
