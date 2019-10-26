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
using static SquadSortNewIndicesSystem;

[UpdateBefore(typeof(SquadSystem))]
[UpdateAfter(typeof(SquadSortNewIndicesSystem))]
public class SquadsortSetIndicesSystem : JobComponentSystem
{
    [BurstCompile]
    public struct SetIndexInSquadJobChunk : IJobChunk
    {
        [ReadOnly] public NativeHashMap<int, SquadSortData> setIndicesDataByChunkIndex;
        [ReadOnly] public NativeMultiHashMap<int, int> newIndicesBySharedIndex;
        [ReadOnly] public ArchetypeChunkSharedComponentType<SquadTagSharedComponentData> squadTagType;

        public ArchetypeChunkComponentType<SquadComponentData> squadDataType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var sharedindex = chunk.GetSharedComponentIndex(squadTagType);

            SquadSortData sortData;
            if(!setIndicesDataByChunkIndex.TryGetValue(chunkIndex, out sortData))
                return;
            var squadDatas = chunk.GetNativeArray(squadDataType);

            int addedCount = 0;
            int currentMapPosition = 0;

            NativeMultiHashMapIterator<int> iterator;
            int currentValue;

            if (!newIndicesBySharedIndex.TryGetFirstValue(sharedindex, out currentValue, out iterator))
                return;
            while (currentMapPosition < sortData.startIndexInCourceArray && newIndicesBySharedIndex.TryGetNextValue(out currentValue, ref iterator))
                currentMapPosition++;

            for (int i = 0; i < chunk.Count; i++)
            {
                var data = squadDatas[i];
                if (data.indexOlreadySet)
                    continue;

                data.indexInSquad = currentValue;
                data.indexOlreadySet = true;
                squadDatas[i] = data;

                addedCount++;
                if(addedCount < sortData.cntToAdd && newIndicesBySharedIndex.TryGetNextValue(out currentValue, ref iterator))
                    currentMapPosition++;
            }
        }
    }

    public static NativeHashMap<int, SquadSortData> setIndicesDataByChunkIndex;

    protected override void OnCreate()
    {
        base.OnCreate();
        setIndicesDataByChunkIndex = new NativeHashMap<int, SquadSortData>(0, Allocator.TempJob);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        setIndicesDataByChunkIndex.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var query = GetEntityQuery(
            typeof(SquadTagSharedComponentData), 
            typeof(SquadComponentData), 
            typeof(LinearMovementComponentData)
        );
        var chunkCount = query.CalculateChunkCount();
        var entityCount = query.CalculateEntityCount();

        setIndicesDataByChunkIndex.Dispose();
        setIndicesDataByChunkIndex = new NativeHashMap<int, SquadSortData>(chunkCount, Allocator.TempJob);
        
        //теперь надо запланировать установку новых индексов
        for (int i = 0; i < SquadSortCalcIndicesSystem.sharedIndices.Length; i++)
        {
            var key = SquadSortCalcIndicesSystem.sharedIndices[i];

            var firstIndex = 0;
            SquadSortCalcIndicesSystem.chunkCountDataBySharedIndex.IterateForKey(key, (ival) =>
            {
                setIndicesDataByChunkIndex.TryAdd(
                    ival.chunkIndex,
                    new SquadSortData()
                    {
                        startIndexInCourceArray = firstIndex,
                        cntToAdd = ival.notSetIndicesCount
                    }
                );
                firstIndex += ival.notSetIndicesCount;
            });
        }
        var retHandle = new SetIndexInSquadJobChunk()
        {
            setIndicesDataByChunkIndex = setIndicesDataByChunkIndex,
            newIndicesBySharedIndex = newIndicesBySharedIndex,
            squadDataType = GetArchetypeChunkComponentType<SquadComponentData>(),
            squadTagType = GetArchetypeChunkSharedComponentType<SquadTagSharedComponentData>()
        }.Schedule(query, inputDeps);  

        return retHandle;
    }
}