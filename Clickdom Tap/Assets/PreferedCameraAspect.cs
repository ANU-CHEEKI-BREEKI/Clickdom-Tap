using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class PreferedCameraAspect : MonoBehaviour
{
    [SerializeField] private float preferedAspect = 2;

    private Camera _camera;
    private Rect _defaultRect = new Rect(0, 0, 1, 1);

    private void Init()
    {
        if (_camera == null)
            _camera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        Init();
        SetViewportRect();
    }

    private void Update()
    {
        Init();
        SetViewportRect();
    }

    private void SetViewportRect()
    {
        _camera.rect = _defaultRect;

        var actualAspect = _camera.aspect;
        var difference = actualAspect / preferedAspect;

        var camRect = _camera.rect;
        if (difference < 1)
        {
            camRect.height = difference;
            camRect.y = (1 - difference) / 2;
        }
        else if (difference > 1)
        {
            var df = 1 / difference;
            camRect.width = df;
            camRect.x = (1 - df) / 2;
        }

        _camera.rect = camRect;
    }
}
