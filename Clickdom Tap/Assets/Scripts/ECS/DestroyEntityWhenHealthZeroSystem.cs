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

public struct DestroyWithHealthComponentData : IComponentData
{
    public float delay;
}

public class DestroyEntityWhenHealthZeroSystem : ComponentSystem
{
    public struct DestroyData
    {
        public Entity entity;
        public DestroyWithHealthComponentData data;
    }

    [BurstCompile]
    public struct DetectLowHealthJob : IJobForEachWithEntity<HealthComponentData, DestroyWithHealthComponentData>
    {
        public NativeQueue<DestroyData>.ParallelWriter toDestroy;

        public void Execute(Entity entity, int index, ref HealthComponentData health, ref DestroyWithHealthComponentData destroyData)
        {
            if (health.value <= 0)
                toDestroy.Enqueue(new DestroyData()
                {
                    entity = entity,
                    data = destroyData
                });
        }
    }

    protected override void OnUpdate()
    {
        var toDestroy = new NativeQueue<DestroyData>(Allocator.TempJob);

        new DetectLowHealthJob()
        {
            toDestroy = toDestroy.AsParallelWriter()
        }.Schedule(this).Complete();

        DestroyData data;
        while (toDestroy.TryDequeue(out data))
        {
            if (data.data.delay > 0)
            {
                //mark to destroy with delay
                EntityManager.RemoveComponent<HealthComponentData>(data.entity);

                var delayData = new DestroyEntityWithDelayComponentData()
                {
                    delay = data.data.delay
                };

                if (EntityManager.HasComponent<DestroyEntityWithDelayComponentData>(data.entity))
                    EntityManager.SetComponentData(data.entity, delayData);
                else
                    EntityManager.AddComponentData(data.entity, delayData);
            }
            else
            {
                //destroy now
                EntityManager.DestroyEntity(data.entity);
            }
        }

        toDestroy.Dispose();
    }
}