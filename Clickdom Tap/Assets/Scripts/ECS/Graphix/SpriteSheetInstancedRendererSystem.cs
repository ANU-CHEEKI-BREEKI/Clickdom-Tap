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

[BurstCompile]
public struct MultiHashToQueueJob<TKey, TVal> : IJob where TKey : struct, IEquatable<TKey> where TVal : struct
{
    public NativeQueue<TVal> queue;
    [ReadOnly] public NativeMultiHashMap<TKey, TVal> map;
    [ReadOnly] public TKey key;

    public void Execute()
    {
        TVal rdata;
        NativeMultiHashMapIterator<TKey> iterator;
        if (map.TryGetFirstValue(key, out rdata, out iterator))
        {
            queue.Enqueue(rdata);
            while (map.TryGetNextValue(out rdata, ref iterator))
                queue.Enqueue(rdata);
        }
    }
}

[BurstCompile]
public struct QueueToArrayJob<T> : IJob where T : struct
{
    public NativeArray<T> array;
    public NativeQueue<T> queue;

    public void Execute()
    {
        int index = 0;
        int arrayLength = array.Length;
        T rdata;
        while (index < arrayLength && queue.TryDequeue(out rdata))
        {
            array[index] = rdata;
            index++;
        }
    }
}

[UpdateAfter(typeof(SpriteSheetAnimationSystem))]
public class SpriteSheetInstancedRendererSystem : ComponentSystem
{
    public struct RenderData : IComparable<RenderData>
    {
        public float3 position;
        public Matrix4x4 matrix;
        public Vector4 uv;
        public float2 renderScale;

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

        [ReadOnly] public NativeArray<float> ySlice;

        public NativeMultiHashMap<int, RenderData>.ParallelWriter chunkDataMap;
        public NativeHashMap<int, ArchetypeChunk>.ParallelWriter chunkMap;

        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        [ReadOnly] public ArchetypeChunkComponentType<SpriteSheetAnimationComponentData> animationType;
        [ReadOnly] public ArchetypeChunkComponentType<RenderScaleComponentdata> renderScaleType;
        [ReadOnly] public ArchetypeChunkComponentType<Scale> scaleType;
        [ReadOnly] public ArchetypeChunkComponentType<Rotation> rotationType;
        [ReadOnly] public ArchetypeChunkSharedComponentType<RenderSharedComponentData> renderDataType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var positions = chunk.GetNativeArray(translationType);
            var animations = chunk.GetNativeArray(animationType);
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

            var sharedIndex = chunk.GetSharedComponentIndex(renderDataType);

