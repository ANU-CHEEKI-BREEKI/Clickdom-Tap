using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParticleExternalForcesMultiplierByScale : MonoBehaviour
{
    [SerializeField] private Transform scaleTransform;
    [Space]
    [SerializeField] private ParticleSystem[] systemsToUpdate;

    private void Reset()
    {
        scaleTransform = transform;
    }

    private void Update()
    {
        var scale = scaleTransform.localScale.Average2D();
        if (scale == 0)
            scale = 0.1f;

        foreach (var psys in systemsToUpdate)
                if(psys != null)
            UpdateSystem(psys, scale);
    }

    private void UpdateSystem(ParticleSystem psys, float scale)
    {
        var extForce = psys.externalForces;
        extForce.multiplier = 1f / scale;
    }
}
