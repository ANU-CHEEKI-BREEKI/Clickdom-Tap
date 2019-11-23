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

public struct MeleeAttackComponentData : IComponentData
{
    public bool attackEventFlag;
    public float damage;
}


public class MeleeAttackSystem : ComponentSystem
{
    public struct MeleeDetectData
    {
        public Entity entity;
        public MeleeAttackComponentData attack;
        public MeleeTargetComponentData target;
    }

    [BurstCompile]
    public struct DetectAnimaionActionJobCunk : IJobChunk
    {
        public NativeHashMap<Entity, MeleeDetectData>.ParallelWriter detectedActionEntities;

        public ArchetypeChunkComponentType<MeleeTargetComponentData> targetType;
        public ArchetypeChunkComponentType<MeleeAttackComponentData> attackType;

        [ReadOnly] public ArchetypeChunkComponentType<ActionOnAnimationFrameComponentData> actionType;
        [ReadOnly] public ArchetypeChunkComponentType<SpriteSheetAnimationComponentData> animationType;
        [ReadOnly] public ArchetypeChunkEntityType entityType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var actions = chunk.GetNativeArray(actionType);
            var animations = chunk.GetNativeArray(animationType);
            var targets = chunk.GetNativeArray(targetType);
            var attacks = chunk.GetNativeArray(attackType);

            var entities = chunk.GetNativeArray(entityType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var action = actions[i];
                var animation = animations[i];

                if (!action.needAction)
                    continue;

                var entity = entities[i];

                if (animation.currentFrame == action.actionData.frame && animation.out_FrameChangedEventFlag)
                {
                    var attack = attacks[i];
                    attack.attackEventFlag = true;
                    attacks[i] = attack;

                    var target = targets[i];

                    var detect = new MeleeDetectData()
                    {
                        entity = entity,
                        attack = attack,
                        target = target,
                    };

                    target.target = Entity.Null;
                    targets[i] = target;

                    detectedActionEntities.TryAdd(entity, detect);
                }
            }


            
        }
    }
    
    [RequireComponentTag(typeof(SwordsmanTagComponentData))]
    [BurstCompile]
    public struct ResetAttackFlag : IJobForEach<MeleeAttackComponentData>
    {
        public void Execute(ref MeleeAttackComponentData attack)
        {
            attack.attackEventFlag = false;
        }
    }

    private EntityQuery query;

    protected override void OnCreate()
    {
        base.OnCreate();

        query = GetEntityQuery(
            ComponentType.ReadOnly<SwordsmanTagComponentData>(),
            ComponentType.ReadOnly<MeleeTargetComponentData>(),
            ComponentType.ReadOnly<ActionOnAnimationFrameComponentData>(),
            ComponentType.ReadOnly<SpriteSheetAnimationComponentData>(),
            ComponentType.ReadWrite<MeleeAttackComponentData>()
        );
    }

    protected override void OnUpdate()
    {
        var resethandle = new ResetAttackFlag()
        {
        }.Schedule(this);
        
        var cnt = query.CalculateEntityCount();

        var detectedActionEntities = new NativeHashMap<Entity, MeleeDetectData>(cnt, Allocator.TempJob);
        var detectingActionJobHandle = new DetectAnimaionActionJobCunk()
        {
            detectedActionEntities = detectedActionEntities.AsParallelWriter(),
            actionType = GetArchetypeChunkComponentType<ActionOnAnimationFrameComponentData>(true),
            animationType = GetArchetypeChunkComponentType<SpriteSheetAnimationComponentData>(true),
            attackType = GetArchetypeChunkComponentType<MeleeAttackComponentData>(),
            entityType = GetArchetypeChunkEntityType(),
            targetType = GetArchetypeChunkComponentType<MeleeTargetComponentData>()

        }.Schedule(query, resethandle);

        detectingActionJobHandle.Complete();
        var keys = detectedActionEntities.GetKeyArray(Allocator.TempJob);

        for (int i = 0; i < keys.Length; i++)
        {
            MeleeDetectData detect;
            if (!detectedActionEntities.TryGetValue(keys[i], out detect))
                continue;

            var target = detect.target.target;
            if (target == Entity.Null)
                continue;

            if (!EntityManager.HasComponent<HealthComponentData>(target))
                continue;

            var atackData = detect.attack;
            //наносим урон
            var enemyHealth = EntityManager.GetComponentData<HealthComponentData>(target);
            enemyHealth.value -= atackData.damage;
            EntityManager.SetComponentData(target, enemyHealth);
        }

        keys.Dispose();
        detectedActionEntities.Dispose();
    }
}