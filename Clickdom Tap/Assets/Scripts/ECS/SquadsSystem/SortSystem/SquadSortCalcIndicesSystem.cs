using ANU.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(SquadSystem))]
public class SquadSortCalcIndicesSystem : ComponentSystem
{
    public struct ChunkSquadCountData :IComparable<ChunkSquadCountData>
    {
        public int sharedIndex;
        public int chunkIndex;
        public int entityCount;
        public int notSetIndicesCount;

        public int CompareTo(ChunkSquadCountData obj)
        {
            return chunkIndex.CompareTo(obj.chunkIndex);
        }
    }

    [BurstCompile]
    public struct SharedIndicesJobChunk : IJobChunk
    {
        public NativeMultiHashMap<int, ChunkSquadCountData>.ParallelWriter chunkCountDataBySharedIndex;
        public NativeMultiHashMap<int, int>.ParallelWriter indicesInSquadBySharedIndices;

        [ReadOnly] public ArchetypeChunkSharedComponentType<SquadTagSharedComponentData> squadTagType;
        [ReadOnly] public ArchetypeChunkComponentType<SquadComponentData> squadDataType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var index = chunk.GetSharedComponentIndex(squadTagType);
            var datas = chunk.GetNativeArray(squadDataType);

            var notSetIndicesCount = 0;
            for (int i = 0; i < chunk.Count; i++)
            {
                if (datas[i].indexOlreadySet)
                {
                    indicesInSquadBySharedIndices.Add(
                        index,
                        datas[i].indexInSquad
                    );
                }
                else
                    notSetIndicesCount++;
            }

            chunkCountDataBySharedIndex.Add(
                index,
                new ChunkSquadCountData() {
                    chunkIndex = chunkIndex,
                    entityCount = chunk.Count,
                    sharedIndex = index,
                    notSetIndicesCount = notSetIndicesCount
                }
            );
        }
    }

    public static NativeArray<int> sharedIndices;
    public static NativeHashMap<int, int> entityCountBySharedIndex;
    public static NativeMultiHashMap<int, ChunkSquadCountData> chunkCountDataBySharedIndex;
    public static NativeMultiHashMap<int, int> indicesInSquadBySharedIndices;

    protected override void OnCreate()
    {
        base.OnCreate();

        sharedIndices = new NativeArray<int>(0, Allocator.TempJob);
        entityCountBySharedIndex = new NativeHashMap<int, int>(0, Allocator.TempJob);
        chunkCountDataBySharedIndex = new NativeMultiHashMap<int, ChunkSquadCountData>(0, Allocator.TempJob);
        indicesInSquadBySharedIndices = new NativeMultiHashMap<int, int>(0, Allocator.TempJob);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        sharedIndices.Dispose();
        entityCountBySharedIndex.Dispose();
        chunkCountDataBySharedIndex.Dispose();
        indicesInSquadBySharedIndices.Dispose();
    }

    protected override void OnUpdate()
    {
        var query = GetEntityQuery(
            typeof(SquadTagSharedComponentData), 
            typeof(SquadComponentData), 
            typeof(LinearMovementComponentData)
        );
        var chunkCount = query.CalculateChunkCount();
        var entityCount = query.CalculateEntityCount();

        chunkCountDataBySharedIndex.Dispose();
        chunkCountDataBySharedIndex = new NativeMultiHashMap<int, ChunkSquadCountData>(chunkCount, Allocator.TempJob);
        indicesInSquadBySharedIndices.Dispose();
        indicesInSquadBySharedIndices = new NativeMultiHashMap<int, int>(entityCount, Allocator.TempJob);
        //получим индексы SquadTagSharedComponentData
        //и количество ентити в каждом чанке
        //а также,уже установленные индексы в отрядах
        var sharedIndicesJob = new SharedIndicesJobChunk()
        {
            chunkCountDataBySharedIndex = chunkCountDataBySharedIndex.AsParallelWriter(),
            indicesInSquadBySharedIndices = indicesInSquadBySharedIndices.AsParallelWriter(),
            squadTagType = GetArchetypeChunkSharedComponentType<SquadTagSharedComponentData>(),
            squadDataType = GetArchetypeChunkComponentType<SquadComponentData>()
        }.Schedule(query);

        sharedIndicesJob.Complete();

        sharedIndices.Dispose();
        sharedIndices = chunkCountDataBySharedIndex.GetUniqueKeys(Allocator.TempJob);

        //для всех отрядов получаем новые индексы
        entityCountBySharedIndex.Dispose();
        entityCountBySharedIndex = new NativeHashMap<int, int>(sharedIndices.Length, Allocator.TempJob);
        for (int i = 0; i < sharedIndices.Length; i++)
        {
            var key = sharedIndices[i];

            var maxCntForSharedIndex = 0;
            chunkCountDataBySharedIndex.IterateForKey(key, (ival) =>
            {
                maxCntForSharedIndex += ival.entityCount;
            });
            entityCountBySharedIndex.TryAdd(key, maxCntForSharedIndex);
        }
        //устанавливаем кол-во человек в отряде пока идёт формирование новых индексов
        for (int i = 0; i < sharedIndices.Length; i++)
        {
            var sharedData = EntityManager.GetSharedComponentData<SquadTagSharedComponentData>(sharedIndices[i]);
            int cnt = 0;
            if (!entityCountBySharedIndex.TryGetValue(sharedIndices[i], out cnt))
                continue;
            if (sharedData.unitCount.value != cnt)
            {
                sharedData.unitCount.value = cnt;
                sharedData.unitCount.valueChangedEventFlag = true;
            }
            else
            {
                sharedData.unitCount.valueChangedEventFlag = false;
            }
        }  
    }
}