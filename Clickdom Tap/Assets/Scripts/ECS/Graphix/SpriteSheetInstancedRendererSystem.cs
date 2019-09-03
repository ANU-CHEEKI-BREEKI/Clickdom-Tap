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

public struct RenderData
{
    public float3 position;
    public Matrix4x4 matrix;
    public Vector4 uv;
}

[ExcludeComponent(typeof(Scale))]
[BurstCompile]
public struct CullAndSliceEntitiesJob : IJobForEach<Translation, SpriteSheetAnimationComponentData>
{
    [ReadOnly] public float minX;
    [ReadOnly] public float maxX;
    /// <summary>
    /// сверху вних (в порядке убывания)
    /// </summary>
    [ReadOnly] public NativeArray<float> ySlice;
    /// <summary>
    /// сверху вних (в порядке убывания)
    /// </summary>
    public NativeMultiHashMap<int, RenderData>.ParallelWriter slicedQueues;

    public void Execute([ReadOnly] ref Translation translation, [ReadOnly] ref SpriteSheetAnimationComponentData animationData)
    {
        SpriteSheetInstancedRendererSystem.Slice(ref ySlice, ref slicedQueues, ref translation, ref animationData, 1, minX, maxX);
    }
}

[BurstCompile]
public struct CullAndSliceScaledEntitiesJob : IJobForEach<Translation, SpriteSheetAnimationComponentData, Scale>
{
    [ReadOnly] public float minX;
    [ReadOnly] public float maxX;
    /// <summary>
    /// сверху вних (в порядке убывания)
    /// </summary>
    [ReadOnly] public NativeArray<float> ySlice;
    /// <summary>
    /// сверху вних (в порядке убывания)
    /// </summary>
    public NativeMultiHashMap<int, RenderData>.ParallelWriter slicedQueues;

    public void Execute([ReadOnly] ref Translation translation, [ReadOnly] ref SpriteSheetAnimationComponentData animationData, [ReadOnly] ref Scale scale)
    {
        SpriteSheetInstancedRendererSystem.Slice(ref ySlice, ref slicedQueues, ref translation, ref animationData, scale.Value, minX, maxX);
    }
}
[BurstCompile]
public struct MultiHashToQueueJob : IJob
{
    public NativeQueue<RenderData> queue;
    [ReadOnly] public NativeMultiHashMap<int, RenderData> map;
    [ReadOnly] public int key;

    public void Execute()
    {
        RenderData rdata;
        NativeMultiHashMapIterator<int> iterator;
        if (map.TryGetFirstValue(key, out rdata, out iterator))
        {
            queue.Enqueue(rdata);
            while (map.TryGetNextValue(out rdata, ref iterator))
                queue.Enqueue(rdata);
        }
    }
}

[BurstCompile]
public struct QueueToArrayJob : IJob
{
    public NativeArray<RenderData> array;
    public NativeQueue<RenderData> queue;

    public void Execute()
    {
        int index = 0;
        int arrayLength = array.Length;
        RenderData rdata;
        while(index < arrayLength && queue.TryDequeue(out rdata))
        {
            array[index] = rdata;
            index++;
        }
    }
}

[BurstCompile]
public struct SwapSowrByPositionJob : IJob
{
    public NativeArray<RenderData> sortArray;

