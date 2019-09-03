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
}

public class ScaleByPositionSystem : JobComponentSystem
{
    [BurstCompile]
    public struct ScaleJob : IJobForEach<Translation, Scale, ScaleByPositionComponentData>
    {
        [ReadOnly] public float minY;
        [ReadOnly] public float maxY;

        public void Execute([ReadOnly] ref Translation translation, [WriteOnly] ref Scale scale, [ReadOnly] ref ScaleByPositionComponentData scaleData)
        {
            if (maxY == minY) return;

            float posY = math.clamp(translation.Value.y, minY, maxY);
            float t = (posY - minY) / (maxY - minY);

            scale.Value = math.lerp(scaleData.maxScale, scaleData.minScale, t);           
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var camera = Camera.main;
        var camPos = camera.transform.position;
        var ymin = camPos.y - camera.orthographicSize * 0.7f;
        var ymax = camPos.y + camera.orthographicSize * 0.7f;

        var job = new ScaleJob()
        {
            maxY = ymax,
            minY = ymin
        };
        return job.Schedule(this, inputDeps);
    }
}
