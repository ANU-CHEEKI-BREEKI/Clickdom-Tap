using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

public struct ArcherTagComponentData : IComponentData { }

public struct LerpShootTargetProvederComponentData : IComponentData
{
    public float2 leftUpCorner;
    public float2 rightUpCocner;

    public float2 rightBotCorner;
    public float2 leftBotCorner;

    public float randomSpread;

    public float2 EvaluateShootTarget(float tx, float ty, bool leftToRight, bool botToTop)
    {
        float2 lerp1, lerp2, lerp3;
        if (leftToRight)
        {
            lerp1 = math.lerp(leftUpCorner, rightUpCocner, tx);
            lerp2 = math.lerp(leftBotCorner, rightBotCorner, tx);
        }
        else
        {
            lerp1 = math.lerp(rightUpCocner, leftUpCorner, tx);
            lerp2 = math.lerp(rightBotCorner, leftBotCorner, tx);
        }
        
        if(botToTop)
        {
            lerp3 = math.lerp(lerp2, lerp1, ty);
        }
        else
        {
            lerp3 = math.lerp(lerp1, lerp2, ty);
        }

        return lerp3;
    }
}


public class ArcherSpawner : ASpawner, IDamageSettable
{
    [SerializeField] ProjectilesData projectilesData;
    [Space]
    [SerializeField] Transform shootTarget1;
    [SerializeField] Transform shootTarget2;
    [SerializeField] Transform shootTarget3;
    [SerializeField] Transform shootTarget4;
    [SerializeField] float shootRandomSpread = 0.5f;

    private SquadProjectileLaunchDataSharedComponentData launchArrowData;

    protected override void Start()
    {
        base.Start();

        launchArrowData = new SquadProjectileLaunchDataSharedComponentData()
        {
            renderScaleData = new RenderScaleComponentdata()
            {
                value = projectilesData.RenderScale
            },
            spriteData = new SpriteRendererComponentData()
            {
                uv = projectilesData.Animation.RandomUV
            },
            animaionData = DataToComponentData.ToComponentData(projectilesData.Animation),
            renderData = new RenderSharedComponentData()
            {
                material = projectilesData.Animation.Material,
                mesh = projectilesData.Animation.Mesh,
            },
            collisionData = projectilesData.Collision,
            launchData = projectilesData.Launch
        };
    }

    protected override void SetEntityComponentsData(Entity entity, EntityManager manager)
    {
        base.SetEntityComponentsData(entity, manager);

        manager.AddComponent(entity, typeof(ArcherTagComponentData));
        manager.AddComponentData(entity, new ArcherTargetPositionComponentData()
        {
            value = new float2(10, 10)
        });
        if (shootTarget1 != null && shootTarget2 != null && shootTarget3 != null && shootTarget4 != null)
        {
            manager.AddComponentData(entity, new LerpShootTargetProvederComponentData()
            {
                leftUpCorner = shootTarget1.position.ToF2(),
                rightUpCocner = shootTarget2.position.ToF2(),
                rightBotCorner = shootTarget3.position.ToF2(),
                leftBotCorner = shootTarget4.position.ToF2(),
                randomSpread = shootRandomSpread
            });
        }
        manager.AddComponent<VolleyAnimationPauseTagComponentData>(entity);
    }

    protected override void SetEntitySharedComponentsData(Entity entity, EntityManager manager)
    {
        base.SetEntitySharedComponentsData(entity, manager);

        manager.AddSharedComponentData(entity, launchArrowData);
    }

    protected override void EndInitEntityData(Entity entity, EntityManager manager)
    {
        base.EndInitEntityData(entity, manager);

        AnimationSetterUtil.SetAnimation(manager, entity, AnimationType.SHOOT, 4);
    }

    protected override string GenerateEntityName()
    {
        return $"Archer {faction}";
    }

    public void SetDamage(float damage)
    {
        launchArrowData.collisionData.processData.damage = damage;
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (shootTarget1 != null && shootTarget2 != null && shootTarget3 != null && shootTarget4 != null)
        {
            Gizmos.DrawLine(shootTarget1.position, shootTarget2.position);
            Gizmos.DrawLine(shootTarget2.position, shootTarget3.position);
            Gizmos.DrawLine(shootTarget3.position, shootTarget4.position);
            Gizmos.DrawLine(shootTarget4.position, shootTarget1.position);
            Handles.Label(shootTarget1.position, "left_up");
            Handles.Label(shootTarget2.position, "right_up");
            Handles.Label(shootTarget3.position, "right_bot");
            Handles.Label(shootTarget4.position, "left_bot");
        }
    }
#endif
}
