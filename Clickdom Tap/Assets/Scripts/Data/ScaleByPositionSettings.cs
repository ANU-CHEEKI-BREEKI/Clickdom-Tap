using UnityEngine;
using System.Collections;
using System;
using System.Linq;

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

}
