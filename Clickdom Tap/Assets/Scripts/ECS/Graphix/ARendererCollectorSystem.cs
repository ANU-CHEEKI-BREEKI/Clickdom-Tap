using ANU.Utils;
using System;
using System.Linq;
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

public struct DisableRenderCullingTagComponentData : IComponentData { }

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

[Serializable]
public struct CastSpritesShadowComponentData : IComponentData
{
    public bool flipRotationByX;
    public bool flipRotationByY;

    public float2 scale;
    /// <summary>
    /// multiplied color tint
    /// </summary>
    public Color color;
    /// <summary>
    /// сдвиг позиции тени в процентах от масштаба оригинального обьекта(positionPercentOffset > 0)
    /// </summary>
    public float3 positionPercentOffset;
    public float3 positionUnitsOffset;
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
        [ReadOnly] public ArchetypeChunkComponentType<CastSpritesShadowComponentData> shadowType;
        [ReadOnly] public ArchetypeChunkSharedComponentType<RenderSharedComponentData> renderDataType;

        [ReadOnly] public ArchetypeChunkComponentType<DisableRenderCullingTagComponentData> disableCullingTagType;

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

            var shadows = new NativeArray<CastSpritesShadowComponentData>();
            var hasShadows = chunk.Has(shadowType);
            if(hasShadows)
                shadows = chunk.GetNativeArray(shadowType);

            var culling = !chunk.Has(disableCullingTagType);

            var sharedIndex = chunk.GetSharedComponentIndex(renderDataType);

