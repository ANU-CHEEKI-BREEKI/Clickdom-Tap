using ANU.Utils;
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
using static ANU.Utils.Jobs;

[UpdateAfter(typeof(VolleyAnimationPauserSystem))]
[UpdateAfter(typeof(VolleySquadsCollectorSystem))]
public class VolleyAnimationResumerSystem : JobComponentSystem
{
    [BurstCompile]
    struct IsAllEntitiesPausedJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int> indices;
        [ReadOnly] public NativeMultiHashMap<int, bool> squadPauses;
        [ReadOnly] public float percent;

        public NativeHashMap<int, int>.ParallelWriter toResumeSquadsIndices;

        public void Execute(int index)
        {
            int squad = indices[index];
            var allCnt = 0;
            var pausesCnt = 0;
            bool pause;
            NativeMultiHashMapIterator<int> iterator;
            if (squadPauses.TryGetFirstValue(squad, out pause, out iterator))
            {
                do
                {
                    if (pause)
                        pausesCnt++;
                    allCnt++;
                }
                while (squadPauses.TryGetNextValue(out pause, ref iterator));
            }

            if ((float)pausesCnt/allCnt >= percent)
                toResumeSquadsIndices.TryAdd(squad, squad);
        }
    }

    [BurstCompile]
    struct EnableResumerJob : IJobChunk
    {
        [ReadOnly] public NativeHashMap<int, int> toResumeSquadsIndices;

        [ReadOnly] public ArchetypeChunkSharedComponentType<SquadTagSharedComponentData> squadTagType;
        public ArchetypeChunkComponentType<AnimationPauseComponentData> pauseType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var index = chunk.GetSharedComponentIndex(squadTagType);

            if (!toResumeSquadsIndices.ContainsKey(index))
                return;

            var pauses = chunk.GetNativeArray(pauseType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var pause = pauses[i];
                pause.needResume = true;
                pauses[i] = pause;
            }
        }
    }

    private NativeArray<int> sharedIndicesSquads;
    private NativeHashMap<int, int> toResumeSquadsIndices;

    protected override void OnCreate()
    {
        base.OnCreate();
        sharedIndicesSquads = new NativeArray<int>(0, Allocator.TempJob);
        toResumeSquadsIndices = new NativeHashMap<int, int>(0, Allocator.TempJob);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        sharedIndicesSquads.Dispose();
        toResumeSquadsIndices.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var query = GetEntityQuery(
           typeof(SquadTagSharedComponentData),
           typeof(VolleyAnimationPauseTagComponentData),
           typeof(AnimatorStatesComponentData),
           typeof(SpriteSheetAnimationComponentData),
           typeof(AnimationPauseComponentData)
        );

        sharedIndicesSquads.Dispose();
        sharedIndicesSquads = VolleySquadsCollectorSystem.squads.GetKeyArray(Allocator.TempJob);
        toResumeSquadsIndices.Dispose();
        toResumeSquadsIndices = new NativeHashMap<int, int>(sharedIndicesSquads.Length, Allocator.TempJob);

        var handle = new IsAllEntitiesPausedJob()
        {
            indices = sharedIndicesSquads,
            squadPauses = VolleySquadsCollectorSystem.squads,
            toResumeSquadsIndices = toResumeSquadsIndices.AsParallelWriter(),
            percent = 0.7f
        }.Schedule(sharedIndicesSquads.Length, 1, inputDeps);

        var job = new EnableResumerJob
        {
            toResumeSquadsIndices = toResumeSquadsIndices,
            pauseType = GetArchetypeChunkComponentType<AnimationPauseComponentData>(),
            squadTagType = GetArchetypeChunkSharedComponentType<SquadTagSharedComponentData>()
        };
        return job.Schedule(query, handle);
    }
}