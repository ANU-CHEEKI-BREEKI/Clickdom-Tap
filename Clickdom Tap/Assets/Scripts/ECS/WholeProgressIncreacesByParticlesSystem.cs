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
public class WholeProgressIncreacesByParticlesSystem : ComponentSystem
{
    private Rect triggerZone;
    private WholeProgressHandle progress;
    private bool itialized = false;

    public void Init(Rect triggerZone, WholeProgressHandle progress)
    {
        this.triggerZone = triggerZone;
        this.progress = progress;

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

    protected override void OnUpdate()
    {
        if (!itialized)
            return;

        var progressData = new NativeQueue<ProgressData>(Allocator.TempJob);

        new CollectTriggeredProjectilesJob()
        {
            progressData = progressData.AsParallelWriter(),
            triggerZone = triggerZone
        }.Schedule(this).Complete();

        ProgressData data;
        while (progressData.TryDequeue(out data))
            progress?.IncreaceProgressInPlace(data.damage, data.position);

        progressData.Dispose();
    }
}