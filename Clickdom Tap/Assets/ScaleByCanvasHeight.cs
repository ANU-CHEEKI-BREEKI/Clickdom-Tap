using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScaleByCanvasHeight : MonoBehaviour
{
    [SerializeField] [Min(1)] private float scaler = 720;

    private Transform _transform;
    private Camera _camera;

    private void Init()
    {
        if (_transform == null)
            _transform = transform;

        if (_camera == null)
            _camera = Camera.main;
    }

    private void Update()
    {
        Init();
        transform.localScale = Vector3.one * (_camera.orthographicSize / scaler);
    }
}
