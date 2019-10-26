using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using Unity.Mathematics;

[CreateAssetMenu(fileName = "ScaleByPositionSettings")]
public class ScaleByPositionSettings : ScriptableObject
{
    [SerializeField] private float minScale = 0.2f;
    [SerializeField] private float maxScale = 1;
    [Space]
    [SerializeField] private float maxY = 12f;
    [SerializeField] private float minY = -12f;

    public float MinScale => minScale;
    public float MaxScale => maxScale;

    public float MaxY => maxY;
    public float MinY => minY;

    public float LerpEvaluete(float3 pos) => Mathf.Lerp(MaxScale, MinScale, (pos.y - MinY) / (MaxY - MinY));

    private void OnValidate()
    {
        if (minY == maxY)
            maxY++;
    }

}
