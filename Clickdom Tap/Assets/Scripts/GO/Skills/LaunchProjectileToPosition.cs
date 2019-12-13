using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class LaunchProjectileToPosition : MonoBehaviour, IDamageSettable
{
    public enum RotationType { NONE, TO_MOVE_DIRECTION }

    [SerializeField] private ShaderSpriteUvAnimationSetupData projectileRenderData;
    public ShaderSpriteUvAnimationSetupData ProjectileRenderData { get => projectileRenderData; set => projectileRenderData = value; }
    [SerializeField] private float2 renderScale;
    [Space]
    [SerializeField] private RotationType rotationType;
    [Space]
    [SerializeField] private ProjectileLaunshSetupComponentData launchData;
    [SerializeField] private ProjectileCollisionComponentData collisionData;
    [Space]
    [SerializeField] private bool drawTrails;
    public bool DrawTrails { get => drawTrails; set => drawTrails = value; }
    [SerializeField] private SpawnTrailsComponentData trailsData = new SpawnTrailsComponentData()
    {
        spawnDeltaTime = 0.1f,
        spawnData = new SpawnTrailData()
        {
            animationDurationByLifetime = true,
            trailLifetime = 0.5f,
            renderScale = new float2(1, 1),
            spawnRandPosition = new float3(1, 1, 1),
            startColor = Color.white,
            endColor=Color.white,
            interpolationEquation = new ANU.Utils.Math.QuadraticEquation(0,1,0)
        }
    };
    [Space]
    [SerializeField] private bool castShadows = true;
    [SerializeField] private ShadowSettings shadowSettings;
    [SerializeField] private Transform startShadowOffset;

    [SerializeField] bool disableIDamageSettable = false;

    public bool CastChadows { get => castShadows; set => castShadows = value; }
    public ProjectileLaunshSetupComponentData LaunchData { get => launchData; set => launchData = value; }

    private EntityManager manager;

    private RenderSharedComponentData renderData;

    private void Start()
    {
        manager = World.Active.EntityManager;
        renderData = new RenderSharedComponentData()
        {
            material = projectileRenderData.Material,
            mesh = projectileRenderData.Mesh,
        };
    }

    public void Launch(float3 from, float3 to, float scale = 1, string name = null)
    {
        var entity = manager.CreateEntity();

#if UNITY_EDITOR
        if(!string.IsNullOrEmpty(name))
            manager.SetName(entity, name);
#endif

        manager.AddComponentData(entity, new Translation()
        {
            Value = from
        });
        manager.AddComponentData(entity, new Scale()
        {
            Value = scale
        });
        manager.AddComponentData(entity, new SpriteRendererComponentData()
        {
            uv = projectileRenderData.UV
        });
        var animation = DataToComponentData.ToComponentData(projectileRenderData);
        if (animation.frameCount > 1)
            manager.AddComponentData(entity, animation);
        manager.AddSharedComponentData(entity, renderData);
        manager.AddComponentData(entity, new RenderScaleComponentdata()
        {
            value = renderScale
        });
        var launch = launchData;
        launch.targetPosition = to.ToF2();
        manager.AddComponentData(entity, launch);
        manager.AddComponentData(entity, collisionData);

        if(drawTrails)
        {
            manager.AddComponentData(entity, trailsData);
        }

        if (castShadows)
        {
            manager.AddComponentData(entity, DataToComponentData.ToComponentData(shadowSettings));
            manager.AddComponentData(entity, new CastProjectileShadowsTagComponentData()
            {
                defaultAlpha = shadowSettings.ShadowsData.color.a,
                defaultScale = shadowSettings.ShadowsData.scale,
                scaleMultiplier = 1.5f,
                alphaMultiplier = 0.2f,
                startPositionOffset = new float2(0, startShadowOffset != null ? startShadowOffset.position.y - from.y : 0),
                maxYOffsetForLerpScaleAndAlpha = 7,
            });
        }

        if(rotationType == RotationType.TO_MOVE_DIRECTION)
        {
            manager.AddComponent<Rotation>(entity);
            manager.AddComponent<RotationToMoveDirectionTagComponentData>(entity);
        }
    }

    public void ForceSetDamage(float damage)
    {
        var temp = disableIDamageSettable;
        disableIDamageSettable = false;
        (this as IDamageSettable).SetDamage(damage);
        disableIDamageSettable = temp;
    }

    void IDamageSettable.SetDamage(float damage)
    {
        if (disableIDamageSettable)
            return;
        collisionData.processData.damage = damage;
    }
}
