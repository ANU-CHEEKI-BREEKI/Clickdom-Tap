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

        public int CompareTo(ChunkSquadCountData obj)
        {
            return chunkIndex.CompareTo(obj.chunkIndex);
        }
    }

    public struct SquadSortData
    {
        public int cntToAdd;
    }


    [BurstCompile]
    public struct SharedIndicesJobChunk : IJobChunk
    {
        public NativeMultiHashMap<int, ChunkSquadCountData>.ParallelWriter sharedIndices;
        [ReadOnly] public ArchetypeChunkSharedComponentType<SquadTagSharedComponentData> squadTagType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var index = chunk.GetSharedComponentIndex(squadTagType);
            sharedIndices.Add(
                index, 
                new ChunkSquadCountData() {
                    chunkIndex = chunkIndex,
                    entityCount = chunk.Count,
                    sharedIndex = index
                }
            );
        }
    }

    [BurstCompile]
    public struct SortJobChunk : IJobChunk
    {
        [ReadOnly] public NativeHashMap<int, SquadSortData> sortDatas;
        public ArchetypeChunkComponentType<SquadComponentData> squadDataType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            SquadSortData sortData;
            if(!sortDatas.TryGetValue(chunkIndex, out sortData))
                return;
            var squadDatas = chunk.GetNativeArray(squadDataType);
            for (int i = 0; i < chunk.Count; i++)
            {
                var data = squadDatas[i];
                if (data.indexOlreadySet) continue;
                data.indexInSquad = i + sortData.cntToAdd;
                data.indexOlreadySet = true;
                squadDatas[i] = data;
            }
        }
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
    }    
    
    protected override void OnUpdate()
    {
        var query = GetEntityQuery(
            typeof(SquadTagSharedComponentData), 
            typeof(SquadComponentData), 
            typeof(LinearMovementComponentData)//, 
            //ComponentType.Exclude<SequenceMovementSharedComponentData>()
            );
        var chunkCount = query.CalculateChunkCount();
        
        var sharedIndices = new NativeMultiHashMap<int, ChunkSquadCountData>(chunkCount, Allocator.TempJob);
        //получим индексы SquadTagSharedComponentData
        //и количество ентити в каждом чанке
        var sharedIndicesJob = new SharedIndicesJobChunk()
        {
            sharedIndices = sharedIndices.AsParallelWriter(),
            squadTagType = GetArchetypeChunkSharedComponentType<SquadTagSharedComponentData>()
        }.Schedule(query);
        
        var sortData = new NativeHashMap<int, SquadSortData>(chunkCount * 2, Allocator.TempJob);

        sharedIndicesJob.Complete();
        //сформируем список с данными, какой минимальный номер
        //ентити должен быть в каждом чанке
        //для каждого индекса SquadTagSharedComponentData
        var indices = sharedIndices.GetKeyArray(Allocator.TempJob);
        var cnts = new NativeHashMap<int, int>(indices.Length, Allocator.TempJob);
        for (int i = 0; i < indices.Length; i++)
        {
            var key = indices[i];
            var cnt = Utils.Native.CountForKey(sharedIndices, key);
            if (cnt <= 0)
                continue;
            var dataArray = new NativeArray<ChunkSquadCountData>(cnt, Allocator.TempJob);
            int index = 0;
            Utils.Native.IterateForKey(sharedIndices, key, (ival)=>
            {
                dataArray[index] = ival;
                index++;
            });
            //сортируем по номеру чанка
            var indexer = new NativeArrayIndexer<ChunkSquadCountData>(dataArray);
            Utils.Algoritm.QuickSort<NativeArrayIndexer<ChunkSquadCountData>, ChunkSquadCountData>(ref indexer);
            //теперь надо создать список с данными, какой минимальный номер
            //ентити должен быть в каждом чанке
            //для каждого индекса SquadTagSharedComponentData
            var maxCntForSharedIndex = 0;
            for (int j = 0; j < dataArray.Length; j++)
            {
                var ival = dataArray[j];
                sortData.TryAdd(ival.chunkIndex, new SquadSortData()
                {
                    cntToAdd = maxCntForSharedIndex
                });
                maxCntForSharedIndex += ival.entityCount;
            }
            cnts.TryAdd(key, maxCntForSharedIndex);
            dataArray.Dispose();
        }
        var retHandle = new SortJobChunk()
        {
            sortDatas = sortData,
            squadDataType = GetArchetypeChunkComponentType<SquadComponentData>()
        }.Schedule(query);

        //устанавливаем кол-во человек в отряде
        for (int i = 0; i < indices.Length; i++)
        {
            var sharedData = EntityManager.GetSharedComponentData<SquadTagSharedComponentData>(indices[i]);
            int cnt = 0;
            if (!cnts.TryGetValue(indices[i], out cnt))
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

        cnts.Dispose();
        indices.Dispose();
        sharedIndices.Dispose();

        retHandle.Complete();

        sortData.Dispose();
    }
}