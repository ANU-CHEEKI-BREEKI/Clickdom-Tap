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

public class WholeProgressIncreacerByMeleeAttack : ComponentSystem
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

        var query = GetEntityQuery(
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<SwordsmanTagComponentData>(),
            ComponentType.ReadOnly<MeleeAttackComponentData>(),
            ComponentType.ReadOnly<FactionComponentData>()
        );

        var translation = query.ToComponentDataArray<Translation>(Allocator.TempJob);
        var faction = query.ToComponentDataArray<FactionComponentData>(Allocator.TempJob);
        var attack = query.ToComponentDataArray<MeleeAttackComponentData>(Allocator.TempJob);

        for (int i = 0; i < faction.Length; i++)
        {
            if (faction[i].value != FactionComponentData.Faction.ALLY)
                continue;
            if (!attack[i].attackEventFlag)
                continue;

            progress?.IncreaceProgressInPlace(attack[i].damage, translation[i].Value);
        }

        faction.Dispose();
        attack.Dispose();
        translation.Dispose();
    }
}