using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MuteOutOfCameraBounds : MonoBehaviour
{
    private AudioSource toMute;
    private Transform _transform;

    private RenderCullingBounds bounds;

    private void Awake()
    {
        toMute = GetComponent<AudioSource>();
        bounds = Camera.main.GetComponent<RenderCullingBounds>();
        _transform = transform;
    }

    private void Update()
    {
        if (bounds == null)
            return;
        var rect = bounds.CullingBounds;
        toMute.enabled = rect.Contains(_transform.position);
    }
}