            for (int i = 0; i < cnt; i++)
            {
                var pos = positions[i].Value;

                if (culling)
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

                if (color.w == 0)
                    continue;

                var actualRenderScale = new Vector3(renderScale.x, renderScale.y, 1) * scale;

                if (actualRenderScale.x == 0 || actualRenderScale.y == 0)
                    continue;

                var actualPivot = sprite.pivot;
                if (!sprite.usePivot)
                    actualPivot = Vector2.one / 2;
                actualPivot -= Vector2.one / 2;

                var actualPosition = pos;
                var pivotedPosition = pos;

                pivotedPosition.x = -actualPivot.x * actualRenderScale.x;
                pivotedPosition.y = -actualPivot.y * actualRenderScale.y;

                var translateMatrix = Matrix4x4.Translate(actualPosition);
                var rorateMatrix = Matrix4x4.Rotate(rotation);
                var pivotedTranslateMatrix = Matrix4x4.Translate(pivotedPosition);
                var scaleMatrix = Matrix4x4.Scale(actualRenderScale);

                var rdata = new RenderData()
                {
                    position = pos,
                    uv = sprite.uv,
                    color = color,
                    matrix = translateMatrix * rorateMatrix * pivotedTranslateMatrix * scaleMatrix
                };
                chunkDataMap.Add(sharedIndex, rdata);

                if(hasShadows)
                {
                    var shadowData = shadows[i];

                    if (shadowData.scale.x == 0 || shadowData.scale.y == 0 || shadowData.color.a == 0)
                        continue;

                    var shadowOffsettedPosition = 
                        actualPosition + 
                        shadowData.positionPercentOffset * actualRenderScale +
                        shadowData.positionUnitsOffset;

                    var shadowScale = actualRenderScale;
                    shadowScale.x *= shadowData.scale.x;
                    shadowScale.y *= shadowData.scale.y;

                    var shadowColor = color * shadowData.color;

                    var shadowRotation = rotation;
                    if (shadowData.flipRotationByX)
                    {
                        Quaternion q = shadowRotation;
                        var angles = q.eulerAngles;
                        angles.z = 180 - angles.z;
                        shadowRotation = Quaternion.Euler(angles);
                    }
                    if (shadowData.flipRotationByY)
                    {
                        Quaternion q = shadowRotation;
                        var angles = q.eulerAngles;
                        angles.z = 360 - angles.z;
                        shadowRotation = Quaternion.Euler(angles);
                    }

                    var shadowTranslateMatrix = Matrix4x4.Translate(shadowOffsettedPosition);
                    var shadowScaleMatrix = Matrix4x4.Scale(shadowScale);
                    var shadowRotateMatrix = Matrix4x4.Rotate(shadowRotation);

                    var rshadowrdata = new RenderData()
                    {
                        position = shadowOffsettedPosition,
                        uv = sprite.uv,
                        color = shadowColor,
                        matrix = shadowTranslateMatrix * shadowRotateMatrix * pivotedTranslateMatrix * shadowScaleMatrix
                    };
                    chunkDataMap.Add(sharedIndex, rshadowrdata);
                }

            }
        }
    }
           
    protected EntityManager manager;
    public bool UseAsDefault { get; set; } = false;

    private EntityQueryDesc defaultQueryDesc;
    private EntityQueryDesc specialQueryDesc;

    private EntityQueryDesc defaultWithShadowsQueryDesc;
    private EntityQueryDesc specialWithShadowsQueryDesc;

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

    private RenderCullingBounds renderCullingBounds;
    private Camera camera;

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

        var shadowQD = new[] { ComponentType.ReadOnly<CastSpritesShadowComponentData>() };
        defaultWithShadowsQueryDesc = new EntityQueryDesc()
        {
            All = AllDefaultQuery.Concat(shadowQD).ToArray(),
            None = AllNoneQuery
        };
        specialWithShadowsQueryDesc = new EntityQueryDesc()
        {
            All = AllSpecialQuery.Concat(shadowQD).ToArray(),
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
        if(camera == null)
        {
            camera = Camera.main;
            renderCullingBounds = camera.GetComponent<RenderCullingBounds>();
        }

        var maxX = 0f;
        var minX = 0f;
        var maxY = 0f;
        var minY = 0f;

        if (renderCullingBounds != null)
        {
            var bounds = renderCullingBounds.CullingBounds;

            maxX = bounds.xMax;
            minX = bounds.xMin;
            maxY = bounds.yMax;
            minY = bounds.yMin;
        }
        else
        {
            var cameraPosition = camera.transform.position;
            var camHeight = camera.orthographicSize;
            var camWidth = camHeight * camera.aspect;

            var minMaxExpand = 1;//чтобы спрайты, которые входят в сцену из за экрана, появлялиль не внезапно

            maxX = cameraPosition.x + camWidth + minMaxExpand;
            minX = cameraPosition.x - camWidth - minMaxExpand;
            maxY = cameraPosition.y + camHeight + minMaxExpand;
            minY = cameraPosition.y - camHeight - minMaxExpand;
        }

        //Debug.DrawLine(new Vector2(maxX, maxY), new Vector2(minX, minY));
        //Debug.DrawLine(new Vector2(maxX, minY), new Vector2(minX, maxY));

        var queryDesc = UseAsDefault ? defaultQueryDesc : specialQueryDesc;
        var shadowQueryDesc = UseAsDefault ? defaultWithShadowsQueryDesc : specialWithShadowsQueryDesc;

        var query = GetEntityQuery(queryDesc);
        var shadowQuery = GetEntityQuery(shadowQueryDesc);

        var entitiesCount = query.CalculateEntityCount();
        var entitiesWithShadowCount = shadowQuery.CalculateEntityCount();
        //для каждого энтити с тенью надо будет отрисовать и тень,
        //так что для них надо в 2 раза больше памяти выделить
        //и всего выйдет (entitiesCount - entitiesWithShadowCount) + entitiesWithShadowCount * 2.
        var maxCount = Mathf.Max(entitiesCount, entitiesCount + entitiesWithShadowCount);

        chunkDataMap.Dispose();
        chunkDataMap = new NativeMultiHashMap<int, RenderData>(maxCount, Allocator.TempJob);
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
            shadowType = GetArchetypeChunkComponentType<CastSpritesShadowComponentData>(true),
            spriteTintType = GetArchetypeChunkComponentType<SpriteTintComponentData>(true),
            disableCullingTagType = GetArchetypeChunkComponentType<DisableRenderCullingTagComponentData>(true)
        };

        jobHandle = chunkJob.Schedule(query, inputDeps);
        return jobHandle;
    }
}