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

public struct FlibHorisontalByMoveDirTagComponentData : IComponentData
{
}

public class FlipHorisontalByMoveDirectionSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(FlibHorisontalByMoveDirTagComponentData))]
    struct FlipHorByLinearMove : IJobForEach<Translation, RenderScaleComponentdata, LinearMovementComponentData>
    {
        public void Execute([ReadOnly] ref Translation pos, ref RenderScaleComponentdata scale, [ReadOnly]  ref LinearMovementComponentData move)
        {
            if (!move.isMoving)
                return;
            var dir = pos.Value.GetDirectionTo(move.previousPosition);
            scale.value.x = math.abs(scale.value.x) * -math.sign(dir.x);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new FlipHorByLinearMove
        {
        };
        return job.Schedule(this, inputDeps);
    }
}