using ANU.Utils;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static ANU.Utils.Jobs;

public struct UseOnlyDynamicRendererTagComponentData : IComponentData { }
public struct UseOnlyInstancedRendererTagComponentData : IComponentData { }

[Serializable]
public struct SpriteRendererComponentData : IComponentData
{
    public Vector4 uv;
    /// <summary>
    /// (0,0) in left lower corner on sprite rect. x and y values in [0, 1] range.
    /// </summary>
    public Vector2 pivot;
    /// <summary>
    /// if not set then pivot will be used as center (0.5, 0.5)
    /// </summary>
    public bool usePivot;
}

[Serializable]
public struct SpriteTintComponentData : IComponentData
{
    public Vector4 color;
}

public struct RenderScaleComponentdata : IComponentData
{
    public float2 value;
}

public struct RenderSharedComponentData : ISharedComponentData, IEquatable<RenderSharedComponentData>
{
    public Mesh mesh;
    public Material material;

    public bool Equals(RenderSharedComponentData other)
    {
        if (mesh == null && other.mesh != null) return false;
        if (mesh != null && other.mesh == null) return false;
        if (material == null && other.material != null) return false;
        if (material != null && other.material == null) return false;
        if (mesh == null && material == null && other.mesh == null && other.material == null) return false;
        return ReferenceEquals(mesh, other.mesh) && ReferenceEquals(material, other.material);
    }

    public override int GetHashCode()
    {
        return mesh.GetHashCode() * 12 + material.GetHashCode() * 5;
    }
}

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateAfter(typeof(SpriteSheetAnimationSystem))]
public abstract class ARendererCollectorSystem : JobComponentSystem
{
    public struct RenderData : IComparable<RenderData>
    {
        public float3 position;
        public Matrix4x4 matrix;
        public Vector4 uv; 
        public Color color;

        public int CompareTo(RenderData other)
        {
            return position.z.CompareTo(other.position.z);
        }
    }

    [BurstCompile]
    public struct SortByChunkIdAndCalcMatrixJobAndCull : IJobChunk
    {
        [ReadOnly] public float minX;
        [ReadOnly] public float maxX;
        [ReadOnly] public float minY;
        [ReadOnly] public float maxY;

        public NativeMultiHashMap<int, RenderData>.ParallelWriter chunkDataMap;

        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        [ReadOnly] public ArchetypeChunkComponentType<SpriteRendererComponentData> spriteType;
        [ReadOnly] public ArchetypeChunkComponentType<SpriteTintComponentData> spriteTintType;
        [ReadOnly] public ArchetypeChunkComponentType<RenderScaleComponentdata> renderScaleType;
        [ReadOnly] public ArchetypeChunkComponentType<Scale> scaleType;
        [ReadOnly] public ArchetypeChunkComponentType<Rotation> rotationType;
        [ReadOnly] public ArchetypeChunkSharedComponentType<RenderSharedComponentData> renderDataType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var positions = chunk.GetNativeArray(translationType);
            var sprites = chunk.GetNativeArray(spriteType);
            
            var renderScales = new NativeArray<RenderScaleComponentdata>();
            var hasRenderScale = chunk.Has(renderScaleType);
            if (hasRenderScale)
                renderScales = chunk.GetNativeArray(renderScaleType);
            var scales = new NativeArray<Scale>();
            var hasScale = chunk.Has(scaleType);
            if (hasScale)
                scales = chunk.GetNativeArray(scaleType);
            var rotations = new NativeArray<Rotation>();
            var hasRotation = chunk.Has(rotationType);
            if (hasRotation)
                rotations = chunk.GetNativeArray(rotationType);
            var cnt = chunk.Count;

            var tints = new NativeArray<SpriteTintComponentData>();
            var hasTint = chunk.Has(spriteTintType);
            if (hasTint)
                tints = chunk.GetNativeArray(spriteTintType);

            var sharedIndex = chunk.GetSharedComponentIndex(renderDataType);

