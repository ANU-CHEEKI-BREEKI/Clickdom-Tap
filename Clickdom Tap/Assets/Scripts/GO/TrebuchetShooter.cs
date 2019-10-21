using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[RequireComponent(typeof(LaunchProjectileToPosition))]
public class TrebuchetShooter : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform[] targetPositions;

    private LaunchProjectileToPosition launcher;

    private void Start()
    {
        launcher = GetComponent<LaunchProjectileToPosition>();
    }

    public void ShootRandomTarget()
    {
        launcher.Launch(
            from:   spawnPoint.transform.position, 
            to:     targetPositions[UnityEngine.Random.Range(0, targetPositions.Length)].position,
            scale:  transform.localScale.x + 0.3f,
            name:   "TrebuchedMissile"
        );
    }
}
