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

    public float2 targetPosition;
    /// <summary>
    /// x, y - всегда будут >= 0. если задать < 0, они будут автоматом приведены в > 0
    /// x - сопротивление горизонтальному движению
    /// y - сопротивление вертикальному движению (гравитация, короче) советую ставить ProjectileComponentData.g
    /// </summary>
    public float2 accelerationResistance;
    public bool removeComponentWhenProjectileStops;

    public float absoluteVelocity;
}

public class LaunchProjectileSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var desc = new EntityQueryDesc()
        {
            All = new ComponentType[] { new ComponentType(typeof(ProjectileLaunshSetupComponentData)), new ComponentType(typeof(Translation)) },
            None = new ComponentType[] { new ComponentType(typeof(ProjectileComponentData)), new ComponentType(typeof(VelocityComponentData)) }
        };
        var query = GetEntityQuery(desc);
        var entities = query.ToEntityArray(Allocator.TempJob);
        var translations = query.ToComponentDataArray<Translation>(Allocator.TempJob);
        var setups = query.ToComponentDataArray<ProjectileLaunshSetupComponentData>(Allocator.TempJob);

        var manager = World.Active.EntityManager;

        var l = entities.Length;
        for (int i = 0; i < l; i++)
        {
            var entity = entities[i];
            var setup = setups[i];
            var translation = translations[i];
            
            manager.RemoveComponent<ProjectileLaunshSetupComponentData>(entity);
            manager.AddComponent<ProjectileComponentData>(entity);
            manager.SetComponentData(entity, new ProjectileComponentData()
            {
                accelerationResistance = setup.accelerationResistance,
                targetPosition = setup.targetPosition,
                removeComponentWhenProjectileStops = setup.removeComponentWhenProjectileStops
            });
            manager.AddComponent<VelocityComponentData>(entity);
            manager.SetComponentData(entity, new VelocityComponentData()
            {
                value = Utils.Physics.GetVelocity(translation.Value, setup.targetPosition, setup.absoluteVelocity, setup.accelerationResistance * -1)
            });
        }

        entities.Dispose();
        translations.Dispose();
        setups.Dispose();
    }
}