    public void Execute()
    {
        var cnt = sortArray.Length;
        for (int i = 0; i < cnt; i++)
        {
            for (int j = i + 1; j < cnt; j++)
            {
                if (sortArray[i].position.y < sortArray[j].position.y)
                {
                    var tmp = sortArray[i];
                    sortArray[i] = sortArray[j];
                    sortArray[j] = tmp;
                }
            }
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
    
    public void Execute(int index)
    {
        var rdata = sourceArray[index];
        matrices[startIndex + index] = rdata.matrix;
        uvs[startIndex + index] = rdata.uv;
    }
}

[UpdateAfter(typeof(SpriteSheetAnimationSystem))]
public class SpriteSheetInstancedRendererSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var entities = GetEntityQuery(typeof(Translation), typeof(SpriteSheetAnimationComponentData));
        var entitiesArray = entities.ToComponentDataArray<Translation>(Allocator.TempJob);
        var entitiesCount = entitiesArray.Length;
        entitiesArray.Dispose();

        //распределить entity на списки, по расположению на экране (зоны экрана, если его разбить горизонтальными прямыми линиями)
        var camera = Camera.main;
        var cameraPosition = camera.transform.position;
        var camHeight = camera.orthographicSize;
        var camWidth = camHeight * camera.aspect;
        var maxX = cameraPosition.x + camWidth;
        var minX = cameraPosition.x - camWidth;
        var sliceCount = math.max(9, entitiesCount / 500);
        var ySliceSize = camHeight * 2 / sliceCount;

        var ySlices = new NativeArray<float>(sliceCount + 1, Allocator.TempJob);
        for (int i = 0; i < ySlices.Length; i++)
            ySlices[i] = cameraPosition.y + camHeight - ySliceSize * i;

        var slicedMultihashConcurent = new NativeMultiHashMap<int, RenderData>(entitiesCount, Allocator.TempJob);
        var sliceJob = new CullAndSliceEntitiesJob()
        {
            maxX = maxX,
            minX = minX,
            ySlice = ySlices,
            slicedQueues = slicedMultihashConcurent.AsParallelWriter()
        };
        var sjobHandle = sliceJob.Schedule(this);
        var sliceScaledJob = new CullAndSliceScaledEntitiesJob()
        {
            maxX = maxX,
            minX = minX,
            ySlice = ySlices,
            slicedQueues = slicedMultihashConcurent.AsParallelWriter()
        };
        var ssjobHandle = sliceScaledJob.Schedule(this, sjobHandle);
        ssjobHandle.Complete();

        //теперь надо получить их словаря всё чо надо
        var slicedQueues = new NativeQueue<RenderData>[sliceCount];
        var jhandles = new NativeArray<JobHandle>(sliceCount, Allocator.TempJob);
        for (int i = 0; i < sliceCount; i++)
        {
            slicedQueues[i] = new NativeQueue<RenderData>(Allocator.TempJob);
            var multihash2queue = new MultiHashToQueueJob()
            {
                queue = slicedQueues[i],
                key = i,
                map = slicedMultihashConcurent
            };
            jhandles[i] = multihash2queue.Schedule();
        }
        JobHandle.CompleteAll(jhandles);

        //списки перевести в массивы, чтобы можно было сортировать
        var slicedArrays = new NativeArray<RenderData>[sliceCount];
        for (int i = 0; i < sliceCount; i++)
        {
            slicedArrays[i] = new NativeArray<RenderData>(slicedQueues[i].Count, Allocator.TempJob);
            var queue2array = new QueueToArrayJob()
            {
                array = slicedArrays[i],
                queue = slicedQueues[i]
            };
            jhandles[i] = queue2array.Schedule();
        }
        JobHandle.CompleteAll(jhandles);

        //сортировка всех массивов паралельно
        int fullVisibleCount = 0;
        for (int i = 0; i < sliceCount; i++)
        {
            var swapSortJob = new SwapSowrByPositionJob()
            {
                sortArray = slicedArrays[i]
            };
            jhandles[i] = swapSortJob.Schedule();
            fullVisibleCount += slicedArrays[i].Length;
        }
        JobHandle.CompleteAll(jhandles);

        //слитие массивов в один большой
        var matrices = new NativeArray<Matrix4x4>(fullVisibleCount, Allocator.TempJob);
        var uvs = new NativeArray<Vector4>(fullVisibleCount, Allocator.TempJob);

        int startIndex = 0;
        for (int i = 0; i < sliceCount; i++)
        {
            var mergejob = new MergeArraysParallelJob()
            {
                sourceArray = slicedArrays[i],
                startIndex = startIndex,
                matrices = matrices,
                uvs = uvs
            };
            jhandles[i] = mergejob.Schedule(slicedArrays[i].Length, 10);
            startIndex += slicedArrays[i].Length;
        }
        JobHandle.CompleteAll(jhandles);

        //драв колы по 1023 ентити за раз
        var mpb = new MaterialPropertyBlock();
        const int drawCallSize = 1023;
        var mesh = EntitySpavner.Instance.quadMesh;
        var material = EntitySpavner.Instance.animatedMeterial;
        var matricesInstanced = new Matrix4x4[drawCallSize];
        var uvsInstanced = new Vector4[drawCallSize];
        int materialPropId = Shader.PropertyToID("_MainTex_UV");
        int drawnCount = 0;
        while (drawnCount < fullVisibleCount)
        {
            var callSize = math.min(drawCallSize, fullVisibleCount - drawnCount);

            NativeArray<Matrix4x4>.Copy(matrices, drawnCount, matricesInstanced, 0, callSize);
            NativeArray<Vector4>.Copy(uvs, drawnCount, uvsInstanced, 0, callSize);
            mpb.SetVectorArray(materialPropId, uvsInstanced);

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

        //диспос всему что не диспоснуто ещё
        for (int i = 0; i < sliceCount; i++)
        {
            slicedQueues[i].Dispose();
            slicedArrays[i].Dispose();
        }
        slicedMultihashConcurent.Dispose();
        ySlices.Dispose();
        jhandles.Dispose();
        matrices.Dispose();
        uvs.Dispose();
    }

    static public void Slice(ref NativeArray<float> ySlice, ref NativeMultiHashMap<int, RenderData>.ParallelWriter slicedQueues, ref Translation translation, ref SpriteSheetAnimationComponentData animationData, float scale, float minX, float maxX)
    {
        if (ySlice == null && ySlice.Length < 2) return;

        var sliceCount = ySlice.Length - 1;
        var pos = translation.Value;
        if (pos.x < minX || pos.x > maxX || pos.y > ySlice[0] || pos.y < ySlice[sliceCount]) return;

        //чтобы рендерить слоями
        pos.z = pos.y * 0.01f;

        var rdata = new RenderData()
        {
            position = pos,
            uv = animationData.uv,
            matrix = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * scale)
        };

        for (int slice = 0; slice < sliceCount; slice++)
        {
            if (pos.y > ySlice[slice + 1])
            {
                slicedQueues.Add(slice, rdata);
                break;
            }
        }
    }
}