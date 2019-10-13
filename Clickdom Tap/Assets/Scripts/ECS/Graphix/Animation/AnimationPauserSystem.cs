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

[Serializable]
public struct PauseData
{
    public int pauseOnFrame;
    public float pauseDuration;
    public float pauseSpread;
}

public struct AnimationPauseComponentData : IComponentData
{
    public bool needPause;
    public PauseData pauseData;
    public float timerToResume;
}

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateAfter(typeof(SpriteSheetAnimationSystem))]
public class AnimationPauserSystem : JobComponentSystem
{
    [BurstCompile]
    public struct PauseJob : IJobForEach<AnimationPauseComponentData, SpriteSheetAnimationComponentData>
    {
        [ReadOnly] public Unity.Mathematics.Random rnd;

        public void Execute(ref AnimationPauseComponentData pause, ref SpriteSheetAnimationComponentData animation)
        {
            if (!pause.needPause) return;
            if (animation.pause) return;

            if (animation.currentFrame == pause.pauseData.pauseOnFrame && animation.frameChangedEventFlag)
            {
                animation.pause = true;
                pause.timerToResume = pause.pauseData.pauseDuration + rnd.NextFloat(-pause.pauseData.pauseSpread, pause.pauseData.pauseSpread);
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return new PauseJob()
        {
            rnd = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, int.MaxValue))
        }.Schedule(this, inputDeps);
    }
}