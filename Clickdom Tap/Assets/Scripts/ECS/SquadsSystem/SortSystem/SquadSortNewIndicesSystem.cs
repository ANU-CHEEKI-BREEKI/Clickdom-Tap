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
[UpdateAfter(typeof(SquadSortCalcIndicesSystem))]
public class SquadSortNewIndicesSystem : JobComponentSystem
{   
    public struct SquadSortData
    {
        public int cntToAdd;
        //
        public int startIndexInCourceArray;
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

    public static NativeMultiHashMap<int, int> newIndicesBySharedIndex;

    protected override void OnCreate()
    {
        base.OnCreate();
        newIndicesBySharedIndex = new NativeMultiHashMap<int, int>(0, Allocator.TempJob);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        newIndicesBySharedIndex.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var query = GetEntityQuery(
            typeof(SquadTagSharedComponentData), 
            typeof(SquadComponentData), 
            typeof(LinearMovementComponentData)
        );
        var entityCount = query.CalculateEntityCount();

        newIndicesBySharedIndex.Dispose();
        newIndicesBySharedIndex = new NativeMultiHashMap<int, int>(entityCount * 3, Allocator.TempJob);

        var getNewIndicesJH = new GetNewIndicesJob()
        {
            indicesInSquadBySharedIndices = SquadSortCalcIndicesSystem.indicesInSquadBySharedIndices,
            entityCountBySharedIndex = SquadSortCalcIndicesSystem.entityCountBySharedIndex,
            sharedIndices = SquadSortCalcIndicesSystem.sharedIndices,
            newIndicesBySharedIndex = newIndicesBySharedIndex.AsParallelWriter()
        }.Schedule(SquadSortCalcIndicesSystem.sharedIndices.Length, 1, inputDeps);
                
        return getNewIndicesJH;
    }
}