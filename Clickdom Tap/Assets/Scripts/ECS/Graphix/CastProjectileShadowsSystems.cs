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

[Serializable]
public struct CastProjectileShadowsTagComponentData : IComponentData
{
    public float2 startPositionOffset;
    public float2 endPositionOffset;

    public float defaultAlpha;
    public float alphaMultiplier;

    public float2 defaultScale;
    public float scaleMultiplier;

    public float maxYOffsetForLerpScaleAndAlpha;
}

public class CastProjectileShadowsSystems : JobComponentSystem
{
    [BurstCompile]
    struct OffsetJob : IJobForEach<Translation, ProjectileComponentData, CastSpritesShadowComponentData, CastProjectileShadowsTagComponentData>
    {
        public void Execute([ReadOnly] ref Translation translation, [ReadOnly] ref ProjectileComponentData projectile, ref CastSpritesShadowComponentData shadow, [ReadOnly] ref CastProjectileShadowsTagComponentData tag)
        {
            var startPos = projectile.startPosition;
            var endPos = projectile.targetPosition;

            var lerpAmount = endPos - startPos;
            var localPos = translation.Value.ToF2() - startPos;

            var t = 0f;
            if(lerpAmount.x != 0)
                t = localPos.x / lerpAmount.x;
            t = math.clamp(math.abs(t), 0, 1);

            //позиция относительно которой надо считать сдвиг
            //и лежит она на прямой (startPos, endPos)
            var lerpedPos = math.lerp(startPos, endPos, t);

            //теперь надо посчитать сaм сдвиг
            var offset = new float2(0,  -math.abs(translation.Value.y - lerpedPos.y) * 2);
            var startOffset = math.lerp(tag.startPositionOffset, 0, t);
            var endOffset = math.lerp(tag.endPositionOffset, 0, t);
            offset += startOffset + endOffset;

            var olphaLerpAmount = 1f;
            if (tag.maxYOffsetForLerpScaleAndAlpha != 0)
                olphaLerpAmount = math.clamp(math.abs(offset.y / tag.maxYOffsetForLerpScaleAndAlpha), 0, 1);
            var alpha = math.lerp(tag.defaultAlpha, tag.defaultAlpha * tag.alphaMultiplier, olphaLerpAmount);
            var scale = math.lerp(tag.defaultScale, tag.defaultScale * tag.scaleMultiplier, olphaLerpAmount);

            shadow.positionUnitsOffset = offset.ToF3(0);
            shadow.color.a = alpha;
            shadow.scale = scale;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new OffsetJob
        {

        };
        return job.Schedule(this, inputDeps);
    }
}