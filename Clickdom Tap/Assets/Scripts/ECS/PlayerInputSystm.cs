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


public class PlayerInputSystm : ComponentSystem
{
    protected override void OnUpdate()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var pos = Utils.GetMouseWorldPosition();

            var prijectileArchetype = EntityManager.CreateArchetype(
                typeof(ProjectileLaunshSetupComponentData),
                typeof(RotationToMoveDirectionComponentData),
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                typeof(RenderMesh),
                typeof(LocalToWorld)
            );

            var mesh = EntitySpavner.Instance.arrowMesh;
            var mat = EntitySpavner.Instance.arrowMeterial;

            Entities.ForEach<ArcherTagComponentData, Translation, Scale>((ref ArcherTagComponentData tag, ref Translation translation, ref Scale scale) =>
            {
                var entity = PostUpdateCommands.CreateEntity(prijectileArchetype);
                PostUpdateCommands.SetComponent(entity, new Translation()
                {
                    Value = translation.Value
                });
                PostUpdateCommands.SetComponent(entity, new ProjectileLaunshSetupComponentData()
                {
                    targetPosition = pos,
                    accelerationResistance = new float2(0, ProjectileLaunshSetupComponentData.g),
                    removeComponentWhenProjectileStops = true,
                    absoluteVelocity = 20
                });
                PostUpdateCommands.SetComponent(entity, new Scale()
                {
                    Value = 0.3f * scale.Value
                });
                PostUpdateCommands.SetSharedComponent(entity, new RenderMesh()
                {
                    mesh = mesh,
                    material = mat
                });
            });
        }
    }
}