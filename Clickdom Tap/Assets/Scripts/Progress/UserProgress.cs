using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UserProgress : MonoBehaviour
{
    [SerializeField] int id;
    [SerializeField] Progress count = new Progress(0, 60);
    [SerializeField] Progress spawnRate = new Progress(1, 60);
    [SerializeField] Progress damage = new Progress(1, 60);
    [SerializeField] Progress attackSpeed = new Progress(1, 60);

    public Progress Count => count;
    public Progress SpawnRate => spawnRate;
    public Progress Damage => damage;
    public Progress AttackSpeed => attackSpeed;

    public int Id { get { return id; } set { id = value; } }

#if UNITY_EDITOR
    [ContextMenu("show json")]
    private void ShowJson()
    {
        Debug.Log(JsonUtility.ToJson(this));
    }
#endif
}
