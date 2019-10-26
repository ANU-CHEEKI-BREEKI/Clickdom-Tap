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

public enum FormationType
{
    RECTANGLE,
    [Obsolete]
    CIRCLE
}

[Serializable]
public struct SquadTagSharedComponentData : ISharedComponentData, IEquatable<SquadTagSharedComponentData>
{
    public RefInt id;
    public RefInt unitCount;

    public Data data;

    [Serializable]
    public struct Data
    {
        public FormationType formation;
        public float formationAccuracy;
        [Space]
        [Range(1, 100f)] public int heightUnitsCount;
        [Range(1, 100f)] public int radiusUnitsCount;
        [Space]
        public float horisontalSpacing;
        public float verticalSpacing;
        [Space]
        [Range(-180f, 180f)] public float rotationDegrees;
        [Space]
        [Range(-45f, 45f)] public float horisontalShift;
        [Range(-45f, 45f)] public float verticalShift;
        [Space]
        public bool directionLeftToRight;
        public bool directionBottomToTop;
        [HideInInspector]
        public float2 formationCenter;
    }
    [Serializable]
    public class RefInt
    {
        public int value = 0;
        public bool valueChangedEventFlag = false;
    }

    public bool Equals(SquadTagSharedComponentData other)
    {
        if (id == null || other.id == null) return false;
        return id.value == other.id.value;
    }

    public override int GetHashCode()
    {
        if (id == null) return base.GetHashCode();
        return id.value.GetHashCode();
    }
}

public struct SquadComponentData : IComponentData
{
    public bool indexOlreadySet;
    public int indexInSquad;
    public float2 indexPositionInSquad;
}

[UpdateAfter(typeof(SquadsortSetIndicesSystem))]
public class SquadSystem : ComponentSystem
{
    public struct SquadFormationData
    {
        public SquadTagSharedComponentData.Data mainData;
        public int count;
    }

