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

[UpdateAfter(typeof(RegisterProjectileHitSystem))]
public class ProcessProjectileHitSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var manager = EntityManager;
        var query = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<ProcessProjectileCollisionTag>()
        );
        var entities = query.ToEntityArray(Allocator.TempJob);
        var processData = query.ToComponentDataArray<ProcessProjectileCollisionTag>(Allocator.TempJob);
        var translation = query.ToComponentDataArray<Translation>(Allocator.TempJob);

        for (int i = 0; i < entities.Length; i++)
        {
            if (processData[i].hittedByProjectile && !processData[i].olreadyProceeded)
            {
                var temp = processData[i];
                temp.olreadyProceeded = true;
                temp.hittedByProjectile = false;
                processData[i] = temp;
                manager.SetComponentData(entities[i], processData[i]);

                if (manager.HasComponent<HealthComponentData>(entities[i]))
                {
                    var health = manager.GetComponentData<HealthComponentData>(entities[i]);
                    health.value -= processData[i].processData.damage;
                    manager.SetComponentData(entities[i], health);
                }

                switch (processData[i].processData.type)
                {
                    case HitProcessingType.REMOVE:
                        manager.DestroyEntity(entities[i]);
                        break;
                    case HitProcessingType.SET_ANIMATION:
                        AnimationSetterUtil.SetAnimation(manager, entities[i], processData[i].processData.animation);
                        break;
                    case HitProcessingType.REMOVE_WITH_DELAY:
                        DestroyEntityWithDelaySystem.MarkToDestroy(manager, entities[i], processData[i].processData.destroyDelay);
                        break;
                    case HitProcessingType.SET_ANIMATION_AND_REMOVE_WITH_DELAY:
                        AnimationSetterUtil.SetAnimation(manager, entities[i], processData[i].processData.animation);
                        DestroyEntityWithDelaySystem.MarkToDestroy(manager, entities[i], processData[i].processData.destroyDelay);
                        break;
                    case HitProcessingType.LAUNCH_AS_PROJECTILE:
                        LaunchProjectileSystem.Launch(
                            manager, 
                            entities[i], 
                            translation[i].Value.ToF2() + processData[i].processData.direction + UnityEngine.Random.Range(-1, 1), 
                            processData[i].processData.absoluteProjectileVelocity, 
                            processData[i].processData.direction, 
                            processData[i].processData.destroyDelay
                        );
                        break;
                    case HitProcessingType.SET_ANIMATION_AND_LAUNCH_AS_PROJECTILE:
                        AnimationSetterUtil.SetAnimation(manager, entities[i], processData[i].processData.animation);
                        LaunchProjectileSystem.Launch(
                            manager, 
                            entities[i], 
                            translation[i].Value.ToF2() + processData[i].processData.direction + UnityEngine.Random.Range(-1, 1),
                            processData[i].processData.absoluteProjectileVelocity, 
                            processData[i].processData.direction, 
                            processData[i].processData.destroyDelay
                        );
                        break;
                }
            }
        }

        entities.Dispose();
        processData.Dispose();
        translation.Dispose();
    }

    
}