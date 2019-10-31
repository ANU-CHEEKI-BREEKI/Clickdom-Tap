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

    protected override void OnUpdate()
    {
        if (!itialized)
            return;

        var query = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<ProjectileComponentData>(),
            ComponentType.ReadOnly<ProjectileCollisionComponentData>()
        );

        var translation = query.ToComponentDataArray<Translation>(Allocator.TempJob);
        var projectile = query.ToComponentDataArray<ProjectileComponentData>(Allocator.TempJob);
        var collision = query.ToComponentDataArray<ProjectileCollisionComponentData>(Allocator.TempJob);

        for (int i = 0; i < translation.Length; i++)
        {
            if (collision[i].ownerFaction == FactionComponentData.Faction.ENEMY)
                continue;
            if (!projectile[i].itStopsRightNow)
                continue;
            if (!triggerZone.Contains(translation[i].Value.ToF2()))
                continue;

            progress?.IncreaceProgressInPlace(collision[i].processData.damage, translation[i].Value);
        }

        translation.Dispose();
        projectile.Dispose();
        collision.Dispose();
    }
}