using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(SequencePath))]
[ExecuteInEditMode]
public class AnimatorTrigger : MonoBehaviour, IConvertGameObjectToEntity
{
    private SequencePath path;
    [SerializeField] new private AnimationType animation;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var points = GetComponent<SequencePath>().CopyOfWorldPointsAsF2;
        dstManager.AddComponentData(entity, new AnimatorStateTriggerComponentData()
        {
            leftUpTriggetZoneCorner = points[0],
            rightUpTriggetZoneCorner = points[1],
            rightBotTriggetZoneCorner = points[2],
            leftBotTriggetZoneCorner = points[3],
            animation = animation
        });
#if UNITY_EDITOR
        dstManager.SetName(entity, gameObject.name);
#endif
    }

#if UNITY_EDITOR

    private void Start()
    {
        path = GetComponent<SequencePath>();
    }

    void Update()
    {
        while (path.LocalPoints.Count < 4)
            path.LocalPoints.Add(Vector3.zero);
        while (path.LocalPoints.Count > 4)
            path.LocalPoints.RemoveAt(path.LocalPoints.Count - 1);
    }

    private void OnDrawGizmos()
    {
        if (path == null)
            return;

        var points = path.CopyOfWorldPointsAsF2;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(points[0].ToV3(), points[1].ToV3());
        Gizmos.DrawLine(points[1].ToV3(), points[2].ToV3());
        Gizmos.DrawLine(points[2].ToV3(), points[3].ToV3());
        Gizmos.DrawLine(points[3].ToV3(), points[0].ToV3());

        Handles.Label(points[0].ToV3(), "left_up");
        Handles.Label(points[1].ToV3(), "right_up");
        Handles.Label(points[2].ToV3(), "right_bot");
        Handles.Label(points[3].ToV3(), "left_bot");

        Handles.Label(transform.position, animation.ToString());
    }

#endif
}
