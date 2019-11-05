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

public struct AnimatorStatesComponentData : IComponentData
{
    public bool running;
    public bool shooting;
    public bool climbing;
    public bool fighting;
    public bool death;
    public bool jumping;
    public bool falling;

    public bool out_StateChangedEventFlag;
}

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateAfter(typeof(AnimatorStatesUpdateSystem))]
public class AnimatorSystem : ComponentSystem
{
    public struct SetterData
    {
        public Entity entity;
        public AnimationType animation;
    }

    [RequireComponentTag(typeof(SpriteSheetAnimationComponentData))]
    public struct StateMachineJob : IJobForEachWithEntity<AnimatorStatesComponentData>
    {
        public NativeQueue<SetterData>.ParallelWriter animationsToSet;

        public void Execute(Entity entity, int index, ref AnimatorStatesComponentData states)
        {
            if (!states.out_StateChangedEventFlag) return;
            //states.stateChangedEventFlag = false;

            var setData = new SetterData() { entity = entity, animation = AnimationType.IDLE };

            if (states.running)
                setData.animation = AnimationType.RUN;
            if (states.climbing)
                setData.animation = AnimationType.CLIMB;
            if (states.fighting)
                setData.animation = AnimationType.FIGHT;
            if (states.shooting)
                setData.animation = AnimationType.SHOOT;

            animationsToSet.Enqueue(setData);
        }
    }

    protected override void OnUpdate()
    {
        var animationsToSet = new NativeQueue<SetterData>(Allocator.TempJob);

        new StateMachineJob()
        {
            animationsToSet = animationsToSet.AsParallelWriter()
        }.Schedule(this).Complete();

        SetterData setData;
        while(animationsToSet.TryDequeue(out setData))
            AnimationSetterUtil.SetAnimation(EntityManager, setData.entity, setData.animation);

        animationsToSet.Dispose();
    }
}