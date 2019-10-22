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
public class SquadSortSystem : ComponentSystem
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

    public struct SquadSortData
    {
        public int cntToAdd;
        //
        public int startIndexInCourceArray;
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

    [BurstCompile]
    public struct GetNewIndicesJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int> sharedIndices;
        [ReadOnly] public NativeMultiHashMap<int, int> indicesInSquadBySharedIndices;
        [ReadOnly] public NativeHashMap<int, int> entityCountBySharedIndex;

        public NativeMultiHashMap<int, int>.ParallelWriter newIndicesBySharedIndex;

        public void Execute(int index)
        {
            var key = sharedIndices[index];
            int count = 0;
            if(!entityCountBySharedIndex.TryGetValue(key, out count))
                return;

            //create new indices
            for (int i = 0; i < count; i++)
            {
                var contains = indicesInSquadBySharedIndices.ContainsValueForKey(key, i);
                if (!contains)
                    newIndicesBySharedIndex.Add(key, i);
            }
        }
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

        var chunkCountDataBySharedIndex = new NativeMultiHashMap<int, ChunkSquadCountData>(chunkCount, Allocator.TempJob);
        var indicesInSquadBySharedIndices = new NativeMultiHashMap<int, int>(entityCount, Allocator.TempJob);
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
        
        var newIndicesBySharedIndex = new NativeMultiHashMap<int, int>(entityCount * 3, Allocator.TempJob);

        sharedIndicesJob.Complete();

        var sharedIndices = chunkCountDataBySharedIndex.GetUniqueKeys(Allocator.TempJob);

        //для всех отрядов получаем новые индексы
        var entityCountBySharedIndex = new NativeHashMap<int, int>(sharedIndices.Length, Allocator.TempJob);
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

        var getNewIndicesJH = new GetNewIndicesJob()
        {
            indicesInSquadBySharedIndices = indicesInSquadBySharedIndices,
            entityCountBySharedIndex = entityCountBySharedIndex,
            sharedIndices = sharedIndices,
            newIndicesBySharedIndex = newIndicesBySharedIndex.AsParallelWriter()
        }.Schedule(sharedIndices.Length, 10);

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

        var setIndicesDataByChunkIndex = new NativeHashMap<int, SquadSortData>(chunkCount, Allocator.TempJob);
        getNewIndicesJH.Complete();

        //теперь надо запланировать установку новых индексов
        for (int i = 0; i < sharedIndices.Length; i++)
        {
            var key = sharedIndices[i];

            var firstIndex = 0;
            chunkCountDataBySharedIndex.IterateForKey(key, (ival) =>
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
        }.Schedule(query);        

        entityCountBySharedIndex.Dispose();
        sharedIndices.Dispose();
        chunkCountDataBySharedIndex.Dispose();

        retHandle.Complete();

        setIndicesDataByChunkIndex.Dispose();

        indicesInSquadBySharedIndices.Dispose();
        newIndicesBySharedIndex.Dispose();
        
    }
}