    [BurstCompile]
    public struct SharedIndicesJobChunk : IJobChunk
    {
        public NativeHashMap<int, int>.ParallelWriter sharedIndices;
        [ReadOnly] public ArchetypeChunkSharedComponentType<SquadTagSharedComponentData> squadTagType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var index = chunk.GetSharedComponentIndex(squadTagType);
            sharedIndices.TryAdd(index, 0);
        }
    }

    [BurstCompile]
    public struct SetPositionJobChunk : IJobChunk
    {
        [ReadOnly] public Unity.Mathematics.Random rnd;
        [ReadOnly] public NativeHashMap<int, SquadFormationData> squadTagMap;
        public ArchetypeChunkComponentType<SquadComponentData> squadType;
        public ArchetypeChunkComponentType<LinearMovementComponentData> moveType;
        [ReadOnly] public ArchetypeChunkComponentType<Scale> scaleType;

        [ReadOnly] public ArchetypeChunkSharedComponentType<SquadTagSharedComponentData> squadTagType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var index = chunk.GetSharedComponentIndex(squadTagType);
            SquadFormationData formationData;
            if (!squadTagMap.TryGetValue(index, out formationData))
                return;

            var squadDatas = chunk.GetNativeArray(squadType);
            var moveDatas = chunk.GetNativeArray(moveType);
            var scaleDatas = new NativeArray<Scale>();

            var hasScale = chunk.Has(scaleType);
            if(hasScale)
                scaleDatas = chunk.GetNativeArray(scaleType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var move = moveDatas[i];
                var scale = 1f;
                if (hasScale)
                    scale = scaleDatas[i].Value;
                var squadData = squadDatas[i];

                var rot = new float2(
                    math.cos(math.radians(formationData.mainData.rotationDegrees)),
                    math.sin(math.radians(formationData.mainData.rotationDegrees))
                );
                var shiftSC = new float2(
                   math.cos(math.radians(formationData.mainData.horisontalShift * 2 + 90)),
                   math.sin(math.radians(formationData.mainData.verticalShift * 2))
                );
                float2 indexPosInSquad;
                move.positionToMove = GetPositionInSquad(
                    out indexPosInSquad,
                    ref formationData,
                    squadData.indexInSquad,
                    scale,
                    ref rnd,
                    rot,
                    shiftSC
                );
                squadData.indexPositionInSquad = indexPosInSquad;
                squadDatas[i] = squadData;
                moveDatas[i] = move;
            }          
        }
    }

    private static float2 GetPositionInSquad(
        out float2 posInSquad,
        ref SquadFormationData formationData,
        int indexInSquad,
        float scale,
        ref Unity.Mathematics.Random rnd,
        float2 rotSC,
        float2 shiftSC)
    {
        var formationMainData = formationData.mainData;

        var index = indexInSquad;        
        var randRange = formationMainData.formationAccuracy;
        posInSquad = new float2();

        switch (formationMainData.formation)
        {
            case FormationType.RECTANGLE:
                //  n+m   ..   9   6   3
                //  ..    ..   ..  ..  ..       m - rows
                //  n+1   ..   8   5   2        n - cols
                //  n     ..   7   4   1
                var dirH = formationMainData.directionLeftToRight ? 1 : -1;
                var dirV = formationMainData.directionBottomToTop ? 1 : -1;

                posInSquad = new float2(
                    (index / formationMainData.heightUnitsCount),
                    (index % formationMainData.heightUnitsCount)
                );

                int maxX = formationData.count / formationMainData.heightUnitsCount;
                if (maxX == 0) maxX = 1;

                var pos = new float2(
                    (posInSquad.x * formationMainData.horisontalSpacing + rnd.NextFloat(-randRange, randRange)) * scale,
                    (posInSquad.y * formationMainData.verticalSpacing   + rnd.NextFloat(-randRange, randRange)) * scale
                );

                var shiftX = pos.y * shiftSC.x;
                var shiftY = pos.x * shiftSC.y;

                pos.x += shiftX * scale;
                pos.y += shiftY * scale;

                // rotate point
                var rotPoint = new float2();
                rotPoint.x = pos.x * rotSC.x - pos.y * rotSC.y;
                rotPoint.y = pos.x * rotSC.y + pos.y * rotSC.x;

                // translate point to center
                pos.x = dirH * rotPoint.x + formationMainData.formationCenter.x;
                pos.y = dirV * rotPoint.y + formationMainData.formationCenter.y;

                return pos;
            case FormationType.CIRCLE:
                return new float2();
            default:
                return new float2();
        }
    }
    
    protected override void OnUpdate()
    {
        var query = GetEntityQuery(
            typeof(SquadTagSharedComponentData),
            typeof(SquadComponentData), 
            typeof(LinearMovementComponentData),
            ComponentType.Exclude<SequenceMovementSharedComponentData>()
            );

        var sharedIndices = new NativeHashMap<int, int>(query.CalculateChunkCount(), Allocator.TempJob);

        new SharedIndicesJobChunk()
        {
            sharedIndices = sharedIndices.AsParallelWriter(),
            squadTagType = GetArchetypeChunkSharedComponentType<SquadTagSharedComponentData>()
        }.Schedule(query).Complete();

        var indices = sharedIndices.GetKeyArray(Allocator.TempJob);
        var sharedDatas = new NativeHashMap<int, SquadFormationData>(indices.Length, Allocator.TempJob);
        
        for (int i = 0; i < indices.Length; i++)
        {
            var sharedData = EntityManager.GetSharedComponentData<SquadTagSharedComponentData>(indices[i]);
            sharedDatas.TryAdd(indices[i], new SquadFormationData()
            {
                mainData = sharedData.data,
                count = sharedData.unitCount != null ? sharedData.unitCount.value : 0
            });
        }

        new SetPositionJobChunk()
        {
            squadTagType = GetArchetypeChunkSharedComponentType<SquadTagSharedComponentData>(),
            squadTagMap = sharedDatas,
            rnd = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, int.MaxValue)),
            moveType = GetArchetypeChunkComponentType<LinearMovementComponentData>(),
            scaleType = GetArchetypeChunkComponentType<Scale>(true),
            squadType = GetArchetypeChunkComponentType<SquadComponentData>()
        }.Schedule(query).Complete();

        indices.Dispose();
        sharedIndices.Dispose();
        sharedDatas.Dispose();
    }
}