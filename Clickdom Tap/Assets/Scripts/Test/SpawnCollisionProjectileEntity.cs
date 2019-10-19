using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class SpawnCollisionProjectileEntity : MonoBehaviour
{
    [SerializeField] private ProjectileCollisionComponentData collisionData;

    private void Awake()
    {
        var manager = World.Active.EntityManager;
        var entity = manager.CreateEntity();
        var pos = transform.position.ToF2();
        manager.AddComponentData(entity, new Translation() { Value = transform.position });
        manager.AddComponentData(entity, new Scale() { Value = transform.localScale.x });
        manager.AddComponentData(entity, collisionData);

        LaunchProjectileSystem.Launch(
            manager,
            entity,
            pos,
            0.01f,
            new Unity.Mathematics.float2(1, UnityEngine.Random.Range(-1, 1)),
            0
        );

        Destroy(gameObject);
    }
}
