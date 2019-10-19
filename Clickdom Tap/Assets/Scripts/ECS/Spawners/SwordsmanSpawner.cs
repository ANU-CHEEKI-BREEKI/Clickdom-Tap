using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct SwordsmanTagComponentData : IComponentData { }

[RequireComponent(typeof(SequencePath))]
public class SwordsmanSpawner : ASpawner
{
    private SequencePath path;
    [Space]
    [SerializeField] float targetFindingRange = 0.5f;
    [SerializeField] float meeleeDamage = 1f;

    protected override void Start()
    {
        base.Start();

        path = GetComponent<SequencePath>();
    }

    protected override void SetEntityComponentsData(Entity entity, EntityManager manager)
    {
        base.SetEntityComponentsData(entity, manager);

        manager.AddComponent(entity, typeof(SwordsmanTagComponentData));
        manager.AddComponent(entity, typeof(SequenceMovementCurrentPositionIndexComponentData));
        manager.AddComponent(entity, typeof(StopMoveIfHasMeleeTargetTagComponentData));
        manager.AddComponent(entity, typeof(MeleeTargetComponentData));
        manager.AddComponentData(entity, new MeleeFindTargetSettingsComponentData()
        {
            findingRadius = targetFindingRange
        });        
        manager.AddComponentData(entity, new MeleeAttackComponentData()
        {
            damage = meeleeDamage
        });
        
    }

    protected override void SetEntitySharedComponentsData(Entity entity, EntityManager manager)
    {
        base.SetEntitySharedComponentsData(entity, manager);

        manager.AddSharedComponentData(entity, new SequenceMovementSharedComponentData()
        {
            movementPositions = path.CopyOfWorldPointsAsF2
        });
    }

    protected override void EndInitEntityData(Entity entity, EntityManager manager)
    {
        base.EndInitEntityData(entity, manager);

        AnimationSetterUtil.SetAnimation(manager, entity, AnimationType.FIGHT, 4);
    }

    protected override string GenerateEntityName()
    {
        return $"Swordsman {faction}";
    }
}
