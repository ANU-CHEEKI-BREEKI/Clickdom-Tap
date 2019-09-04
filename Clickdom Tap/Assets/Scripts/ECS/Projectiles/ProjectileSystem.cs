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

/// <summary>
/// Чтобы запустить projectile используйте ProjectileLaunshSetupComponentData.
/// </summary>
public struct ProjectileComponentData : IComponentData
{
    public float2 targetPosition;
    /// <summary>
    /// x >= 0, y => 0.
    /// x - сопротивление горизонтальному движению (сопротивления воздуха).
    /// y - сопротивление вертикальному движению (гравитация).
    /// </summary>
    public float2 accelerationResistance;
    /// <summary>
    /// удалить ProjectileComponentData, когда projectile остановит своё движение
    /// </summary>
    public bool removeComponentWhenProjectileStops;
}

public class ProjectileSystem : JobComponentSystem
{
    [BurstCompile]
    [ExcludeComponent(typeof(Scale))]
    public struct ProjectileMovingJob : IJobForEachWithEntity<Translation, ProjectileComponentData, VelocityComponentData>
    {
        public float deltaTime;
       
        public void Execute(Entity entity, int index, ref Translation translation, ref ProjectileComponentData projectileData, ref VelocityComponentData velocity)
        {
            MoveProjectile(ref translation, ref projectileData, ref velocity, 1, deltaTime);
        }
    }

    [BurstCompile]
    public struct ProjectileMovingScaledJob : IJobForEachWithEntity<Translation, ProjectileComponentData, VelocityComponentData, Scale>
    {
        public float deltaTime;
        
        public void Execute(Entity entity, int index, ref Translation translation, ref ProjectileComponentData projectileData, ref VelocityComponentData velocity, [ReadOnly] ref Scale scale)
        {
            MoveProjectile(ref translation, ref projectileData, ref velocity,  scale.Value, deltaTime);
        }
    }

    static void MoveProjectile(ref Translation translation, ref ProjectileComponentData projectileData, ref VelocityComponentData velocity, float scale, float deltaTime)
    {
        var acceleration = projectileData.accelerationResistance * -1;

        //пока летит - двигаем
        if (velocity.value.y != 0 || velocity.value.x != 0)
        {
            //подвинем горизонтально
            translation.Value.y += Utils.Physics.GetDisplacement(velocity.value.y, deltaTime, acceleration.y);

            //если падает вниз и коснулoсь пола, то положим ровно на пол и уменьшим дельта тайм соответствующе пройденому пути
            if (velocity.value.y < 0 && translation.Value.y < projectileData.targetPosition.y)
            {
                projectileData.targetPosition.y = translation.Value.y;
                var delta = Utils.Physics.GetTime(
                    projectileData.targetPosition.y - translation.Value.y,
                    velocity.value.y,
                    acceleration.y
                );
                deltaTime -= delta;

                velocity.value.y = 0;
            }

            //ну и двигаем вбок
            translation.Value.x += Utils.Physics.GetDisplacement(velocity.value.x, deltaTime, acceleration.x);

            if (velocity.value.y == 0)
            {
                velocity.value.x = 0;
                return;
            }

            //обновляем велосити
            velocity.value.x = Utils.Physics.GetVelocity(velocity.value.x, deltaTime, acceleration.x);
            velocity.value.y = Utils.Physics.GetVelocity(velocity.value.y, deltaTime, acceleration.y);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {    

        var dt = Time.deltaTime;
        var j1 = new ProjectileMovingJob()
        {
            deltaTime = dt
        };
        var jh1 = j1.Schedule(this, inputDeps);
        var j2 = new ProjectileMovingScaledJob()
        {
            deltaTime = dt
        };
        return j2.Schedule(this, jh1);        
    }
}