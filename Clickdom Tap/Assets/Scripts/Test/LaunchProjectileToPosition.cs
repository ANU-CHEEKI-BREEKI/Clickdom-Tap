using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class LaunchProjectileToPosition : MonoBehaviour, IDamageSettable
{
    [Space]
    [SerializeField] private ShaderSpriteUvAnimationSetupData projectileRenderData;
    [SerializeField] private bool animatedProjectile = false;
    [SerializeField] private float2 renderScale;

    [SerializeField] private ProjectileLaunshSetupComponentData launchData;
    [SerializeField] private ProjectileCollisionComponentData collisionData;
    [Space]
    [SerializeField] private bool castShadows = true;
    [SerializeField] private ShadowSettings shadowSettings;
    [SerializeField] private Transform startShadowOffset;

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
        if(animatedProjectile)
            manager.AddComponentData(entity, DataToComponentData.ToComponentData(projectileRenderData));
        manager.AddSharedComponentData(entity, renderData);
        manager.AddComponentData(entity, new RenderScaleComponentdata()
        {
            value = renderScale
        });
        var launch = launchData;
        launch.targetPosition = to.ToF2();
        manager.AddComponentData(entity, launch);
        manager.AddComponentData(entity, collisionData);

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
    }

    void IDamageSettable.SetDamage(float damage)
    {
        collisionData.processData.damage = damage;
    }
}
