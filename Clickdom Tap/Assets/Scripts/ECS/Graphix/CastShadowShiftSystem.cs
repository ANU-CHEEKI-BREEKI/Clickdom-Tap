using ANU.Utils;
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
using UnityEditor;
using UnityEngine;

/// <summary>
/// этим помечаем ентити, которому надо сдвигать тень
/// </summary>
public struct ShiftCastShadowsTagComponentData : IComponentData
{
    /// <summary>
    /// это значение будет установлено сдвигу тени перед добавлением доп сдвига
    /// </summary>
    public float3 positionUnitsOffsetDefaultValue;
}

/// <summary>
/// этим помечаем зоны, в которых будут сдвигаться тени
/// </summary>
public struct ShiftCastShadowsZoneComponentTagData : IComponentData
{
    // границы зоны
    public float2 leftUpTriggetZoneCorner;
    public float2 rightUpTriggetZoneCorner;
    public float2 rightBotTriggetZoneCorner;
    public float2 leftBotTriggetZoneCorner;

    // дополнительный сдвиг для тени
    public float2 startAdditionalPositionUnitsOffset;
    public float2 endAdditionalPositionUnitsOffset;

    // позиции по которым будет осуществлена линейная интерполяция доп сдвига
    public float2 startLerpPosition;
    public float2 endLerpPosition;
}

public class CastShadowShiftSystem : JobComponentSystem
{
    [BurstCompile]
    struct ResetOffsetJob : IJobForEach<ShiftCastShadowsTagComponentData, CastSpritesShadowComponentData>
    {
        public void Execute(ref ShiftCastShadowsTagComponentData shift, ref CastSpritesShadowComponentData shadow)
        {
            shadow.positionUnitsOffset = shift.positionUnitsOffsetDefaultValue;
        }
    }

    /// <summary>
    /// если зон тригеров будет слишком много, то надо эту работу выделить в отдельную систему
    /// и использовать QuadrantSystem.
    /// Пока что, это используется в небольшом кол-ве мест. и и так сойдет
    /// </summary>
    //[BurstCompile]
    [RequireComponentTag(typeof(ShiftCastShadowsTagComponentData))]
    struct ShiftTriggerZoneUpdateJob : IJobForEachWithEntity<Translation>
    {
        [ReadOnly] public NativeArray<ShiftCastShadowsZoneComponentTagData> triggerZones;

        public NativeMultiHashMap<Entity, float3>.ParallelWriter additionalOffsets;

        public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
        {
            for (int i = 0; i < triggerZones.Length; i++)
            {
                bool triggered = false;
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
                            }

                if(triggered)
                {
                    var t = 0f;
                    var localPos = pos.ToF2() - zone.startLerpPosition;
                    var lerpAmoumt = zone.endLerpPosition - zone.startLerpPosition;

                    var t2 = localPos / lerpAmoumt;
                    var tx = (!float.IsNaN(t2.x) && !float.IsInfinity(t2.x)) ? t2.x : 1;
                    var ty = (!float.IsNaN(t2.y) && !float.IsInfinity(t2.y)) ? t2.y : 1;

                    t = (tx + ty) / 2;
                    t = math.abs(t);
                    t = math.clamp(t, 0, 1);

                    var addPos = math.lerp(
                        zone.startAdditionalPositionUnitsOffset,
                        zone.endAdditionalPositionUnitsOffset,
                        t
                    );

                    additionalOffsets.Add(
                        entity,
                        (addPos - localPos).ToF3()
                    );
                }
            }
        }
    }

    [BurstCompile]
    public struct DoOffsetJob : IJobForEachWithEntity<CastSpritesShadowComponentData>
    {
        [ReadOnly] public NativeMultiHashMap<Entity, float3> additionalOffsets;

        public void Execute(Entity entity, int index, ref CastSpritesShadowComponentData shadow)
        {
            float3 offset;
            NativeMultiHashMapIterator<Entity> iterator;
            if(additionalOffsets.TryGetFirstValue(entity, out offset, out iterator))
            {
                do
                {
                    shadow.positionUnitsOffset += offset;
                }
                while (additionalOffsets.TryGetNextValue(out offset, ref iterator));
            }
        }
    }

    private NativeArray<ShiftCastShadowsZoneComponentTagData> zones;
    private NativeMultiHashMap<Entity, float3> additionalOffsets;

    protected override void OnCreate()
    {
        base.OnCreate();
        zones = new NativeArray<ShiftCastShadowsZoneComponentTagData>(0, Allocator.TempJob);
        additionalOffsets = new NativeMultiHashMap<Entity, float3>(0, Allocator.TempJob);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        zones.Dispose();
        additionalOffsets.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var query = GetEntityQuery(typeof(ShiftCastShadowsZoneComponentTagData));
        var shiftedEntitiesQuery = GetEntityQuery(typeof(ShiftCastShadowsTagComponentData));

        zones.Dispose();
        JobHandle toArrayHandle;
        zones = query.ToComponentDataArray<ShiftCastShadowsZoneComponentTagData>(Allocator.TempJob, out toArrayHandle);

        additionalOffsets.Dispose();
        //размер придется по максимуму задавать...
        additionalOffsets = new NativeMultiHashMap<Entity, float3>(
            shiftedEntitiesQuery.CalculateEntityCount() * query.CalculateEntityCount(),
            Allocator.TempJob
        );

        var resetHandler = new ResetOffsetJob().Schedule(this, JobHandle.CombineDependencies(toArrayHandle, inputDeps));

        var triggerUpdaterHandler = new ShiftTriggerZoneUpdateJob()
        {
            triggerZones = zones,
            additionalOffsets = additionalOffsets.AsParallelWriter()
        }.Schedule(this, resetHandler);

        var offsetJob = new DoOffsetJob()
        {
            additionalOffsets = additionalOffsets
        }.Schedule(this, triggerUpdaterHandler);

        return offsetJob;
    }
}