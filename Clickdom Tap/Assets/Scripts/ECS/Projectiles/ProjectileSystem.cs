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
    public enum GrountType { START_Y, TARGET_Y }

    public float2 startPosition;
    public float2 targetPosition;
    public float targetWidth;

    public GrountType ground;

    public float3 previousProjectilePosition;
    /// <summary>
    /// если true, это значит, что частица остановилась в этом кадре
    /// </summary>
    public bool itStopsRightNow;

    /// <summary>
    /// x >= 0, y => 0.
    /// x - сопротивление горизонтальному движению (сопротивления воздуха).
    /// y - сопротивление вертикальному движению (гравитация).
    /// </summary>
    public float2 accelerationResistance;
    /// <summary>
    /// удалить ProjectileComponentData, когда projectile остановит своё движение
    /// </summary>
    public bool removeEntityWhenProjectileStops;
    /// <summary>
    /// когда projectile остановит своё движение, этот счетчик начнет уменьшаться.
    /// когда lifetimeAfterProjectileStop <= 0 projectile entity будет удален
    /// </summary>
    public float lifetimeAfterProjectileStop;

    public EffectId effectOnParticleStops;
    public EffectId effectOnParticleRemoves;
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
            var moved = MoveProjectile(ref translation, ref projectileData, ref velocity, 1, deltaTime);
            if (!moved)
                DecreaseLifetime(ref projectileData, deltaTime);
        }
    }

    [BurstCompile]
    public struct ProjectileMovingScaledJob : IJobForEachWithEntity<Translation, ProjectileComponentData, VelocityComponentData, Scale>
    {
        public float deltaTime;
        
        public void Execute(Entity entity, int index, ref Translation translation, ref ProjectileComponentData projectileData, ref VelocityComponentData velocity, [ReadOnly] ref Scale scale)
        {
            var moved = MoveProjectile(ref translation, ref projectileData, ref velocity,  scale.Value, deltaTime);
            if (!moved)
                DecreaseLifetime(ref projectileData, deltaTime);
        }
    }

    static bool MoveProjectile(ref Translation translation, ref ProjectileComponentData projectileData, ref VelocityComponentData velocity, float scale, float deltaTime)
    {
        projectileData.itStopsRightNow = false;
        projectileData.previousProjectilePosition = translation.Value;
        var acceleration = projectileData.accelerationResistance * -1;
        var realVelocity = velocity.value * scale;

        //пока летит - двигаем
        if (velocity.value.y != 0 || velocity.value.x != 0)
        {
            //обновляем велосити
            velocity.value.x = ANU.Utils.Physics.GetVelocity(velocity.value.x, deltaTime, acceleration.x);
            velocity.value.y = ANU.Utils.Physics.GetVelocity(velocity.value.y, deltaTime, acceleration.y);

            //подвинем горизонтально
            translation.Value.y += ANU.Utils.Physics.GetDisplacement(realVelocity.y, deltaTime, acceleration.y);

            //если падает вниз и коснулoсь пола, то положим ровно на пол и уменьшим дельта тайм соответствующе пройденому пути
            if (realVelocity.y < 0)//падает вниз
            {
                if (
                    //если пол - это таргет_У, то останавливаем после достижения У
                    (projectileData.ground == ProjectileComponentData.GrountType.TARGET_Y &&
                    translation.Value.y <= projectileData.targetPosition.y) 
                    ||
                    //если пол - это старт_У, то останавливаем на таргет_У, если упало в радиусе targetWidth от таргет_X.
                    (projectileData.ground == ProjectileComponentData.GrountType.START_Y &&
                    translation.Value.y <= projectileData.targetPosition.y &&
                    translation.Value.x >= projectileData.targetPosition.x - projectileData.targetWidth / 2 &&
                    translation.Value.x <= projectileData.targetPosition.x + projectileData.targetWidth / 2) 
                    ||
                    //иначе - останавливаем на старт_У
                    (projectileData.ground == ProjectileComponentData.GrountType.START_Y &&
                    translation.Value.y <= projectileData.startPosition.y)
                )
                {
                    var delta = ANU.Utils.Physics.GetTime(
                        math.abs(projectileData.targetPosition.y - translation.Value.y),
                        math.abs(realVelocity.y),
                        acceleration.y
                    );

                    deltaTime -= delta;
                    velocity.value.y = 0;
                    translation.Value.y = projectileData.targetPosition.y;
                }
            }

            //ну и двигаем вбок
            translation.Value.x += ANU.Utils.Physics.GetDisplacement(realVelocity.x, deltaTime, acceleration.x);

            if (velocity.value.y == 0)
            {
                velocity.value.x = 0;
                projectileData.itStopsRightNow = true;
                return false;
            }

            return true;
        }

        return false;
    }
    
    static void DecreaseLifetime(ref ProjectileComponentData projectileData, float deltaTime)
    {
        if (projectileData.removeEntityWhenProjectileStops)
            projectileData.lifetimeAfterProjectileStop -= deltaTime;
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