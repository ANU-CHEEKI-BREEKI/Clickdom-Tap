using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(WholeProgress))]
public class WholeProgressHandle : MonoBehaviour
{
    [SerializeField] Rect triggerZone;

    private WholeProgress progress;

    private void Start()
    {
        progress = GetComponent<WholeProgress>();
        var system = World.Active.GetOrCreateSystem<WholeProgressIncreacesByParticlesSystem>();
        system.Init(triggerZone, this);
    }

    public void IncreaceProgressInPlace(float damage, Vector3 position)
    {
        PopUpNumbers.Instance.WriteLine((int)damage, position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(triggerZone.center, triggerZone.size);
    }
}
