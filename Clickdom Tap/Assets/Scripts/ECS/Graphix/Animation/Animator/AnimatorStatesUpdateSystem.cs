﻿using System;
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

public struct AnimatorStateTriggerComponentData : IComponentData
{
    public float2 leftUpTriggetZoneCorner;
    public float2 rightUpTriggetZoneCorner;
    public float2 rightBotTriggetZoneCorner;
    public float2 leftBotTriggetZoneCorner;

    public AnimationType animation;
}

public struct AnimatorStateLastTriggeredAnimationComponentData : IComponentData
{
    public AnimationType triggeredAnimation;
}

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateBefore(typeof(SpriteSheetAnimationSystem))]
public class AnimatorStatesUpdateSystem : JobComponentSystem
{
    [BurstCompile]
    struct ResetFlagSystem : IJobForEach<AnimatorStatesComponentData>
    {
        public void Execute(ref AnimatorStatesComponentData states)
        {
            states.out_StateChangedEventFlag = false;
        }
    }

    [BurstCompile]
    struct MovementStateUpdateJob : IJobForEach<LinearMovementComponentData, AnimatorStatesComponentData>
    {
        public void Execute([ReadOnly] ref LinearMovementComponentData move, ref AnimatorStatesComponentData states)
        {
            if (states.running != move.isMoving)
                states.out_StateChangedEventFlag = true;
            states.running = move.isMoving;
        }
    }

    [BurstCompile]
    struct MeleeAttackStateUpdateJob : IJobForEach<MeleeTargetComponentData, AnimatorStatesComponentData>
    {
        public void Execute([ReadOnly] ref MeleeTargetComponentData melee, ref AnimatorStatesComponentData states)
        {
            var hasTarget = melee.target != Entity.Null;
            if (states.fighting != hasTarget)
                states.out_StateChangedEventFlag = true;
            states.fighting = hasTarget;
        }
    }

    [BurstCompile]
    [RequireComponentTag(typeof(ArcherTagComponentData))]
    struct ShootingStateUpdateJob : IJobForEach<LinearMovementComponentData, AnimatorStatesComponentData>
    {
        public void Execute([ReadOnly] ref LinearMovementComponentData move, ref AnimatorStatesComponentData states)
        {
            if (states.shooting == move.isMoving)
                states.out_StateChangedEventFlag = true;
            states.shooting = !move.isMoving;
        }
    }

    /// <summary>
    /// если зон тригеров будет слишком много, то надо эту работу выделить в отдельную систему
    /// и использовать QuadrantSystem.
    /// Пока что, это используется в небольшом кол-ве мест. и и так сойдет (лестницы например...)
    /// </summary>
    [BurstCompile]
    struct TriggerStateUpdateJob : IJobForEach<Translation, AnimatorStatesComponentData, AnimatorStateLastTriggeredAnimationComponentData>
    {
        [ReadOnly] public NativeArray<AnimatorStateTriggerComponentData> triggerZones;

        public void Execute([ReadOnly] ref Translation translation, ref AnimatorStatesComponentData states, ref AnimatorStateLastTriggeredAnimationComponentData lastTriggeredData)
        {
            bool triggered = false;
            AnimationType animation = lastTriggeredData.triggeredAnimation;

            for (int i = 0; i < triggerZones.Length; i++)
            {
                var pos = translation.Value;
                var zone = triggerZones[i];

                //get 4 lines and check if positioon inside 4sided poligon
                var upline = ANU.Utils.Math.GetLineEquation(zone.leftUpTriggetZoneCorner, zone.rightUpTriggetZoneCorner);
                var rightline = ANU.Utils.Math.GetLineEquation(zone.rightUpTriggetZoneCorner, zone.rightBotTriggetZoneCorner);
                var botline = ANU.Utils.Math.GetLineEquation(zone.rightBotTriggetZoneCorner, zone.leftBotTriggetZoneCorner);
                var leftline = ANU.Utils.Math.GetLineEquation(zone.leftBotTriggetZoneCorner, zone.leftUpTriggetZoneCorner);

                //if zone rotated right
                //          lu
                //  lb               
                //                  ru
                //          rb
                if (ANU.Utils.Math.PointUnderOrLeftLine(pos.ToF2(), upline))
                    if (!ANU.Utils.Math.PointUnderOrLeftLine(pos.ToF2(), rightline, false))
                        if (!ANU.Utils.Math.PointUnderOrLeftLine(pos.ToF2(), botline, false))
                            if (ANU.Utils.Math.PointUnderOrLeftLine(pos.ToF2(), leftline))
                            {
                                triggered = true;
                                animation = zone.animation;
                                break;
                            }

                //if zone rotated left
                //          ru
                //  lu               
                //                  rb
                //          lb
                if (ANU.Utils.Math.PointUnderOrLeftLine(pos.ToF2(), upline))
                    if (ANU.Utils.Math.PointUnderOrLeftLine(pos.ToF2(), rightline))
                        if (!ANU.Utils.Math.PointUnderOrLeftLine(pos.ToF2(), botline, false))
                            if (!ANU.Utils.Math.PointUnderOrLeftLine(pos.ToF2(), leftline, false))
                            {
                                triggered = true;
                                animation = zone.animation;
                                break;
                            }
            }

            if (triggered)
                lastTriggeredData.triggeredAnimation = animation;

            //set animation state
            switch (animation)
            {
                case AnimationType.RUN:
                    if (states.running != triggered)
                        states.out_StateChangedEventFlag = true;
                    states.running = triggered;
                    break;
                case AnimationType.CLIMB:
                    if (states.climbing != triggered)
                        states.out_StateChangedEventFlag = true;
                    states.climbing = triggered;
                    break;
                case AnimationType.FIGHT:
                    if (states.fighting != triggered)
                        states.out_StateChangedEventFlag = true;
                    states.fighting = triggered;
                    break;
                case AnimationType.SHOOT:
                    if (states.shooting != triggered)
                        states.out_StateChangedEventFlag = true;
                    states.shooting = triggered;
                    break;
                case AnimationType.DEATH:
                    if (states.death != triggered)
                        states.out_StateChangedEventFlag = true;
                    states.death = triggered;
                    break;
                case AnimationType.JUMP:
                    if (states.jumping != triggered)
                        states.out_StateChangedEventFlag = true;
                    states.jumping = triggered;
                    break;
                case AnimationType.FALL:
                    if (states.falling != triggered)
                        states.out_StateChangedEventFlag = true;
                    states.falling = triggered;
                    break;
                default:
                    break;
            }
        }
    }

    private NativeArray<AnimatorStateTriggerComponentData> zones;

    protected override void OnCreate()
    {
        base.OnCreate();
        zones = new NativeArray<AnimatorStateTriggerComponentData>(0, Allocator.TempJob);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        zones.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var query = GetEntityQuery(typeof(AnimatorStateTriggerComponentData));

        zones.Dispose();
        zones = query.ToComponentDataArray<AnimatorStateTriggerComponentData>(Allocator.TempJob);

        var resetHandler = new ResetFlagSystem().Schedule(this, inputDeps);

        var triggetUpdaterHandler = new TriggerStateUpdateJob()
        {
            triggerZones = zones
        }.Schedule(this, resetHandler);        

        var handle = new MovementStateUpdateJob().Schedule(this, triggetUpdaterHandler);
        handle = new MeleeAttackStateUpdateJob().Schedule(this, handle);
        handle = new ShootingStateUpdateJob().Schedule(this, handle);

        //triggetUpdaterHandler.Complete();
        //zones.Dispose();

        return handle;
    }
}