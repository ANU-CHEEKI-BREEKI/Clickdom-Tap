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
public struct SpriteSheetAnimationComponentData : IComponentData
{
    public int currentFrame;
    public int frameCount;

    public float frameWidth;
    public float frameHeight;

    public float horisontalOffset;
    public float verticalOffset;

    public float frameDuration;

    public bool pause;

    public bool out_FrameChangedEventFlag;
    public float out_frameTimer;
}

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class SpriteSheetAnimationSystem : JobComponentSystem
{
    [BurstCompile]
    public struct UnscaledSpritesJob : IJobForEach<Translation, SpriteSheetAnimationComponentData, SpriteRendererComponentData>
    {
        [ReadOnly] public float deltaTime;
        
        public void Execute([ReadOnly] ref Translation translation, ref SpriteSheetAnimationComponentData animationData, ref SpriteRendererComponentData sprite)
        {
            if (animationData.frameCount == 0) return;
            if (animationData.pause) return;

            if (animationData.frameCount > 1 && animationData.frameDuration != 0)
            {
                animationData.out_frameTimer += deltaTime;
                if(animationData.out_frameTimer >= animationData.frameDuration)
                {
                    while (animationData.out_frameTimer >= animationData.frameDuration)
                    {
                        animationData.out_frameTimer -= animationData.frameDuration;
                        animationData.currentFrame = (animationData.currentFrame + 1) % animationData.frameCount;
                        animationData.out_FrameChangedEventFlag = true;
                    }
                }
                else
                {
                    animationData.out_FrameChangedEventFlag = false;
                }                
            }

            sprite.uv = new Vector4(
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