            for (int i = 0; i < cnt; i++)
            {
                var pos = positions[i].Value;
                if (pos.x < minX || pos.x > maxX || pos.y > ySlice[0] || pos.y < ySlice[sliceCount]) continue;

                var anim = animations[i];
                var renderScale = float2.zero;
                if (hasRenderScale) renderScale = renderScales[i].value;
                else renderScale = new float2(1, 1);
                var scale = 1f;
                if (hasScale) scale = scales[i].Value;
                var rotation = quaternion.identity;
                if (hasRotation) rotation = rotations[i].Value;

                pos.z = pos.y * 0.01f;
                var rdata = new RenderData()
                {
                    position = pos,
                    uv = anim.uv,
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

        public void Execute(int index)
        {
            var rdata = sourceArray[index];
            matrices[startIndex + index] = rdata.matrix;
            uvs[startIndex + index] = rdata.uv;
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

    /// <summary>
    /// БОльшие элементы будут первыми в массиве
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [BurstCompile]
    public struct DecreacingQuickSortRecursivelyJob<T> : IJob where T : struct, IComparable<T>
    {
        public NativeArray<T> sortArray;

        public void Execute()
        {
            QSort(ref sortArray, 0, sortArray.Length - 1);
        }

        void QSort(ref NativeArray<T> arr, int low, int high)
        {
            if (low < high)
            {

                var pi = PartitionLomuto(ref arr, low, high);
                //var pi = PartitionHoar(ref arr, low, high);
                QSort(ref arr, low, pi - 1);
                QSort(ref arr, pi + 1, high);
            }
        }

        /// <summary>
        /// Разбиение Ломуто
        /// </summary>
        /// <returns></returns>
        int PartitionLomuto(ref NativeArray<T> arr, int low, int high)
        {
            T pivot = arr[high];

            int i = low - 1;
            for (int j = low; j <= high - 1; j++)
            {
                if (arr[j].CompareTo(pivot) >= 0)
                {
                    i++;
                    Swap(ref arr, i, j);
                }
            }
            Swap(ref arr, i + 1, high);
            return i + 1;
        }

        /// <summary>
        /// Разбиение Хоара
        /// </summary>
        /// <returns></returns>
        int PartitionHoar(ref NativeArray<T> arr, int low, int high)
        {
            T pivot = arr[(low + high) / 2];
            var i = low - 1;
            var j = high + 1;
            while (true)
            {
                do i++; while (arr[i].CompareTo(pivot) > 0);
                do j--; while (arr[j].CompareTo(pivot) < 0);
                if (i >= j) return j;
                Swap(ref arr, i, j);
            }
        }

        void Swap(ref NativeArray<T> arr, int i, int j)
        {
            T temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }
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
            Slice(ref ySlice, ref slicedQueues, ref translation, ref animationData, 1, minX, maxX);
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
            Slice(ref ySlice, ref slicedQueues, ref translation, ref animationData, scale.Value, minX, maxX);
        }
    }

    [BurstCompile]
    public struct CullAndSliceJob : IJob
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
        public NativeMultiHashMap<int, RenderData>.ParallelWriter slicedData;

        [ReadOnly] public int chunkKey;
        [ReadOnly] public NativeMultiHashMap<int, RenderData> chunkMap;

        public void Execute()
        {
            RenderData data;
            NativeMultiHashMapIterator<int> iterator;
            if (chunkMap.TryGetFirstValue(chunkKey, out data, out iterator))
            {
                do
                {
                    Slice(ref ySlice, ref slicedData, ref data, minX, maxX);
                } while (chunkMap.TryGetNextValue(out data, ref iterator));
            }

        }
    }

    EntityManager manager;
    NativeQueue<RenderData>[] slicedQueues;
    NativeArray<RenderData>[] slicedArrays;
    const int sliceCount = 20;

    protected override void OnCreate()
    {
        base.OnCreate();
        manager = EntityManager;

        slicedQueues = new NativeQueue<RenderData>[sliceCount];
        slicedArrays = new NativeArray<RenderData>[sliceCount];
    }

    protected override void OnUpdate()
    {
        var camera = Camera.main;
        var cameraPosition = camera.transform.position;
        var camHeight = camera.orthographicSize;
        var camWidth = camHeight * camera.aspect;
        var maxX = cameraPosition.x + camWidth;
        var minX = cameraPosition.x - camWidth;

        //var sliceCount = math.max(9, entitiesCount / 500);
        var ySliceSize = camHeight * 2 / sliceCount;
        var ySlices = new NativeArray<float>(sliceCount + 1, Allocator.TempJob);
        for (int i = 0; i < ySlices.Length; i++)
            ySlices[i] = cameraPosition.y + camHeight - ySliceSize * i;

        var query = GetEntityQuery(ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<SpriteSheetAnimationComponentData>(), ComponentType.ReadOnly<RenderSharedComponentData>());
        var entitiesCount = query.CalculateEntityCount();
        var chunkCnt = query.CalculateChunkCount();

        var chunkDataMap = new NativeMultiHashMap<int, RenderData>(entitiesCount, Allocator.TempJob);
        var chunkMap = new NativeHashMap<int, ArchetypeChunk>(chunkCnt, Allocator.TempJob);
        var chunkJob = new SortByChunkIdAndCalcMatrixJobAndCull()
        {
            maxX = maxX,
            minX = minX,
            ySlice = ySlices,
            chunkDataMap = chunkDataMap.AsParallelWriter(),
            chunkMap = chunkMap.AsParallelWriter(),
            animationType = GetArchetypeChunkComponentType<SpriteSheetAnimationComponentData>(true),
            translationType = GetArchetypeChunkComponentType<Translation>(true),
            renderScaleType = GetArchetypeChunkComponentType<RenderScaleComponentdata>(true),
            scaleType = GetArchetypeChunkComponentType<Scale>(true),
            renderDataType = GetArchetypeChunkSharedComponentType<RenderSharedComponentData>(),
            rotationType = GetArchetypeChunkComponentType<Rotation>(true)
        };

        var slicedMultihashConcurent = new NativeMultiHashMap<int, RenderData>(entitiesCount, Allocator.TempJob);

        chunkJob.Schedule(query).Complete();
        var chunksIndices = chunkMap.GetKeyArray(Allocator.TempJob);

        for (int j = 0; j < chunksIndices.Length; j++)
        {
            slicedMultihashConcurent.Clear();

            ArchetypeChunk chunk;
            if (!chunkMap.TryGetValue(chunksIndices[j], out chunk)) continue;

            new CullAndSliceJob()
            {
                maxX = maxX,
                minX = minX,
                ySlice = ySlices,
                slicedData = slicedMultihashConcurent.AsParallelWriter(),
                chunkKey = chunksIndices[j],
                chunkMap = chunkDataMap
            }.Schedule().Complete();

            //теперь надо получить их словаря всё чо надо
            var jhandles = new NativeArray<JobHandle>(sliceCount, Allocator.TempJob);
            for (int i = 0; i < sliceCount; i++)
            {
                slicedQueues[i] = new NativeQueue<RenderData>(Allocator.TempJob);
                var multihash2queue = new MultiHashToQueueJob<int, RenderData>()
                {
                    queue = slicedQueues[i],
                    key = i,
                    map = slicedMultihashConcurent
                };
                jhandles[i] = multihash2queue.Schedule();
            }
            JobHandle.CompleteAll(jhandles);

            //списки перевести в массивы, чтобы можно было сортировать            
            for (int i = 0; i < sliceCount; i++)
            {
                slicedArrays[i] = new NativeArray<RenderData>(slicedQueues[i].Count, Allocator.TempJob);
                var queue2array = new QueueToArrayJob<RenderData>()
                {
                    array = slicedArrays[i],
                    queue = slicedQueues[i]
                };
                jhandles[i] = queue2array.Schedule();
            }
            JobHandle.CompleteAll(jhandles);

            //сортировка всех массивов паралельно
            int fullVisibleCount = 0;
            //for (int i = 0; i < sliceCount; i++)
            //{
            //    var swapSortJob = new SwapSowrByPositionJob()
            //    {
            //        sortArray = slicedArrays[i]
            //    };
            //    jhandles[i] = swapSortJob.Schedule();
            //    fullVisibleCount += slicedArrays[i].Length;
            //}
            for (int i = 0; i < sliceCount; i++)
            {
                var swapSortJob = new DecreacingQuickSortRecursivelyJob<RenderData>()
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

            //var mesh = EntitySpavner.Instance.quadMesh;
            //var material = EntitySpavner.Instance.animatedMeterial;
            var sharedData = manager.GetSharedComponentData<RenderSharedComponentData>(chunksIndices[j]);
            var mesh = sharedData.mesh;
            var material = sharedData.material;

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
            jhandles.Dispose();
            matrices.Dispose();
            uvs.Dispose();
        }

        chunksIndices.Dispose();
        chunkMap.Dispose();
        chunkDataMap.Dispose();
        slicedMultihashConcurent.Dispose();
        ySlices.Dispose();
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

    static public void Slice(ref NativeArray<float> ySlice, ref NativeMultiHashMap<int, RenderData>.ParallelWriter slicedData, ref RenderData data, float minX, float maxX)
    {
        if (ySlice == null && ySlice.Length < 2) return;

        var sliceCount = ySlice.Length - 1;
        var pos = data.position;
        if (pos.x < minX || pos.x > maxX || pos.y > ySlice[0] || pos.y < ySlice[sliceCount]) return;

        for (int slice = 0; slice < sliceCount; slice++)
        {
            if (pos.y > ySlice[slice + 1])
            {
                slicedData.Add(slice, data);
                break;
            }
        }
    }
}