using UnityEngine;
using System.Collections;
using Unity.Entities;

public class SimpleConvertionToEntity : MonoBehaviour
{
    [SerializeField] MonoBehaviour toConvert;

    private void Awake()
    {
        var convert = toConvert as IConvertGameObjectToEntity;
        if (convert == null)
            return;

        var manager = Unity.Entities.World.Active.EntityManager;
        var entity = manager.CreateEntity();
        convert.Convert(entity, manager, null);
    }
}
