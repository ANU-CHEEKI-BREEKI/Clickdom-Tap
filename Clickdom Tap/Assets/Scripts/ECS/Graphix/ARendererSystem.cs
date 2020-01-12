using ANU.Utils;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static ANU.Utils.Jobs;

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateAfter(typeof(ARendererCollectorSystem))]
public abstract class ARendererSystem : ComponentSystem
{           
    protected EntityManager manager;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        manager = EntityManager;
    }

    protected int uv_MaterialPropId = Shader.PropertyToID("_MainTex_UV");
    protected int color_MaterialPropId = Shader.PropertyToID("_Color");
    protected int crackDisolve_MaterialPropId = Shader.PropertyToID("_CracksDisolve");
    protected MaterialPropertyBlock mpb = new MaterialPropertyBlock();
}