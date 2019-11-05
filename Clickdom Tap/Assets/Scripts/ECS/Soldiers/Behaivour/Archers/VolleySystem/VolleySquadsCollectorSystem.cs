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

public struct VolleyAnimationPauseTagComponentData : IComponentData
{
}

public class VolleySquadsCollectorSystem : ComponentSystem
{

    [BurstCompile]
    struct CollectSquads : IJobChunk
    {
        public NativeMultiHashMap<int, bool>.ParallelWriter squadsPauses;

        [ReadOnly] public ArchetypeChunkSharedComponentType<SquadTagSharedComponentData> squadTagType;
        [ReadOnly] public ArchetypeChunkComponentType<SpriteSheetAnimationComponentData> animationType;
        [ReadOnly] public ArchetypeChunkComponentType<AnimatorStatesComponentData> animatorType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var index = chunk.GetSharedComponentIndex(squadTagType);
            var animators = chunk.GetNativeArray(animatorType);
            var animations = chunk.GetNativeArray(animationType);

            for (int i = 0; i < chunk.Count; i++)
                if (animators[i].shooting)
                    squadsPauses.Add(index, animations[i].pause);
        }
    }

    public static NativeMultiHashMap<int, bool> squads;

    protected override void OnCreate()
    {
        base.OnCreate();
        squads = new NativeMultiHashMap<int, bool>(0, Allocator.TempJob);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        squads.Dispose();
    }
    
    protected override void OnUpdate()
    {
        var query = GetEntityQuery(
            typeof(SquadTagSharedComponentData),
            typeof(VolleyAnimationPauseTagComponentData),
            typeof(AnimatorStatesComponentData),
            typeof(SpriteSheetAnimationComponentData),
            typeof(AnimationPauseComponentData)
        );

        squads.Dispose();
        squads = new NativeMultiHashMap<int, bool>(query.CalculateEntityCount(), Allocator.TempJob);

        var job = new CollectSquads
        {
            squadsPauses = squads.AsParallelWriter(),
            squadTagType = GetArchetypeChunkSharedComponentType<SquadTagSharedComponentData>(),
            animationType = GetArchetypeChunkComponentType<SpriteSheetAnimationComponentData>(),
            animatorType = GetArchetypeChunkComponentType<AnimatorStatesComponentData>(),
        };
        job.Schedule(query).Complete();
    }
}