using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(SwordsmanSpawner))]
public class SwordDamageApdater : MonoBehaviour, IDamageSettable
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

        public NativeHashMap<int, int>.ParallelWriter indices;
        public NativeMultiHashMap<int, Entity>.ParallelWriter entities;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            int squadIndex = chunk.GetSharedComponentIndex(squadTagType);

            indices.TryAdd(squadIndex, squadIndex);

            var es = chunk.GetNativeArray(entityType);

            for (int i = 0; i < es.Length; i++)
                entities.Add(squadIndex, es[i]);
        }
    }

    private void UpdateDamageEntities(float damage)
    {
        var manager = World.Active.EntityManager;

        var query = manager.CreateEntityQuery(
            typeof(MeleeAttackComponentData),
            typeof(SquadTagSharedComponentData)
        );

        var indices = new NativeHashMap<int, int>(query.CalculateChunkCount(), Allocator.TempJob);
        var entities = new NativeMultiHashMap<int, Entity>(query.CalculateEntityCount(), Allocator.TempJob);

        new SharedIndicesJob()
        {
            indices = indices.AsParallelWriter(),
            entities = entities.AsParallelWriter(),
            squadTagType = manager.GetArchetypeChunkSharedComponentType<SquadTagSharedComponentData>(),
            entityType = manager.GetArchetypeChunkEntityType()
        }.Schedule(query).Complete();

        var keys = indices.GetKeyArray(Allocator.TempJob);

        for (int i = 0; i < keys.Length; i++)
        {
            var squad = manager.GetSharedComponentData<SquadTagSharedComponentData>(keys[i]);
            if (squad.id.value != spawner.SquadId)
                continue;

            ANU.Utils.NativeUtils.IterateForKey(entities, keys[i], (val) =>
            {
                manager.SetComponentData(val, new MeleeAttackComponentData()
                {
                    damage = damage
                });
            });
        }

        keys.Dispose();
        indices.Dispose();
        entities.Dispose();
    }
}
