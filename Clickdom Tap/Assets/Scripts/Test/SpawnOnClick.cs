using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnClick : MonoBehaviour
{
    [SerializeField] GameObject[] prefabs;
    [SerializeField] bool setZByY;
    [SerializeField] ZByYSettings settings;

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            var pos = Utils.GetMouseWorldPosition().ToV3();

            if (setZByY)
                pos.z = pos.y * settings.Scale;

            foreach (var prefab in prefabs)
                Instantiate(prefab, pos, Quaternion.identity);
        }
    }
}
