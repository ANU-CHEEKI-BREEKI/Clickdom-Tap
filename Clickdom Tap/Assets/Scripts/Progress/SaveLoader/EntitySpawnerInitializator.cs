using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ASpawner))]
public class EntitySpawnerInitializator : MonoBehaviour
{
    private void Start()
    {
        Init();
    }

    private void Init()
    {
        GetComponent<ASpawner>().SpawnImmidiate();
    }
}
