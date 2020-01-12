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

public class CracksAmountUpdateSystem : JobComponentSystem
{
    [BurstCompile]
    struct CracksAmountUpdaterJob : IJobForEach<SpriteCracksComponentData>
    {
        [ReadOnly] public float cracksAmount;

        public void Execute(ref SpriteCracksComponentData cracks)
        {
            cracks.cracksAmount = cracksAmount;
        }
    }

    public float CracksAmount { get; set; } = 0;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new CracksAmountUpdaterJob
        {
            cracksAmount = CracksAmount
        };
        return job.Schedule(this, inputDeps);
    }

    
}