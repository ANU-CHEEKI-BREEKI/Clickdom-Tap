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

public struct AlphaAnimationComponentData : IComponentData
{
    public float startAlpha;
    public float endAlpha;
    public float duration;
    public float elapcedTime;
}

public class AlphaAnimation : JobComponentSystem
{
    [BurstCompile]
    struct TmplateJob : IJobForEach<AlphaAnimationComponentData, SpriteTintComponentData>
    {
        [ReadOnly] public float deltaTime;

        public void Execute(ref AlphaAnimationComponentData alpha, ref SpriteTintComponentData tint)
        {
            if (alpha.elapcedTime > alpha.duration)
                return;

            if (alpha.elapcedTime == alpha.duration || alpha.duration == 0)
            {
                tint.color.w = alpha.endAlpha;
            }
            else
            {
                alpha.elapcedTime += deltaTime;
                tint.color.w = math.lerp(alpha.startAlpha, alpha.endAlpha, alpha.elapcedTime / alpha.duration);
            }
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