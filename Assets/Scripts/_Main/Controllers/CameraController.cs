
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3 PositionOffset = new Vector3(0, 10, -10);
    public Vector3 RotationOffset = new Vector3(45, 0, 0);
    
    private void Update()
    {
        if (Camera.main == null) return;

        Camera.main.transform.position = transform.position + PositionOffset;
        Camera.main.transform.rotation = Quaternion.Euler(RotationOffset);
    }
}
