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

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateAfter(typeof(SpriteSheetAnimationSystem))]
[UpdateBefore(typeof(SpriteSheetInstancedRendererSystem))]
public class SpriteSheetDynamicRendererSystem : ComponentSystem
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
           
    private EntityManager manager;
    public static bool DoUpdate { get; set; } = true;

    protected override void OnCreate()
    {
        base.OnCreate();
        manager = EntityManager;
    }

    protected override void OnUpdate()
    {
        if (!DoUpdate)
            return;

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

        int uv_MaterialPropId = Shader.PropertyToID("_MainTex_UV");
        int color_MaterialPropId = Shader.PropertyToID("_Color");
        var mpb = new MaterialPropertyBlock();

        for (int j = 0; j < chunksIndices.Length; j++)
        {
            var chunkIndex = chunksIndices[j];

            ArchetypeChunk chunk;
            if (!chunkMap.TryGetValue(chunkIndex, out chunk)) continue;

            var sharedData = manager.GetSharedComponentData<RenderSharedComponentData>(chunkIndex);
            var mesh = sharedData.mesh;
            var material = sharedData.material;

            chunkDataMap.IterateForKey(chunkIndex, (data)=>
            {
                mpb.SetVector(uv_MaterialPropId, data.uv);
                mpb.SetColor(color_MaterialPropId, data.color);

                Graphics.DrawMesh(
                    mesh,
                    data.matrix,
                    material,
                    0,
                    camera,
                    0,
                    mpb
                );
            }); 
        }

        chunksIndices.Dispose();
        chunkMap.Dispose();
        chunkDataMap.Dispose();
    }
}