using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.Events;

[Serializable] public class Vector2IntEvent : UnityEvent<Vector2Int> {}

public class InputManager : MonoBehaviour
{
    public static Vector2IntEvent OnSwipeDirection = new Vector2IntEvent(); 
    
    private void OnEnable()
    {
        LeanTouch.OnFingerSwipe += HandleFingerSwipe;
    }
    
    private void OnDisable()
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
        var direction = Vector2Int.zero;
        
        if (Math.Abs(finalDelta.x) > Math.Abs(finalDelta.y))
        {
            direction = finalDelta.x > 0 ? Vector2Int.left : Vector2Int.right;
        }
        else
        {
            direction = finalDelta.y > 0 ? Vector2Int.down : Vector2Int.up;
        }
        
        OnSwipeDirection.Invoke(direction);
    }
}
