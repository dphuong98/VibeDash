using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private void OnEnable()
    {
        LeanTouch.OnFingerSwipe += HandleFingerSwipe;
    }
    
    protected virtual void OnDisable()
    {
        LeanTouch.OnFingerSwipe -= HandleFingerSwipe;
    }

    private void HandleFingerSwipe(LeanFinger finger)
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
        
        Debug.Log(direction);
    }
}
