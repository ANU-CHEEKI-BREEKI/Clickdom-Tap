using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "ZByYSettings")]
public class ZByYSettings : ScriptableObject
{
    [SerializeField] private float scale = 0.01f;
    [SerializeField] private float zOffset = 0f;

    public float Scale => scale;
    public float ZOffset => zOffset;
}
