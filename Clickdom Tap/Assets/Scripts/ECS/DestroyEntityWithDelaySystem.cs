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

public struct DestroyEntityWithDelayComponentData : IComponentData
{
    public float delay;
}

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public class DestroyEntityWithDelaySystem : ComponentSystem
{
    [BurstCompile]
    public struct DecreaseTimerJob : IJobForEachWithEntity<DestroyEntityWithDelayComponentData>
    {
        [ReadOnly] public float deltaTime;
        public NativeQueue<Entity>.ParallelWriter toDestroy;

        public void Execute(Entity entity, int index, ref DestroyEntityWithDelayComponentData data)
        {
            data.delay -= deltaTime;
            if (data.delay <= 0)
                toDestroy.Enqueue(entity);
        }
    }

    protected override void OnUpdate()
    {
        var manager = EntityManager;
        var toDestroy = new NativeQueue<Entity>(Allocator.TempJob);
        var timerJob = new DecreaseTimerJob()
        {
            deltaTime = Time.deltaTime,
            toDestroy = toDestroy.AsParallelWriter()
        };
        timerJob.Schedule(this).Complete();
        Entity entity;
        while(toDestroy.TryDequeue(out entity))
            manager.DestroyEntity(entity);
        toDestroy.Dispose();
    }

    public static void MarkToDestroy(EntityManager manager, Entity entity, float delay)
    {
        if (!manager.HasComponent<DestroyEntityWithDelayComponentData>(entity))
            manager.AddComponentData(entity, new DestroyEntityWithDelayComponentData() { delay = delay });                
    }
}