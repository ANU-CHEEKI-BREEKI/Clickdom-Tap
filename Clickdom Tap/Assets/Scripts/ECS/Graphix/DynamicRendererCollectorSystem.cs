using ANU.Utils;
using System;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateBefore(typeof(InstancedRendererCollectorSystem))]
public class DynamicRendererCollectorSystem : ARendererCollectorSystem
{
    ComponentType[] allSpecialQuery;
    protected override ComponentType[] AllSpecialQuery => allSpecialQuery;

    ComponentType[] allNoneQuery;
    protected override ComponentType[] AllNoneQuery => allNoneQuery;

    public static DynamicRendererCollectorSystem Instance { get; private set; }

    public DynamicRendererCollectorSystem()
    {
        Instance = this;
        UseAsDefault = false;

        allNoneQuery = new[]
        {
            ComponentType.ReadOnly<UseOnlyInstancedRendererTagComponentData>()
        };

        allSpecialQuery = AllDefaultQuery
        .Concat(
            new[]
            {
                ComponentType.ReadOnly<UseOnlyDynamicRendererTagComponentData>()
            }
        ).ToArray();
    }
}