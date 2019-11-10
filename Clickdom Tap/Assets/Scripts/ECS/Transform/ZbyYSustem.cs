using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct ZbyYComponentData : IComponentData
{
    public float scale;
    public float zOffset;
}

public class ZbyYSystem : JobComponentSystem
{
    [BurstCompile]
    struct TmplateJob : IJobForEach<Translation, ZbyYComponentData>
    {
        public void Execute(ref Translation pos, ref ZbyYComponentData zbyy)
        {
            pos.Value.z = pos.Value.y * zbyy.scale;
            pos.Value.z += zbyy.zOffset;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new TmplateJob
        {

        };
        return job.Schedule(this, inputDeps);
    }
}