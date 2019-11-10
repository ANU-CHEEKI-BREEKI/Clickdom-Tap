using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct IncreaceProgressByAnimationEventTagComponentData : IComponentData { }

public class WholeProgressIncreacerByAnimationAction : ComponentSystem
{
    public struct IncreaceProgressData
    {
        public float damage;
        public float3 position;
    }

    [BurstCompile]
    [RequireComponentTag(typeof(SwordsmanTagComponentData), typeof(IncreaceProgressByAnimationEventTagComponentData))]
    public struct DetectAnimaionActionJob : IJobForEach<ActionOnAnimationFrameComponentData, SpriteSheetAnimationComponentData, FactionComponentData, MeleeAttackComponentData, Translation>
    {
        public NativeQueue<IncreaceProgressData>.ParallelWriter detectedActionEntities;

        public void Execute([ReadOnly] ref ActionOnAnimationFrameComponentData action, [ReadOnly] ref SpriteSheetAnimationComponentData animation, [ReadOnly] ref FactionComponentData faction, [ReadOnly] ref MeleeAttackComponentData attack, [ReadOnly] ref Translation pos)
        {
            if (faction.value != FactionComponentData.Faction.ALLY)
                return;

            if (!action.needAction)
                return;

            if (animation.currentFrame == action.actionData.frame && animation.out_FrameChangedEventFlag)
                detectedActionEntities.Enqueue(new IncreaceProgressData()
                {
                    position = pos.Value,
                    damage = attack.damage
                });
        }
    }

    private WholeProgressHandle progress;
    private bool itialized = false;

    public void Init(WholeProgressHandle progress)
    {
        this.progress = progress;

        itialized = true;
    }

    protected override void OnUpdate()
    {
        if (!itialized)
            return;

        var detectedActionEntities = new NativeQueue<IncreaceProgressData>(Allocator.TempJob);
        new DetectAnimaionActionJob()
        {
            detectedActionEntities = detectedActionEntities.AsParallelWriter()
        }.Schedule(this).Complete();

        IncreaceProgressData data;
        while(detectedActionEntities.TryDequeue(out data))
            progress?.IncreaceProgressInPlace(data.damage, data.position);

        detectedActionEntities.Dispose();
    }
}