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

public class DeleteProjectileSystem : ComponentSystem
{
    EntityManager manager;

    protected override void OnCreate()
    {
        base.OnCreate();
        manager = World.Active.EntityManager;
    }

    protected override void OnUpdate()
    {
        var query = GetEntityQuery(ComponentType.ReadOnly<ProjectileComponentData>(), ComponentType.ReadOnly<VelocityComponentData>() );
        var entities = query.ToEntityArray(Allocator.TempJob);
        var velocities = query.ToComponentDataArray<VelocityComponentData>(Allocator.TempJob);
        var datas = query.ToComponentDataArray<ProjectileComponentData>(Allocator.TempJob);

        var l = entities.Length;
        for (int i = 0; i < l; i++)
        {
            var v = velocities[i].value;
            if (v.x == 0 && v.y == 0)
            {
                if (datas[i].removeEntityWhenProjectileStops)
                {
                    if (datas[i].lifetimeAfterProjectileStop <= 0)
                        manager.DestroyEntity(entities[i]); 
                }
                else
                {
                    manager.RemoveComponent<ProjectileComponentData>(entities[i]);
                    manager.RemoveComponent<VelocityComponentData>(entities[i]);
                }
            }
        }

        datas.Dispose();
        velocities.Dispose();
        entities.Dispose();
    }
}