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

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateAfter(typeof(AnimationPauserSystem))]
public class AnimationResumerSystem : JobComponentSystem
{
    [BurstCompile]
    public struct ResumeJob : IJobForEach<AnimationPauseComponentData, SpriteSheetAnimationComponentData>
    {
        [ReadOnly] public float deltaTime;

        public void Execute(ref AnimationPauseComponentData pause, ref SpriteSheetAnimationComponentData animation)
        {
            pause.timerToResume -= deltaTime;
            if (pause.timerToResume <= 0)
            {
                animation.pause = false;
                pause.timerToResume = 0;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return new ResumeJob()
        {
            deltaTime = Time.deltaTime
        }.Schedule(this, inputDeps);
    }
}