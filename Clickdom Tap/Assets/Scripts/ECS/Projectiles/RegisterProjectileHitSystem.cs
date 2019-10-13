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

/// <summary>
/// Этим надо помечать ентити, в которые будут летель частицы (лучники, мечники, и т.п.)
/// </summary>
public struct ProcessProjectileCollisionTag : IComponentData
{
    public bool olreadyProceeded;

    /// <summary>
    /// есть ли столкновение
    /// </summary>
    public bool hittedByProjectile;
    /// <summary>
    /// как обработать столкновение
    /// </summary>
    public ProcessCollisionData processData;
    /// <summary>
    /// если запускают как частицу, то масса уменшит начальную скорость
    /// </summary>
    public float mass;
}

[Serializable]
public struct ProcessCollisionData
{
    public float damage;

    /// <summary>
    /// как обработать столкновение
    /// </summary>
    public HitProcessingType type;
    /// <summary>
    /// если type == .*REMOVE_WITH_DELAY.*, то это  и есть задержка
    /// </summary>
    public float destroyDelay;
    /// <summary>
    /// если type == .*SET_ANIMATION.*, то это тип нужной анимации
    /// </summary>
    public AnimationType animation;
    /// <summary>
    /// если type == .*LAUNCH_AS_PROJECTILE.*, то это скорость, котороя будет задана (в направлении полета частицы, которая спровоцировала колизию)
    /// </summary>
    public float absoluteProjectileVelocity;
    public float2 direction;
}

public enum HitProcessingType
{
    REMOVE,
    SET_ANIMATION,
    REMOVE_WITH_DELAY,
    SET_ANIMATION_AND_REMOVE_WITH_DELAY,
    LAUNCH_AS_PROJECTILE,
    SET_ANIMATION_AND_LAUNCH_AS_PROJECTILE
}

/// <summary>
/// Этим надо помечать частицы, которые должны попадать в другие ентити
/// </summary>
[Serializable]
public struct ProjectileCollisionComponentData : IComponentData
{
    public enum DetectCillisionTime
    {
        WHEN_MOVE,
        WHEN_STOPS,
        ALL_TIME
    }

    /// <summary>
    /// сколько раз частица может совершить колизий
    /// </summary>
    public int maxHitCount;
    /// <summary>
    /// таймер, чтобы отключить начальную проверку столкновений. 
    /// чтобы ентити которые запускают снаряды, не убивали сами себя
    /// </summary>
    public float colisionTimeOut;
    /// <summary>
    /// если Neutral, то будет френдли фаер. остальные по своим не бьют
    /// </summary>
    public FactionComponentData.Faction ownerFaction;
    /// <summary>
    /// когда регистрировать столкновения
    /// </summary>
    public DetectCillisionTime detectTime;

    public ProcessCollisionData processData;
}

[UpdateAfter(typeof(QuadrantSystem))]
[UpdateAfter(typeof(ProjectileSystem))]
public class RegisterProjectileHitSystem : ComponentSystem
{
    public struct RegisteredHitData
    {
        public Entity entity;
        public ProcessProjectileCollisionTag processData;
    }

    [BurstCompile]
    struct RegisterHitJob : IJobForEach<Translation, ProjectileComponentData, ProjectileCollisionComponentData, VelocityComponentData>
    {
        [ReadOnly] public NativeMultiHashMap<int, QuadrantSystem.QuadrandEntityData> quadrantMap;
        public NativeHashMap<Entity, RegisteredHitData>.ParallelWriter registegedHits;
        [ReadOnly] public float deltaTime;

