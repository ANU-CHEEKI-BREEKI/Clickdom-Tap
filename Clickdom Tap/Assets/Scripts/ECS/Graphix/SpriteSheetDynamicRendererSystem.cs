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

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateAfter(typeof(DynamicRendererCollectorSystem))]
[DisableAutoCreation]
public class SpriteSheetDynamicRendererSystem : ARendererSystem
{
    private Camera mainCamera;

    protected override void OnUpdate()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        DynamicRendererCollectorSystem.Instance.jobHandle.Complete();
        var chunkDataMap = DynamicRendererCollectorSystem.Instance.chunkDataMap;

        var chunksIndices = chunkDataMap.GetUniqueKeys(Allocator.TempJob);
        for (int j = 0; j < chunksIndices.Length; j++)
        {
            var chunkIndex = chunksIndices[j];
            if (!chunkDataMap.ContainsKey(chunkIndex))
                continue;

            var sharedData = manager.GetSharedComponentData<RenderSharedComponentData>(chunkIndex);
            var mesh = sharedData.mesh;
            var material = sharedData.material;

            chunkDataMap.IterateForKey(chunkIndex, (data) =>
            {
                mpb.SetVector(uv_MaterialPropId, data.uv);
                mpb.SetColor(color_MaterialPropId, data.color);
                mpb.SetFloat(crackDisolve_MaterialPropId, 1 - data.cracksAmount);

                Graphics.DrawMesh(
                    mesh,
                    data.matrix,
                    material,
                    0,
                    mainCamera,
                    0,
                    mpb
                );
            });
        }

        chunksIndices.Dispose();
    }
}