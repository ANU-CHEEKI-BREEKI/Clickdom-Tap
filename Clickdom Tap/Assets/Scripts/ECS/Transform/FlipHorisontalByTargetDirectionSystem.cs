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

public struct FlibHorisontalByTargetTagComponentData : IComponentData
{
}

public class FlipHorisontalByTargetDirectionSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(FlibHorisontalByTargetTagComponentData))]
    struct FlipHorShootTargetMove : IJobForEach<Translation, RenderScaleComponentdata, ArcherTargetPositionComponentData, LinearMovementComponentData>
    {
        public void Execute([ReadOnly] ref Translation pos, ref RenderScaleComponentdata scale, [ReadOnly] ref ArcherTargetPositionComponentData target, [ReadOnly] ref LinearMovementComponentData move)
        {
            if (move.isMoving)
                return;
            var dir = pos.Value.GetDirectionTo(target.value);
            scale.value.x = math.abs(scale.value.x) * math.sign(dir.x);
        }
    }

    [BurstCompile]
    [RequireComponentTag(typeof(FlibHorisontalByTargetTagComponentData))]
    struct FlipHorMeleeTargetMove : IJobForEach<Translation, RenderScaleComponentdata, MeleeTargetComponentData, LinearMovementComponentData>
    {
        public void Execute([ReadOnly] ref Translation pos, ref RenderScaleComponentdata scale, [ReadOnly] ref MeleeTargetComponentData target, [ReadOnly] ref LinearMovementComponentData move)
        {
            if (move.isMoving)
                return;
            var dir = pos.Value.GetDirectionTo(target.targetPosition.ToF2());
            scale.value.x = math.abs(scale.value.x) * math.sign(dir.x);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var jh1 = new FlipHorShootTargetMove
        {
        }.Schedule(this, inputDeps);
        var jh2 = new FlipHorMeleeTargetMove
        {
        }.Schedule(this, jh1);
        return jh2;
    }
}