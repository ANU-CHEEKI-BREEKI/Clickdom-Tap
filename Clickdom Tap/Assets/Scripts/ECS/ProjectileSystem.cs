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

public struct ProjectileComponentData : IComponentData
{
    public const float g = 9.8f;

    public float2 targetPosition;
    /// <summary>
    /// x, y - всегда будут >= 0. если задать < 0, они будут автоматом приведены в > 0
    /// x - сопротивление горизонтальному движению
    /// y - сопротивление вертикальному движению (гравитация, короче) советую ставить ProjectileComponentData.g
    /// </summary>
    public float2 accelerationResistance;
    public float2 velocity;
}

public struct ProjectileRotationComponentData : IComponentData
{
    public enum RotationType { NO_ROTATION, TO_DIRECTION, CLOCKWISE, COUNTERCLOCKWISE }

    public RotationType rotationType;
    public float rotationDegreeVelocity;
}

public class ProjectileSystem : JobComponentSystem
{
    [BurstCompile]
    [ExcludeComponent(typeof(Scale), typeof(Rotation))]
    public struct ProjectileMovingJob : IJobForEachWithEntity<Translation, ProjectileComponentData>
    {
        public float deltaTime;
       
        public void Execute(Entity entity, int index, ref Translation translation, ref ProjectileComponentData projectileData)
        {
            MoveProjectile(ref translation, ref projectileData, 1, deltaTime);
        }
    }

    [BurstCompile]
    [ExcludeComponent(typeof(Rotation))]
    public struct ProjectileMovingScaledJob : IJobForEachWithEntity<Translation, ProjectileComponentData, Scale>
    {
        public float deltaTime;
        
        public void Execute(Entity entity, int index, ref Translation translation, ref ProjectileComponentData projectileData, [ReadOnly] ref Scale scale)
        {
            MoveProjectile(ref translation, ref projectileData, scale.Value, deltaTime);
        }
    }

    [BurstCompile]
    [ExcludeComponent(typeof(Scale))]
    public struct ProjectileMovingRotatedJob : IJobForEachWithEntity<Translation, ProjectileComponentData, Rotation, ProjectileRotationComponentData>
    {
        public float deltaTime;
       
        public void Execute(Entity entity, int index, ref Translation translation, ref ProjectileComponentData projectileData, ref Rotation rotation, [ReadOnly] ref ProjectileRotationComponentData projectileRotationData)
        {
            var s = MoveProjectile(ref translation, ref projectileData, 1, deltaTime);
            RotateProjectile(s, ref projectileData, ref rotation, ref projectileRotationData, deltaTime);
        }
    }

    [BurstCompile]
    public struct ProjectileMovingSaledRotatedJob : IJobForEachWithEntity<Translation, ProjectileComponentData, Scale, Rotation, ProjectileRotationComponentData>
    {
        public float deltaTime;
       
        public void Execute(Entity entity, int index, ref Translation translation, ref ProjectileComponentData projectileData, [ReadOnly] ref Scale scale, ref Rotation rotation, [ReadOnly] ref ProjectileRotationComponentData projectileRotationData)
        {
            var s = MoveProjectile(ref translation, ref projectileData, scale.Value, deltaTime);
            RotateProjectile(s, ref projectileData, ref rotation, ref projectileRotationData, scale.Value * deltaTime);

            if(s.x == 0 && s.y == 0)
        }
    }

    static float2 MoveProjectile(ref Translation translation, ref ProjectileComponentData projectileData, float scale, float deltaTime)
    {
        float2 displacement = float2.zero;

        if (projectileData.accelerationResistance.x < 0)
            projectileData.accelerationResistance.x *= -1;
        if (projectileData.accelerationResistance.y < 0)
            projectileData.accelerationResistance.y *= -1;

        var acceleration = projectileData.accelerationResistance * -1;

        //пока летит - двигаем
        if (projectileData.velocity.y != 0 || projectileData.velocity.x != 0)
        {
            //подвинем горизонтально
            displacement.y = Utils.Physics.GetDisplacement(projectileData.velocity.y, deltaTime, acceleration.y);
            translation.Value.y += displacement.y;

            //если падает вниз и коснулoсь пола, то положим ровно на пол и уменьшим дельта тайм соответствующе пройденому пути
            if (projectileData.velocity.y < 0 && translation.Value.y < projectileData.targetPosition.y)
            {
                projectileData.targetPosition.y = translation.Value.y;
                var delta = Utils.Physics.GetTime(
                    projectileData.targetPosition.y - translation.Value.y, 
                    projectileData.velocity.y,
                    acceleration.y
                );
                deltaTime -= delta;

                projectileData.velocity.y = 0;
            }

            //ну и двигаем вбок
            displacement.x = Utils.Physics.GetDisplacement(projectileData.velocity.x, deltaTime, acceleration.x);
            translation.Value.x += displacement.x;

            if (projectileData.velocity.y == 0)
            {
                projectileData.velocity.x = 0;
                return displacement;
            }

            //обновляем велосити
            projectileData.velocity.x = Utils.Physics.GetVelocity(projectileData.velocity.x, deltaTime, acceleration.x);
            projectileData.velocity.y = Utils.Physics.GetVelocity(projectileData.velocity.y, deltaTime, acceleration.y);
        }

        return displacement;
    }

