using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ASpawner))]
public class EntitySpawnerInitializator : MonoBehaviour
{
    private void Start()
    {
        GetComponent<ASpawner>().SpawnImmidiate();
    }
}
