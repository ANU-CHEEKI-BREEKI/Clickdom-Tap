using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

[ExecuteInEditMode]
public class SequencePath : MonoBehaviour
{
    [HideInInspector] [SerializeField] List<Vector3> localPositions = new List<Vector3>() { Vector3.left, Vector3.right};
    public const int MinPositionsCount = 2;

    public List<Vector3> LocalPoints { get { return localPositions; } }
    public float2[] CopyOfLocalPointsAsF2 { get { return localPositions.Select(p => new float2(p.x, p.y)).ToArray(); } }
    public float2[] CopyOfWorldPointsAsF2 { get { return localPositions.Select(p => transform.TransformPoint(p)).Select(p => new float2(p.x, p.y)).ToArray(); } }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Selection.gameObjects.Contains(this.gameObject))
        {
            var radius = 0.1f;
            Gizmos.color = Color.red;
            for (int i = 0; i < localPositions.Count - 1; i++)
            {
                var point = localPositions[i];
                point = transform.TransformPoint(point);
                var nextpoint = localPositions[i + 1];
                nextpoint = transform.TransformPoint(nextpoint);
                Gizmos.DrawWireSphere(point, radius);
                Gizmos.DrawLine(point, nextpoint);
            }
            if (LocalPoints.Count > 0)
                Gizmos.DrawWireSphere(transform.TransformPoint(localPositions[localPositions.Count - 1]), radius);
        }
    }
#endif
}
