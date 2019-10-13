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

public struct MeleeTargetComponentData : IComponentData
{
    public Entity target;
    public float3 targetPosition;
}

public struct MeleeFindTargetSettingsComponentData :IComponentData
{
    public float findingRadius;
}

[UpdateAfter(typeof(QuadrantSystem))]
public class FindMeleeTargetSystem : JobComponentSystem
{
    [RequireComponentTag(typeof(SwordsmanTagComponentData))]
    [BurstCompile]
    public struct FindMeleeTargetJob : IJobForEach<Translation, MeleeFindTargetSettingsComponentData, MeleeTargetComponentData, FactionComponentData>
    {
        [ReadOnly] public NativeMultiHashMap<int, QuadrantSystem.QuadrandEntityData> quadrantMap;

        public void Execute([ReadOnly] ref Translation translation, [ReadOnly] ref MeleeFindTargetSettingsComponentData findingSettings, ref MeleeTargetComponentData meleeData, [ReadOnly] ref FactionComponentData faction)
        {
            FindTarget(ref translation, ref findingSettings, ref meleeData, ref faction, quadrantMap);
        }
    }

    [RequireComponentTag(typeof(SwordsmanTagComponentData))]
    [ExcludeComponent(typeof(FactionComponentData))]
    [BurstCompile]
    public struct FindMeleeTargetJobNeutralFaction : IJobForEach<Translation, MeleeFindTargetSettingsComponentData, MeleeTargetComponentData>
    {
        [ReadOnly] public NativeMultiHashMap<int, QuadrantSystem.QuadrandEntityData> quadrantMap;

        public void Execute([ReadOnly] ref Translation translation, [ReadOnly] ref MeleeFindTargetSettingsComponentData findingSettings, ref MeleeTargetComponentData meleeData)
        {
            var faction = new FactionComponentData() { value = FactionComponentData.Faction.NEUTRAL };
            FindTarget(ref translation, ref findingSettings, ref meleeData, ref faction, quadrantMap);
        }
    }

    private static void FindTarget(ref Translation translation, ref MeleeFindTargetSettingsComponentData findingSettings, ref MeleeTargetComponentData meleeData, ref FactionComponentData faction, NativeMultiHashMap<int, QuadrantSystem.QuadrandEntityData> quadrantMap)
    {
        var pos = translation.Value;
        var quadrant = QuadrantSystem.GetQuadrant(pos);

        var closest = new MeleeTargetComponentData() { target = Entity.Null };
        closest = FindClosest(findingSettings, faction, quadrantMap, pos, QuadrantSystem.GetNearestQuadrantKey(quadrant, QuadrantSystem.NearestQuadrant.CURRENT), closest);
        closest = FindClosest(findingSettings, faction, quadrantMap, pos, QuadrantSystem.GetNearestQuadrantKey(quadrant, QuadrantSystem.NearestQuadrant.DOWN), closest);
        closest = FindClosest(findingSettings, faction, quadrantMap, pos, QuadrantSystem.GetNearestQuadrantKey(quadrant, QuadrantSystem.NearestQuadrant.LEFT), closest);
        closest = FindClosest(findingSettings, faction, quadrantMap, pos, QuadrantSystem.GetNearestQuadrantKey(quadrant, QuadrantSystem.NearestQuadrant.LEFT_DOWN), closest);
        closest = FindClosest(findingSettings, faction, quadrantMap, pos, QuadrantSystem.GetNearestQuadrantKey(quadrant, QuadrantSystem.NearestQuadrant.LEFT_UP), closest);
        closest = FindClosest(findingSettings, faction, quadrantMap, pos, QuadrantSystem.GetNearestQuadrantKey(quadrant, QuadrantSystem.NearestQuadrant.RIGHT), closest);
        closest = FindClosest(findingSettings, faction, quadrantMap, pos, QuadrantSystem.GetNearestQuadrantKey(quadrant, QuadrantSystem.NearestQuadrant.RIGHT_DOWN), closest);
        closest = FindClosest(findingSettings, faction, quadrantMap, pos, QuadrantSystem.GetNearestQuadrantKey(quadrant, QuadrantSystem.NearestQuadrant.RIGHT_UP), closest);
        closest = FindClosest(findingSettings, faction, quadrantMap, pos, QuadrantSystem.GetNearestQuadrantKey(quadrant, QuadrantSystem.NearestQuadrant.UP), closest);

        meleeData.target = closest.target;
        meleeData.targetPosition = closest.targetPosition;
    }

    private static MeleeTargetComponentData FindClosest(MeleeFindTargetSettingsComponentData findingSettings, FactionComponentData faction, NativeMultiHashMap<int, QuadrantSystem.QuadrandEntityData> quadrantMap, float3 pos, int key, MeleeTargetComponentData closest)
    {
        NativeMultiHashMapIterator<int> iterator;
        QuadrantSystem.QuadrandEntityData qdata;

        if (quadrantMap.TryGetFirstValue(key, out qdata, out iterator))
        {
            do
            {
                if (qdata.faction != faction.value &&
                    qdata.corps == QuadrantSystem.QuadrandEntityData.Corps.SWORDSMAN &&
                    qdata.position.EqualsEpsilon(pos, findingSettings.findingRadius) &&
                    (closest.target == Entity.Null || pos.CompareToAnother(qdata.position, closest.targetPosition) < 0)
                )
                    {
                        closest.targetPosition = qdata.position;
                        closest.target = qdata.entity;
                    }
            }
            while (quadrantMap.TryGetNextValue(out qdata, ref iterator));
        }
        return closest;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var map = QuadrantSystem.Instance.quadrantMap;
        var hj = new FindMeleeTargetJob()
        {
            quadrantMap = map
        }.Schedule(this, inputDeps);
        return new FindMeleeTargetJobNeutralFaction()
        {
            quadrantMap = map
        }.Schedule(this, hj);
    }
}