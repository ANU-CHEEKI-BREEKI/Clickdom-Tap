using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

public class ASpawnerShadowCastUpdater : MonoBehaviour
{
    private CastShadowsUpdateSystem castShdowApdater;
    private CastShadowShiftSystem shiftShadows;

    private void Init()
    {
        if (castShdowApdater == null)
            castShdowApdater = World.Active.GetOrCreateSystem<CastShadowsUpdateSystem>();
        if (shiftShadows == null)
            shiftShadows = World.Active.GetOrCreateSystem<CastShadowShiftSystem>();        
    }

    public void ApdateCastShadows(bool castSpriteShadows)
    {
        var spawners = FindObjectsOfType<ASpawner>();
        foreach (var spawner in spawners)
            spawner.castSpriteShadows = castSpriteShadows;

        Init();

        castShdowApdater.CastSoldiersSpriteShadows = castSpriteShadows;
        shiftShadows.Enabled = castSpriteShadows;
    }

    public void UpdateCastArrowsShadows(bool castArrowsShadows)
    {
        var spawners = FindObjectsOfType<ArcherSpawner>();
        foreach (var spawner in spawners)
        {
            spawner.LaunchArrowData.data.castShadows = castArrowsShadows;
            spawner.LaunchArrowData.data.calcShadowsShifts = castArrowsShadows;
        }

        Debug.Log("UpdateCastArrowsShadows");
    }
}



[UpdateBefore(typeof(CastProjectileShadowsSystems))]
[UpdateBefore(typeof(CastShadowShiftSystem))]
[UpdateBefore(typeof(ARendererCollectorSystem))]
public class CastShadowsUpdateSystem : JobComponentSystem
{
    private bool changedCastSoldiersSpriteShadows = false;
    private bool castSoldiersSpriteShadows = true;
    public bool CastSoldiersSpriteShadows
    {
        get => castSoldiersSpriteShadows;
        set
        {
            changedCastSoldiersSpriteShadows = castSoldiersSpriteShadows != value;
            castSoldiersSpriteShadows = value;
        }
    }

    private EntityQuery query;

    protected override void OnCreate()
    {
        base.OnCreate();

        query = GetEntityQuery(new EntityQueryDesc()
        {
            All = new[] {
                ComponentType.ReadWrite<CastSpritesShadowComponentData>()
            },
            Any = new[]
            {
                ComponentType.ReadOnly<ArcherTagComponentData>(),
                ComponentType.ReadOnly<SwordsmanTagComponentData>()
            }
        });
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var retHandle = inputDeps;

        if (changedCastSoldiersSpriteShadows)
        {
            changedCastSoldiersSpriteShadows = false;
            retHandle = new SetCastSpriteShadowsJob()
            {
                shadowType = GetArchetypeChunkComponentType<CastSpritesShadowComponentData>(false),
                cast = CastSoldiersSpriteShadows
            }.Schedule(query, retHandle);
        }       
        return retHandle;
    }

    [BurstCompile]
    public struct SetCastSpriteShadowsJob : IJobChunk
    {
        public ArchetypeChunkComponentType<CastSpritesShadowComponentData> shadowType;

        [ReadOnly] public bool cast;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var castDatas = chunk.GetNativeArray(shadowType);
            for (int i = 0; i < chunk.Count; i++)
            {
                var data = castDatas[i];               
                data.disableCastShadow = !cast;
                castDatas[i] = data;
            }
        }
    }
}