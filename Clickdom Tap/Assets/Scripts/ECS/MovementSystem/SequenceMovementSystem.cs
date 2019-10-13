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

public struct SequenceMovementSharedComponentData : ISharedComponentData, IEquatable<SequenceMovementSharedComponentData>
{
    public float2[] movementPositions;

    public bool Equals(SequenceMovementSharedComponentData other)
    {
        return ReferenceEquals(movementPositions, other.movementPositions);
    }

    public override int GetHashCode()
    {
        int hash = 60;
        for (int i = 0; i < movementPositions.Length; i++)
            hash += movementPositions[i].GetHashCode() * 60;
        return hash;
    }
}

public struct SequenceMovementCurrentPositionIndexComponentData: IComponentData
{
    public int value;
}

[UpdateBefore(typeof(LinearMovementSystem))]
public class SequenceMovementSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, SequenceMovementSharedComponentData sequence, ref LinearMovementComponentData line, ref SequenceMovementCurrentPositionIndexComponentData index) => 
        {
            var maxIndex = sequence.movementPositions.Length - 1;
            if (maxIndex == 0) return;
            if (!line.isMoving)
            {
                if (index.value < maxIndex)
                {               
                    index.value++;
                    line.positionToMove = sequence.movementPositions[index.value];
                }
                else
                {
                    PostUpdateCommands.RemoveComponent<SequenceMovementSharedComponentData>(entity);
                    PostUpdateCommands.RemoveComponent<SequenceMovementCurrentPositionIndexComponentData>(entity);
                }
            }
            
        });
    }
}