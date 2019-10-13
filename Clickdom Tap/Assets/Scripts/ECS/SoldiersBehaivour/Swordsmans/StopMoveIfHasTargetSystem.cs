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

public struct StopMoveIfHasMeleeTargetTagComponentData : IComponentData
{
}

[UpdateAfter(typeof(FindMeleeTargetSystem))]
public class StopMoveIfHasTargetSystem : JobComponentSystem
{    
    [BurstCompile]
    [RequireComponentTag(typeof(SwordsmanTagComponentData), typeof(StopMoveIfHasMeleeTargetTagComponentData))]
    struct TmplateJob : IJobForEach<LinearMovementComponentData, MeleeTargetComponentData>
    {
        public void Execute(ref LinearMovementComponentData moveData, ref MeleeTargetComponentData target)
        {
            if(target.target != Entity.Null)
                moveData.doMoving = false;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {        
        var job = new TmplateJob
        {
            
        };
        return job.Schedule(this, inputDeps);
    }
}