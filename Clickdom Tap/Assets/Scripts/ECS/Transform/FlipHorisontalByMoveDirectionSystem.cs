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
    public bool defaultFlipped;
}

public class FlipHorisontalByMoveDirectionSystem : JobComponentSystem
{
    [BurstCompile]
    struct FlipHorByLinearMove : IJobForEach<Translation, RenderScaleComponentdata, LinearMovementComponentData, FlibHorisontalByMoveDirTagComponentData>
    {
        public void Execute([ReadOnly] ref Translation pos, ref RenderScaleComponentdata scale, [ReadOnly] ref LinearMovementComponentData move, [ReadOnly] ref FlibHorisontalByMoveDirTagComponentData tag)
        {
            if (!move.isMoving)
                return;
            var dir = pos.Value.GetDirectionTo(move.previousPosition);
            scale.value.x = math.abs(scale.value.x);
            if (dir.x != 0)
                scale.value.x *= -math.sign(dir.x);
            else if (tag.defaultFlipped)
                scale.value.x *= -1;
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