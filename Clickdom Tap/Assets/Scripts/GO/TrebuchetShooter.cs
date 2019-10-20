using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class TrebuchetShooter : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform[] targetPositions;
    //[Space]
    //[SerializeField] ScaleByPositionSettings scaleByPosSettings;
    [Space]
    [SerializeField] private ShaderSpriteUvAnimationSetupData projectileRenderData;
    [SerializeField] private ProjectileLaunshSetupComponentData launchData;
    [SerializeField] private ProjectileCollisionComponentData collisionData;
    

    private EntityManager manager;

    private RenderSharedComponentData renderData;
    

    private void Start()
    {
        manager = World.Active.EntityManager;
        renderData = new RenderSharedComponentData()
        {
            material = EntitySpavner.Instance.animatedMeterial,
            mesh = EntitySpavner.Instance.quadMesh
        };
    }

    public void ShootRandomTarget()
    {
        var entity = manager.CreateEntity();
        manager.AddComponentData(entity, new Translation()
        {
            Value = spawnPoint.transform.position
        });
        manager.AddComponentData(entity, new Scale()
        {
            Value = transform.localScale.x + 0.3f
        });
        //manager.AddComponentData(entity, new ScaleByPositionComponentData()
        //{
        //    maxScale = scaleByPosSettings.MaxScale,
        //    minScale = scaleByPosSettings.MinScale,
        //    maxY = scaleByPosSettings.MaxY,
        //    minY = scaleByPosSettings.MinY
        //});
        manager.AddComponentData(entity, DataToComponentData.ToComponentData(projectileRenderData));
        manager.AddSharedComponentData(entity, renderData);
        manager.AddComponentData(entity, new RenderScaleComponentdata()
        {
            value = new Unity.Mathematics.float2(0.7f, 0.7f)
        });
        var launch = launchData;
        launch.targetPosition = targetPositions[UnityEngine.Random.Range(0, targetPositions.Length)].position.ToF2();
        manager.AddComponentData(entity, launch);
        manager.AddComponentData(entity, collisionData);
    }
}
