using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class VirtualCameraShaker : Shaker
{
    public CinemachineVirtualCamera VirtualCamera { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        VirtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public static void S_ShakeAllVirtualCameras(ShakeSettings settings)
    {
        foreach (var transform in transforms.Where(t => t.GetComponent<VirtualCameraShaker>() != null))
            transform.Shake(settings);
    }

    public void ShakeAllVirtualCameras(ShakeSettings settings)
    {
        VirtualCameraShaker.S_ShakeAllVirtualCameras(settings);
    }
}