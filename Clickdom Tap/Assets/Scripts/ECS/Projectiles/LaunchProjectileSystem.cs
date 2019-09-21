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

public struct ProjectileLaunshSetupComponentData : IComponentData
{
    public const float g = 9.8f;
    public const float scaledG = 4.8f;

    public ProjectileComponentData.GrountType ground;
    public float targetWidth;

    public float2 targetPosition;
    /// <summary>
    /// x, y - всегда будут >= 0. если задать < 0, они будут автоматом приведены в > 0
    /// x - сопротивление горизонтальному движению
    /// y - сопротивление вертикальному движению (гравитация, короче) советую ставить ProjectileComponentData.g
    /// </summary>
    public float2 accelerationResistance;
    public bool removeEntityWhenProjectileStops;

    /// <summary>
    /// когда projectile остановит своё движение, этот счетчик начнет уменьшаться.
    /// когда lifetimeAfterProjectileStop <= 0 projectile entity будет удален
    /// </summary>
    public float lifetimeAfterProjectileStop;

    public float absoluteVelocity;
}

public class LaunchProjectileSystem : ComponentSystem
{
    EntityManager manager;
    EntityArchetype arrowArchetype;

    public static LaunchProjectileSystem Instance { get; private set; }

    protected override void OnCreate()
    {
        Instance = this;

        base.OnCreate();
        manager = World.Active.EntityManager;

        arrowArchetype = EntityManager.CreateArchetype(
            typeof(ProjectileLaunshSetupComponentData),
            typeof(RotationToMoveDirectionComponentData),
            typeof(Translation),
            typeof(Rotation),
            typeof(Scale),
            typeof(SpriteSheetAnimationComponentData),
            typeof(RenderScaleComponentdata),
            typeof(RenderSharedComponentData),
            typeof(ProjectileCollisionComponentData)
        );
    }

    protected override void OnUpdate()
    {
        var desc = new EntityQueryDesc()
        {
            All = new ComponentType[] { ComponentType.ReadOnly<ProjectileLaunshSetupComponentData>(), ComponentType.ReadOnly<Translation>(), ComponentType.ReadOnly<Scale>() },
            None = new ComponentType[] { ComponentType.ReadOnly<ProjectileComponentData>(), ComponentType.ReadOnly<VelocityComponentData>() }
        };
        var query = GetEntityQuery(desc);
        var entities = query.ToEntityArray(Allocator.TempJob);
        var translations = query.ToComponentDataArray<Translation>(Allocator.TempJob);
        var scales = query.ToComponentDataArray<Scale>(Allocator.TempJob);
        var setups = query.ToComponentDataArray<ProjectileLaunshSetupComponentData>(Allocator.TempJob);

        var l = entities.Length;
        for (int i = 0; i < l; i++)
        {
            var entity = entities[i];
            var setup = setups[i];
            var translation = translations[i];
            var scale = scales[i].Value;
            if (scale == 0) scale = 0.01f;


            manager.RemoveComponent<ProjectileLaunshSetupComponentData>(entity);
            manager.AddComponent<ProjectileComponentData>(entity);
            manager.SetComponentData(entity, new ProjectileComponentData()
            {
                accelerationResistance = setup.accelerationResistance,
                targetPosition = setup.targetPosition,
                startPosition = new float2(translation.Value.x, translation.Value.y),
                removeEntityWhenProjectileStops = setup.removeEntityWhenProjectileStops,
                lifetimeAfterProjectileStop = setup.lifetimeAfterProjectileStop,
                ground = setup.ground,
                targetWidth = setup.targetWidth
            });
            manager.AddComponent<VelocityComponentData>(entity);
            manager.SetComponentData(entity, new VelocityComponentData()
            {
                value = Utils.Physics.GetVelocity(translation.Value / scale, setup.targetPosition / scale, setup.absoluteVelocity, setup.accelerationResistance * -1)
            });
        }

        entities.Dispose();
        translations.Dispose();
        setups.Dispose();
        scales.Dispose();
    }

    public static void Launch(EntityManager manager, Entity entity, float absoluteVelocity, float mass, float2 direction)
    {
        if (!manager.HasComponent<ProjectileComponentData>(entity))
        {
            manager.AddComponent<ProjectileComponentData>(entity);
            var pos = manager.GetComponentData<Translation>(entity);
            var data = manager.GetComponentData<ProjectileComponentData>(entity);
            var scale = manager.GetComponentData<Scale>(entity);
            data.accelerationResistance = new float2(0, ProjectileLaunshSetupComponentData.scaledG);
            data.ground = ProjectileComponentData.GrountType.TARGET_Y;
            data.lifetimeAfterProjectileStop = 3;
            data.removeEntityWhenProjectileStops = true;
            data.targetPosition = new float2(0, pos.Value.y - 5f * scale.Value);
            manager.SetComponentData(entity, data);
            if (!manager.HasComponent<VelocityComponentData>(entity))
                manager.AddComponent<VelocityComponentData>(entity);
            var velocity = manager.GetComponentData<VelocityComponentData>(entity);
            velocity.value = direction.GetNormalized() * scale.Value * absoluteVelocity;
            manager.SetComponentData(entity, velocity);
        }
    }
    
    public void LaunchArrow(float3 startPos, float2 targetPos, float scale, ProjectileComponentData.GrountType ground, float absVelocity = 12, float g = ProjectileLaunshSetupComponentData.scaledG, float horResist = 0)
    {
        var arrow = manager.CreateEntity(arrowArchetype);

        manager.SetComponentData(arrow, new Translation()
        {
            Value = startPos
        });
        manager.SetComponentData(arrow, new ProjectileLaunshSetupComponentData()
        {
            targetPosition = targetPos,
            accelerationResistance = new float2(horResist, g),
            removeEntityWhenProjectileStops = true,
            absoluteVelocity = absVelocity,
            lifetimeAfterProjectileStop = 5f,
            ground = ground,
            targetWidth = 2 * scale
        });
        manager.SetComponentData(arrow, new Scale()
        {
            Value = scale
        });
        manager.SetComponentData(arrow, new Rotation()
        {
            Value = quaternion.identity
        });
        var data = EntitySpavner.Instance.arrow;
        data.InitRandomSprite();
        manager.SetComponentData(arrow, new SpriteSheetAnimationComponentData()
        {
            currentFrame = data.RamdomInitFrame,
            frameCount = data.FramesCount,
            frameDuration = data.FrameDuration,
            frameHeight = data.FrameHeigth,
            frameWidth = data.FrameWidth,
            horisontalOffset = data.HorisontalOffset,
            verticalOffset = data.VerticalOffset
        });
        manager.SetComponentData(arrow, new RenderScaleComponentdata()
        {
            value = Vector2.one * 0.35f
        });
        manager.SetComponentData(arrow, new ProjectileCollisionComponentData()
        {
            processData = new ProcessCollisionData()
            {
                type = HitProcessingType.LAUNCH_AS_PROJECTILE,
                destroyDelay = 5,
                absoluteProjectileVelocity = UnityEngine.Random.Range(6, 10)
            },
            maxHitCount = 1,
            colisionTimeOut = 1f,
            detectTime = ProjectileCollisionComponentData.DetectCillisionTime.WHEN_STOPS,
            ownerFaction = FactionComponentData.Faction.NEUTRAL
        });
        manager.SetSharedComponentData(arrow, new RenderSharedComponentData()
        {
            mesh = data.Mesh,
            material = data.Material
        });
    }
}