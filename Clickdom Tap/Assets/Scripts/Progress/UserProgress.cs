using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UserProgress : ASavable
{
    [SerializeField] Progress count = new Progress(0, 60);
    [SerializeField] Progress spawnRate = new Progress(1, 60);
    [SerializeField] Progress damage = new Progress(1, 60);
    [SerializeField] Progress attackSpeed = new Progress(1, 60);

    public Progress Count => count;
    public Progress SpawnRate => spawnRate;
    public Progress Damage => damage;
    public Progress AttackSpeed => attackSpeed;

#if UNITY_EDITOR
    [ContextMenu("show json")]
    private void ShowJson()
    {
        Debug.Log(JsonUtility.ToJson(this));
    }
#endif
}
