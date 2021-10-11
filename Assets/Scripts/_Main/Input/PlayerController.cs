using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private void OnEnable()
    {
        InputManager.OnSwipeDirection.AddListener(HandleSwipe);
    }

    private void OnDisable()
    {
        InputManager.OnSwipeDirection.RemoveListener(HandleSwipe);
    }

    private void HandleSwipe(Vector2Int direction)
    {
        Debug.Log(direction);
    }
}