            for (int i = 0; i < cnt; i++)
            {
                var pos = positions[i].Value;
                if (pos.x < minX || pos.x > maxX || pos.y < minY || pos.y > maxY) continue;

                var sprite = sprites[i];
                var renderScale = float2.zero;
                if (hasRenderScale) renderScale = renderScales[i].value;
                else renderScale = new float2(1, 1);
                var scale = 1f;
                if (hasScale) scale = scales[i].Value;
                var rotation = quaternion.identity;
                if (hasRotation) rotation = rotations[i].Value;

                var color = Vector4.one;
                if (hasTint) color = tints[i].color;

                var actualRenderScale = new Vector3(renderScale.x, renderScale.y, 1) * scale;

                var actualPivot = sprite.pivot;
                if (!sprite.usePivot)
                    actualPivot = Vector2.one / 2;
                actualPivot -= Vector2.one / 2;

                var actualPosition = pos;
                var pivotedPosition = pos;

                pivotedPosition.x = actualPivot.x * actualRenderScale.x;
                pivotedPosition.y = -actualPivot.y * actualRenderScale.y;

                var matrix = Matrix4x4.Translate(actualPosition);
                matrix *= Matrix4x4.Rotate(rotation);
                matrix *= Matrix4x4.Translate(pivotedPosition);
                matrix *= Matrix4x4.Scale(actualRenderScale);

                var rdata = new RenderData()
                {
                    position = pos,
                    uv = sprite.uv,
                    color = color,
                    matrix = matrix//Matrix4x4.TRS(pos, rotation, actualRenderScale)
                };
                chunkDataMap.Add(sharedIndex, rdata);
            }
        }
    }
           
    protected EntityManager manager;
    public bool UseAsDefault { get; set; } = false;

    private EntityQueryDesc defaultQueryDesc;
    private EntityQueryDesc specialQueryDesc;

    protected virtual ComponentType[] AllDefaultQuery { get; } =
        new[]
        {
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<SpriteRendererComponentData>(),
            ComponentType.ReadOnly<RenderSharedComponentData>()
        };
    protected abstract ComponentType[] AllSpecialQuery { get; }
    protected abstract ComponentType[] AllNoneQuery { get; }
    
    public NativeMultiHashMap<int, RenderData> chunkDataMap;
    public JobHandle jobHandle;

    protected sealed override void OnCreate()
    {
        base.OnCreate();

        chunkDataMap = new NativeMultiHashMap<int, RenderData>(0, Allocator.TempJob);

        manager = EntityManager;

        defaultQueryDesc = new EntityQueryDesc()
        {
            All = AllDefaultQuery,
            None = AllNoneQuery
        };
        specialQueryDesc = new EntityQueryDesc()
        {
            All = AllSpecialQuery,
            None = AllNoneQuery
        };
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        chunkDataMap.Dispose();
    }

    protected sealed override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var camera = Camera.main;
        var cameraPosition = camera.transform.position;
        var camHeight = camera.orthographicSize;
        var camWidth = camHeight * camera.aspect;
        var maxX = cameraPosition.x + camWidth;
        var minX = cameraPosition.x - camWidth;
        var maxY = cameraPosition.y + camHeight;
        var minY = cameraPosition.y - camHeight;

        var query = GetEntityQuery(
            UseAsDefault ? defaultQueryDesc : specialQueryDesc
        );

        var entitiesCount = query.CalculateEntityCount();

        chunkDataMap.Dispose();
        chunkDataMap = new NativeMultiHashMap<int, RenderData>(entitiesCount, Allocator.TempJob);
        var chunkJob = new SortByChunkIdAndCalcMatrixJobAndCull()
        {
            maxX = maxX,
            minX = minX,
            maxY = maxY,
            minY = minY,
            chunkDataMap = chunkDataMap.AsParallelWriter(),
            spriteType = GetArchetypeChunkComponentType<SpriteRendererComponentData>(true),
            translationType = GetArchetypeChunkComponentType<Translation>(true),
            renderScaleType = GetArchetypeChunkComponentType<RenderScaleComponentdata>(true),
            scaleType = GetArchetypeChunkComponentType<Scale>(true),
            renderDataType = GetArchetypeChunkSharedComponentType<RenderSharedComponentData>(),
            rotationType = GetArchetypeChunkComponentType<Rotation>(true),
            spriteTintType = GetArchetypeChunkComponentType<SpriteTintComponentData>(true)
        };

        jobHandle = chunkJob.Schedule(query, inputDeps);
        return jobHandle;
    }
}