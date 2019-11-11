using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraShaker : Shaker
{
    public Camera Camera { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Camera = GetComponent<Camera>();
    }

    public static void S_ShakeAllCameras(ShakeSettings settings)
    {
        foreach (var transform in transforms.Where(t=>t.GetComponent<CameraShaker>() != null))
           transform.Shake(settings);
    }

    public void ShakeAllCameras(ShakeSettings settings)
    {
        CameraShaker.S_ShakeAllCameras(settings);
    }
}
