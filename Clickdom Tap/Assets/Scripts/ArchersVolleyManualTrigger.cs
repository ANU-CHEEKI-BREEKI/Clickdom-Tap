using UnityEngine;
using System.Collections;
using ANU.Utils;
using UnityEngine.UI;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

[RequireComponent(typeof(ArcherSpawner))]
public class ArchersVolleyManualTrigger : MonoBehaviour
{
    [SerializeField] private GameObject[] buttonLaunch;
    [SerializeField] private CircleProgressBarAutoTimer progressPresenter;

    private ArcherSpawner spawner;

    private void Start()
    {
        spawner = GetComponent<ArcherSpawner>();

        VolleyAnimationResumerSystem.Instance.OnSquadsToResumeChanged += Instance_OnSquadsToResumeChanged;

        try
        {
            if (progressPresenter != null)
            {
                var pauseData = spawner.AnimationProvider.PausesData[(int)AnimationType.SHOOT];
                var duration = pauseData.value.pauseDuration;
                progressPresenter.Duration = duration;
            }
        }
        catch { }

        var temp = new NativeArray<int>(0, Allocator.Temp);
        Instance_OnSquadsToResumeChanged(temp);
        temp.Dispose();
    }

    private void OnDestroy()
    {
        if(VolleyAnimationResumerSystem.Instance != null)
            VolleyAnimationResumerSystem.Instance.OnSquadsToResumeChanged -= Instance_OnSquadsToResumeChanged;
    }

    private void Instance_OnSquadsToResumeChanged(Unity.Collections.NativeArray<int> sharedIndices)
    {
        if (buttonLaunch.Length == 0)
            return;

        var manager = World.Active.EntityManager;
        bool contains = false;
        for (int i = 0; i < sharedIndices.Length; i++)
        {
            var data = manager.GetSharedComponentData<SquadTagSharedComponentData>(sharedIndices[i]);
            if (data.id.value == spawner.SquadId)
            {
                contains = true;
                break;
            }
        }
        foreach (var item in buttonLaunch)
            item.SetActive(contains);
    }

    public void Launch()
    {
        var manager = World.Active.EntityManager;

        var query = manager.CreateEntityQuery(
            typeof(ArcherTagComponentData),
            typeof(AnimationPauseComponentData),
            typeof(SquadTagSharedComponentData)
        );

        var entities = new NativeMultiHashMap<int, Entity>(query.CalculateEntityCount(), Allocator.TempJob);

        new SharedIndicesJob()
        {
            entities = entities.AsParallelWriter(),
            squadTagType = manager.GetArchetypeChunkSharedComponentType<SquadTagSharedComponentData>(),
            entityType = manager.GetArchetypeChunkEntityType()
        }.Schedule(query).Complete();

        var keys = entities.GetUniqueKeys(Allocator.TempJob);

        for (int i = 0; i < keys.Length; i++)
        {
            var squad = manager.GetSharedComponentData<SquadTagSharedComponentData>(keys[i]);
            if (squad.id.value != spawner.SquadId)
                continue;

            entities.IterateForKey(keys[i], (val) =>
            {
                var pause = manager.GetComponentData< AnimationPauseComponentData>(val);

                var spread = pause.pauseData.pauseSpread;
                pause.timerToResume = Random.value * spread;

                manager.SetComponentData(val, pause);
            });

            break;
        }

        keys.Dispose();
        entities.Dispose();
    }

    [BurstCompile]
    private struct SharedIndicesJob : IJobChunk
    {
        [ReadOnly] public ArchetypeChunkEntityType entityType;
        [ReadOnly] public ArchetypeChunkSharedComponentType<SquadTagSharedComponentData> squadTagType;

        public NativeMultiHashMap<int, Entity>.ParallelWriter entities;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            int squadIndex = chunk.GetSharedComponentIndex(squadTagType);

            var es = chunk.GetNativeArray(entityType);

            for (int i = 0; i < es.Length; i++)
                entities.Add(squadIndex, es[i]);
        }
    }

}
