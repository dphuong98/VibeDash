using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.Events;

[Serializable] public class Vector3IntEvent : UnityEvent<Vector3Int> {}

public interface IInputController: IBasicObject
{
    Vector3IntEvent OnSwipeDirection { get; }

    void HandleFingerSwipe(LeanFinger finger);
}

public class InputController : MonoBehaviour, IInputController
{
    public Vector3IntEvent OnSwipeDirection { get; } = new Vector3IntEvent();
    
    
    public void Setup()
    {
        LeanTouch.OnFingerSwipe += HandleFingerSwipe;
    }

    public void CleanUp()
    {
        LeanTouch.OnFingerSwipe -= HandleFingerSwipe;
    }

    public void HandleFingerSwipe(LeanFinger finger)
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
