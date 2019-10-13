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
public struct SpriteSheetAnimationComponentData : IComponentData
{
    public bool frameChangedEventFlag;

    public int currentFrame;
    public int frameCount;

    public float frameWidth;
    public float frameHeight;

    public float horisontalOffset;
    public float verticalOffset;

    public Vector4 uv;

    public float frameDuration;
    public float frameTimer;

    public bool pause;
}

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class SpriteSheetAnimationSystem : JobComponentSystem
{
    [BurstCompile]
    public struct UnscaledSpritesJob : IJobForEach<Translation, SpriteSheetAnimationComponentData>
    {
        [ReadOnly] public float deltaTime;

        public void Execute([ReadOnly] ref Translation translation, ref SpriteSheetAnimationComponentData animationData)
        {
            if (animationData.frameCount == 0) return;
            if (animationData.pause) return;

            if (animationData.frameCount > 1 && animationData.frameDuration != 0)
            {
                animationData.frameTimer += deltaTime;
                if(animationData.frameTimer >= animationData.frameDuration)
                {
                    while (animationData.frameTimer >= animationData.frameDuration)
                    {
                        animationData.frameTimer -= animationData.frameDuration;
                        animationData.currentFrame = (animationData.currentFrame + 1) % animationData.frameCount;
                        animationData.frameChangedEventFlag = true;
                    }
                }
                else
                {
                    animationData.frameChangedEventFlag = false;
                }                
            }

            animationData.uv = new Vector4(
                animationData.frameWidth, 
                animationData.frameHeight, 
                animationData.horisontalOffset + animationData.frameWidth * animationData.currentFrame,
                animationData.verticalOffset
            );
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new UnscaledSpritesJob()
        {
            deltaTime = Time.deltaTime
        };
        return job.Schedule(this, inputDeps);
    }
}
