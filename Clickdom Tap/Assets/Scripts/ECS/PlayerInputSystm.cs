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
            //typeof(SpriteSheetAnimationComponentData),
            typeof(LocalToWorld),
            typeof(RenderMesh)
        );
    }

    protected override void OnUpdate()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var pos = Utils.GetMouseWorldPosition();           

            var mesh = EntitySpavner.Instance.arrowMesh;
            var mat = EntitySpavner.Instance.arrowMeterial;

            var query = GetEntityQuery(ComponentType.ReadOnly<ArcherTagComponentData>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Scale>());
            var translations = query.ToComponentDataArray<Translation>(Allocator.TempJob);
            var scales = query.ToComponentDataArray<Scale>(Allocator.TempJob);

            var entities = new NativeArray<Entity>(translations.Length, Allocator.TempJob);
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
                    accelerationResistance = new float2(0, ProjectileLaunshSetupComponentData.g),
                    removeEntityWhenProjectileStops = true,
                    absoluteVelocity = 25,
                    lifetimeAfterProjectileStop = 5f,
                    ground = ProjectileComponentData.GrountType.START_Y,
                    targetWidth = 2 * scale
                });
                manager.SetComponentData(entity, new Scale()
                {
                    Value = 0.5f * scale
                });
                var data = EntitySpavner.Instance.arrow;
                data.InitRandomSprite();
                //manager.SetComponentData(entity, new SpriteSheetAnimationComponentData()
                //{
                //    currentFrame = data.RamdomInitFrame,
                //    frameCount = data.FramesCount,
                //    frameDuration = data.FrameDuration,
                //    frameHeight = data.FrameHeigth,
                //    frameWidth = data.FrameWidth,
                //    horisontalOffset = data.HorisontalOffset,
                //    verticalOffset = data.VerticalOffset
                //});
                manager.SetSharedComponentData(entity, new RenderMesh()
                {
                    mesh = data.Mesh,
                    material = data.NewMaterial
                });
            }

            translations.Dispose();
            scales.Dispose();
            entities.Dispose();
        }
    }
}