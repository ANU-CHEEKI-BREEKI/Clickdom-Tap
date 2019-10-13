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

public struct CacheByQuadrandSystemTag : IComponentData {}
public struct FactionComponentData : IComponentData
{
    public enum Faction { NEUTRAL, ALLY, ENEMY }
    public Faction value;
}

public class QuadrantSystem : ComponentSystem
{
    public struct QuadrandEntityData : IComponentData
    {
        public enum Corps { UNKNOWN, SWORDSMAN, ARCHER }

        public Entity entity;
        public float3 position;
        public FactionComponentData.Faction faction;
        public Corps corps;
        public bool processProjectileCollision;
    }

    [BurstCompile]
    struct CacheEntitiesChunkJob : IJobChunk
    {
        public NativeMultiHashMap<int, QuadrandEntityData>.ParallelWriter map;

        [ReadOnly] public ArchetypeChunkEntityType entityType;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        [ReadOnly] public ArchetypeChunkComponentType<FactionComponentData> factionType;
        [ReadOnly] public ArchetypeChunkComponentType<ArcherTagComponentData> archerType;
        [ReadOnly] public ArchetypeChunkComponentType<SwordsmanTagComponentData> swordsmanType;
        [ReadOnly] public ArchetypeChunkComponentType<ProcessProjectileCollisionTag> projectileProcessType;
        
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var isArcher = chunk.Has(archerType);
            var isSwordsman = chunk.Has(swordsmanType);
            var processProjectiles = chunk.Has(projectileProcessType);

            var entities = chunk.GetNativeArray(entityType);
            var positions = chunk.GetNativeArray(translationType);
            var factions = chunk.GetNativeArray(factionType);
            
            for (int i = 0; i < chunk.Count; i++)
            {
                var key = GetHashKeyByPosition(positions[i].Value);
                map.Add(
                    key,
                    new QuadrandEntityData()
                    {
                        entity = entities[i],
                        corps =   isArcher ? QuadrandEntityData.Corps.ARCHER 
                                : isSwordsman ? QuadrandEntityData.Corps.SWORDSMAN 
                                : QuadrandEntityData.Corps.UNKNOWN,
                        faction = factions[i].value,
                        position = positions[i].Value,
                        processProjectileCollision = processProjectiles
                    }
                );
            }

            //entityes.Dispose();
            //positions.Dispose();
            //factions.Dispose();
        }
    }

    public const int xQuadrantSize = 1;
    public const int yQuadrantSize = 1;

    public static QuadrantSystem Instance { get; private set; }
    public NativeMultiHashMap<int, QuadrandEntityData> quadrantMap;
    
    public static int2 GetQuadrant(float3 position)
    {
        var quadrant = new int2((int)position.x, (int)position.y);
        quadrant.x = (int)(math.floor(position.x / xQuadrantSize));
        quadrant.y = (int)(math.floor(position.y / yQuadrantSize));
        return quadrant;
    }

    public static int GetHashKeyByPosition(float3 position)
    {
        var quadrant = GetQuadrant(position);
        return GetHashKeyByQuadrant(quadrant);
    }

    public static int GetHashKeyByQuadrant(int2 quadrant)
    {
        return (quadrant.x << 16) + quadrant.y;
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        Instance = this;
        quadrantMap = new NativeMultiHashMap<int, QuadrandEntityData>(0, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        quadrantMap.Dispose();
        base.OnDestroy();        
    }
    
    protected override void OnUpdate()
    {
        var queryDesc = new EntityQueryDesc()
        {
            All = new[] {
                ComponentType.ReadOnly<CacheByQuadrandSystemTag>(),
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<FactionComponentData>()
            },
            Any = new[] {
                ComponentType.ReadOnly<ArcherTagComponentData>(),
                ComponentType.ReadOnly<SwordsmanTagComponentData>()
            }
        };

        var query = GetEntityQuery(queryDesc);
        var entityCount = query.CalculateEntityCount();
        quadrantMap.Clear();
        if (quadrantMap.Capacity < entityCount) quadrantMap.Capacity = entityCount;

        var job = new CacheEntitiesChunkJob
        {
            map = quadrantMap.AsParallelWriter(),
            entityType = GetArchetypeChunkEntityType(),
            archerType = GetArchetypeChunkComponentType<ArcherTagComponentData>(true),
            swordsmanType = GetArchetypeChunkComponentType<SwordsmanTagComponentData>(true),
            factionType = GetArchetypeChunkComponentType<FactionComponentData>(true),
            projectileProcessType = GetArchetypeChunkComponentType<ProcessProjectileCollisionTag>(true),
            translationType = GetArchetypeChunkComponentType<Translation>(true)
        };
        var archHandle = job.Schedule(query);
        archHandle.Complete();
    }

    public static void DrawQuadrant(float3 position, Color color)
    {
        var quadrant = GetQuadrant(position);
        Debug.DrawLine(new Vector3(quadrant.x, quadrant.y, -9), new Vector3(quadrant.x + xQuadrantSize, quadrant.y, -9), color);
        Debug.DrawLine(new Vector3(quadrant.x + xQuadrantSize, quadrant.y, -9), new Vector3(quadrant.x + xQuadrantSize, quadrant.y + yQuadrantSize, -9), color);
        Debug.DrawLine(new Vector3(quadrant.x + xQuadrantSize, quadrant.y + yQuadrantSize, -9), new Vector3(quadrant.x, quadrant.y + yQuadrantSize, -9), color);
        Debug.DrawLine(new Vector3(quadrant.x, quadrant.y + yQuadrantSize, -9), new Vector3(quadrant.x, quadrant.y, -9), color);
    }

    public enum NearestQuadrant { CURRENT, LEFT_UP, UP, RIGHT_UP, RIGHT, RIGHT_DOWN, DOWN, LEFT_DOWN, LEFT }

    public static int GetNearestQuadrantKey(int2 quadrant, NearestQuadrant nearest)
    {
        switch (nearest)
        {
            case NearestQuadrant.LEFT_UP:
                quadrant.x -= 1;
                quadrant.y += 1;
                break;
            case NearestQuadrant.UP:
                quadrant.y += 1;
                break;
            case NearestQuadrant.RIGHT_UP:
                quadrant.x += 1;
                quadrant.y += 1;
                break;
            case NearestQuadrant.RIGHT:
                quadrant.x += 1;
                break;
            case NearestQuadrant.RIGHT_DOWN:
                quadrant.x += 1;
                quadrant.y -= 1;
                break;
            case NearestQuadrant.DOWN:
                quadrant.y -= 1;
                break;
            case NearestQuadrant.LEFT_DOWN:
                quadrant.x -= 1;
                quadrant.y -= 1;
                break;
            case NearestQuadrant.LEFT:
                quadrant.x -= 1;
                break;
        }
        return GetHashKeyByQuadrant(quadrant);
    }
}