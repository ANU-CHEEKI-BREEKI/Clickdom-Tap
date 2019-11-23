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
public struct ActionData
{
    public int frame;
}

public struct ActionOnAnimationFrameComponentData : IComponentData
{
    public bool needAction;
    public ActionData actionData;

    public bool out_ActionFlag;
}

public class DetectAnimationActionSystem : JobComponentSystem
{
    [BurstCompile]
    struct ResetActionFlagJob : IJobForEach<ActionOnAnimationFrameComponentData>
    {
        public void Execute(ref ActionOnAnimationFrameComponentData action)
        {
            action.out_ActionFlag = false;
        }
    }

    [BurstCompile]
    struct DetectActionJob : IJobForEach<ActionOnAnimationFrameComponentData, SpriteSheetAnimationComponentData>
    {
        public void Execute(ref ActionOnAnimationFrameComponentData action, ref SpriteSheetAnimationComponentData animation)
        {
            if (animation.currentFrame != action.actionData.frame || !animation.out_FrameChangedEventFlag)
                return;
            action.out_ActionFlag = true;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var resetJobHandle = new ResetActionFlagJob().Schedule(this, inputDeps);
        var detectJobHandle = new DetectActionJob().Schedule(this, resetJobHandle);
        return detectJobHandle;
    }
}