    static void RotateProjectile(float2 displacement, ref ProjectileComponentData projectileData, ref Rotation rotation, [ReadOnly] ref ProjectileRotationComponentData projectileRotationData, float scale)
    {
        switch (projectileRotationData.rotationType)
        {
            case ProjectileRotationComponentData.RotationType.NO_ROTATION:
                break;
            case ProjectileRotationComponentData.RotationType.TO_DIRECTION:
                if(displacement.x != 0 && displacement.y != 0)
                    rotation.Value = rotation.Value.XLookTo(displacement);
                break;
            case ProjectileRotationComponentData.RotationType.CLOCKWISE:
                break;
            case ProjectileRotationComponentData.RotationType.COUNTERCLOCKWISE:
                break;
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
        var jh2 = j2.Schedule(this, jh1);
        var j3 = new ProjectileMovingRotatedJob()
        {
            deltaTime = dt
        };
        var jh3 = j3.Schedule(this, jh2);
        var j4 = new ProjectileMovingSaledRotatedJob()
        {
            deltaTime = dt
        };
       return j4.Schedule(this, jh3);
    }

    public static void LaunchProjectile(EntityManager manager, Material mat, Mesh mesh, float3 pos, float2 targetPos, 
        float absoluteVelocity,
        float rotationVelocity = 0, ProjectileRotationComponentData.RotationType rotType = ProjectileRotationComponentData.RotationType.NO_ROTATION, 
        float scale = 1,
        float horisontalAccelerationResistance = 0,
        float verticalAcceleretion = ProjectileComponentData.g)
    {
        if (horisontalAccelerationResistance < 0)
            horisontalAccelerationResistance *= -1;
        if (verticalAcceleretion < 0)
            verticalAcceleretion *= -1;

        var projectileArchetype = manager.CreateArchetype(
            typeof(Translation),
            typeof(ProjectileComponentData),
            typeof(Rotation),
            typeof(ProjectileRotationComponentData),
            typeof(Scale),
            typeof(RenderMesh),
            typeof(LocalToWorld)
        );

        var entity = manager.CreateEntity(projectileArchetype);

        manager.SetComponentData(entity, new Translation()
        {
            Value = pos
        });
        var acceleration = new float2(-horisontalAccelerationResistance, -verticalAcceleretion);
        manager.SetComponentData(entity, new ProjectileComponentData()
        {
            targetPosition = targetPos,
            accelerationResistance = acceleration,
            velocity = Utils.Physics.GetVelocity(pos, targetPos, absoluteVelocity, acceleration)
        });
        manager.SetComponentData(entity, new Rotation()
        {
            Value = quaternion.identity
        });
        manager.SetComponentData(entity, new ProjectileRotationComponentData()
        {
            rotationDegreeVelocity = rotationVelocity,
            rotationType = rotType,
        });
        manager.SetComponentData(entity, new Scale()
        {
            Value = scale
        });
        manager.SetSharedComponentData(entity, new RenderMesh()
        {
            mesh = mesh,
            material = mat
        });
    }

    public static void LaunchProjectile(Entity entity, EntityManager manager, float velocity, float2 target, float rotationVelocity = 0, float scale = 1)
    {
        if(manager.HasComponent<ProjectileComponentData>(entity))
        {

        }else
        {

        }

        if (manager.HasComponent<VelocityAbsoluteComponentData>(entity))
        {

        }
        else
        {

        }
    }

    //public static void LaunchProjectileFromJob(Entity entity, EntityManager manager)
    //{

    //}

}