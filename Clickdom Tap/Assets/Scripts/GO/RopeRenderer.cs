using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class RopeRenderer : MonoBehaviour
{
    private LineRenderer line;
    [SerializeField] Transform[] points;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (line == null || points == null) return;

        line.positionCount = points.Length;
        for (int i = 0; i < points.Length; i++)
            line.SetPosition(i, points[i].position);

    }
}