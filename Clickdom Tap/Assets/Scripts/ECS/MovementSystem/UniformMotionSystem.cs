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

public struct UniformMotiontagComponentData : IComponentData { }

public class UniformMotionSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(UniformMotiontagComponentData))]
    struct TmplateJob : IJobForEach<Translation, VelocityComponentData>
    {
        [ReadOnly] public float deltaTime;

        public void Execute(ref Translation translation, ref VelocityComponentData velocity)
        {
            translation.Value += velocity.value.ToF3() * deltaTime;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new TmplateJob
        {
            deltaTime = Time.deltaTime
        };
        return job.Schedule(this, inputDeps);
    }
}