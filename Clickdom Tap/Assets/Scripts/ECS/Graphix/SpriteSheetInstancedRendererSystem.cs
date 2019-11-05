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
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using static ANU.Utils.Jobs;

[Serializable]
public struct SpriteRendererComponentData : IComponentData
{
    public Vector4 uv;
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
public class SpriteSheetInstancedRendererSystem : ComponentSystem
{
    public struct RenderData : IComparable<RenderData>
    {
        public float3 position;
        public Matrix4x4 matrix;
        public Vector4 uv;
        public Color color;

        public int CompareTo(RenderData other)
        {
            return position.y.CompareTo(other.position.y);
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
        public NativeHashMap<int, ArchetypeChunk>.ParallelWriter chunkMap;

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

                pos.z = pos.y;// * 0.01f;
                var rdata = new RenderData()
                {
                    position = pos,
                    uv = sprite.uv,
                    color = color,
                    matrix = Matrix4x4.TRS(pos, rotation, new Vector3(renderScale.x, renderScale.y, 1) * scale)
                };
                chunkDataMap.Add(sharedIndex, rdata);
                chunkMap.TryAdd(sharedIndex, chunk);
            }
        }
    }

    [BurstCompile]
    public struct MergeArraysParallelJob : IJobParallelFor
    {
        [ReadOnly] public int startIndex;
        [ReadOnly] public NativeArray<RenderData> sourceArray;
        [NativeDisableContainerSafetyRestriction] public NativeArray<Matrix4x4> matrices;
        [NativeDisableContainerSafetyRestriction] public NativeArray<Vector4> uvs;
        [NativeDisableContainerSafetyRestriction] public NativeArray<Vector4> colors;

        public void Execute(int index)
        {
            var rdata = sourceArray[index];
            matrices[startIndex + index] = rdata.matrix;
            uvs[startIndex + index] = rdata.uv;
            colors[startIndex + index] = rdata.color;
        }
    }
           
    EntityManager manager;

    protected override void OnCreate()
    {
        base.OnCreate();
        manager = EntityManager;
    }

    protected override void OnUpdate()
    {
        var camera = Camera.main;
        var cameraPosition = camera.transform.position;
        var camHeight = camera.orthographicSize;
        var camWidth = camHeight * camera.aspect;
        var maxX = cameraPosition.x + camWidth;
        var minX = cameraPosition.x - camWidth;
        var maxY = cameraPosition.y + camHeight;
        var minY = cameraPosition.y - camHeight;

        var query = GetEntityQuery(ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<SpriteRendererComponentData>(), ComponentType.ReadOnly<RenderSharedComponentData>());
        var entitiesCount = query.CalculateEntityCount();
        var chunkCnt = query.CalculateChunkCount();

        var chunkDataMap = new NativeMultiHashMap<int, RenderData>(entitiesCount, Allocator.TempJob);
        var chunkMap = new NativeHashMap<int, ArchetypeChunk>(chunkCnt, Allocator.TempJob);
        var chunkJob = new SortByChunkIdAndCalcMatrixJobAndCull()
        {
            maxX = maxX,
            minX = minX,
            maxY = maxY,
            minY = minY,
            chunkDataMap = chunkDataMap.AsParallelWriter(),
            chunkMap = chunkMap.AsParallelWriter(),
            spriteType = GetArchetypeChunkComponentType<SpriteRendererComponentData>(true),
            translationType = GetArchetypeChunkComponentType<Translation>(true),
            renderScaleType = GetArchetypeChunkComponentType<RenderScaleComponentdata>(true),
            scaleType = GetArchetypeChunkComponentType<Scale>(true),
            renderDataType = GetArchetypeChunkSharedComponentType<RenderSharedComponentData>(),
            rotationType = GetArchetypeChunkComponentType<Rotation>(true),
            spriteTintType = GetArchetypeChunkComponentType<SpriteTintComponentData>(true)
        };

        chunkJob.Schedule(query).Complete();
        var chunksIndices = chunkMap.GetKeyArray(Allocator.TempJob);

        for (int j = 0; j < chunksIndices.Length; j++)
        {
            ArchetypeChunk chunk;
            if (!chunkMap.TryGetValue(chunksIndices[j], out chunk)) continue;

            //теперь надо получить из словаря данные для нужного sharedIndex
            var sharedQueue = new NativeQueue<RenderData>(Allocator.TempJob);
            new MultiHashToQueueJob<int, RenderData>()
            {
                queue = sharedQueue,
                key = chunksIndices[j],
                map = chunkDataMap
            }.Schedule().Complete();
            //списки перевести в массивы, чтобы можно было сортировать            
            var sharedArray = new NativeArray<RenderData>(sharedQueue.Count, Allocator.TempJob);
            new QueueToArrayJob<RenderData>()
            {
                array = sharedArray,
                queue = sharedQueue
            }.Schedule().Complete();

            //сортировка всех массивов паралельно
            int fullVisibleCount = sharedArray.Length;          
            new Jobs.QuickSortRecursivelyJob<RenderData>
            {
                sortArray = sharedArray,
                descending = true
            }.Schedule().Complete();

            //слитие массивов в один большой
            var matrices = new NativeArray<Matrix4x4>(fullVisibleCount, Allocator.TempJob);
            var uvs = new NativeArray<Vector4>(fullVisibleCount, Allocator.TempJob);
            var colors = new NativeArray<Vector4>(fullVisibleCount, Allocator.TempJob);

            new MergeArraysParallelJob()
            {
                sourceArray = sharedArray,
                startIndex = 0,
                matrices = matrices,
                uvs = uvs,
                colors = colors
            }.Schedule(sharedArray.Length, 10).Complete();

            //драв колы по 1023 ентити за раз
            var mpb = new MaterialPropertyBlock();
            const int drawCallSize = 1023;

            var sharedData = manager.GetSharedComponentData<RenderSharedComponentData>(chunksIndices[j]);
            var mesh = sharedData.mesh;
            var material = sharedData.material;

            var matricesInstanced = new Matrix4x4[drawCallSize];
            var uvsInstanced = new Vector4[drawCallSize];
            var colorsInstanced = new Vector4[drawCallSize];

            int uv_MaterialPropId = Shader.PropertyToID("_MainTex_UV");
            int color_MaterialPropId = Shader.PropertyToID("_Color");
            int drawnCount = 0;
            while (drawnCount < fullVisibleCount)
            {
                var callSize = math.min(drawCallSize, fullVisibleCount - drawnCount);

                NativeArray<Matrix4x4>.Copy(matrices, drawnCount, matricesInstanced, 0, callSize);
                NativeArray<Vector4>.Copy(uvs, drawnCount, uvsInstanced, 0, callSize);
                NativeArray<Vector4>.Copy(colors, drawnCount, colorsInstanced, 0, callSize);
                
                mpb.SetVectorArray(uv_MaterialPropId, uvsInstanced);
                mpb.SetVectorArray(color_MaterialPropId, colorsInstanced);

                Graphics.DrawMeshInstanced(
                    mesh,
                    0,
                    material,
                    matricesInstanced,
                    callSize,
                    mpb
                );

                drawnCount += callSize;
            }

            sharedQueue.Dispose();
            sharedArray.Dispose();           
            matrices.Dispose();
            uvs.Dispose();
            colors.Dispose();
        }

        chunksIndices.Dispose();
        chunkMap.Dispose();
        chunkDataMap.Dispose();
    }
}