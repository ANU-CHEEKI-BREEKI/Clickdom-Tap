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
using static ARendererCollectorSystem;

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateAfter(typeof(InstancedRendererCollectorSystem))]
public class SpriteSheetInstancedRendererSystem : ARendererSystem
{    
    [BurstCompile]
    public struct MergeArraysParallelJob : IJobParallelFor
    {
        [ReadOnly] public int startIndex;
        [ReadOnly] public NativeArray<RenderData> sourceArray;
        public NativeArray<Matrix4x4> matrices;
        public NativeArray<Vector4> uvs;
        public NativeArray<Vector4> colors;
        public NativeArray<float> crackAmounts;

        public void Execute(int index)
        {
            var rdata = sourceArray[index];
            matrices[startIndex + index] = rdata.matrix;
            uvs[startIndex + index] = rdata.uv;
            colors[startIndex + index] = rdata.color;
            crackAmounts[startIndex + index] = 1 - rdata.cracksAmount;
        }
    }

    private const int drawCallSize = 1023;
    private Matrix4x4[] matricesInstanced = new Matrix4x4[drawCallSize];
    private Vector4[] uvsInstanced = new Vector4[drawCallSize];
    private Vector4[] colorsInstanced = new Vector4[drawCallSize];
    private float[] cracksInstanced = new float[drawCallSize];

    protected override void OnUpdate()
    {
        InstancedRendererCollectorSystem.Instance.jobHandle.Complete();
        var chunkDataMap = InstancedRendererCollectorSystem.Instance.chunkDataMap;

        var chunksIndices = chunkDataMap.GetUniqueKeys(Allocator.TempJob);
        for (int j = 0; j < chunksIndices.Length; j++)
        {
            var sharedIndex = chunksIndices[j];
            if (!chunkDataMap.ContainsKey(sharedIndex))
                continue;

            //теперь надо получить из словаря данные для нужного sharedIndex     
            var mapCnt = chunkDataMap.CountForKey(sharedIndex);
            var sharedArray = new NativeArray<RenderData>(mapCnt, Allocator.TempJob);
            var m2aHandle = new MultiHashToArrayValueJob<int, RenderData>()
            {
                array = sharedArray,
                map = chunkDataMap,
                key = sharedIndex
            }.Schedule();

            //сортировка всех массивов паралельно
            int fullVisibleCount = mapCnt;
            var sortHandle = new QuickSortRecursivelyJob<RenderData>
            {
                sortArray = sharedArray,
                descending = true
            }.Schedule(m2aHandle);

            //слитие массивов в один большой
            var matrices = new NativeArray<Matrix4x4>(fullVisibleCount, Allocator.TempJob);
            var uvs = new NativeArray<Vector4>(fullVisibleCount, Allocator.TempJob);
            var colors = new NativeArray<Vector4>(fullVisibleCount, Allocator.TempJob);
            var crackAmounts = new NativeArray<float>(fullVisibleCount, Allocator.TempJob);

            new MergeArraysParallelJob()
            {
                sourceArray = sharedArray,
                startIndex = 0,
                matrices = matrices,
                uvs = uvs,
                colors = colors,
                crackAmounts = crackAmounts
            }.Schedule(sharedArray.Length, 10, sortHandle).Complete();

            //драв колы по 1023 ентити за раз
            var sharedData = manager.GetSharedComponentData<RenderSharedComponentData>(sharedIndex);
            var mesh = sharedData.mesh;
            var material = sharedData.material;

            int drawnCount = 0;
            while (drawnCount < fullVisibleCount)
            {
                var callSize = math.min(drawCallSize, fullVisibleCount - drawnCount);

                NativeArray<Matrix4x4>.Copy(matrices, drawnCount, matricesInstanced, 0, callSize);
                NativeArray<Vector4>.Copy(uvs, drawnCount, uvsInstanced, 0, callSize);
                NativeArray<Vector4>.Copy(colors, drawnCount, colorsInstanced, 0, callSize);
                NativeArray<float>.Copy(crackAmounts, drawnCount, cracksInstanced, 0, callSize);

                mpb.SetVectorArray(uv_MaterialPropId, uvsInstanced);
                mpb.SetVectorArray(color_MaterialPropId, colorsInstanced);
                mpb.SetFloatArray(crackDisolve_MaterialPropId, cracksInstanced);

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

            sharedArray.Dispose();
            matrices.Dispose();
            uvs.Dispose();
            colors.Dispose();
            crackAmounts.Dispose();
        }

        chunksIndices.Dispose();
    }
}