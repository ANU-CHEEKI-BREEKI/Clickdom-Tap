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

public struct LinearMovementComponentData : IComponentData
{
    public float2 positionToMove;
    public bool isMoving;
    public bool doMoving;

    public float2 previousPosition;
}

public class LinearMovementSystem : JobComponentSystem
{
    [ExcludeComponent(typeof(Scale), typeof(VelocityComponentData), typeof(ProjectileComponentData))]
    [BurstCompile]
    struct MovementJob : IJobForEach<Translation, LinearMovementComponentData, VelocityAbsoluteComponentData>
    {
        [ReadOnly] public float deltatime;

        public void Execute(ref Translation translation, ref LinearMovementComponentData movementData, [ReadOnly] ref VelocityAbsoluteComponentData velocity)
        {
            Move(ref translation, ref movementData, ref velocity, 1, deltatime);
        }
    }

    [ExcludeComponent(typeof(VelocityComponentData), typeof(ProjectileComponentData))]
    [BurstCompile]
    struct MovementScaledJob : IJobForEach<Translation, LinearMovementComponentData, VelocityAbsoluteComponentData, Scale>
    {
        [ReadOnly] public float deltatime;

        public void Execute(ref Translation translation, ref LinearMovementComponentData movementData, [ReadOnly] ref VelocityAbsoluteComponentData velocity, [ReadOnly] ref Scale scale)
        {
            Move(ref translation, ref movementData, ref velocity, scale.Value, deltatime);
        }
    }

    static private void Move(ref Translation translation, ref LinearMovementComponentData movementData, ref VelocityAbsoluteComponentData velocity, float scale, float deltatime)
    {
        var realVelocity = velocity.value * scale;

        if(!movementData.doMoving)
        {
            movementData.isMoving = false;
            //включаем обратно. (а в других системах, будем только убирать флаг, запрещая движение)
            movementData.doMoving = true;
            return;
        }

        movementData.isMoving = !translation.Value.EqualsEpsilon(movementData.positionToMove, realVelocity / 4);

        if (movementData.isMoving)
        {
            movementData.previousPosition = translation.Value.ToF2();

            var direction = translation.Value
                .GetDirectionTo(movementData.positionToMove)
                .GetNormalized();
            translation.Value.x += direction.x * deltatime * realVelocity;
            translation.Value.y += direction.y * deltatime * realVelocity;
        }
        
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new MovementJob
        {
            deltatime = Time.deltaTime
        };
        var jh = job.Schedule(this, inputDeps);
        var jobScaled = new MovementScaledJob
        {
            deltatime = Time.deltaTime
        };
        return jobScaled.Schedule(this, jh);
    }
}