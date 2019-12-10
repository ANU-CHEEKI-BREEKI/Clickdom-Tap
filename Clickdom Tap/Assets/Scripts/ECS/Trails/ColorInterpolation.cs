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

public struct ColorInterpolationComponentData : IComponentData
{
    public enum Interpolationtype { LINEAR, QUADRATIC }

    public Color startColor;
    public Color endColor;

    public float duration;
    public float timer;

    public Interpolationtype type;

    public ANU.Utils.Math.QuadraticEquation quadraticQeuation;
}

public class ColorInterpolation : JobComponentSystem
{
    [BurstCompile]
    struct TmplateJob : IJobForEach<SpriteTintComponentData, ColorInterpolationComponentData>
    {
        [ReadOnly] public float deltaTime;

        public void Execute(ref SpriteTintComponentData tint, ref ColorInterpolationComponentData interp)
        {
            var s = 1f;
            if (interp.duration != 0)
                s = interp.timer / interp.duration;

            if (interp.type == ColorInterpolationComponentData.Interpolationtype.LINEAR)
                tint.color = Color.Lerp(interp.startColor, interp.endColor, s);
            else
            {
                var res = interp.quadraticQeuation.Solve(s);
                tint.color = Color.Lerp(interp.startColor, interp.endColor, res);
            }

            if (interp.timer <= interp.duration)
                interp.timer += deltaTime;
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