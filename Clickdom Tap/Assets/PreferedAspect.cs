using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
[ExecuteInEditMode]
public class PreferedAspect : MonoBehaviour
{
    [Header("width to heihgt")]
    [Space]
    [SerializeField] [Range(1f, 50f)] private float width = 24f;

    private CinemachineVirtualCamera _vortualCamera = null;
    private Camera _camera;

    private void OnEnable()
    {
        SetWidth();
    }

#if UNITY_EDITOR
    private void Update()
    {
        SetWidth();
    }
#endif

    private void SetWidth()
    {
        if (_camera == null)
            _camera = Camera.main;

        if (_vortualCamera == null)
            _vortualCamera = GetComponent<CinemachineVirtualCamera>();
        _vortualCamera.m_Lens.OrthographicSize = width / _camera.aspect;
    }

   

   
}
