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
using static ANU.Utils.Jobs;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class InstancedRendererCollectorSystem : ARendererCollectorSystem
{    
    ComponentType[] allSpecialQuery;
    protected override ComponentType[] AllSpecialQuery => allSpecialQuery;

    ComponentType[] allNoneQuery;
    protected override ComponentType[] AllNoneQuery => allNoneQuery;

    public static InstancedRendererCollectorSystem Instance { get; private set; }

    public InstancedRendererCollectorSystem()
    {
        Instance = this;
        UseAsDefault = true;

        allNoneQuery = new[]
        {
            ComponentType.ReadOnly<UseOnlyDynamicRendererTagComponentData>()
        };

        allSpecialQuery = AllDefaultQuery
        .Concat(
            new[]
            {
                ComponentType.ReadOnly<UseOnlyInstancedRendererTagComponentData>()
            }
        ).ToArray();
    }    
}