        public void Execute(ref Translation translation, ref ProjectileComponentData projectileData, ref ProjectileCollisionComponentData processData, ref VelocityComponentData velocity)
        {
            if (processData.colisionTimeOut > 0)
            {
                processData.colisionTimeOut -= deltaTime;
                return;
            }
            if (processData.maxHitCount <= 0)
                return;

            var projectileMoved = velocity.value.x != 0 || velocity.value.y != 0;
            var projectileStopsRightNow = !projectileMoved && projectileData.itStopsRightNow;

            if (processData.detectTime == ProjectileCollisionComponentData.DetectCillisionTime.WHEN_MOVE && !projectileMoved)
                return;
            else if (processData.detectTime == ProjectileCollisionComponentData.DetectCillisionTime.WHEN_STOPS && (projectileMoved || !projectileStopsRightNow))
                return;

            processData.processData.direction = projectileData.previousProjectilePosition.ToF2().GetDirectionTo(translation.Value);
            processData.processData.direction.y = 0;

            var quadrantKey = QuadrantSystem.GetHashKeyByPosition(translation.Value);
            //получаем все энтити в квадранте
            QuadrantSystem.QuadrandEntityData qdata;
            NativeMultiHashMapIterator<int> iterator;
            if (quadrantMap.TryGetFirstValue(quadrantKey, out qdata, out iterator))
            {
                do
                {
                    if (processData.maxHitCount <= 0)
                        break;

                    //ну и проверяем на столкновение и создаём RegisteredHitData
                    var radius = 0.5f;
                    var radSqr = radius * radius;
                    var hit = false;

                    if (projectileMoved)
                    {
                        hit = Utils.Math.IsSegmentIntersectsPoint(
                            projectileData.previousProjectilePosition.ToF2(),
                            translation.Value.ToF2(),
                            qdata.position.ToF2(),
                            radius
                        );
                    }
                    else
                    {
                        hit = math.distancesq(translation.Value, qdata.position) <= radSqr ||
                        math.distancesq(projectileData.previousProjectilePosition, qdata.position) <= radSqr;
                    }
                    
                    //если попал и попал не в своего
                    if (hit && (processData.ownerFaction == FactionComponentData.Faction.NEUTRAL || processData.ownerFaction != qdata.faction))
                    {
                        registegedHits.TryAdd(
                            qdata.entity,
                            new RegisteredHitData()
                            {
                                entity = qdata.entity,
                                processData = new ProcessProjectileCollisionTag()
                                {
                                    hittedByProjectile = true,
                                    processData = processData.processData
                                }
                            }
                        );
                        processData.maxHitCount--;
                    }
                } while (quadrantMap.TryGetNextValue(out qdata, ref iterator));
            }          
        }
    }

    [BurstCompile]
    public struct SetRegisterHitDataJob : IJobForEachWithEntity<ProcessProjectileCollisionTag>
    {
        [ReadOnly] public NativeHashMap<Entity, RegisteredHitData> registegedHits;

        public void Execute(Entity entity, int index, ref ProcessProjectileCollisionTag collisionData)
        {
            RegisteredHitData rdata;
            if(registegedHits.TryGetValue(entity, out rdata))
            {
                if (!collisionData.olreadyProceeded)
                {
                    collisionData.hittedByProjectile = rdata.processData.hittedByProjectile;
                    collisionData.processData = rdata.processData.processData;
                }

                collisionData.olreadyProceeded = false;
            }
        }
    }

    NativeHashMap<Entity, RegisteredHitData> registegedHits;

    protected override void OnCreate()
    {
        base.OnCreate();
        registegedHits = new NativeHashMap<Entity, RegisteredHitData>(10_000, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        registegedHits.Dispose();
        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        UnityEngine.Profiling.Profiler.BeginSample("Init");

        UnityEngine.Profiling.Profiler.BeginSample("Query");
        var query = GetEntityQuery(ComponentType.ReadOnly<ProcessProjectileCollisionTag>());
        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("Count entities");
        var entityCount = query.CalculateEntityCount();
        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("Set hashmap capacity");
        registegedHits.Clear();
        if (registegedHits.Capacity < entityCount)
            registegedHits.Capacity = entityCount;
        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("PlanningJobs");
        var qmap = QuadrantSystem.Instance.quadrantMap;
        var registerHitJob = new RegisterHitJob()
        {
            quadrantMap = qmap,
            registegedHits = registegedHits.AsParallelWriter(),
            deltaTime = Time.deltaTime
        };
        var rhjh = registerHitJob.Schedule(this);
        var setHitDataJob = new SetRegisterHitDataJob()
        {
            registegedHits = registegedHits
        };
        var shdjh = setHitDataJob.Schedule(this, rhjh);

        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.EndSample();
        shdjh.Complete();
    }
}