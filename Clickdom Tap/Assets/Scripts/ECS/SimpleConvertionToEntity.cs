using UnityEngine;
using System.Collections;
using Unity.Entities;

public class SimpleConvertionToEntity : MonoBehaviour
{
    private void Awake()
    {
        var toConvert = GetComponents<IConvertGameObjectToEntity>();

        if (toConvert == null)
            return;

        var manager = Unity.Entities.World.Active.EntityManager;
        var entity = manager.CreateEntity();
        foreach (var item in toConvert)
            item.Convert(entity, manager, null);
    }
}
