using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;

[RequireComponent(typeof(SimpleEntityConverter))]
public class SimpleEntityTransformConverter : MonoBehaviour, ISimpleEntityConverter
{
    [Header("optional settings")]
    [SerializeField] private ScaleByPositionSettings scaleSettings;
    [SerializeField] protected ZByYSettings ZbyYSettings;

    public void ConvertToEntity(Entity entity, EntityManager manager)
    {
        var transform = this.transform;

        manager.AddComponentData(entity, new Translation()
        {
            Value = transform.position
        });
        manager.AddComponentData(entity, new Scale()
        {
            Value = transform.lossyScale.Average2D()
        });
        manager.AddComponentData(entity, new Rotation()
        {
            Value = transform.rotation
        });
        if (scaleSettings != null)
            manager.AddComponentData(entity, DataToComponentData.ToComponentData(scaleSettings));
        if (ZbyYSettings != null)
            manager.AddComponentData(entity, DataToComponentData.ToComponentData(ZbyYSettings));        
    }
}
