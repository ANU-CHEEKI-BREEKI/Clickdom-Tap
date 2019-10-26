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

/// <summary>
/// устанавливает цель для сьрельбы лучников, основываясь на его позиции в отряде.
/// </summary>
public class ShootTargetBySquadUpdateSystem : ComponentSystem
{
    public struct SquadFormationData
    {
        public int yUnitCount;
        public int xUnitCount;
        public bool left2right;
        public bool bot2top;
    }

    [BurstCompile]
    public struct SharedIndicesJob : IJobChunk
    {
        [ReadOnly] public ArchetypeChunkSharedComponentType<SquadTagSharedComponentData> squadTagType;
        public NativeHashMap<int, int>.ParallelWriter indices;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var index = chunk.GetSharedComponentIndex(squadTagType);
            indices.TryAdd(index, index);
        }
    }

    [BurstCompile]
    public struct SetTargetJob : IJobChunk
    {
        [ReadOnly] public ArchetypeChunkSharedComponentType<SquadTagSharedComponentData> squadTagType;
        [ReadOnly] public NativeHashMap<int, SquadFormationData> sharedData;
        [ReadOnly] public ArchetypeChunkComponentType<SquadComponentData> squadType;
        [ReadOnly] public ArchetypeChunkComponentType<LerpShootTargetProvederComponentData> targetProviderType;
        [ReadOnly] public Unity.Mathematics.Random rnd;

        public ArchetypeChunkComponentType<ArcherTargetPositionComponentData> targetTypr;
        
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var sharedIndex = chunk.GetSharedComponentIndex(squadTagType);
            SquadFormationData data;
            if (!sharedData.TryGetValue(sharedIndex, out data))
                return;
            if (data.xUnitCount == 0) data.xUnitCount = 1;
            if (data.yUnitCount == 0) data.yUnitCount = 1;

            data.xUnitCount += 2;
            data.yUnitCount += 2;

            var indices = chunk.GetNativeArray(squadType);
            var providers = chunk.GetNativeArray(targetProviderType);
            var targets = chunk.GetNativeArray(targetTypr);

            for (int i = 0; i < chunk.Count; i++)
            {
                var index = indices[i].indexPositionInSquad;
                index.x++;
                index.y++;

                var target = targets[i];
                target.value = providers[i].EvaluateShootTarget(index.x / data.xUnitCount, index.y / data.yUnitCount, data.left2right, data.bot2top);
                target.value += rnd.NextFloat2(new float2(1, 1) * -providers[i].randomSpread, new float2(1, 1) * providers[i].randomSpread);
                targets[i] = target;
            }
        }
    }

    protected override void OnUpdate()
    {
        var query = GetEntityQuery(typeof(SquadTagSharedComponentData), typeof(SquadComponentData), typeof(LerpShootTargetProvederComponentData), typeof(ArcherTargetPositionComponentData));
        var chunkCount = query.CalculateChunkCount();

        var sharedIndicesMap = new NativeHashMap<int, int>(chunkCount, Allocator.TempJob);
        var indicesJobH = new SharedIndicesJob()
        {
            indices = sharedIndicesMap.AsParallelWriter(),
            squadTagType = GetArchetypeChunkSharedComponentType<SquadTagSharedComponentData>()
        }.Schedule(query);

        var sharedData = new NativeHashMap<int, SquadFormationData>(chunkCount, Allocator.TempJob);
        indicesJobH.Complete();

        var indices = sharedIndicesMap.GetKeyArray(Allocator.TempJob);

        for (int i = 0; i < indices.Length; i++)
        {
            var data = EntityManager.GetSharedComponentData<SquadTagSharedComponentData>(indices[i]);
            sharedData.TryAdd(indices[i], new SquadFormationData()
            {
                yUnitCount = data.data.heightUnitsCount,
                xUnitCount = data.unitCount.value / data.data.heightUnitsCount + (data.unitCount.value % data.data.heightUnitsCount == 0 ? 0 : 1),
                bot2top = data.data.directionBottomToTop,
                left2right = data.data.directionLeftToRight
            });
        }

        var setTargetJobH = new SetTargetJob()
        {
            sharedData = sharedData,
            squadTagType = GetArchetypeChunkSharedComponentType<SquadTagSharedComponentData>(),
            squadType = GetArchetypeChunkComponentType<SquadComponentData>(true),
            targetProviderType = GetArchetypeChunkComponentType<LerpShootTargetProvederComponentData>(true),
            targetTypr = GetArchetypeChunkComponentType<ArcherTargetPositionComponentData>(false),
            rnd = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, int.MaxValue))
        }.Schedule(query);
        setTargetJobH.Complete();

        indices.Dispose();
        sharedData.Dispose();
        sharedIndicesMap.Dispose();

    }
}