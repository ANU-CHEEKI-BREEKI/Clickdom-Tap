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

public struct ScaleByPositionComponentData :IComponentData
{
    public float maxScale;
    public float minScale;

    public float maxY;
    public float minY;
}

public class ScaleByPositionSystem : JobComponentSystem
{
    [BurstCompile]
    public struct ScaleJob : IJobForEach<Translation, Scale, ScaleByPositionComponentData>
    {
        public void Execute([ReadOnly] ref Translation translation, [WriteOnly] ref Scale scale, [ReadOnly] ref ScaleByPositionComponentData scaleData)
        {
            if (scaleData.maxY == scaleData.minY) return;

            float posY = math.clamp(translation.Value.y, scaleData.minY, scaleData.maxY);
            float t = (posY - scaleData.minY) / (scaleData.maxY - scaleData.minY);

            scale.Value = math.lerp(scaleData.maxScale, scaleData.minScale, t);           
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new ScaleJob()
        {
        };
        return job.Schedule(this, inputDeps);
    }
}
