using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(ArcherSpawner))]
public class ArrowDamageUpdater : MonoBehaviour, IDamageSettable
{
    private ASpawner spawner;

    private void Start()
    {
        spawner = GetComponent<ASpawner>();
    }

    public void SetDamage(float damage)
    {
        UpdateDamageEntities(damage);
    }

    [BurstCompile]
    private struct SharedIndicesJob : IJobChunk
    {
        [ReadOnly] public ArchetypeChunkEntityType entityType;
        [ReadOnly] public ArchetypeChunkSharedComponentType<SquadTagSharedComponentData> squadTagType;
        [ReadOnly] public ArchetypeChunkSharedComponentType<SquadProjectileLaunchDataSharedComponentData> launchType;

        public NativeHashMap<int, int>.ParallelWriter indices;
        public NativeMultiHashMap<int, Entity>.ParallelWriter entities;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            int squadIndex = chunk.GetSharedComponentIndex(squadTagType);
            int launchIndex = chunk.GetSharedComponentIndex(launchType);

            indices.TryAdd(squadIndex, launchIndex);

            var es = chunk.GetNativeArray(entityType);

            for (int i = 0; i < es.Length; i++)
                entities.Add(squadIndex, es[i]);
        }
    }

    private void UpdateDamageEntities(float damage)
    {
        var manager = World.Active.EntityManager;

        var query = manager.CreateEntityQuery(
            typeof(SquadProjectileLaunchDataSharedComponentData),
            typeof(SquadTagSharedComponentData)
        );

        var indices = new NativeHashMap<int, int>(query.CalculateChunkCount(), Allocator.TempJob);
        var entities = new NativeMultiHashMap<int, Entity>(query.CalculateEntityCount(), Allocator.TempJob);

        new SharedIndicesJob()
        {
            indices = indices.AsParallelWriter(),
            entities = entities.AsParallelWriter(),
            launchType = manager.GetArchetypeChunkSharedComponentType<SquadProjectileLaunchDataSharedComponentData>(),
            squadTagType = manager.GetArchetypeChunkSharedComponentType<SquadTagSharedComponentData>(),
            entityType = manager.GetArchetypeChunkEntityType()
        }.Schedule(query).Complete();

        var keys = indices.GetKeyArray(Allocator.TempJob);

        for (int i = 0; i < keys.Length; i++)
        {
            var squad = manager.GetSharedComponentData<SquadTagSharedComponentData>(keys[i]);
            if (squad.id.value != spawner.SquadId)
                continue;

            var lindex = 0;
            indices.TryGetValue(keys[i], out lindex);
            var launch = manager.GetSharedComponentData<SquadProjectileLaunchDataSharedComponentData>(lindex);
            launch.collisionData.processData.damage = damage;

            ANU.Utils.NativeUtils.IterateForKey(entities, keys[i], (val) =>
            {
                manager.SetSharedComponentData(val, launch);
            });
        }

        keys.Dispose();
        indices.Dispose();
        entities.Dispose();
    }
}
