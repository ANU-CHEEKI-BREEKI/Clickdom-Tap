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

public struct SquadProjectileLaunchDataSharedComponentData : ISharedComponentData, IEquatable<SquadProjectileLaunchDataSharedComponentData>
{
    public ProjectileLaunshSetupComponentData launchData;
    public SpriteSheetAnimationComponentData animaionData;
    public RenderSharedComponentData renderData;
    public ProjectileCollisionComponentData collisionData;

    public bool Equals(SquadProjectileLaunchDataSharedComponentData other)
    {
        return launchData.Equals(other.launchData) &&
            animaionData.Equals(other.animaionData) &&
            renderData.Equals(other.renderData) &&
            collisionData.Equals(other.collisionData);
    }

    public override int GetHashCode()
    {
        return launchData.GetHashCode()     * 2 +
                animaionData.GetHashCode()  * 6 +
                renderData.GetHashCode()    * 3 +
                collisionData.GetHashCode() * 8;
    }
}

public struct ArcherTargetPositionComponentData :IComponentData
{
    public float2 value;
}

[Serializable]
public struct ActionData
{
    public int frame;
}

public struct ActionOnAnimationFrameComponentData : IComponentData
{
    public bool needAction;
    public ActionData actionData;
}

//[DisableAutoCreation]
public class ArcherShootSystem : ComponentSystem
{
    public struct ShootData
    {
        public float2 targetPosition;
        public float3 ownerPosition;
        public float ownerScale;
    }

    public struct SharedIndicesJob : IJobChunk
    {
        [ReadOnly] public ArchetypeChunkSharedComponentType<SquadProjectileLaunchDataSharedComponentData> launchType;
        public NativeHashMap<int, int>.ParallelWriter sharedIndices;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var index = chunk.GetSharedComponentIndex(launchType);
            sharedIndices.TryAdd(index, index);
        }
    }

    public struct DetectActionSquadJob : IJobChunk
    {
        [ReadOnly] public ArchetypeChunkSharedComponentType<SquadProjectileLaunchDataSharedComponentData> launchType;

        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        [ReadOnly] public ArchetypeChunkComponentType<Scale> scaleType;
        [ReadOnly] public ArchetypeChunkComponentType<SpriteSheetAnimationComponentData> animationType;
        [ReadOnly] public ArchetypeChunkComponentType<ActionOnAnimationFrameComponentData> actionType;
        [ReadOnly] public ArchetypeChunkComponentType<ArcherTargetPositionComponentData> targetType;

        public NativeMultiHashMap<int, ShootData>.ParallelWriter detectedActions;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var index = chunk.GetSharedComponentIndex(launchType);

            var translations = chunk.GetNativeArray(translationType);
            var scales = chunk.GetNativeArray(scaleType);
            var animations = chunk.GetNativeArray(animationType);
            var actions = chunk.GetNativeArray(actionType);
            var targets = chunk.GetNativeArray(targetType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var action = actions[i];
                if (!action.needAction)
                    continue;
                if (animations[i].currentFrame != action.actionData.frame || !animations[i].frameChangedEventFlag) 
                    continue;

                detectedActions.Add(index, new ShootData()
                {
                    ownerPosition = translations[i].Value,
                    ownerScale = scales[i].Value,
                    targetPosition = targets[i].value
                });
            }
        }
    }

    protected override void OnUpdate()
    {
        var query = GetEntityQuery(
            typeof(ArcherTagComponentData), 
            typeof(SquadProjectileLaunchDataSharedComponentData), 
            typeof(Translation), 
            typeof(Scale), 
            typeof(SpriteSheetAnimationComponentData),
            typeof(ActionOnAnimationFrameComponentData),
            typeof(ArcherTargetPositionComponentData)
        );
        var sharedIndices = new NativeHashMap<int, int>(query.CalculateChunkCount(), Allocator.TempJob);
        var detectedActions = new NativeMultiHashMap<int, ShootData>(query.CalculateEntityCount(), Allocator.TempJob);

        var sharedIndicesJH = new SharedIndicesJob()
        {
            launchType = GetArchetypeChunkSharedComponentType<SquadProjectileLaunchDataSharedComponentData>(),
            sharedIndices = sharedIndices.AsParallelWriter()
        }.Schedule(query);

        var detectActionsJH = new DetectActionSquadJob()
        {
            detectedActions = detectedActions.AsParallelWriter(),
            launchType = GetArchetypeChunkSharedComponentType<SquadProjectileLaunchDataSharedComponentData>(),
            actionType = GetArchetypeChunkComponentType<ActionOnAnimationFrameComponentData>(true),
            animationType = GetArchetypeChunkComponentType<SpriteSheetAnimationComponentData>(true),
            scaleType = GetArchetypeChunkComponentType<Scale>(true),
            targetType = GetArchetypeChunkComponentType<ArcherTargetPositionComponentData>(true),
            translationType = GetArchetypeChunkComponentType<Translation>(true)
        }.Schedule(query);

        sharedIndicesJH.Complete();
        var indices = sharedIndices.GetKeyArray(Allocator.TempJob);
        detectActionsJH.Complete();

        for (int i = 0; i < indices.Length; i++)
        {
            var sharedData = EntityManager.GetSharedComponentData<SquadProjectileLaunchDataSharedComponentData>(indices[i]);

            var animation = sharedData.animaionData;
            var collision = sharedData.collisionData;
            var render = sharedData.renderData;

            detectedActions.IterateForKey(indices[i], (detect) =>
            {
                var launch = sharedData.launchData;
                launch.targetPosition = detect.targetPosition;
                launch.targetWidth *= detect.ownerScale;

                LaunchProjectileSystem.Instance.LaunchArrow(
                   detect.ownerPosition,
                   detect.ownerScale,
                   launch,
                   animation,
                   render,
                   collision
                );
            });
        }

        indices.Dispose();
        sharedIndices.Dispose();
        detectedActions.Dispose();
    }
}