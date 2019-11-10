using UnityEngine;
using System.Collections;
using Unity.Entities;

public class SimpleEntityConverter : MonoBehaviour
{
    public enum AfterConvertionEvent { DELETE, DEACTIVATE, NOTHING }

    [SerializeField] private AfterConvertionEvent afterConvEvent = AfterConvertionEvent.DEACTIVATE;

    private void Start()
    {
        ConvertToEntity();
    }

    public void ConvertToEntity()
    {
        var converters = GetComponents<ISimpleEntityConverter>();
        var manager = World.Active.EntityManager;
        var entity = manager.CreateEntity();

        foreach (var conv in converters)
            conv.ConvertToEntity(entity, manager);

        if (afterConvEvent == AfterConvertionEvent.DELETE)
            Destroy(gameObject);
        else if (afterConvEvent == AfterConvertionEvent.DEACTIVATE)
            gameObject.SetActive(false);
    }
}
