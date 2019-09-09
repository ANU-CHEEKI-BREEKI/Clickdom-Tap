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

public struct ProjectileLaunshSetupComponentData : IComponentData
{
    public const float g = 9.8f;

    public ProjectileComponentData.GrountType ground;
    public float targetWidth;

    public float2 targetPosition;
    /// <summary>
    /// x, y - всегда будут >= 0. если задать < 0, они будут автоматом приведены в > 0
    /// x - сопротивление горизонтальному движению
    /// y - сопротивление вертикальному движению (гравитация, короче) советую ставить ProjectileComponentData.g
    /// </summary>
    public float2 accelerationResistance;
    public bool removeEntityWhenProjectileStops;

    /// <summary>
    /// когда projectile остановит своё движение, этот счетчик начнет уменьшаться.
    /// когда lifetimeAfterProjectileStop <= 0 projectile entity будет удален
    /// </summary>
    public float lifetimeAfterProjectileStop;

    public float absoluteVelocity;
}

public class LaunchProjectileSystem : ComponentSystem
{
    EntityManager manager;

    protected override void OnCreate()
    {
        base.OnCreate();
        manager = World.Active.EntityManager;
    }

    protected override void OnUpdate()
    {
        var desc = new EntityQueryDesc()
        {
            All = new ComponentType[] { ComponentType.ReadOnly<ProjectileLaunshSetupComponentData>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Scale>() },
            None = new ComponentType[] { ComponentType.ReadOnly<ProjectileComponentData>(), ComponentType.ReadOnly<VelocityComponentData>() }
        };
        var query = GetEntityQuery(desc);
        var entities = query.ToEntityArray(Allocator.TempJob);
        var translations = query.ToComponentDataArray<Translation>(Allocator.TempJob);
        var scales = query.ToComponentDataArray<Scale>(Allocator.TempJob);
        var setups = query.ToComponentDataArray<ProjectileLaunshSetupComponentData>(Allocator.TempJob);

        var l = entities.Length;
        for (int i = 0; i < l; i++)
        {
            var entity = entities[i];
            var setup = setups[i];
            var translation = translations[i];
            var scale = scales[i].Value;
            if (scale == 0) scale = 0.01f;


            manager.RemoveComponent<ProjectileLaunshSetupComponentData>(entity);
            manager.AddComponent<ProjectileComponentData>(entity);
            manager.SetComponentData(entity, new ProjectileComponentData()
            {
                accelerationResistance = setup.accelerationResistance,
                targetPosition = setup.targetPosition,
                startPosition = new float2(translation.Value.x, translation.Value.y),
                removeEntityWhenProjectileStops = setup.removeEntityWhenProjectileStops,
                lifetimeAfterProjectileStop = setup.lifetimeAfterProjectileStop,
                ground = setup.ground,
                targetWidth = setup.targetWidth
            });
            manager.AddComponent<VelocityComponentData>(entity);
            manager.SetComponentData(entity, new VelocityComponentData()
            {
                value = Utils.Physics.GetVelocity(translation.Value / scale, setup.targetPosition / scale, setup.absoluteVelocity, setup.accelerationResistance * -1)
            });
        }

        entities.Dispose();
        translations.Dispose();
        setups.Dispose();
        scales.Dispose();
    }
}