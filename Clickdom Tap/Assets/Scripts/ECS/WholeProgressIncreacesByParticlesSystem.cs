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
using static WholeProgressParticlesCollectorSystem;

[UpdateBefore(typeof(DeleteProjectileSystem))]
[UpdateAfter(typeof(WholeProgressParticlesCollectorSystem))]
public class WholeProgressIncreacesByParticlesSystem : ComponentSystem
{
    private WholeProgressHandle progress;

    private bool itialized = false;

    public void Init(WholeProgressHandle progress)
    {
        this.progress = progress;

        itialized = true;
    }

    protected override void OnUpdate()
    {
        if (!itialized)
            return;

        WholeProgressParticlesCollectorSystem.collectJobHandle.Complete();
        var progressData = WholeProgressParticlesCollectorSystem.triggeredProjectiles;

        ProgressData data;
        while (progressData.TryDequeue(out data))
            progress?.IncreaceProgressInPlace(data.damage, data.position);
    }
}