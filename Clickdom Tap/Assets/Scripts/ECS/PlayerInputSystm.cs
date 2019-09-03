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


public class PlayerInputSystm : ComponentSystem
{
    protected override void OnUpdate()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var pos = Utils.GetMouseWorldPosition();

            Entities.ForEach<ArcherTagComponentData, Translation, Scale>((ref ArcherTagComponentData tag, ref Translation translation, ref Scale scale) =>
            {
                ProjectileSystem.LaunchProjectile(
                    EntitySpavner.Instance.EntityManager,
                    EntitySpavner.Instance.arrowMeterial, 
                    EntitySpavner.Instance.arrowMesh,
                    translation.Value,
                    new float2(pos.x + translation.Value.x + 7, pos.y + translation.Value.y + 4),
                    20, 
                    scale: 0.3f * scale.Value,
                    rotType: ProjectileRotationComponentData.RotationType.TO_DIRECTION
                );
            });
        }
    }
}