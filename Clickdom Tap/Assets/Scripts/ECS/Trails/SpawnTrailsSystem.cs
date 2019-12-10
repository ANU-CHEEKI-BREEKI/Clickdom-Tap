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
using static TrailsDetectSpawnEvent;

[UpdateAfter(typeof(TrailsDetectSpawnEvent))]
public class SpawnTrailsSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        TrailsDetectSpawnEvent.handle.Complete();
        var spawnData = TrailsDetectSpawnEvent.spawnData;
        var manager = EntityManager;

        InternalSpawnTrailData data;
        while (spawnData.TryDequeue(out data))
        {
            var entity = manager.CreateEntity();

            manager.AddComponentData(entity, new Translation() { Value = data.position });
            manager.AddComponentData(entity, new Scale() { Value = data.scale });
            manager.AddComponentData(entity, new Rotation() { Value = data.rotation });
            manager.AddComponentData(entity, new RenderScaleComponentdata() { value = data.spawnData.renderScale });
            manager.AddComponentData(entity, new DestroyEntityWithDelayComponentData() { delay = data.spawnData.trailLifetime });
            manager.AddComponentData(entity, new SpriteTintComponentData() { color = data.spawnData.startColor });
            manager.AddComponentData(entity, new ColorInterpolationComponentData()
            {
                type = ColorInterpolationComponentData.Interpolationtype.QUADRATIC,
                duration = data.spawnData.trailLifetime,
                quadraticQeuation = data.spawnData.interpolationEquation,
                startColor = data.spawnData.startColor,
                endColor = data.spawnData.endColor,
            });

            var spriteSetupData = TrailsRenderSettingsHolder.Instance.Settings.GetSetupData(data.spriteRenderDataId);
            manager.AddComponentData(entity, new SpriteRendererComponentData()
            {
                uv = spriteSetupData.UV,
                pivot = spriteSetupData.Pivot,
                usePivot = true
            });

            var animationData = TrailsRenderSettingsHolder.Instance.Settings.GetAnimationData(data.spriteRenderDataId);
            if (animationData.frameCount > 1)
            {
                if (data.spawnData.animationDurationByLifetime)
                    animationData.frameDuration = data.spawnData.trailLifetime / animationData.frameCount;
                manager.AddComponentData(entity, animationData);
            }
            manager.AddSharedComponentData(entity, TrailsRenderSettingsHolder.Instance.Settings.GetRenderData(data.spriteRenderDataId));
        }
    }
}