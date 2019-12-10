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

public struct RotationToMoveDirectionTagComponentData : IComponentData { }

public class RotationToMoveDirectionSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(RotationToMoveDirectionTagComponentData))]
    struct TmplateJob : IJobForEach<Translation, Rotation, VelocityComponentData>
    {
        public void Execute(ref Translation translation, ref Rotation rotation, [ReadOnly] ref VelocityComponentData velocity)
        {
            if (velocity.value.x != 0 && velocity.value.y != 0)
                rotation.Value = rotation.Value.XLookTo(velocity.value);
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