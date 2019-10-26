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
    public float damage;
}

public class MeleeAttackComponentSystem : ComponentSystem
{
    [RequireComponentTag(typeof(SwordsmanTagComponentData), typeof(MeleeTargetComponentData), typeof(MeleeAttackComponentData))]
    [BurstCompile]
    public struct DetectAnimaionActionJob : IJobForEachWithEntity<ActionOnAnimationFrameComponentData, SpriteSheetAnimationComponentData>
    {
        public NativeHashMap<Entity, Entity>.ParallelWriter detectedActionEntities;
        
        public void Execute(Entity entity, int index, [ReadOnly] ref ActionOnAnimationFrameComponentData action, [ReadOnly] ref SpriteSheetAnimationComponentData animation)
        {
            if (!action.needAction)
                return;

            if (animation.currentFrame == action.actionData.frame && animation.out_FrameChangedEventFlag)
                detectedActionEntities.TryAdd(entity, entity);
        }
    }

    protected override void OnUpdate()
    {
        var query = GetEntityQuery(
            ComponentType.ReadOnly<SwordsmanTagComponentData>(),
            ComponentType.ReadOnly<MeleeTargetComponentData>(),
            ComponentType.ReadOnly<ActionOnAnimationFrameComponentData>(),
            ComponentType.ReadOnly<SpriteSheetAnimationComponentData>(),
            ComponentType.ReadOnly<MeleeAttackComponentData>()
        );
        var cnt = query.CalculateEntityCount();

        var detectedActionEntities = new NativeHashMap<Entity, Entity>(cnt, Allocator.TempJob);
        var detectingActionJobHandle = new DetectAnimaionActionJob()
        {
            detectedActionEntities = detectedActionEntities.AsParallelWriter()
        }.Schedule(this);

        var entities = query.ToEntityArray(Allocator.TempJob);
        var targets = query.ToComponentDataArray<MeleeTargetComponentData>(Allocator.TempJob);
        var atackDatas = query.ToComponentDataArray<MeleeAttackComponentData>(Allocator.TempJob);

        detectingActionJobHandle.Complete();
        for (int i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            if (!detectedActionEntities.ContainsKey(entity))
                continue;

            var target = targets[i].target;

            if (target == Entity.Null)
                continue;

            if (!EntityManager.HasComponent<HealthComponentData>(target))
                continue;

            var atackData = atackDatas[i];
            //наносим урон
            var enemyHealth = EntityManager.GetComponentData<HealthComponentData>(target);
            enemyHealth.value -= atackData.damage;
            EntityManager.SetComponentData(target, enemyHealth);
        }

        entities.Dispose();
        targets.Dispose();
        atackDatas.Dispose();
        detectedActionEntities.Dispose();
    }
}