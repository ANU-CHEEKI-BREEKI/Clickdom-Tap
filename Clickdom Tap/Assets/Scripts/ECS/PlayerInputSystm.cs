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
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[DisableAutoCreation]
public class PlayerInputSystm : ComponentSystem
{
    EntityManager manager;
    EntityArchetype prijectileArchetype;

    protected override void OnCreate()
    {
        base.OnCreate();
        manager = World.Active.EntityManager;

        prijectileArchetype = EntityManager.CreateArchetype(
            typeof(ProjectileLaunshSetupComponentData),
            typeof(RotationToMoveDirectionComponentData),
            typeof(Translation),
            typeof(Rotation),
            typeof(Scale),
            typeof(SpriteSheetAnimationComponentData),
            typeof(RenderScaleComponentdata),
            typeof(RenderSharedComponentData),
            typeof(ProjectileCollisionComponentData)
            //typeof(LocalToWorld),
            //typeof(RenderMesh)
        );
    }

    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKey(KeyCode.Space))
        {
            var pos = Utils.GetMouseWorldPosition();           

            var mesh = EntitySpavner.Instance.arrowMesh;
            var mat = EntitySpavner.Instance.arrowMeterial;

            var query = GetEntityQuery(ComponentType.ReadOnly<ArcherTagComponentData>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Scale>());
            var translations = query.ToComponentDataArray<Translation>(Allocator.TempJob);
            var scales = query.ToComponentDataArray<Scale>(Allocator.TempJob);

            var cnt = translations.Length;// > 0 ? 1 : 0;
            var entities = new NativeArray<Entity>(cnt /*translations.Length*/, Allocator.TempJob);
            manager.CreateEntity(this.prijectileArchetype, entities);
            
            for (int i = 0; i < entities.Length; i++)
            {
                var trans = translations[i].Value;
                var scale = scales[i].Value;
                var entity = entities[i];
                manager.SetComponentData(entity, new Translation()
                {
                    Value = trans
                });
                manager.SetComponentData(entity, new ProjectileLaunshSetupComponentData()
                {
                    targetPosition = pos,
                    accelerationResistance = new float2(0, ProjectileLaunshSetupComponentData.g - 5),
                    removeEntityWhenProjectileStops = true,
                    absoluteVelocity = 17 - 5,
                    lifetimeAfterProjectileStop = 5f,
                    ground = ProjectileComponentData.GrountType.START_Y,
                    targetWidth = 2 * scale
                });
                manager.SetComponentData(entity, new Scale()
                {
                    Value = scale
                });
                manager.SetComponentData(entity, new Rotation()
                {
                    Value = quaternion.identity
                });
                var data = EntitySpavner.Instance.arrow;
                data.InitRandomSprite();
                manager.SetComponentData(entity, new SpriteSheetAnimationComponentData()
                {
                    currentFrame = data.RamdomInitFrame,
                    frameCount = data.FramesCount,
                    frameDuration = data.FrameDuration,
                    frameHeight = data.FrameHeigth,
                    frameWidth = data.FrameWidth,
                    horisontalOffset = data.HorisontalOffset,
                    verticalOffset = data.VerticalOffset
                });
                manager.SetComponentData(entity, new RenderScaleComponentdata()
                {
                    value = Vector2.one * 0.35f
                });
                manager.SetComponentData(entity, new ProjectileCollisionComponentData()
                {
                    processData = new ProcessCollisionData()
                    {
                        type = HitProcessingType.LAUNCH_AS_PROJECTILE,
                        destroyDelay = 5,
                        absoluteProjectileVelocity = UnityEngine.Random.Range(6, 10)                        
                    },
                    maxHitCount = 1, 
                    colisionTimeOut = 1f,
                    detectTime = ProjectileCollisionComponentData.DetectCillisionTime.WHEN_STOPS,
                    ownerFaction = FactionComponentData.Faction.NEUTRAL
                });
                manager.SetSharedComponentData(entity, new RenderSharedComponentData()
                {
                    mesh = data.Mesh,
                    material = data.Material
                });

            }

            translations.Dispose();
            scales.Dispose();
            entities.Dispose();
        }
    }
}