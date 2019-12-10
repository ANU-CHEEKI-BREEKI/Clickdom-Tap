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

public enum TrailSpriteDataNamedId
{
    TRAILS_FIRE_1
}

//Этим компонентом надо пометить ентити, который должен оставлять после себя Trails
[Serializable]
public struct SpawnTrailsComponentData : IComponentData
{
    public float spawnDeltaTime;
    [HideInInspector] public float spawnTimer;
    [HideInInspector] public float3 prevPosition;

    /// <summary>
    /// будет задаваться id, по которому потом будет произведён поиск в каком то контейнере (в момент спавна?)
    /// </summary>
    public TrailSpriteDataNamedId spriteRenderDataId;

    public SpawnTrailData spawnData;
}

[Serializable]
public struct SpawnTrailData
{
    public float3 spawnRandPosition;
    public float trailLifetime;
    public Color startColor;
    public Color endColor;
    public float2 renderScale;
    /// <summary>
    /// доп значение поворота, чтобы перезаписать верх спрайта
    /// </summary>
    public float3 upRotation;
    public bool rotateByMovementDirection;
    /// <summary>
    /// выставить длительность анимации так, чтобы за время жизни частицы trail анимация проигралась полностью один раз
    /// </summary>
    public bool animationDurationByLifetime;
    /// <summary>
    /// color interpolation equation
    /// </summary>
    public ANU.Utils.Math.QuadraticEquation interpolationEquation;
}

public class TrailsDetectSpawnEvent : JobComponentSystem
{
    public struct InternalSpawnTrailData
    {
        public TrailSpriteDataNamedId spriteRenderDataId;
        public float3 position;
        public quaternion rotation;
        public float scale;
        public SpawnTrailData spawnData;
    }

    [BurstCompile]
    public struct CollectTrailedEntities : IJobForEach<SpawnTrailsComponentData, Translation, Scale>
    {
        [ReadOnly] public float deltaTime;
        [ReadOnly] public Unity.Mathematics.Random rand;
        public NativeQueue<InternalSpawnTrailData>.ParallelWriter spawnData;

        public void Execute(ref SpawnTrailsComponentData spawntrails, [ReadOnly] ref Translation translation, [ReadOnly]ref Scale scale)
        {
            if (spawntrails.spawnTimer < spawntrails.spawnDeltaTime)
            {
                spawntrails.spawnTimer += deltaTime;
            }
            else
            {
                int trailSpawnCount = (int)(spawntrails.spawnTimer / spawntrails.spawnDeltaTime);
                spawntrails.spawnTimer -= trailSpawnCount * spawntrails.spawnDeltaTime;

                for (int i = 0; i <= trailSpawnCount; i++)
                {
                    var s = (float)i / trailSpawnCount;
                    var pos = math.lerp(translation.Value, spawntrails.prevPosition, s);
                    var addpos = spawntrails.spawnData.spawnRandPosition * rand.NextFloat3() * 2 - spawntrails.spawnData.spawnRandPosition * rand.NextFloat3();

                    var angle = spawntrails.spawnData.upRotation;
                    if (spawntrails.spawnData.rotateByMovementDirection)
                    {
                        var dir = spawntrails.prevPosition.GetDirectionTo(translation.Value.ToF2());
                        angle += new float3(0, 0, ANU.Utils.Math.AngleRadians(dir));
                    }
                    var rot = quaternion.Euler(angle);

                    //record spawn data
                    spawnData.Enqueue(new InternalSpawnTrailData()
                    {
                        position = pos + addpos,
                        spawnData = spawntrails.spawnData,
                        spriteRenderDataId = spawntrails.spriteRenderDataId,
                        scale = scale.Value,
                        rotation = rot
                    });
                    spawntrails.spawnTimer -= spawntrails.spawnDeltaTime;
                }
            }
            spawntrails.prevPosition = translation.Value;
        }
    }

    public static JobHandle handle;

    public static NativeQueue<InternalSpawnTrailData> spawnData;

    protected override void OnCreate()
    {
        base.OnCreate();
        spawnData = new NativeQueue<InternalSpawnTrailData>(Allocator.TempJob);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        spawnData.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        spawnData.Dispose();
        spawnData = new NativeQueue<InternalSpawnTrailData>(Allocator.TempJob);

        handle = new CollectTrailedEntities()
        {
            deltaTime = Time.deltaTime,
            spawnData = spawnData.AsParallelWriter(),
            rand = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, int.MaxValue))
        }.Schedule(this, inputDeps);

        return handle;
    }
}