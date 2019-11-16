using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMatrixTRS : MonoBehaviour
{
    [SerializeField] Transform pos;
    [SerializeField] Vector2 size;
    [SerializeField] Vector2 pivot = Vector2.one / 2;

    private void OnDrawGizmos()
    {
        var oldColor = Gizmos.color;
        var oldMatrix = Gizmos.matrix;

        Gizmos.color = Color.cyan;

        var actualPivot = pivot - Vector2.one / 2;
        var pivotedPos = pos.position;
        pivotedPos.x = actualPivot.x * size.x;
        pivotedPos.y = actualPivot.y * size.y;

        var rotate = Matrix4x4.Rotate(pos.rotation);
        var scale = Matrix4x4.Scale(size);
        var trans = Matrix4x4.Translate(pos.position);
        var trans1 = Matrix4x4.Translate(pivotedPos);

        var matrix = trans;
        //matrix *= trans1;
        matrix *= rotate;
        matrix *= trans1;
        matrix *= scale;

        Gizmos.matrix = matrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.color = oldColor;
        Gizmos.matrix = oldMatrix;
    }
}
