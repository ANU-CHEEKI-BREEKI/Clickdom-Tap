using System
    ;
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

[UpdateBefore(typeof(DeleteProjectileSystem))]
[UpdateAfter(typeof(ProjectileSystem))]
public class WholeProgressParticlesCollectorSystem : JobComponentSystem
{
    private Rect triggerZone;
    
    private bool itialized = false;

    public void Init(Rect triggerZone)
    {
        this.triggerZone = triggerZone;

        itialized = true;
    }

    public struct ProgressData
    {
        public float3 position;
        public float damage;
    }

    [BurstCompile]
    public struct CollectTriggeredProjectilesJob : IJobForEach<Translation, ProjectileComponentData, ProjectileCollisionComponentData>
    {
        public NativeQueue<ProgressData>.ParallelWriter progressData;
        [ReadOnly] public Rect triggerZone;

        public void Execute([ReadOnly] ref Translation translation, [ReadOnly] ref ProjectileComponentData projectile, [ReadOnly] ref ProjectileCollisionComponentData collision)
        {
            if (collision.ownerFaction == FactionComponentData.Faction.ENEMY)
                return;
            if (!projectile.itStopsRightNow)
                return;
            if (!triggerZone.Contains(translation.Value.ToF2()))
                return;

            progressData.Enqueue(new ProgressData()
            {
                damage = collision.processData.damage,
                position = translation.Value
            });
        }
    }

    public static NativeQueue<ProgressData> triggeredProjectiles;
    public static JobHandle collectJobHandle;

    protected override void OnCreate()
    {
        base.OnCreate();
        triggeredProjectiles = new NativeQueue<ProgressData>(Allocator.TempJob);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        triggeredProjectiles.Dispose();
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (!itialized)
            return inputDeps;

        triggeredProjectiles.Dispose();
        triggeredProjectiles = new NativeQueue<ProgressData>(Allocator.TempJob);

        collectJobHandle = new CollectTriggeredProjectilesJob()
        {
            progressData = triggeredProjectiles.AsParallelWriter(),
            triggerZone = triggerZone
        }.Schedule(this, inputDeps);
        return collectJobHandle;
    }
}