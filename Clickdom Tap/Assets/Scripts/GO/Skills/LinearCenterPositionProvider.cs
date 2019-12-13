using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class LinearCenterPositionProvider : ASkillTargetPositionProvider
{
    [SerializeField] private Rect bounds;
    [SerializeField] private Vector2 perspectivePoint;

    private void OnValidate()
    {
        //bounds.size = new Vector2(20, 20);
        bounds.center = transform.position;
    }

    public override Vector3 GetTargetPosition(Vector3 position, int maxCnt, int index, float scale = 1)
    {
        if (maxCnt == 0 || maxCnt == 1)
            return position;

        var pos = new Vector3(
            Mathf.Clamp(position.x, bounds.min.x, bounds.max.x),
            Mathf.Clamp(position.y, bounds.min.y, bounds.max.y),
            position.z
        );

        var perspectiveLine = ANU.Utils.Math.GetLineEquation(new float2(position.x, bounds.yMin), perspectivePoint);
        var minPos = ANU.Utils.Math.GetLinesCrossPoint(
            perspectiveLine,
            ANU.Utils.Math.GetLineEquation(new float2(bounds.xMin, bounds.yMin), new float2(bounds.xMax, bounds.yMin))
        ).ToV3(position.z);
        var maxPos = ANU.Utils.Math.GetLinesCrossPoint(
            perspectiveLine,
            ANU.Utils.Math.GetLineEquation(new float2(bounds.xMin, bounds.yMax), new float2(bounds.xMax, bounds.yMax))
        ).ToV3(position.z);

        var newPos = Vector3.Lerp(minPos, maxPos, (float)index / (maxCnt - 1));
        return newPos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.DrawSphere(perspectivePoint, 1);
    }
}
