
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public interface ICameraController: IBasicObject
{
    Transform Target { get; }
}

public class CameraController : MonoBehaviour, ICameraController
{
    public Vector3 PositionOffset = new Vector3(0, 10, -10);
    public Vector3 RotationOffset = new Vector3(45, 0, 0);

    [SerializeField] private Transform target;

    public Transform Target => target;
    
    
    public void Setup()
    {
        
    }

    public void CleanUp()
    {
        
    }
    
    private void Update()
    {
        Track();
    }
    
    public void Track()
    {
        if (Camera.main == null || Target == null) return;
        
        Camera.main.transform.position = Target.position + PositionOffset;
        Camera.main.transform.rotation = Quaternion.Euler(RotationOffset);
    }
}
