using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class CastShadowsShift : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private SequencePath zonePath;
    [SerializeField] private SequencePath lerpPath;
    [SerializeField] private SequencePath addOffsetPath;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var zoneBounds = zonePath.CopyOfWorldPointsAsF2;
        var lerpPoints = lerpPath.CopyOfWorldPointsAsF2;

        var pos = transform.position.ToF2();
        var offsetPoints = addOffsetPath.CopyOfWorldPointsAsF2.Select(p => p -= pos).ToArray();
        dstManager.AddComponentData(entity, new ShiftCastShadowsZoneComponentTagData()
        {
            leftUpTriggetZoneCorner = zoneBounds[0],
            rightUpTriggetZoneCorner = zoneBounds[1],
            rightBotTriggetZoneCorner = zoneBounds[2],
            leftBotTriggetZoneCorner = zoneBounds[3],
            startLerpPosition = lerpPoints[0],
            endLerpPosition = lerpPoints[1],
            startAdditionalPositionUnitsOffset = offsetPoints[0],
            endAdditionalPositionUnitsOffset = offsetPoints[1]
        });

#if UNITY_EDITOR
        dstManager.SetName(entity, gameObject.name + "_" + nameof(CastShadowsShift));
#endif
    }

#if UNITY_EDITOR

    private void Update()
    {
        ControlPointsCount(zonePath, 4);
        ControlPointsCount(lerpPath, 2);
        ControlPointsCount(addOffsetPath, 2);
    }

    private void ControlPointsCount(SequencePath path, int pointsCount)
    {
        if (path == null)
            return;

        while (path.LocalPoints.Count < pointsCount)
            path.LocalPoints.Add(Vector3.zero);
        while (path.LocalPoints.Count > pointsCount)
            path.LocalPoints.RemoveAt(path.LocalPoints.Count - 1);
    }

    private void OnDrawGizmosSelected()
    {
        float2[] points;

        if (zonePath != null)
        {
            points = zonePath.CopyOfWorldPointsAsF2;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(points[0].ToV3(), points[1].ToV3());
            Gizmos.DrawLine(points[1].ToV3(), points[2].ToV3());
            Gizmos.DrawLine(points[2].ToV3(), points[3].ToV3());
            Gizmos.DrawLine(points[3].ToV3(), points[0].ToV3());
            Handles.Label(points[0].ToV3(), "left_up");
            Handles.Label(points[1].ToV3(), "right_up");
            Handles.Label(points[2].ToV3(), "right_bot");
            Handles.Label(points[3].ToV3(), "left_bot");
        }

        if (lerpPath != null)
        {
            points = lerpPath.CopyOfWorldPointsAsF2;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(points[0].ToV3(), points[1].ToV3());
            Handles.Label(points[0].ToV3(), "lerp_start");
            Handles.Label(points[1].ToV3(), "lerp_end");
        }

        if (addOffsetPath != null)
        {
            points = addOffsetPath.CopyOfWorldPointsAsF2;
            var pos = transform.position.ToF2();
            var pp = addOffsetPath.CopyOfWorldPointsAsF2.Select(p => p -= pos).ToArray();
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(points[0].ToV3(), points[1].ToV3());
            Handles.Label(points[0].ToV3(), "offset_start");
            Handles.Label(points[1].ToV3(), "offset_end");
            Handles.Label(points[0].ToV3() + Vector3.one / 3, pp[0].ToString());
            Handles.Label(points[1].ToV3() + Vector3.one / 3, pp[1].ToString());
        }
    }

#endif
}
