using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class RadialCenterPositionProvider : ASkillTargetPositionProvider
{
    [SerializeField] private float radius;

    public override Vector3 GetTargetPosition(Vector3 position, int maxCnt, int index, float scale = 1)
    {
        if (maxCnt == 0 || maxCnt == 1 || index == 0)
            return position;

        var radius = scale * this.radius;

        var angle = 360f / (maxCnt - 1) * Mathf.Deg2Rad;
        angle = angle * index;
        var sin = Mathf.Sin(angle);
        var cos = Mathf.Cos(angle);

        return new Vector3(cos * radius + position.x, sin * radius + position.y, position.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
