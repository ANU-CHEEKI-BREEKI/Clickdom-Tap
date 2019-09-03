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
    public int currentFrame;
    public int frameCount;

    public float frameWidth;
    public float frameHeight;

    public float horisontalOffset;
    public float verticalOffset;

    public Vector4 uv;

    public float frameDuration;
    public float frameTimer;
}

public class SpriteSheetAnimationSystem : JobComponentSystem
{
    [BurstCompile]
    public struct UnscaledSpritesJob : IJobForEach<Translation, SpriteSheetAnimationComponentData>
    {
        [ReadOnly] public float deltaTime;

        public void Execute([ReadOnly] ref Translation translation, ref SpriteSheetAnimationComponentData animationData)
        {
            animationData.frameTimer += deltaTime;
            while (animationData.frameTimer >= animationData.frameDuration)
            {
                animationData.frameTimer -= animationData.frameDuration;
                animationData.currentFrame = (animationData.currentFrame + 1) % animationData.frameCount;
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
